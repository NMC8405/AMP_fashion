using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Areas.QuanLy.Controllers
{
    /// <summary>Quản lý tài khoản người dùng - chỉ dành cho Quản trị viên (UC23-UC27 phía Admin).</summary>
    [Area("QuanLy")]
    [Authorize(Roles = "QuanTriVien")]
    public class TaiKhoanController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly PasswordHasher<NguoiDung> _hasher = new();

        public TaiKhoanController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult DanhSach() => RedirectToAction(nameof(KhachHang));

        // ================= Quản lý Nhân viên (Figma: Admin-Quản lý Nhân viên) =================
        [HttpGet]
        public async Task<IActionResult> NhanVien(string? tuKhoa, VaiTro? vaiTro, TrangThaiTaiKhoan? trangThai)
        {
            var query = _db.NguoiDungs
                .Where(n => n.VaiTro == VaiTro.NhanVien || n.VaiTro == VaiTro.QuanTriVien)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                query = query.Where(n => n.HoTen.ToLower().Contains(tk) || n.Email.ToLower().Contains(tk) || (n.SoDienThoai != null && n.SoDienThoai.Contains(tk)));
            }

            if (vaiTro.HasValue)
            {
                query = query.Where(n => n.VaiTro == vaiTro.Value);
            }

            if (trangThai.HasValue)
            {
                query = query.Where(n => n.TrangThai == trangThai.Value);
            }

            // KPIs
            ViewBag.TongNhanVien = await _db.NguoiDungs.CountAsync(n => n.VaiTro == VaiTro.NhanVien || n.VaiTro == VaiTro.QuanTriVien);
            ViewBag.TongQuanTriVien = await _db.NguoiDungs.CountAsync(n => n.VaiTro == VaiTro.QuanTriVien);
            ViewBag.TongQuanLy = await _db.NguoiDungs.CountAsync(n => n.VaiTro == VaiTro.NhanVien);

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            ViewBag.TrangThai = trangThai;

            var danhSach = await query
                .OrderByDescending(n => n.NgayTao)
                .Select(n => new NhanVienItemViewModel
                {
                    Id = n.Id,
                    HoTen = n.HoTen,
                    Email = n.Email,
                    SoDienThoai = n.SoDienThoai,
                    NgayThamGia = n.NgayTao,
                    VaiTro = n.VaiTro,
                    TrangThai = n.TrangThai
                })
                .ToListAsync();

            return View(danhSach);
        }

        // ================= Quản lý Khách hàng (Figma: Admin-Quản lý Khách hàng) =================
        [HttpGet]
        public async Task<IActionResult> KhachHang(string? tuKhoa, TrangThaiTaiKhoan? trangThai)
        {
            var query = _db.NguoiDungs
                .Where(n => n.VaiTro == VaiTro.KhachHang)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                query = query.Where(n => n.HoTen.ToLower().Contains(tk) || n.Email.ToLower().Contains(tk) || (n.SoDienThoai != null && n.SoDienThoai.Contains(tk)));
            }

            if (trangThai.HasValue)
            {
                query = query.Where(n => n.TrangThai == trangThai.Value);
            }

            var dauThangNay = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // KPIs
            var tongKhachHang = await _db.NguoiDungs.CountAsync(n => n.VaiTro == VaiTro.KhachHang);
            var khachMoiThangNay = await _db.NguoiDungs.CountAsync(n => n.VaiTro == VaiTro.KhachHang && n.NgayTao >= dauThangNay);

            // Tải khách hàng kèm đơn hàng để tính tổng chi tiêu
            var rawList = await query
                .OrderByDescending(n => n.NgayTao)
                .Select(n => new
                {
                    n.Id,
                    n.HoTen,
                    n.Email,
                    n.SoDienThoai,
                    n.NgayTao,
                    n.TrangThai,
                    TongDon = n.DonHangs.Count(d => d.TrangThaiDonHang != TrangThaiDonHang.DaHuy),
                    TongChiTieu = n.DonHangs.Where(d => d.TrangThaiDonHang != TrangThaiDonHang.DaHuy).Sum(d => d.TongTien)
                })
                .ToListAsync();

            var vipCount = rawList.Count(x => x.TongChiTieu >= 10000000 || x.TongDon >= 5);

            ViewBag.TongKhachHang = tongKhachHang;
            ViewBag.KhachMoiThangNay = khachMoiThangNay;
            ViewBag.KhachHangVIP = vipCount;
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;

            var danhSach = rawList.Select(x => new KhachHangItemViewModel
            {
                Id = x.Id,
                HoTen = x.HoTen,
                Email = x.Email,
                SoDienThoai = x.SoDienThoai,
                NgayTao = x.NgayTao,
                TongDon = x.TongDon,
                TongChiTieu = x.TongChiTieu,
                TrangThai = x.TrangThai
            }).ToList();

            return View(danhSach);
        }

        // ================= UC23 (Admin): Thêm tài khoản =================
        [HttpGet]
        public IActionResult Them(string? loai, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            var defaultRole = loai == "nhanvien" ? VaiTro.NhanVien : VaiTro.KhachHang;
            return View(new TaiKhoanFormViewModel { VaiTro = defaultRole });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(TaiKhoanFormViewModel model, string? returnUrl)
        {
            if (string.IsNullOrWhiteSpace(model.MatKhau) || model.MatKhau.Length < 6)
                ModelState.AddModelError(nameof(model.MatKhau), "Vui lòng nhập mật khẩu tối thiểu 6 ký tự cho tài khoản mới.");

            var trungEmail = await _db.NguoiDungs.AnyAsync(n => n.Email == model.Email.Trim().ToLower());
            if (trungEmail) ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");

            if (!ModelState.IsValid)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            var nguoiDung = new NguoiDung
            {
                HoTen = model.HoTen.Trim(),
                Email = model.Email.Trim().ToLower(),
                SoDienThoai = model.SoDienThoai?.Trim(),
                VaiTro = model.VaiTro,
                TrangThai = TrangThaiTaiKhoan.HoatDong
            };
            nguoiDung.MatKhauHash = _hasher.HashPassword(nguoiDung, model.MatKhau!);

            _db.NguoiDungs.Add(nguoiDung);
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã thêm tài khoản \"{nguoiDung.HoTen}\".";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return nguoiDung.VaiTro == VaiTro.KhachHang
                ? RedirectToAction(nameof(KhachHang))
                : RedirectToAction(nameof(NhanVien));
        }

        // ================= UC24 (Admin): Sửa tài khoản =================
        [HttpGet]
        public async Task<IActionResult> Sua(int id, string? returnUrl)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            ViewBag.ReturnUrl = returnUrl;
            return View(new TaiKhoanFormViewModel
            {
                Id = user.Id,
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai,
                VaiTro = user.VaiTro
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(int id, TaiKhoanFormViewModel model, string? returnUrl)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            var trungEmail = await _db.NguoiDungs.AnyAsync(n => n.Email == model.Email.Trim().ToLower() && n.Id != id);
            if (trungEmail) ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");

            if (!string.IsNullOrWhiteSpace(model.MatKhau) && model.MatKhau.Length < 6)
                ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu mới cần tối thiểu 6 ký tự.");

            if (!ModelState.IsValid)
            {
                model.Id = id;
                ViewBag.ReturnUrl = returnUrl;
                return View(model);
            }

            user.HoTen = model.HoTen.Trim();
            user.Email = model.Email.Trim().ToLower();
            user.SoDienThoai = model.SoDienThoai?.Trim();
            user.VaiTro = model.VaiTro;
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
                user.MatKhauHash = _hasher.HashPassword(user, model.MatKhau);

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật tài khoản \"{user.HoTen}\".";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return user.VaiTro == VaiTro.KhachHang
                ? RedirectToAction(nameof(KhachHang))
                : RedirectToAction(nameof(NhanVien));
        }

        // ================= UC25 (Admin): Khóa tài khoản =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Khoa(int id, string? returnUrl)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            if (user.VaiTro == VaiTro.QuanTriVien)
            {
                TempData["Loi"] = "Không thể khóa tài khoản Quản trị viên.";
                return SafeRedirect(returnUrl, user.VaiTro);
            }

            var soDonChuaHoanTat = await _db.DonHangs.CountAsync(d => d.NguoiDungId == id
                && d.TrangThaiDonHang != TrangThaiDonHang.DaGiao && d.TrangThaiDonHang != TrangThaiDonHang.DaHuy);

            user.TrangThai = TrangThaiTaiKhoan.BiKhoa;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = soDonChuaHoanTat > 0
                ? $"Đã khóa tài khoản. Lưu ý: tài khoản này đang có {soDonChuaHoanTat} đơn hàng chưa hoàn tất."
                : "Đã khóa tài khoản thành công.";

            return SafeRedirect(returnUrl, user.VaiTro);
        }

        // ================= UC26 (Admin): Mở khóa tài khoản =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoKhoa(int id, string? returnUrl)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            user.TrangThai = TrangThaiTaiKhoan.HoatDong;
            user.SoLanDangNhapSai = 0;
            user.KhoaDangNhapDenLuc = null;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đã mở khóa tài khoản thành công.";
            return SafeRedirect(returnUrl, user.VaiTro);
        }

        // ================= UC27 (Admin): Xóa tài khoản =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id, string? returnUrl)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            if (user.VaiTro == VaiTro.QuanTriVien)
            {
                TempData["Loi"] = "Không thể xóa tài khoản Quản trị viên.";
                return SafeRedirect(returnUrl, user.VaiTro);
            }

            var conDonChuaHoanTat = await _db.DonHangs.AnyAsync(d => d.NguoiDungId == id
                && d.TrangThaiDonHang != TrangThaiDonHang.DaGiao && d.TrangThaiDonHang != TrangThaiDonHang.DaHuy);

            if (conDonChuaHoanTat)
            {
                TempData["Loi"] = "Tài khoản còn đơn hàng chưa hoàn tất. Vui lòng xử lý xong đơn trước khi xóa.";
                return SafeRedirect(returnUrl, user.VaiTro);
            }

            _db.GioHangItems.RemoveRange(_db.GioHangItems.Where(g => g.NguoiDungId == id));
            _db.YeuThichs.RemoveRange(_db.YeuThichs.Where(y => y.NguoiDungId == id));
            await _db.SaveChangesAsync();

            var conLichSuDonHang = await _db.DonHangs.AnyAsync(d => d.NguoiDungId == id);
            if (conLichSuDonHang)
            {
                user.TrangThai = TrangThaiTaiKhoan.BiKhoa;
                user.Email = $"deleted_{user.Id}_{user.Email}";
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Tài khoản có lịch sử đơn hàng nên đã được vô hiệu hóa an toàn.";
            }
            else
            {
                _db.NguoiDungs.Remove(user);
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xóa tài khoản vĩnh viễn.";
            }

            return SafeRedirect(returnUrl, user.VaiTro);
        }

        private IActionResult SafeRedirect(string? returnUrl, VaiTro vaiTro)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return vaiTro == VaiTro.KhachHang
                ? RedirectToAction(nameof(KhachHang))
                : RedirectToAction(nameof(NhanVien));
        }
    }
}

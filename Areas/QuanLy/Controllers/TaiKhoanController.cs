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
        public async Task<IActionResult> DanhSach(string? tuKhoa, VaiTro? vaiTro)
        {
            var query = _db.NguoiDungs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                query = query.Where(n => n.HoTen.ToLower().Contains(tk) || n.Email.ToLower().Contains(tk));
            }
            if (vaiTro.HasValue) query = query.Where(n => n.VaiTro == vaiTro.Value);

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.VaiTro = vaiTro;
            return View(await query.OrderByDescending(n => n.NgayTao).ToListAsync());
        }

        // ================= UC23 (Admin): Thêm tài khoản =================
        [HttpGet]
        public IActionResult Them() => View(new TaiKhoanFormViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(TaiKhoanFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.MatKhau) || model.MatKhau.Length < 6)
                ModelState.AddModelError(nameof(model.MatKhau), "Vui lòng nhập mật khẩu tối thiểu 6 ký tự cho tài khoản mới.");

            var trungEmail = await _db.NguoiDungs.AnyAsync(n => n.Email == model.Email.Trim().ToLower());
            if (trungEmail) ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");

            if (!ModelState.IsValid) return View(model);

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
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC24 (Admin): Sửa tài khoản =================
        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

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
        public async Task<IActionResult> Sua(int id, TaiKhoanFormViewModel model)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            var trungEmail = await _db.NguoiDungs.AnyAsync(n => n.Email == model.Email.Trim().ToLower() && n.Id != id);
            if (trungEmail) ModelState.AddModelError(nameof(model.Email), "Email này đã được sử dụng.");

            if (!string.IsNullOrWhiteSpace(model.MatKhau) && model.MatKhau.Length < 6)
                ModelState.AddModelError(nameof(model.MatKhau), "Mật khẩu mới cần tối thiểu 6 ký tự.");

            if (!ModelState.IsValid) { model.Id = id; return View(model); }

            user.HoTen = model.HoTen.Trim();
            user.Email = model.Email.Trim().ToLower();
            user.SoDienThoai = model.SoDienThoai?.Trim();
            user.VaiTro = model.VaiTro;
            if (!string.IsNullOrWhiteSpace(model.MatKhau))
                user.MatKhauHash = _hasher.HashPassword(user, model.MatKhau);

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã cập nhật tài khoản \"{user.HoTen}\".";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC25 (Admin): Khóa tài khoản =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Khoa(int id)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            if (user.VaiTro == VaiTro.QuanTriVien)
            {
                TempData["Loi"] = "Không thể khóa tài khoản Quản trị viên khác.";
                return RedirectToAction(nameof(DanhSach));
            }

            var soDonChuaHoanTat = await _db.DonHangs.CountAsync(d => d.NguoiDungId == id
                && d.TrangThaiDonHang != TrangThaiDonHang.DaGiao && d.TrangThaiDonHang != TrangThaiDonHang.DaHuy);

            user.TrangThai = TrangThaiTaiKhoan.BiKhoa;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = soDonChuaHoanTat > 0
                ? $"Đã khóa tài khoản. Lưu ý: tài khoản này đang có {soDonChuaHoanTat} đơn hàng chưa hoàn tất."
                : "Đã khóa tài khoản.";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC26 (Admin): Mở khóa tài khoản =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoKhoa(int id)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            user.TrangThai = TrangThaiTaiKhoan.HoatDong;
            user.SoLanDangNhapSai = 0;
            user.KhoaDangNhapDenLuc = null;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đã mở khóa tài khoản.";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC27 (Admin): Xóa tài khoản =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var user = await _db.NguoiDungs.FindAsync(id);
            if (user == null) return NotFound();

            if (user.VaiTro == VaiTro.QuanTriVien)
            {
                TempData["Loi"] = "Không thể xóa tài khoản Quản trị viên khác.";
                return RedirectToAction(nameof(DanhSach));
            }

            var conDonChuaHoanTat = await _db.DonHangs.AnyAsync(d => d.NguoiDungId == id
                && d.TrangThaiDonHang != TrangThaiDonHang.DaGiao && d.TrangThaiDonHang != TrangThaiDonHang.DaHuy);

            if (conDonChuaHoanTat)
            {
                TempData["Loi"] = "Tài khoản này còn đơn hàng chưa hoàn tất. Vui lòng xử lý xong (giao hàng hoặc hủy) trước khi xóa.";
                return RedirectToAction(nameof(DanhSach));
            }

            // Xóa dữ liệu phụ thuộc không quan trọng để có thể xóa hẳn tài khoản (đơn hàng lịch sử được giữ nguyên nhờ ràng buộc Restrict)
            _db.GioHangItems.RemoveRange(_db.GioHangItems.Where(g => g.NguoiDungId == id));
            _db.YeuThichs.RemoveRange(_db.YeuThichs.Where(y => y.NguoiDungId == id));
            await _db.SaveChangesAsync();

            var conLichSuDonHang = await _db.DonHangs.AnyAsync(d => d.NguoiDungId == id);
            if (conLichSuDonHang)
            {
                // Vẫn còn lịch sử đơn hàng (đã giao/đã hủy) -> không thể xóa cứng do ràng buộc khóa ngoại, chuyển sang khóa vĩnh viễn
                user.TrangThai = TrangThaiTaiKhoan.BiKhoa;
                user.Email = $"deleted_{user.Id}_{user.Email}";
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Tài khoản có lịch sử đơn hàng nên được vô hiệu hóa vĩnh viễn (đổi email, khóa đăng nhập) thay vì xóa hẳn khỏi hệ thống.";
            }
            else
            {
                _db.NguoiDungs.Remove(user);
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xóa tài khoản.";
            }

            return RedirectToAction(nameof(DanhSach));
        }
    }
}

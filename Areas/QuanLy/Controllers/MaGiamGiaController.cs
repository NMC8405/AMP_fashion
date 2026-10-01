using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    [Authorize(Roles = "NhanVien,QuanTriVien")]
    public class MaGiamGiaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public MaGiamGiaController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> DanhSach(string? tuKhoa, string? trangThai)
        {
            var query = _db.MaGiamGias.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                query = query.Where(m => m.Ma.ToLower().Contains(tk) || (m.MoTa != null && m.MoTa.ToLower().Contains(tk)));
            }

            var now = DateTime.Now;

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (trangThai == "hoat_dong")
                {
                    query = query.Where(m => m.TrangThaiHoatDong && m.NgayBatDau <= now && m.NgayKetThuc >= now && (m.SoLuongToiDa == null || m.SoLuongDaDung < m.SoLuongToiDa));
                }
                else if (trangThai == "het_han")
                {
                    query = query.Where(m => !m.TrangThaiHoatDong || m.NgayKetThuc < now || (m.SoLuongToiDa != null && m.SoLuongDaDung >= m.SoLuongToiDa));
                }
            }

            // 4 KPIs theo thiết kế Figma
            ViewBag.TongMaGiamGia = await _db.MaGiamGias.CountAsync();
            ViewBag.DangHoatDong = await _db.MaGiamGias.CountAsync(m => m.TrangThaiHoatDong && m.NgayBatDau <= now && m.NgayKetThuc >= now && (m.SoLuongToiDa == null || m.SoLuongDaDung < m.SoLuongToiDa));
            ViewBag.DaHetHan = await _db.MaGiamGias.CountAsync(m => !m.TrangThaiHoatDong || m.NgayKetThuc < now || (m.SoLuongToiDa != null && m.SoLuongDaDung >= m.SoLuongToiDa));
            ViewBag.TongLuotSuDung = (await _db.MaGiamGias.SumAsync(m => (int?)m.SoLuongDaDung)) ?? 0;

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.TrangThai = trangThai;

            var danhSach = await query.OrderByDescending(m => m.Id).ToListAsync();
            return View(danhSach);
        }

        // ================= UC23: Thêm mã giảm giá =================
        [HttpGet]
        public IActionResult Them() => View(new MaGiamGia());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(MaGiamGia model)
        {
            model.Ma = model.Ma?.Trim().ToUpper() ?? "";

            if (ModelState.IsValid)
            {
                var trung = await _db.MaGiamGias.AnyAsync(m => m.Ma == model.Ma);
                if (trung) ModelState.AddModelError(nameof(model.Ma), "Mã giảm giá này đã tồn tại.");
            }

            if (model.NgayKetThuc < model.NgayBatDau)
                ModelState.AddModelError(nameof(model.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");

            if (!ModelState.IsValid) return View(model);

            _db.MaGiamGias.Add(model);
            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã thêm mã giảm giá \"{model.Ma}\".";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC25: Sửa mã giảm giá =================
        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var maGiamGia = await _db.MaGiamGias.FindAsync(id);
            if (maGiamGia == null) return NotFound();
            return View(maGiamGia);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(int id, MaGiamGia model)
        {
            var maGiamGia = await _db.MaGiamGias.FindAsync(id);
            if (maGiamGia == null) return NotFound();

            model.Ma = model.Ma?.Trim().ToUpper() ?? "";
            var trung = await _db.MaGiamGias.AnyAsync(m => m.Ma == model.Ma && m.Id != id);
            if (trung) ModelState.AddModelError(nameof(model.Ma), "Mã giảm giá này đã tồn tại.");
            if (model.NgayKetThuc < model.NgayBatDau)
                ModelState.AddModelError(nameof(model.NgayKetThuc), "Ngày kết thúc phải sau ngày bắt đầu.");

            if (!ModelState.IsValid)
            {
                model.Id = id;
                return View(model);
            }

            maGiamGia.Ma = model.Ma;
            maGiamGia.MoTa = model.MoTa;
            maGiamGia.LoaiGiamGia = model.LoaiGiamGia;
            maGiamGia.GiaTri = model.GiaTri;
            maGiamGia.GiaTriDonHangToiThieu = model.GiaTriDonHangToiThieu;
            maGiamGia.SoTienGiamToiDa = model.SoTienGiamToiDa;
            maGiamGia.SoLuongToiDa = model.SoLuongToiDa;
            maGiamGia.NgayBatDau = model.NgayBatDau;
            maGiamGia.NgayKetThuc = model.NgayKetThuc;
            maGiamGia.TrangThaiHoatDong = model.TrangThaiHoatDong;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã cập nhật mã giảm giá \"{maGiamGia.Ma}\".";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC24: Xóa mã giảm giá =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var maGiamGia = await _db.MaGiamGias.FindAsync(id);
            if (maGiamGia == null) return NotFound();

            var daSuDung = await _db.DonHangs.AnyAsync(d => d.MaGiamGiaId == id);
            if (daSuDung)
            {
                // Đã có đơn hàng dùng mã này -> chỉ vô hiệu hóa để giữ lịch sử đơn hàng
                maGiamGia.TrangThaiHoatDong = false;
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Mã giảm giá đã được sử dụng trong đơn hàng nên chỉ được VÔ HIỆU HÓA thay vì xóa vĩnh viễn.";
            }
            else
            {
                _db.MaGiamGias.Remove(maGiamGia);
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xóa mã giảm giá.";
            }
            return RedirectToAction(nameof(DanhSach));
        }
    }
}

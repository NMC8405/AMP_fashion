using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    [Authorize(Roles = "NhanVien,QuanTriVien")]
    public class DanhGiaController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DanhGiaController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> DanhSach(string? tuKhoa, int? soSao, string? trangThai)
        {
            var query = _db.DanhGias
                .Include(d => d.NguoiDung)
                .Include(d => d.SanPham)
                .Include(d => d.DonHang)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                query = query.Where(d =>
                    (d.NguoiDung != null && d.NguoiDung.HoTen.ToLower().Contains(tk)) ||
                    (d.SanPham != null && d.SanPham.TenSanPham.ToLower().Contains(tk)) ||
                    d.NoiDung.ToLower().Contains(tk));
            }

            if (soSao.HasValue && soSao.Value >= 1 && soSao.Value <= 5)
            {
                query = query.Where(d => d.SoSao == soSao.Value);
            }

            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                if (trangThai == "cho_phan_hoi")
                {
                    query = query.Where(d => string.IsNullOrEmpty(d.PhanHoi));
                }
                else if (trangThai == "da_phan_hoi")
                {
                    query = query.Where(d => !string.IsNullOrEmpty(d.PhanHoi));
                }
            }

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.SoSao = soSao;
            ViewBag.TrangThai = trangThai;

            // Thống kê nhanh
            ViewBag.TongDanhGia = await _db.DanhGias.CountAsync();
            ViewBag.ChoPhanHoi = await _db.DanhGias.CountAsync(d => string.IsNullOrEmpty(d.PhanHoi));
            ViewBag.DaPhanHoi = await _db.DanhGias.CountAsync(d => !string.IsNullOrEmpty(d.PhanHoi));

            var danhSach = await query.OrderByDescending(d => d.NgayDanhGia).ToListAsync();
            return View(danhSach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanHoi(int id, string noiDungPhanHoi)
        {
            var danhGia = await _db.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá." });
            }

            if (string.IsNullOrWhiteSpace(noiDungPhanHoi))
            {
                return Json(new { success = false, message = "Vui lòng nhập nội dung phản hồi." });
            }

            danhGia.PhanHoi = noiDungPhanHoi.Trim();
            danhGia.NgayPhanHoi = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đã gửi phản hồi đánh giá thành công.";
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var danhGia = await _db.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                TempData["Loi"] = "Không tìm thấy đánh giá để xóa.";
                return RedirectToAction(nameof(DanhSach));
            }

            _db.DanhGias.Remove(danhGia);
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đã xóa đánh giá thành công.";
            return RedirectToAction(nameof(DanhSach));
        }
    }
}

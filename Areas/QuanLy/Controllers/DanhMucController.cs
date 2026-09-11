using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    [Authorize(Roles = "NhanVien,QuanTriVien")]
    public class DanhMucController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DanhMucController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> DanhSach()
        {
            var danhMucs = await _db.DanhMucs.Include(d => d.SanPhams).OrderBy(d => d.TenDanhMuc).ToListAsync();
            return View(danhMucs);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(DanhMuc model)
        {
            if (!string.IsNullOrWhiteSpace(model.TenDanhMuc))
            {
                _db.DanhMucs.Add(new DanhMuc { TenDanhMuc = model.TenDanhMuc.Trim(), MoTa = model.MoTa?.Trim() });
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã thêm danh mục mới.";
            }
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(DanhMuc model)
        {
            var danhMuc = await _db.DanhMucs.FindAsync(model.Id);
            if (danhMuc != null && !string.IsNullOrWhiteSpace(model.TenDanhMuc))
            {
                danhMuc.TenDanhMuc = model.TenDanhMuc.Trim();
                danhMuc.MoTa = model.MoTa?.Trim();
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã cập nhật danh mục.";
            }
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var conSanPham = await _db.SanPhams.AnyAsync(s => s.DanhMucId == id);
            if (conSanPham)
            {
                TempData["Loi"] = "Không thể xóa danh mục vì vẫn còn sản phẩm thuộc danh mục này.";
                return RedirectToAction(nameof(DanhSach));
            }

            var danhMuc = await _db.DanhMucs.FindAsync(id);
            if (danhMuc != null)
            {
                _db.DanhMucs.Remove(danhMuc);
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xóa danh mục.";
            }
            return RedirectToAction(nameof(DanhSach));
        }
    }
}

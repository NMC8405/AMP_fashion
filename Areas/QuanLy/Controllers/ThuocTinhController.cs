using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Areas.QuanLy.Controllers
{
    /// <summary>Quản lý dữ liệu dùng chung: màu sắc &amp; kích thước cho biến thể sản phẩm.</summary>
    [Area("QuanLy")]
    [Authorize(Roles = "NhanVien,QuanTriVien")]
    public class ThuocTinhController : Controller
    {
        private readonly ApplicationDbContext _db;

        public ThuocTinhController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> DanhSach()
        {
            ViewBag.MauSacs = await _db.MauSacs.ToListAsync();
            ViewBag.KichThuocs = await _db.KichThuocs.OrderBy(k => k.ThuTu).ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemMau(string tenMau, string maHex)
        {
            if (!string.IsNullOrWhiteSpace(tenMau))
            {
                _db.MauSacs.Add(new MauSac { TenMau = tenMau.Trim(), MaHex = string.IsNullOrWhiteSpace(maHex) ? "#000000" : maHex });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaMau(int id)
        {
            var dangDuocDung = await _db.BienTheSanPhams.AnyAsync(b => b.MauSacId == id);
            if (dangDuocDung)
            {
                TempData["Loi"] = "Không thể xóa màu này vì đang được dùng cho sản phẩm.";
            }
            else
            {
                var mau = await _db.MauSacs.FindAsync(id);
                if (mau != null) { _db.MauSacs.Remove(mau); await _db.SaveChangesAsync(); }
            }
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemKichThuoc(string tenKichThuoc, int thuTu)
        {
            if (!string.IsNullOrWhiteSpace(tenKichThuoc))
            {
                _db.KichThuocs.Add(new KichThuoc { TenKichThuoc = tenKichThuoc.Trim(), ThuTu = thuTu });
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaKichThuoc(int id)
        {
            var dangDuocDung = await _db.BienTheSanPhams.AnyAsync(b => b.KichThuocId == id);
            if (dangDuocDung)
            {
                TempData["Loi"] = "Không thể xóa kích thước này vì đang được dùng cho sản phẩm.";
            }
            else
            {
                var size = await _db.KichThuocs.FindAsync(id);
                if (size != null) { _db.KichThuocs.Remove(size); await _db.SaveChangesAsync(); }
            }
            return RedirectToAction(nameof(DanhSach));
        }
    }
}

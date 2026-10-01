using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Controllers
{
    [Authorize]
    public class YeuThichController : Controller
    {
        private readonly ApplicationDbContext _db;

        public YeuThichController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            var sanPhamYeuThich = await _db.YeuThichs
                .Include(y => y.SanPham).ThenInclude(s => s!.BienThes)
                .Include(y => y.SanPham).ThenInclude(s => s!.DanhGias)
                .Where(y => y.NguoiDungId == userId)
                .OrderByDescending(y => y.NgayThem)
                .Select(y => y.SanPham!)
                .ToListAsync();

            var idsYeuThich = sanPhamYeuThich.Select(s => s.Id).ToList();
            ViewBag.GoiY = await _db.SanPhams
                .Where(s => s.TrangThaiHienThi && !idsYeuThich.Contains(s.Id))
                .OrderByDescending(s => s.NgayTao)
                .Take(4)
                .ToListAsync();

            return View(sanPhamYeuThich);
        }

        // ================= UC32: Thêm sản phẩm yêu thích =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(int sanPhamId, string? veTrang, string? returnUrl)
        {
            var userId = User.GetUserId();
            var daCo = await _db.YeuThichs.AnyAsync(y => y.NguoiDungId == userId && y.SanPhamId == sanPhamId);
            if (!daCo)
            {
                _db.YeuThichs.Add(new YeuThich { NguoiDungId = userId, SanPhamId = sanPhamId, NgayThem = DateTime.Now });
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã thêm vào danh sách yêu thích.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                var count = await _db.YeuThichs.CountAsync(y => y.NguoiDungId == userId);
                return Json(new { success = true, daYeuThich = true, count, message = "Đã thêm vào danh sách yêu thích." });
            }

            return DieuHuongVeTrang(veTrang, returnUrl, sanPhamId);
        }

        // ================= UC33: Xóa sản phẩm yêu thích =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int sanPhamId, string? veTrang, string? returnUrl)
        {
            var userId = User.GetUserId();
            var dong = await _db.YeuThichs.FirstOrDefaultAsync(y => y.NguoiDungId == userId && y.SanPhamId == sanPhamId);
            if (dong != null)
            {
                _db.YeuThichs.Remove(dong);
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã bỏ khỏi danh sách yêu thích.";
            }

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers["Accept"].ToString().Contains("application/json"))
            {
                var count = await _db.YeuThichs.CountAsync(y => y.NguoiDungId == userId);
                return Json(new { success = true, daYeuThich = false, count, message = "Đã bỏ khỏi danh sách yêu thích." });
            }

            return DieuHuongVeTrang(veTrang, returnUrl, sanPhamId);
        }

        private IActionResult DieuHuongVeTrang(string? veTrang, string? returnUrl, int sanPhamId)
        {
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && (referer.StartsWith("/") || referer.Contains(Request.Host.Value)))
                return Redirect(referer);

            if (veTrang == "chi-tiet") return RedirectToAction("ChiTiet", "SanPham", new { id = sanPhamId });
            if (veTrang == "danh-sach") return RedirectToAction("DanhSach", "SanPham");
            return RedirectToAction(nameof(Index));
        }
    }
}

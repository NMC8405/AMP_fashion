using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var sanPhamCoTheHienThi = _db.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienThes)
                .Where(s => s.TrangThaiHienThi);

            ViewBag.DanhMucNoiBat = await _db.DanhMucs.Take(3).ToListAsync();

            ViewBag.SanPhamNoiBat = await sanPhamCoTheHienThi
                .OrderByDescending(s => s.DanhGias.Count)
                .Take(8)
                .ToListAsync();

            ViewBag.SanPhamMoi = await sanPhamCoTheHienThi
                .OrderByDescending(s => s.NgayTao)
                .Take(4)
                .ToListAsync();

            ViewBag.SanPhamGiamGia = await sanPhamCoTheHienThi
                .Where(s => s.GiaKhuyenMai != null && s.GiaKhuyenMai < s.GiaGoc)
                .OrderByDescending(s => s.NgayTao)
                .Take(4)
                .ToListAsync();

            return View();
        }

        public async Task<IActionResult> KhuyenMai()
        {
            var vouchers = await _db.MaGiamGias
                .Where(m => m.TrangThaiHoatDong && m.NgayKetThuc >= DateTime.Today)
                .OrderByDescending(m => m.GiaTri)
                .ToListAsync();

            var sanPhamGiamGia = await _db.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienThes)
                .Where(s => s.TrangThaiHienThi && s.GiaKhuyenMai != null && s.GiaKhuyenMai < s.GiaGoc)
                .OrderByDescending(s => s.NgayTao)
                .ToListAsync();

            ViewBag.Vouchers = vouchers;
            return View(sanPhamGiamGia);
        }

        public IActionResult Loi()
        {
            return View("Error");
        }
    }
}

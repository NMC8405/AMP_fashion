using System.Text;
using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    [Authorize(Roles = "NhanVien,QuanTriVien")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _db;

        public DashboardController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ================= UC15: Xem báo cáo thống kê =================
        [HttpGet]
        public async Task<IActionResult> Index(DateTime? tuNgay, DateTime? denNgay)
        {
            var model = await XayDungThongKeAsync(tuNgay, denNgay);
            return View(model);
        }

        // ================= UC16: Xuất file thống kê =================
        [HttpGet]
        public async Task<IActionResult> XuatFile(DateTime? tuNgay, DateTime? denNgay)
        {
            var model = await XayDungThongKeAsync(tuNgay, denNgay);

            var sb = new StringBuilder();
            sb.AppendLine("\uFEFFBAO CAO THONG KE - AMP FASHION STORE");
            sb.AppendLine($"Tu ngay,{model.TuNgay:dd/MM/yyyy},Den ngay,{model.DenNgay:dd/MM/yyyy}");
            sb.AppendLine();
            sb.AppendLine("TONG QUAN");
            sb.AppendLine($"Tong don hang moi,{model.TongDonHangMoi}");
            sb.AppendLine($"Tong doanh thu,{model.TongDoanhThu}");
            sb.AppendLine($"Khach hang moi,{model.TongKhachHangMoi}");
            sb.AppendLine($"San pham da ban,{model.TongSanPhamDaBan}");
            sb.AppendLine();
            sb.AppendLine("DOANH THU THEO NGAY");
            sb.AppendLine("Ngay,So don hang,Doanh thu");
            foreach (var d in model.DoanhThuTheoNgay)
                sb.AppendLine($"{d.Ngay:dd/MM/yyyy},{d.SoDonHang},{d.DoanhThu}");
            sb.AppendLine();
            sb.AppendLine("TOP SAN PHAM BAN CHAY");
            sb.AppendLine("Ten san pham,So luong da ban,Doanh thu");
            foreach (var p in model.TopSanPhamBanChay)
                sb.AppendLine($"\"{p.TenSanPham}\",{p.SoLuongDaBan},{p.DoanhThu}");

            var bytes = Encoding.UTF8.GetBytes(sb.ToString());
            var tenFile = $"ThongKe_AMP_{DateTime.Now:yyyyMMddHHmmss}.csv";
            return File(bytes, "text/csv", tenFile);
        }

        private async Task<ThongKeViewModel> XayDungThongKeAsync(DateTime? tuNgay, DateTime? denNgay)
        {
            var tu = (tuNgay ?? DateTime.Today.AddDays(-6)).Date;
            var den = (denNgay ?? DateTime.Today).Date;
            var denCoGio = den.AddDays(1).AddTicks(-1);

            var donHangTrongKy = await _db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.NgayDat >= tu && d.NgayDat <= denCoGio)
                .ToListAsync();

            var donHopLe = donHangTrongKy.Where(d => d.TrangThaiDonHang != TrangThaiDonHang.DaHuy).ToList();

            var model = new ThongKeViewModel
            {
                TuNgay = tu,
                DenNgay = den,
                TongDonHangMoi = donHangTrongKy.Count,
                TongDoanhThu = donHopLe.Sum(d => d.TongTien),
                TongSanPhamDaBan = donHopLe.SelectMany(d => d.ChiTietDonHangs).Sum(c => c.SoLuong),
                TongKhachHangMoi = await _db.NguoiDungs.CountAsync(n => n.VaiTro == VaiTro.KhachHang && n.NgayTao >= tu && n.NgayTao <= denCoGio)
            };

            for (var ngay = tu; ngay <= den; ngay = ngay.AddDays(1))
            {
                var donTrongNgay = donHopLe.Where(d => d.NgayDat.Date == ngay.Date).ToList();
                model.DoanhThuTheoNgay.Add(new DiemDoanhThuTheoNgay
                {
                    Ngay = ngay,
                    DoanhThu = donTrongNgay.Sum(d => d.TongTien),
                    SoDonHang = donTrongNgay.Count
                });
            }

            model.TopSanPhamBanChay = donHopLe
                .SelectMany(d => d.ChiTietDonHangs)
                .Where(c => c.SanPhamId.HasValue)
                .GroupBy(c => new { c.SanPhamId, c.TenSanPham, c.HinhAnh })
                .Select(g => new SanPhamBanChay
                {
                    SanPhamId = g.Key.SanPhamId!.Value,
                    TenSanPham = g.Key.TenSanPham,
                    HinhAnh = g.Key.HinhAnh,
                    SoLuongDaBan = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.ThanhTien)
                })
                .OrderByDescending(x => x.SoLuongDaBan)
                .Take(5)
                .ToList();

            model.DonHangTheoTrangThai = donHangTrongKy
                .GroupBy(d => d.TenTrangThai)
                .ToDictionary(g => g.Key, g => g.Count());

            return model;
        }
    }
}

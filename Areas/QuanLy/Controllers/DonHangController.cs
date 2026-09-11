using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Areas.QuanLy.Controllers
{
    [Area("QuanLy")]
    [Authorize(Roles = "NhanVien,QuanTriVien")]
    public class DonHangController : Controller
    {
        private const int SoDongMoiTrang = 15;
        private readonly ApplicationDbContext _db;

        public DonHangController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> DanhSach(string? trangThai, string? tuKhoa, int trang = 1)
        {
            var query = _db.DonHangs.Include(d => d.NguoiDung).Include(d => d.ChiTietDonHangs).AsQueryable();

            if (!string.IsNullOrEmpty(trangThai) && Enum.TryParse<TrangThaiDonHang>(trangThai, out var tt))
                query = query.Where(d => d.TrangThaiDonHang == tt);

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                query = query.Where(d => d.MaDonHang.ToLower().Contains(tk)
                    || d.TenNguoiNhan.ToLower().Contains(tk)
                    || d.SoDienThoaiNhan.Contains(tk));
            }

            var tongSo = await query.CountAsync();
            var tongTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)SoDongMoiTrang));
            trang = Math.Clamp(trang, 1, tongTrang);

            var donHangs = await query.OrderByDescending(d => d.NgayDat)
                .Skip((trang - 1) * SoDongMoiTrang).Take(SoDongMoiTrang).ToListAsync();

            ViewBag.TrangThaiHienTai = trangThai ?? "";
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongSo = tongSo;

            // Đếm nhanh số lượng theo từng trạng thái để hiển thị badge trên tab lọc
            ViewBag.DemTheoTrangThai = await _db.DonHangs
                .GroupBy(d => d.TrangThaiDonHang)
                .Select(g => new { TrangThai = g.Key, SoLuong = g.Count() })
                .ToDictionaryAsync(x => x.TrangThai, x => x.SoLuong);

            return View(donHangs);
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var don = await _db.DonHangs
                .Include(d => d.NguoiDung)
                .Include(d => d.ChiTietDonHangs)
                .Include(d => d.MaGiamGiaApDung)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (don == null) return NotFound();
            return View(don);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanThanhToan(int id)
        {
            var don = await _db.DonHangs.FindAsync(id);
            if (don == null) return NotFound();

            if (don.TrangThaiDonHang == TrangThaiDonHang.ChoThanhToan)
            {
                don.TrangThaiThanhToan = TrangThaiThanhToan.DaThanhToan;
                don.TrangThaiDonHang = TrangThaiDonHang.ChoXacNhan;
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xác nhận thanh toán cho đơn hàng.";
            }
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDon(int id)
        {
            var don = await _db.DonHangs.FindAsync(id);
            if (don == null) return NotFound();

            if (don.TrangThaiDonHang == TrangThaiDonHang.ChoXacNhan)
            {
                don.TrangThaiDonHang = TrangThaiDonHang.DaXacNhan;
                don.NgayXacNhan = DateTime.Now;
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã xác nhận đơn hàng, chuẩn bị giao cho đơn vị vận chuyển.";
            }
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GiaoVanChuyen(int id)
        {
            var don = await _db.DonHangs.FindAsync(id);
            if (don == null) return NotFound();

            if (don.TrangThaiDonHang == TrangThaiDonHang.DaXacNhan)
            {
                don.TrangThaiDonHang = TrangThaiDonHang.DangGiao;
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = "Đã bàn giao đơn hàng cho đơn vị vận chuyển.";
            }
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(int id, string lyDoHuy)
        {
            var don = await _db.DonHangs.Include(d => d.ChiTietDonHangs).FirstOrDefaultAsync(d => d.Id == id);
            if (don == null) return NotFound();

            if (don.TrangThaiDonHang is TrangThaiDonHang.DangGiao or TrangThaiDonHang.DaGiao or TrangThaiDonHang.DaHuy)
            {
                TempData["Loi"] = "Không thể hủy đơn hàng ở trạng thái hiện tại.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            don.TrangThaiDonHang = TrangThaiDonHang.DaHuy;
            don.LyDoHuy = string.IsNullOrWhiteSpace(lyDoHuy) ? "Hủy bởi cửa hàng" : lyDoHuy;
            don.NgayHuy = DateTime.Now;

            foreach (var ct in don.ChiTietDonHangs)
            {
                if (ct.BienTheSanPhamId.HasValue)
                {
                    var bienThe = await _db.BienTheSanPhams.FindAsync(ct.BienTheSanPhamId.Value);
                    if (bienThe != null) bienThe.SoLuongTon += ct.SoLuong;
                }
            }

            if (don.TrangThaiThanhToan == TrangThaiThanhToan.DaThanhToan)
                don.TrangThaiThanhToan = TrangThaiThanhToan.DaHoanTien;

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Đã hủy đơn hàng và hoàn trả tồn kho.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }
    }
}

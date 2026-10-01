using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.Services;
using AMPFashionStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Controllers
{
    [Authorize]
    public class GioHangController : Controller
    {
        private readonly ApplicationDbContext _db;

        public GioHangController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ================= UC08: Xem giỏ hàng =================
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.GetUserId();
            var items = await _db.GioHangItems
                .Include(g => g.BienTheSanPham).ThenInclude(b => b!.SanPham)
                .Include(g => g.BienTheSanPham).ThenInclude(b => b!.MauSac)
                .Include(g => g.BienTheSanPham).ThenInclude(b => b!.KichThuoc)
                .Where(g => g.NguoiDungId == userId)
                .OrderByDescending(g => g.NgayThem)
                .ToListAsync();

            var sanPhamIds = items.Select(i => i.BienTheSanPham!.SanPhamId).Distinct().ToList();
            ViewBag.SanPhamLienQuan = await _db.SanPhams
                .Where(s => sanPhamIds.Contains(s.Id) == false && s.TrangThaiHienThi)
                .OrderByDescending(s => s.NgayTao)
                .Take(4)
                .ToListAsync();

            return View(new GioHangViewModel { DongSanPhams = items });
        }

        // ================= UC07: Thêm vào giỏ hàng / Mua ngay =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(ThemVaoGioRequest request)
        {
            var bienThe = await _db.BienTheSanPhams
                .Include(b => b.SanPham)
                .FirstOrDefaultAsync(b => b.SanPhamId == request.SanPhamId
                    && b.MauSacId == request.MauSacId
                    && b.KichThuocId == request.KichThuocId);

            if (bienThe == null)
            {
                TempData["Loi"] = "Không tìm thấy màu sắc/kích thước bạn chọn.";
                return RedirectToAction("ChiTiet", "SanPham", new { id = request.SanPhamId });
            }

            var soLuong = Math.Max(1, request.SoLuong);

            if (bienThe.SoLuongTon <= 0)
            {
                TempData["Loi"] = "Rất tiếc, sản phẩm với màu sắc/kích thước này đã hết hàng.";
                return RedirectToAction("ChiTiet", "SanPham", new { id = request.SanPhamId });
            }

            if (soLuong > bienThe.SoLuongTon)
            {
                TempData["Loi"] = $"Chỉ còn {bienThe.SoLuongTon} sản phẩm trong kho.";
                return RedirectToAction("ChiTiet", "SanPham", new { id = request.SanPhamId });
            }

            if (request.MuaNgay)
            {
                HttpContext.Session.SetObject("CheckoutItems", new List<DongCheckout>
                {
                    new() { BienTheSanPhamId = bienThe.Id, SoLuong = soLuong }
                });
                return RedirectToAction("ThanhToan", "DonHang");
            }

            var userId = User.GetUserId();
            var dongCoSan = await _db.GioHangItems
                .FirstOrDefaultAsync(g => g.NguoiDungId == userId && g.BienTheSanPhamId == bienThe.Id);

            if (dongCoSan != null)
            {
                dongCoSan.SoLuong = Math.Min(dongCoSan.SoLuong + soLuong, bienThe.SoLuongTon);
            }
            else
            {
                _db.GioHangItems.Add(new GioHangItem
                {
                    NguoiDungId = userId,
                    BienTheSanPhamId = bienThe.Id,
                    SoLuong = soLuong
                });
            }

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã thêm \"{bienThe.SanPham!.TenSanPham}\" vào giỏ hàng.";
            return RedirectToAction("ChiTiet", "SanPham", new { id = request.SanPhamId });
        }

        // ================= UC08: Cập nhật số lượng =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat(CapNhatGioRequest request)
        {
            var userId = User.GetUserId();
            var dong = await _db.GioHangItems
                .Include(g => g.BienTheSanPham)
                .FirstOrDefaultAsync(g => g.Id == request.GioHangItemId && g.NguoiDungId == userId);

            if (dong != null)
            {
                var toiDa = dong.BienTheSanPham?.SoLuongTon ?? 1;
                dong.SoLuong = Math.Clamp(request.SoLuong, 1, Math.Max(1, toiDa));
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ================= UC09: Xóa sản phẩm khỏi giỏ hàng =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int gioHangItemId)
        {
            var userId = User.GetUserId();
            var dong = await _db.GioHangItems.FirstOrDefaultAsync(g => g.Id == gioHangItemId && g.NguoiDungId == userId);
            if (dong != null)
            {
                _db.GioHangItems.Remove(dong);
                await _db.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaTatCa()
        {
            var userId = User.GetUserId();
            var items = _db.GioHangItems.Where(g => g.NguoiDungId == userId);
            _db.GioHangItems.RemoveRange(items);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ================= Xóa các sản phẩm đã chọn khỏi giỏ hàng =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XoaCacSanPham(List<int> selectedItemIds)
        {
            var userId = User.GetUserId();
            if (selectedItemIds != null && selectedItemIds.Count > 0)
            {
                var items = await _db.GioHangItems
                    .Where(g => g.NguoiDungId == userId && selectedItemIds.Contains(g.Id))
                    .ToListAsync();
                if (items.Count > 0)
                {
                    _db.GioHangItems.RemoveRange(items);
                    await _db.SaveChangesAsync();
                    TempData["ThongBao"] = $"Đã xóa {items.Count} sản phẩm được chọn khỏi giỏ hàng.";
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // ================= Kiểm tra mã giảm giá trực tiếp trên giỏ hàng =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KiemTraVoucher(string ma, decimal tamTinh)
        {
            if (string.IsNullOrWhiteSpace(ma))
                return Json(new { hopLe = false, thongBao = "Vui lòng nhập mã giảm giá." });

            var maSach = ma.Trim().ToUpper();
            var maGiamGia = await _db.MaGiamGias.FirstOrDefaultAsync(m => m.Ma.ToUpper() == maSach);

            if (maGiamGia == null)
                return Json(new { hopLe = false, thongBao = "Mã giảm giá không tồn tại." });

            if (!maGiamGia.ConHieuLuc)
                return Json(new { hopLe = false, thongBao = "Mã giảm giá đã hết hạn hoặc đã sử dụng hết lượt." });

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.GetUserId();
                var daDung = await _db.DonHangs.AnyAsync(d => d.NguoiDungId == userId && d.MaGiamGiaId == maGiamGia.Id && d.TrangThaiDonHang != TrangThaiDonHang.DaHuy);
                if (daDung)
                    return Json(new { hopLe = false, thongBao = "Bạn đã sử dụng mã giảm giá này cho một đơn hàng trước đó. Mỗi tài khoản chỉ được sử dụng một lần." });
            }

            if (tamTinh < maGiamGia.GiaTriDonHangToiThieu)
                return Json(new { hopLe = false, thongBao = $"Đơn hàng cần tối thiểu {maGiamGia.GiaTriDonHangToiThieu:N0}₫ để áp dụng mã này." });

            var soTienGiam = maGiamGia.TinhSoTienGiam(tamTinh);
            var moTaGiam = maGiamGia.LoaiGiamGia == LoaiGiamGia.PhanTram
                ? $"Đã giảm -{maGiamGia.GiaTri:N0}%" + (maGiamGia.SoTienGiamToiDa.HasValue ? $" (tối đa {maGiamGia.SoTienGiamToiDa.Value:N0}₫)" : "")
                : $"Đã giảm -{maGiamGia.GiaTri:N0}₫";

            return Json(new
            {
                hopLe = true,
                thongBao = "Áp dụng mã giảm giá thành công!",
                soTienGiam,
                moTa = moTaGiam,
                ma = maGiamGia.Ma
            });
        }
    }
}

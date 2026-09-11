using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.Services;
using AMPFashionStore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AMPFashionStore.Controllers
{
    [Authorize]
    public class DonHangController : Controller
    {
        private const decimal NguongMienPhiVanChuyen = 500000;
        private const decimal PhiVanChuyenMacDinh = 30000;

        private readonly ApplicationDbContext _db;
        private readonly BankSettings _bankSettings;

        public DonHangController(ApplicationDbContext db, IOptions<BankSettings> bankSettings)
        {
            _db = db;
            _bankSettings = bankSettings.Value;
        }

        // ================= Bắt đầu thanh toán từ giỏ hàng =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BatDauThanhToan()
        {
            var userId = User.GetUserId();
            var items = await _db.GioHangItems
                .Where(g => g.NguoiDungId == userId)
                .Select(g => new DongCheckout { BienTheSanPhamId = g.BienTheSanPhamId, SoLuong = g.SoLuong })
                .ToListAsync();

            if (items.Count == 0)
            {
                TempData["Loi"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "GioHang");
            }

            HttpContext.Session.SetObject("CheckoutItems", items);
            return RedirectToAction(nameof(ThanhToan));
        }

        // ================= UC09/UC010: Thanh toán =================
        [HttpGet]
        public async Task<IActionResult> ThanhToan()
        {
            var (donHang, tamTinh, loi) = await XayDungXemTruocDonHangAsync();
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction("Index", "GioHang");
            }

            var user = await _db.NguoiDungs.FindAsync(User.GetUserId());
            var model = new ThanhToanViewModel
            {
                TenNguoiNhan = user?.HoTen ?? "",
                SoDienThoaiNhan = user?.SoDienThoai ?? "",
                DiaChiNhan = user?.DiaChi ?? "",
                DongHang = donHang,
                TamTinh = tamTinh,
                PhiVanChuyen = tamTinh >= NguongMienPhiVanChuyen ? 0 : PhiVanChuyenMacDinh
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(ThanhToanViewModel model, string hanhDong = "DatHang")
        {
            var (donHang, tamTinh, loi) = await XayDungXemTruocDonHangAsync();
            if (loi != null)
            {
                TempData["Loi"] = loi;
                return RedirectToAction("Index", "GioHang");
            }

            model.DongHang = donHang;
            model.TamTinh = tamTinh;
            model.PhiVanChuyen = tamTinh >= NguongMienPhiVanChuyen ? 0 : PhiVanChuyenMacDinh;

            var (maHopLe, thongBaoMa, soTienGiam, maGiamGia) = await KiemTraMaGiamGiaAsync(model.MaGiamGia, tamTinh);
            model.MaHopLe = maHopLe;
            model.ThongBaoMa = thongBaoMa;
            model.SoTienGiamGia = soTienGiam;

            if (hanhDong == "ApDungMa")
            {
                ModelState.Clear();
                return View(model);
            }

            if (!ModelState.IsValid) return View(model);

            var userId = User.GetUserId();
            var maDonHang = "DH" + DateTime.Now.ToString("yyyyMMddHHmmss") + Random.Shared.Next(10, 99);

            var donHangMoi = new DonHang
            {
                MaDonHang = maDonHang,
                NguoiDungId = userId,
                NgayDat = DateTime.Now,
                TenNguoiNhan = model.TenNguoiNhan.Trim(),
                SoDienThoaiNhan = model.SoDienThoaiNhan.Trim(),
                DiaChiNhan = model.DiaChiNhan.Trim(),
                GhiChu = model.GhiChu?.Trim(),
                PhuongThucThanhToan = model.PhuongThucThanhToan,
                TrangThaiThanhToan = TrangThaiThanhToan.ChuaThanhToan,
                TrangThaiDonHang = model.PhuongThucThanhToan == PhuongThucThanhToan.ChuyenKhoanNganHang
                    ? TrangThaiDonHang.ChoThanhToan
                    : TrangThaiDonHang.ChoXacNhan,
                TamTinh = tamTinh,
                PhiVanChuyen = model.PhiVanChuyen,
                SoTienGiamGia = soTienGiam,
                MaGiamGiaId = maGiamGia?.Id,
                ChiTietDonHangs = donHang
            };

            // Trừ kho + xóa khỏi giỏ hàng cho từng dòng đã đặt
            var checkoutItems = HttpContext.Session.GetObject<List<DongCheckout>>("CheckoutItems") ?? new();
            foreach (var item in checkoutItems)
            {
                var bienThe = await _db.BienTheSanPhams.FindAsync(item.BienTheSanPhamId);
                if (bienThe != null)
                    bienThe.SoLuongTon = Math.Max(0, bienThe.SoLuongTon - item.SoLuong);

                var dongGio = await _db.GioHangItems
                    .FirstOrDefaultAsync(g => g.NguoiDungId == userId && g.BienTheSanPhamId == item.BienTheSanPhamId);
                if (dongGio != null) _db.GioHangItems.Remove(dongGio);
            }

            if (maGiamGia != null) maGiamGia.SoLuongDaDung++;

            _db.DonHangs.Add(donHangMoi);
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("CheckoutItems");

            return RedirectToAction(nameof(DatHangThanhCong), new { id = donHangMoi.Id });
        }

        [HttpGet]
        public async Task<IActionResult> DatHangThanhCong(int id)
        {
            var don = await LayDonHangCuaToiAsync(id);
            if (don == null) return NotFound();

            if (don.PhuongThucThanhToan == PhuongThucThanhToan.ChuyenKhoanNganHang
                && don.TrangThaiThanhToan == TrangThaiThanhToan.ChuaThanhToan)
            {
                var noiDung = $"{don.MaDonHang}";
                ViewBag.QrUrl = _bankSettings.TaoLinkQr(don.TongTien, noiDung);
                ViewBag.BankSettings = _bankSettings;
            }

            return View(don);
        }

        // ================= UC010: Áp mã giảm giá =================
        private async Task<(bool hopLe, string thongBao, decimal soTienGiam, MaGiamGia? ma)> KiemTraMaGiamGiaAsync(string? ma, decimal tamTinh)
        {
            if (string.IsNullOrWhiteSpace(ma)) return (false, "", 0, null);

            var maSach = ma.Trim().ToUpper();
            var maGiamGia = await _db.MaGiamGias.FirstOrDefaultAsync(m => m.Ma.ToUpper() == maSach);

            if (maGiamGia == null)
                return (false, "Mã giảm giá không tồn tại.", 0, null);

            if (!maGiamGia.ConHieuLuc)
                return (false, "Mã giảm giá đã hết hạn hoặc đã sử dụng hết lượt.", 0, null);

            if (tamTinh < maGiamGia.GiaTriDonHangToiThieu)
                return (false, $"Đơn hàng cần tối thiểu {maGiamGia.GiaTriDonHangToiThieu:N0}đ để áp dụng mã này.", 0, null);

            var soTienGiam = maGiamGia.TinhSoTienGiam(tamTinh);
            return (true, "Áp dụng mã giảm giá thành công!", soTienGiam, maGiamGia);
        }

        private async Task<(List<ChiTietDonHang> dongHang, decimal tamTinh, string? loi)> XayDungXemTruocDonHangAsync()
        {
            var checkoutItems = HttpContext.Session.GetObject<List<DongCheckout>>("CheckoutItems");
            if (checkoutItems == null || checkoutItems.Count == 0)
                return (new(), 0, "Không có sản phẩm nào để thanh toán.");

            var dongHang = new List<ChiTietDonHang>();
            foreach (var item in checkoutItems)
            {
                var bienThe = await _db.BienTheSanPhams
                    .Include(b => b.SanPham)
                    .Include(b => b.MauSac)
                    .Include(b => b.KichThuoc)
                    .FirstOrDefaultAsync(b => b.Id == item.BienTheSanPhamId);

                if (bienThe?.SanPham == null) continue;

                var soLuong = Math.Min(item.SoLuong, Math.Max(0, bienThe.SoLuongTon));
                if (soLuong <= 0) continue;

                var donGia = bienThe.SanPham.GiaHienThi;
                dongHang.Add(new ChiTietDonHang
                {
                    BienTheSanPhamId = bienThe.Id,
                    SanPhamId = bienThe.SanPhamId,
                    TenSanPham = bienThe.SanPham.TenSanPham,
                    TenMau = bienThe.MauSac?.TenMau ?? "",
                    TenKichThuoc = bienThe.KichThuoc?.TenKichThuoc ?? "",
                    HinhAnh = bienThe.SanPham.HinhAnhChinh,
                    SoLuong = soLuong,
                    DonGia = donGia,
                    ThanhTien = donGia * soLuong
                });
            }

            if (dongHang.Count == 0) return (dongHang, 0, "Sản phẩm trong đơn đã hết hàng.");

            return (dongHang, dongHang.Sum(d => d.ThanhTien), null);
        }

        // ================= Đơn hàng của tôi =================
        [HttpGet]
        public async Task<IActionResult> DanhSach(string? tab)
        {
            var userId = User.GetUserId();
            var query = _db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Where(d => d.NguoiDungId == userId)
                .OrderByDescending(d => d.NgayDat)
                .AsQueryable();

            if (!string.IsNullOrEmpty(tab) && Enum.TryParse<TrangThaiDonHang>(tab, out var trangThai))
                query = query.Where(d => d.TrangThaiDonHang == trangThai);

            ViewBag.TabHienTai = tab ?? "";
            return View(await query.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var don = await LayDonHangCuaToiAsync(id);
            if (don == null) return NotFound();

            var userId = User.GetUserId();
            ViewBag.SanPhamDaDanhGia = await _db.DanhGias
                .Where(dg => dg.DonHangId == id)
                .Select(dg => dg.SanPhamId)
                .ToListAsync();

            return View(don);
        }

        // ================= UC011: Cập nhật thông tin đơn hàng =================
        [HttpGet]
        public async Task<IActionResult> CapNhat(int id)
        {
            var don = await LayDonHangCuaToiAsync(id);
            if (don == null) return NotFound();
            if (!don.ChoPhepSuaThongTin)
            {
                TempData["Loi"] = "Đơn hàng đã được bàn giao vận chuyển nên không thể chỉnh sửa.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            return View(new CapNhatDonHangViewModel
            {
                DonHangId = don.Id,
                TenNguoiNhan = don.TenNguoiNhan,
                SoDienThoaiNhan = don.SoDienThoaiNhan,
                DiaChiNhan = don.DiaChiNhan,
                GhiChu = don.GhiChu
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CapNhat(CapNhatDonHangViewModel model)
        {
            var don = await LayDonHangCuaToiAsync(model.DonHangId);
            if (don == null) return NotFound();
            if (!don.ChoPhepSuaThongTin)
            {
                TempData["Loi"] = "Đơn hàng đã được bàn giao vận chuyển nên không thể chỉnh sửa.";
                return RedirectToAction(nameof(ChiTiet), new { id = model.DonHangId });
            }

            if (!ModelState.IsValid) return View(model);

            don.TenNguoiNhan = model.TenNguoiNhan.Trim();
            don.SoDienThoaiNhan = model.SoDienThoaiNhan.Trim();
            don.DiaChiNhan = model.DiaChiNhan.Trim();
            don.GhiChu = model.GhiChu?.Trim();
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Cập nhật thông tin đơn hàng thành công.";
            return RedirectToAction(nameof(ChiTiet), new { id = model.DonHangId });
        }

        // ================= UC012: Hủy đơn hàng =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(HuyDonHangRequest request)
        {
            var don = await LayDonHangCuaToiAsync(request.DonHangId);
            if (don == null) return NotFound();

            if (!don.ChoPhepHuy)
            {
                TempData["Loi"] = "Đơn hàng ở trạng thái hiện tại không thể hủy.";
                return RedirectToAction(nameof(ChiTiet), new { id = request.DonHangId });
            }

            var trongVong24h = (DateTime.Now - don.NgayDat).TotalHours <= 24;
            var daThanhToanChuyenKhoan = don.PhuongThucThanhToan == PhuongThucThanhToan.ChuyenKhoanNganHang
                && don.TrangThaiThanhToan == TrangThaiThanhToan.DaThanhToan;

            don.TrangThaiDonHang = TrangThaiDonHang.DaHuy;
            don.LyDoHuy = request.LyDoHuy;
            don.NgayHuy = DateTime.Now;

            // Hoàn trả tồn kho
            foreach (var ct in don.ChiTietDonHangs)
            {
                if (ct.BienTheSanPhamId.HasValue)
                {
                    var bienThe = await _db.BienTheSanPhams.FindAsync(ct.BienTheSanPhamId.Value);
                    if (bienThe != null) bienThe.SoLuongTon += ct.SoLuong;
                }
            }

            var thongBaoHoanTien = "";
            if (daThanhToanChuyenKhoan && trongVong24h)
            {
                don.TrangThaiThanhToan = TrangThaiThanhToan.DaHoanTien;
                thongBaoHoanTien = " Đơn hàng đã thanh toán trong vòng 24 giờ, số tiền sẽ được hoàn lại cho bạn theo chính sách hoàn tiền.";
            }

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Đơn hàng đã được hủy thành công." + thongBaoHoanTien;
            return RedirectToAction(nameof(ChiTiet), new { id = request.DonHangId });
        }

        // ================= Khách hàng xác nhận đã nhận hàng =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanDaNhan(int id)
        {
            var don = await LayDonHangCuaToiAsync(id);
            if (don == null) return NotFound();

            if (don.TrangThaiDonHang != TrangThaiDonHang.DangGiao)
            {
                TempData["Loi"] = "Đơn hàng chưa ở trạng thái đang giao.";
                return RedirectToAction(nameof(ChiTiet), new { id });
            }

            don.TrangThaiDonHang = TrangThaiDonHang.DaGiao;
            don.NgayGiao = DateTime.Now;
            if (don.PhuongThucThanhToan == PhuongThucThanhToan.ThanhToanKhiNhanHang)
                don.TrangThaiThanhToan = TrangThaiThanhToan.DaThanhToan;

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = "Cảm ơn bạn đã xác nhận nhận hàng! Đừng quên để lại đánh giá cho sản phẩm nhé.";
            return RedirectToAction(nameof(ChiTiet), new { id });
        }

        // ================= UC28: Đánh giá sản phẩm =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiDanhGia(DanhGiaRequest request)
        {
            var don = await LayDonHangCuaToiAsync(request.DonHangId);
            if (don == null) return NotFound();

            if (don.TrangThaiDonHang != TrangThaiDonHang.DaGiao)
            {
                TempData["Loi"] = "Chỉ có thể đánh giá sau khi đơn hàng đã được giao.";
                return RedirectToAction(nameof(ChiTiet), new { id = request.DonHangId });
            }

            var daDanhGia = await _db.DanhGias.AnyAsync(d => d.DonHangId == request.DonHangId && d.SanPhamId == request.SanPhamId);
            if (daDanhGia)
            {
                TempData["Loi"] = "Bạn đã đánh giá sản phẩm này cho đơn hàng này rồi.";
                return RedirectToAction(nameof(ChiTiet), new { id = request.DonHangId });
            }

            _db.DanhGias.Add(new DanhGia
            {
                SanPhamId = request.SanPhamId,
                NguoiDungId = User.GetUserId(),
                DonHangId = request.DonHangId,
                SoSao = request.SoSao,
                NoiDung = request.NoiDung.Trim(),
                NgayDanhGia = DateTime.Now
            });
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Cảm ơn bạn đã đánh giá sản phẩm!";
            return RedirectToAction(nameof(ChiTiet), new { id = request.DonHangId });
        }

        private async Task<DonHang?> LayDonHangCuaToiAsync(int id)
        {
            var userId = User.GetUserId();
            return await _db.DonHangs
                .Include(d => d.ChiTietDonHangs)
                .Include(d => d.MaGiamGiaApDung)
                .FirstOrDefaultAsync(d => d.Id == id && d.NguoiDungId == userId);
        }
    }
}

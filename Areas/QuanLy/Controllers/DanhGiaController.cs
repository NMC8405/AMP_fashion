using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.Services;
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
        private readonly IThongBaoService _thongBaoService;

        public DanhGiaController(ApplicationDbContext db, IThongBaoService thongBaoService)
        {
            _db = db;
            _thongBaoService = thongBaoService;
        }

        [HttpGet]
        public async Task<IActionResult> DanhSach(string? tuKhoa, int? soSao, string? trangThai, string? camXuc)
        {
            var query = _db.DanhGias
                .Include(d => d.NguoiDung)
                .Include(d => d.SanPham).ThenInclude(s => s!.DanhMuc)
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

            // Lọc theo trạng thái kiểm duyệt hoặc trạng thái phản hồi
            if (!string.IsNullOrWhiteSpace(trangThai))
            {
                switch (trangThai.ToLower())
                {
                    case "cho_duyet":
                        query = query.Where(d => d.TrangThai == TrangThaiDanhGia.ChoDuyet);
                        break;
                    case "da_duyet":
                        query = query.Where(d => d.TrangThai == TrangThaiDanhGia.DaDuyet);
                        break;
                    case "tu_choi":
                        query = query.Where(d => d.TrangThai == TrangThaiDanhGia.TuChoi);
                        break;
                    case "cho_phan_hoi":
                        query = query.Where(d => string.IsNullOrEmpty(d.PhanHoi));
                        break;
                    case "da_phan_hoi":
                        query = query.Where(d => !string.IsNullOrEmpty(d.PhanHoi));
                        break;
                }
            }

            // Lọc theo cảm xúc AI PhoBERT
            if (!string.IsNullOrWhiteSpace(camXuc))
            {
                var cx = camXuc.Trim().ToLower();
                query = query.Where(d => d.NhanCamXuc != null && d.NhanCamXuc.ToLower() == cx);
            }

            ViewBag.TuKhoa = tuKhoa;
            ViewBag.SoSao = soSao;
            ViewBag.TrangThai = trangThai;
            ViewBag.CamXuc = camXuc;

            // Thống kê nhanh
            ViewBag.TongDanhGia = await _db.DanhGias.CountAsync();
            ViewBag.ChoDuyet = await _db.DanhGias.CountAsync(d => d.TrangThai == TrangThaiDanhGia.ChoDuyet);
            ViewBag.DaDuyet = await _db.DanhGias.CountAsync(d => d.TrangThai == TrangThaiDanhGia.DaDuyet);
            ViewBag.TuChoi = await _db.DanhGias.CountAsync(d => d.TrangThai == TrangThaiDanhGia.TuChoi);
            ViewBag.TieuCuc = await _db.DanhGias.CountAsync(d => d.NhanCamXuc == "negative" || d.BiCanhBaoTieuCuc);
            ViewBag.ChoPhanHoi = await _db.DanhGias.CountAsync(d => string.IsNullOrEmpty(d.PhanHoi));
            ViewBag.DaPhanHoi = await _db.DanhGias.CountAsync(d => !string.IsNullOrEmpty(d.PhanHoi));

            var danhSach = await query.OrderByDescending(d => d.NgayDanhGia).ToListAsync();
            return View(danhSach);
        }

        // ================= DUYỆT ĐÁNH GIÁ (Admin chấp thuận hiển thị) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Duyet(int id)
        {
            var danhGia = await _db.DanhGias
                .Include(d => d.SanPham)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (danhGia == null)
            {
                TempData["Loi"] = "Không tìm thấy đánh giá cần duyệt.";
                return RedirectToAction(nameof(DanhSach));
            }

            danhGia.TrangThai = TrangThaiDanhGia.DaDuyet;
            danhGia.NgayDuyet = DateTime.Now;
            danhGia.LyDoTuChoi = null;
            await _db.SaveChangesAsync();

            // Gửi thông báo cho khách hàng
            await _thongBaoService.GuiThongBaoAsync(
                danhGia.NguoiDungId,
                "Đánh giá của bạn đã được phê duyệt",
                $"Đánh giá của bạn cho sản phẩm \"{danhGia.SanPham?.TenSanPham}\" đã được Quản trị viên duyệt và hiển thị công khai.",
                LoaiThongBao.DanhGia,
                $"/SanPham/ChiTiet/{danhGia.SanPhamId}");

            TempData["ThongBao"] = "Đã phê duyệt đánh giá thành công. Đánh giá hiện đã được hiển thị công khai.";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= TỪ CHỐI ĐÁNH GIÁ (Admin ẩn bình luận tiêu cực) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(int id, string? lyDoTuChoi)
        {
            var danhGia = await _db.DanhGias
                .Include(d => d.SanPham)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (danhGia == null)
            {
                TempData["Loi"] = "Không tìm thấy đánh giá để từ chối.";
                return RedirectToAction(nameof(DanhSach));
            }

            var lyDo = string.IsNullOrWhiteSpace(lyDoTuChoi)
                ? "Nội dung mang tính tiêu cực gay gắt hoặc không phù hợp với tiêu chuẩn đánh giá của cửa hàng."
                : lyDoTuChoi.Trim();

            danhGia.TrangThai = TrangThaiDanhGia.TuChoi;
            danhGia.LyDoTuChoi = lyDo;
            danhGia.NgayDuyet = DateTime.Now;
            await _db.SaveChangesAsync();

            // Gửi thông báo cho khách hàng
            await _thongBaoService.GuiThongBaoAsync(
                danhGia.NguoiDungId,
                "Đánh giá của bạn không được phê duyệt",
                $"Đánh giá của bạn cho sản phẩm \"{danhGia.SanPham?.TenSanPham}\" đã bị từ chối hiển thị. Lý do: {lyDo}",
                LoaiThongBao.DanhGia,
                $"/DonHang/ChiTiet/{danhGia.DonHangId}");

            TempData["ThongBao"] = "Đã từ chối đánh giá thành công. Đánh giá này sẽ không hiển thị trên trang sản phẩm.";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= PHÂN TÍCH LẠI BẰNG AI =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanTichLai(int id, [FromServices] IAiSentimentService aiService)
        {
            var danhGia = await _db.DanhGias.FindAsync(id);
            if (danhGia == null)
            {
                TempData["Loi"] = "Không tìm thấy đánh giá.";
                return RedirectToAction(nameof(DanhSach));
            }

            var aiResult = await aiService.AnalyzeSentimentAsync(danhGia.NoiDung, danhGia.SoSao);
            danhGia.NhanCamXuc = aiResult.Label;
            danhGia.DoTinCay = aiResult.Confidence;
            danhGia.BiCanhBaoTieuCuc = aiResult.IsNegative;

            await _db.SaveChangesAsync();

            TempData["ThongBao"] = $"Đã phân tích lại cảm xúc bằng AI: {aiResult.LabelDisplayName} ({Math.Round(aiResult.Confidence * 100, 1)}%)";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= PHÂN TÍCH TẤT CẢ ĐÁNH GIÁ CHƯA CÓ NHÃN (DÙNG BATCH PREDICT CỦA PHOBERT-AMP) =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DongBoTatCaAI([FromServices] IAiSentimentService aiService)
        {
            var chuaPhanTich = await _db.DanhGias
                .Where(d => string.IsNullOrEmpty(d.NhanCamXuc))
                .ToListAsync();

            if (!chuaPhanTich.Any())
            {
                TempData["ThongBao"] = "Tất cả các đánh giá đều đã được phân tích cảm xúc.";
                return RedirectToAction(nameof(DanhSach));
            }

            var batchItems = chuaPhanTich.Select(d => new AiCommentBatchItem
            {
                Id = d.Id,
                Text = d.NoiDung,
                HasImage = false
            }).ToList();

            var batchResults = await aiService.AnalyzeBatchSentimentAsync(batchItems);

            int count = 0;
            foreach (var dg in chuaPhanTich)
            {
                if (batchResults.TryGetValue(dg.Id, out var res) && res.Success)
                {
                    dg.NhanCamXuc = res.Label;
                    dg.DoTinCay = res.Confidence;
                    dg.BiCanhBaoTieuCuc = res.IsNegative;
                    if (res.IsNegative)
                    {
                        dg.TrangThai = TrangThaiDanhGia.ChoDuyet;
                    }
                    count++;
                }
            }

            await _db.SaveChangesAsync();
            TempData["ThongBao"] = $"Đã đồng bộ AI PhoBERT-AMP thành công cho {count}/{chuaPhanTich.Count} đánh giá.";
            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PhanHoi(int id, string noiDungPhanHoi)
        {
            var danhGia = await _db.DanhGias
                .Include(d => d.SanPham)
                .FirstOrDefaultAsync(d => d.Id == id);
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

            await _thongBaoService.GuiThongBaoAsync(
                danhGia.NguoiDungId,
                "Cửa hàng đã phản hồi đánh giá của bạn",
                $"AMP Fashion Store đã phản hồi đánh giá của bạn về sản phẩm \"{danhGia.SanPham?.TenSanPham}\": \"{danhGia.PhanHoi}\"",
                LoaiThongBao.DanhGia,
                $"/SanPham/ChiTiet/{danhGia.SanPhamId}");

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

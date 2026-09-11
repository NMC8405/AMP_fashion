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
    public class SanPhamController : Controller
    {
        private const int SoDongMoiTrang = 10;
        private readonly ApplicationDbContext _db;

        public SanPhamController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> DanhSach(string? tuKhoa, int? danhMucId, int trang = 1)
        {
            var query = _db.SanPhams.Include(s => s.DanhMuc).Include(s => s.BienThes).AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
                query = query.Where(s => s.TenSanPham.ToLower().Contains(tuKhoa.Trim().ToLower()));
            if (danhMucId.HasValue)
                query = query.Where(s => s.DanhMucId == danhMucId.Value);

            var tongSo = await query.CountAsync();
            var tongTrang = Math.Max(1, (int)Math.Ceiling(tongSo / (double)SoDongMoiTrang));
            trang = Math.Clamp(trang, 1, tongTrang);

            var sanPhams = await query.OrderByDescending(s => s.NgayTao)
                .Skip((trang - 1) * SoDongMoiTrang).Take(SoDongMoiTrang).ToListAsync();

            ViewBag.DanhMucs = await _db.DanhMucs.OrderBy(d => d.TenDanhMuc).ToListAsync();
            ViewBag.TuKhoa = tuKhoa;
            ViewBag.DanhMucId = danhMucId;
            ViewBag.Trang = trang;
            ViewBag.TongTrang = tongTrang;
            ViewBag.TongSo = tongSo;
            return View(sanPhams);
        }

        // ================= UC20: Thêm sản phẩm =================
        [HttpGet]
        public async Task<IActionResult> Them()
        {
            return View(await XayDungFormAsync(new SanPham(), new List<BienTheSanPham>()));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Them(SanPhamFormViewModel model)
        {
            ModelState.Remove(nameof(SanPhamFormViewModel.DanhMucs));
            ModelState.Remove(nameof(SanPhamFormViewModel.TatCaMauSac));
            ModelState.Remove(nameof(SanPhamFormViewModel.TatCaKichThuoc));
            ModelState.Remove(nameof(SanPhamFormViewModel.BienThes));

            if (!ModelState.IsValid)
                return View(await XayDungFormAsync(model.SanPham, new List<BienTheSanPham>(), model.DuongDanAnhPhu));

            var sanPham = model.SanPham;
            sanPham.NgayTao = DateTime.Now;
            GanDanhSachAnh(sanPham, model.DuongDanAnhPhu);

            _db.SanPhams.Add(sanPham);
            await _db.SaveChangesAsync();

            await LuuBienTheTuFormAsync(sanPham.Id);

            TempData["ThongBao"] = $"Đã thêm sản phẩm \"{sanPham.TenSanPham}\".";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC21: Sửa thông tin sản phẩm =================
        [HttpGet]
        public async Task<IActionResult> Sua(int id)
        {
            var sanPham = await _db.SanPhams.Include(s => s.HinhAnhs).FirstOrDefaultAsync(s => s.Id == id);
            if (sanPham == null) return NotFound();

            var bienThes = await _db.BienTheSanPhams.Where(b => b.SanPhamId == id).ToListAsync();
            var anhPhu = string.Join(Environment.NewLine, sanPham.HinhAnhs.OrderBy(h => h.ThuTu).Select(h => h.DuongDan));
            return View(await XayDungFormAsync(sanPham, bienThes, anhPhu));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sua(int id, SanPhamFormViewModel model)
        {
            ModelState.Remove(nameof(SanPhamFormViewModel.DanhMucs));
            ModelState.Remove(nameof(SanPhamFormViewModel.TatCaMauSac));
            ModelState.Remove(nameof(SanPhamFormViewModel.TatCaKichThuoc));
            ModelState.Remove(nameof(SanPhamFormViewModel.BienThes));

            var sanPham = await _db.SanPhams.Include(s => s.HinhAnhs).FirstOrDefaultAsync(s => s.Id == id);
            if (sanPham == null) return NotFound();

            if (!ModelState.IsValid)
            {
                var bienThesHienTai = await _db.BienTheSanPhams.Where(b => b.SanPhamId == id).ToListAsync();
                return View(await XayDungFormAsync(model.SanPham, bienThesHienTai, model.DuongDanAnhPhu));
            }

            sanPham.TenSanPham = model.SanPham.TenSanPham;
            sanPham.MoTa = model.SanPham.MoTa;
            sanPham.ThuongHieu = model.SanPham.ThuongHieu;
            sanPham.DanhMucId = model.SanPham.DanhMucId;
            sanPham.GiaGoc = model.SanPham.GiaGoc;
            sanPham.GiaKhuyenMai = model.SanPham.GiaKhuyenMai;
            sanPham.HinhAnhChinh = model.SanPham.HinhAnhChinh;
            sanPham.TrangThaiHienThi = model.SanPham.TrangThaiHienThi;

            _db.HinhAnhSanPhams.RemoveRange(sanPham.HinhAnhs);
            sanPham.HinhAnhs.Clear();
            GanDanhSachAnh(sanPham, model.DuongDanAnhPhu);

            await _db.SaveChangesAsync();
            await LuuBienTheTuFormAsync(id);

            TempData["ThongBao"] = $"Đã cập nhật sản phẩm \"{sanPham.TenSanPham}\".";
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= UC22: Xóa sản phẩm =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Xoa(int id)
        {
            var sanPham = await _db.SanPhams.FirstOrDefaultAsync(s => s.Id == id);
            if (sanPham == null) return NotFound();

            var soDonHangLienQuan = await _db.ChiTietDonHangs.CountAsync(c => c.SanPhamId == id);

            if (soDonHangLienQuan > 0)
            {
                // Giữ lại lịch sử đơn hàng: chỉ ẩn sản phẩm thay vì xóa vĩnh viễn
                sanPham.TrangThaiHienThi = false;
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = $"Sản phẩm đã nằm trong {soDonHangLienQuan} đơn hàng nên không thể xóa vĩnh viễn. " +
                    "Hệ thống đã tự động ẨN sản phẩm khỏi cửa hàng để bảo toàn lịch sử đơn hàng.";
            }
            else
            {
                _db.SanPhams.Remove(sanPham);
                await _db.SaveChangesAsync();
                TempData["ThongBao"] = $"Đã xóa sản phẩm \"{sanPham.TenSanPham}\".";
            }

            return RedirectToAction(nameof(DanhSach));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AnHien(int id)
        {
            var sanPham = await _db.SanPhams.FindAsync(id);
            if (sanPham == null) return NotFound();
            sanPham.TrangThaiHienThi = !sanPham.TrangThaiHienThi;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(DanhSach));
        }

        // ================= Hàm hỗ trợ =================
        private async Task<SanPhamFormViewModel> XayDungFormAsync(SanPham sanPham, List<BienTheSanPham> bienThes, string? anhPhu = null)
        {
            return new SanPhamFormViewModel
            {
                SanPham = sanPham,
                DanhMucs = await _db.DanhMucs.OrderBy(d => d.TenDanhMuc).ToListAsync(),
                TatCaMauSac = await _db.MauSacs.ToListAsync(),
                TatCaKichThuoc = await _db.KichThuocs.OrderBy(k => k.ThuTu).ToListAsync(),
                BienThes = bienThes,
                DuongDanAnhPhu = anhPhu
            };
        }

        private static void GanDanhSachAnh(SanPham sanPham, string? anhPhu)
        {
            if (string.IsNullOrWhiteSpace(anhPhu)) return;

            var dongs = anhPhu.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int thuTu = 0;
            foreach (var url in dongs.Where(u => !string.IsNullOrWhiteSpace(u)))
            {
                sanPham.HinhAnhs.Add(new HinhAnhSanPham { DuongDan = url, ThuTu = thuTu++ });
            }
        }

        /// <summary>Đọc ma trận số lượng tồn (Màu x Size) trực tiếp từ Request.Form và upsert BienTheSanPham.</summary>
        private async Task LuuBienTheTuFormAsync(int sanPhamId)
        {
            var tatCaMau = await _db.MauSacs.ToListAsync();
            var tatCaSize = await _db.KichThuocs.ToListAsync();
            var bienTheHienCo = await _db.BienTheSanPhams.Where(b => b.SanPhamId == sanPhamId).ToListAsync();

            foreach (var mau in tatCaMau)
            {
                foreach (var size in tatCaSize)
                {
                    var key = $"soLuong_{mau.Id}_{size.Id}";
                    if (!Request.Form.ContainsKey(key)) continue;

                    _ = int.TryParse(Request.Form[key], out var soLuong);
                    var bienThe = bienTheHienCo.FirstOrDefault(b => b.MauSacId == mau.Id && b.KichThuocId == size.Id);

                    if (bienThe == null && soLuong > 0)
                    {
                        _db.BienTheSanPhams.Add(new BienTheSanPham
                        {
                            SanPhamId = sanPhamId,
                            MauSacId = mau.Id,
                            KichThuocId = size.Id,
                            SoLuongTon = soLuong
                        });
                    }
                    else if (bienThe != null)
                    {
                        bienThe.SoLuongTon = Math.Max(0, soLuong);
                    }
                }
            }

            await _db.SaveChangesAsync();
        }
    }
}

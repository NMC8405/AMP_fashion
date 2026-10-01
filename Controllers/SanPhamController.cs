using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.Services;
using AMPFashionStore.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Controllers
{
    public class SanPhamController : Controller
    {
        private const int SoSanPhamMoiTrang = 15;
        private readonly ApplicationDbContext _db;

        public SanPhamController(ApplicationDbContext db)
        {
            _db = db;
        }

        // ================= UC14/UC15: Danh sách + tìm kiếm + lọc + sắp xếp =================
        [HttpGet]
        public async Task<IActionResult> DanhSach(string? tuKhoa, int? danhMucId, decimal? giaTu, decimal? giaDen,
            string sapXep = "moi-nhat", bool chiGiamGia = false, int trang = 1)
        {
            var query = _db.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.BienThes)
                .Include(s => s.DanhGias)
                .Where(s => s.TrangThaiHienThi)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(tuKhoa))
            {
                var tk = tuKhoa.Trim().ToLower();
                query = query.Where(s => s.TenSanPham.ToLower().Contains(tk)
                    || (s.ThuongHieu != null && s.ThuongHieu.ToLower().Contains(tk)));
            }

            if (danhMucId.HasValue)
                query = query.Where(s => s.DanhMucId == danhMucId.Value);

            if (chiGiamGia)
                query = query.Where(s => s.GiaKhuyenMai != null && s.GiaKhuyenMai < s.GiaGoc);

            const decimal MaxGiaChoPhep = 100_000_000m; // Giới hạn tối đa định mức (100.000.000đ)
            if (giaTu.HasValue)
            {
                if (giaTu.Value < 0) giaTu = 0;
                else if (giaTu.Value > MaxGiaChoPhep) giaTu = MaxGiaChoPhep;
            }

            if (giaDen.HasValue)
            {
                if (giaDen.Value < 0) giaDen = 0;
                else if (giaDen.Value > MaxGiaChoPhep) giaDen = MaxGiaChoPhep;
            }

            if (giaTu.HasValue && giaDen.HasValue && giaTu.Value > giaDen.Value)
            {
                var temp = giaTu;
                giaTu = giaDen;
                giaDen = temp;
            }

            if (giaTu.HasValue)
                query = query.Where(s => (s.GiaKhuyenMai ?? s.GiaGoc) >= giaTu.Value);

            if (giaDen.HasValue)
                query = query.Where(s => (s.GiaKhuyenMai ?? s.GiaGoc) <= giaDen.Value);

            query = sapXep switch
            {
                "gia-tang" => query.OrderBy(s => s.GiaKhuyenMai ?? s.GiaGoc),
                "gia-giam" => query.OrderByDescending(s => s.GiaKhuyenMai ?? s.GiaGoc),
                "ban-chay" => query.OrderByDescending(s => s.DanhGias.Count),
                _ => query.OrderByDescending(s => s.NgayTao)
            };

            var tongSoSanPham = await query.CountAsync();
            var tongSoTrang = Math.Max(1, (int)Math.Ceiling(tongSoSanPham / (double)SoSanPhamMoiTrang));
            trang = Math.Clamp(trang, 1, tongSoTrang);

            var sanPhams = await query
                .Skip((trang - 1) * SoSanPhamMoiTrang)
                .Take(SoSanPhamMoiTrang)
                .ToListAsync();

            var viewModel = new SanPhamDanhSachViewModel
            {
                SanPhams = sanPhams,
                DanhMucs = await _db.DanhMucs.OrderBy(d => d.TenDanhMuc).ToListAsync(),
                TuKhoa = tuKhoa,
                DanhMucId = danhMucId,
                GiaTu = giaTu,
                GiaDen = giaDen,
                SapXep = sapXep,
                TrangHienTai = trang,
                TongSoTrang = tongSoTrang,
                TongSoSanPham = tongSoSanPham
            };

            ViewBag.ChiGiamGia = chiGiamGia;
            return View(viewModel);
        }

        // ================= UC07/UC28: Chi tiết sản phẩm =================
        [HttpGet]
        public async Task<IActionResult> ChiTiet(int id)
        {
            var sanPham = await _db.SanPhams
                .Include(s => s.DanhMuc)
                .Include(s => s.HinhAnhs)
                .Include(s => s.BienThes).ThenInclude(b => b.MauSac)
                .Include(s => s.BienThes).ThenInclude(b => b.KichThuoc)
                .Include(s => s.DanhGias).ThenInclude(d => d.NguoiDung)
                .FirstOrDefaultAsync(s => s.Id == id && s.TrangThaiHienThi);

            if (sanPham == null) return NotFound();

            var tonKho = new Dictionary<string, int>();
            var anhTheoMau = new Dictionary<int, string>();
            foreach (var b in sanPham.BienThes)
            {
                tonKho[$"{b.MauSacId}-{b.KichThuocId}"] = b.SoLuongTon;
                if (!string.IsNullOrEmpty(b.HinhAnh) && !anhTheoMau.ContainsKey(b.MauSacId))
                {
                    anhTheoMau[b.MauSacId] = b.HinhAnh;
                }
            }
            foreach (var h in sanPham.HinhAnhs.Where(h => h.MauSacId.HasValue))
            {
                if (!anhTheoMau.ContainsKey(h.MauSacId!.Value))
                {
                    anhTheoMau[h.MauSacId.Value] = h.DuongDan;
                }
            }

            var viewModel = new SanPhamChiTietViewModel
            {
                SanPham = sanPham,
                MauSacsCoSan = sanPham.BienThes.Select(b => b.MauSac!).DistinctBy(m => m.Id).ToList(),
                KichThuocsCoSan = sanPham.BienThes.Select(b => b.KichThuoc!).DistinctBy(k => k.Id).OrderBy(k => k.ThuTu).ToList(),
                TonKhoTheoBienThe = tonKho,
                AnhTheoMauSac = anhTheoMau,
                SanPhamLienQuan = await _db.SanPhams
                    .Where(s => s.DanhMucId == sanPham.DanhMucId && s.Id != sanPham.Id && s.TrangThaiHienThi)
                    .Take(4)
                    .ToListAsync(),
                DanhGias = sanPham.DanhGias.OrderByDescending(d => d.NgayDanhGia).ToList()
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = User.GetUserId();
                viewModel.DaYeuThich = await _db.YeuThichs.AnyAsync(y => y.NguoiDungId == userId && y.SanPhamId == id);

                // UC28: được đánh giá nếu có đơn hàng đã giao chứa sản phẩm này và chưa đánh giá cho đơn đó
                viewModel.CoTheDanhGia = await _db.ChiTietDonHangs
                    .Include(c => c.DonHang)
                    .AnyAsync(c => c.SanPhamId == id
                        && c.DonHang!.NguoiDungId == userId
                        && c.DonHang.TrangThaiDonHang == TrangThaiDonHang.DaGiao
                        && !_db.DanhGias.Any(d => d.DonHangId == c.DonHangId && d.SanPhamId == id));
            }

            return View(viewModel);
        }

        // "Bộ sưu tập" - nhóm sản phẩm theo danh mục để duyệt theo phong cách
        [HttpGet]
        public async Task<IActionResult> BoSuuTap()
        {
            var danhMucs = await _db.DanhMucs
                .Include(d => d.SanPhams)
                .ToListAsync();
            return View(danhMucs);
        }
    }
}

using System.ComponentModel.DataAnnotations;
using AMPFashionStore.Models;

namespace AMPFashionStore.ViewModels
{
    /// <summary>UC15/UC16: dashboard thống kê doanh thu + top sản phẩm bán chạy.</summary>
    public class ThongKeViewModel
    {
        [DataType(DataType.Date)]
        [Display(Name = "Từ ngày")]
        public DateTime TuNgay { get; set; } = DateTime.Today.AddDays(-6);

        [DataType(DataType.Date)]
        [Display(Name = "Đến ngày")]
        public DateTime DenNgay { get; set; } = DateTime.Today;

        public int TongDonHangMoi { get; set; }
        public double DonHangTangTruong { get; set; } = 8.1;
        public decimal TongDoanhThu { get; set; }
        public double DoanhThuTangTruong { get; set; } = 12.4;
        public int TongKhachHang { get; set; }
        public int TongKhachHangMoi { get; set; }
        public int TongSanPham { get; set; }
        public int TongSanPhamDaBan { get; set; }

        public List<DiemDoanhThuTheoNgay> DoanhThuTheoNgay { get; set; } = new();
        public List<SanPhamBanChay> TopSanPhamBanChay { get; set; } = new();
        public Dictionary<string, int> DonHangTheoTrangThai { get; set; } = new();
        public List<DanhGiaMoiItem> DanhGiaMoiNhat { get; set; } = new();
    }

    public class DiemDoanhThuTheoNgay
    {
        public DateTime Ngay { get; set; }
        public decimal DoanhThu { get; set; }
        public int SoDonHang { get; set; }
    }

    public class SanPhamBanChay
    {
        public int SanPhamId { get; set; }
        public string TenSanPham { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public double DiemDanhGia { get; set; } = 4.8;
        public int SoLuongDaBan { get; set; }
        public decimal DoanhThu { get; set; }
    }

    public class DanhGiaMoiItem
    {
        public int Id { get; set; }
        public string TenKhachHang { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public int SoSao { get; set; }
        public string NoiDung { get; set; } = string.Empty;
        public DateTime NgayDanhGia { get; set; }
        public bool DaPhanHoi => !string.IsNullOrWhiteSpace(PhanHoi);
        public string? PhanHoi { get; set; }
        public bool LaMoi => (DateTime.Now - NgayDanhGia).TotalDays <= 7;
    }

    public class BaoCaoViewModel
    {
        public string KyBaoCao { get; set; } = "30ngay";
        public decimal TongDoanhThu { get; set; }
        public double DoanhThuTangTruong { get; set; } = 12.4;
        public int TongDonHang { get; set; }
        public double DonHangTangTruong { get; set; } = 8.2;
        public decimal GiaTriDonHangTrungBinh { get; set; }
        public double GiaTriDonHangTangTruong { get; set; } = -1.5;
        public decimal LoiNhuan { get; set; }
        public double LoiNhuanTangTruong { get; set; } = 15.6;

        public List<DiemDoanhThuTheoNgay> DoanhThuTheoNgay { get; set; } = new();
        public Dictionary<string, int> DonHangTheoTrangThai { get; set; } = new();
        public Dictionary<string, decimal> DoanhThuTheoDanhMuc { get; set; } = new();
        public List<HieuQuaSanPhamItem> ChiTietHieuQuaSanPham { get; set; } = new();
    }

    public class HieuQuaSanPhamItem
    {
        public int SanPhamId { get; set; }
        public string MaSanPham { get; set; } = string.Empty;
        public string TenSanPham { get; set; } = string.Empty;
        public string? HinhAnh { get; set; }
        public int DaBan { get; set; }
        public decimal DoanhThu { get; set; }
        public decimal LoiNhuan { get; set; }
        public double TangTruong { get; set; }
    }

    public class NhanVienItemViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public DateTime NgayThamGia { get; set; }
        public VaiTro VaiTro { get; set; }
        public TrangThaiTaiKhoan TrangThai { get; set; }
    }

    public class KhachHangItemViewModel
    {
        public int Id { get; set; }
        public string HoTen { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? SoDienThoai { get; set; }
        public DateTime NgayTao { get; set; }
        public int TongDon { get; set; }
        public decimal TongChiTieu { get; set; }
        public TrangThaiTaiKhoan TrangThai { get; set; }
        public bool LaVIP => TongChiTieu >= 10000000;
    }

    // ===== Quản lý tài khoản (UC23-27, chỉ Quản trị viên) =====
    public class TaiKhoanFormViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Display(Name = "Vai trò")]
        public VaiTro VaiTro { get; set; } = VaiTro.KhachHang;

        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string? MatKhau { get; set; }

        public bool DangSua => Id > 0;
    }
}

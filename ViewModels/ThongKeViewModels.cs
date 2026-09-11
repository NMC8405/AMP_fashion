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
        public decimal TongDoanhThu { get; set; }
        public int TongKhachHangMoi { get; set; }
        public int TongSanPhamDaBan { get; set; }

        public List<DiemDoanhThuTheoNgay> DoanhThuTheoNgay { get; set; } = new();
        public List<SanPhamBanChay> TopSanPhamBanChay { get; set; } = new();
        public Dictionary<string, int> DonHangTheoTrangThai { get; set; } = new();
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
        public int SoLuongDaBan { get; set; }
        public decimal DoanhThu { get; set; }
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

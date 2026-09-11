using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMPFashionStore.Models
{
    /// <summary>Tài khoản người dùng (Khách hàng / Nhân viên / Quản trị viên).</summary>
    public class NguoiDung
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string MatKhauHash { get; set; } = string.Empty;

        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? DiaChi { get; set; }

        [StringLength(300)]
        public string? AnhDaiDien { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Giới tính")]
        public GioiTinh GioiTinh { get; set; } = GioiTinh.KhongMuonTietLo;

        public VaiTro VaiTro { get; set; } = VaiTro.KhachHang;

        public TrangThaiTaiKhoan TrangThai { get; set; } = TrangThaiTaiKhoan.HoatDong;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Phục vụ UC02 (4-c): khóa tạm sau khi đăng nhập sai quá số lần quy định
        public int SoLanDangNhapSai { get; set; } = 0;

        public DateTime? KhoaDangNhapDenLuc { get; set; }

        [NotMapped]
        public string TenVaiTro => VaiTro switch
        {
            VaiTro.QuanTriVien => "Quản trị viên",
            VaiTro.NhanVien => "Nhân viên",
            _ => "Khách hàng"
        };

        // Navigation
        public ICollection<GioHangItem> GioHangItems { get; set; } = new List<GioHangItem>();
        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
    }
}

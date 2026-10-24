using System.ComponentModel.DataAnnotations;

namespace AMPFashionStore.Models
{
    public enum TrangThaiDanhGia
    {
        [Display(Name = "Đã duyệt")]
        DaDuyet = 0,

        [Display(Name = "Chờ duyệt")]
        ChoDuyet = 1,

        [Display(Name = "Từ chối")]
        TuChoi = 2
    }

    /// <summary>Đánh giá sản phẩm (UC28) - chỉ tạo được sau khi đơn hàng ở trạng thái Đã giao.</summary>
    public class DanhGia
    {
        public int Id { get; set; }

        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        public int NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        public int DonHangId { get; set; }
        public DonHang? DonHang { get; set; }

        [Range(1, 5)]
        [Display(Name = "Số sao")]
        public int SoSao { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000)]
        [Display(Name = "Nội dung đánh giá")]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayDanhGia { get; set; } = DateTime.Now;

        [StringLength(1000)]
        [Display(Name = "Phản hồi từ cửa hàng")]
        public string? PhanHoi { get; set; }

        public DateTime? NgayPhanHoi { get; set; }

        // ===== Tích hợp AI PhoBERT phân tích cảm xúc & Kiểm duyệt =====
        [Display(Name = "Trạng thái kiểm duyệt")]
        public TrangThaiDanhGia TrangThai { get; set; } = TrangThaiDanhGia.DaDuyet;

        [StringLength(20)]
        [Display(Name = "Cảm xúc AI")]
        public string? NhanCamXuc { get; set; } // negative, normal, positive

        [Display(Name = "Độ tin cậy AI")]
        public double? DoTinCay { get; set; }

        [Display(Name = "Cảnh báo tiêu cực")]
        public bool BiCanhBaoTieuCuc { get; set; } = false;

        [StringLength(500)]
        [Display(Name = "Lý do từ chối")]
        public string? LyDoTuChoi { get; set; }

        public DateTime? NgayDuyet { get; set; }
    }

    /// <summary>Sản phẩm yêu thích của người dùng (UC32/UC33).</summary>
    public class YeuThich
    {
        public int Id { get; set; }

        public int NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        public DateTime NgayThem { get; set; } = DateTime.Now;
    }

    /// <summary>Tin nhắn liên hệ gửi từ khách hàng (use case "Liên hệ" trong sơ đồ tổng quát).</summary>
    public class LienHe
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không đúng định dạng (ví dụ: example@gmail.com)")]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string? SoDienThoai { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập nội dung")]
        [StringLength(1000)]
        [Display(Name = "Nội dung")]
        public string NoiDung { get; set; } = string.Empty;

        public DateTime NgayGui { get; set; } = DateTime.Now;

        public bool DaXuLy { get; set; } = false;
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMPFashionStore.Models
{
    /// <summary>Mã giảm giá (UC23/24/25 - Quản lý mã giảm giá).</summary>
    public class MaGiamGia
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mã giảm giá")]
        [StringLength(30)]
        [Display(Name = "Mã giảm giá")]
        public string Ma { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [Display(Name = "Loại giảm giá")]
        public LoaiGiamGia LoaiGiamGia { get; set; } = LoaiGiamGia.PhanTram;

        [Required]
        [Display(Name = "Giá trị")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá trị phải lớn hơn 0")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaTri { get; set; }

        [Display(Name = "Giá trị đơn hàng tối thiểu")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal GiaTriDonHangToiThieu { get; set; } = 0;

        [Display(Name = "Số tiền giảm tối đa")]
        [Column(TypeName = "decimal(18,0)")]
        public decimal? SoTienGiamToiDa { get; set; }

        [Display(Name = "Số lượt sử dụng tối đa")]
        public int? SoLuongToiDa { get; set; }

        public int SoLuongDaDung { get; set; } = 0;

        [Required]
        [Display(Name = "Ngày bắt đầu")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Ngày kết thúc")]
        [DataType(DataType.Date)]
        public DateTime NgayKetThuc { get; set; } = DateTime.Today.AddMonths(1);

        [Display(Name = "Đang kích hoạt")]
        public bool TrangThaiHoatDong { get; set; } = true;

        [NotMapped]
        public bool ConHieuLuc =>
            TrangThaiHoatDong
            && DateTime.Today >= NgayBatDau
            && DateTime.Today <= NgayKetThuc
            && (SoLuongToiDa == null || SoLuongDaDung < SoLuongToiDa);

        public ICollection<DonHang> DonHangs { get; set; } = new List<DonHang>();

        /// <summary>Tính số tiền được giảm cho một đơn hàng có giá trị tạm tính cho trước.</summary>
        public decimal TinhSoTienGiam(decimal tamTinh)
        {
            if (!ConHieuLuc || tamTinh < GiaTriDonHangToiThieu) return 0;

            decimal soTienGiam = LoaiGiamGia == LoaiGiamGia.PhanTram
                ? Math.Round(tamTinh * GiaTri / 100m)
                : GiaTri;

            if (SoTienGiamToiDa.HasValue && soTienGiam > SoTienGiamToiDa.Value)
                soTienGiam = SoTienGiamToiDa.Value;

            return soTienGiam > tamTinh ? tamTinh : soTienGiam;
        }
    }
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMPFashionStore.Models
{
    /// <summary>Danh mục sản phẩm (Áo, Quần, Váy, Phụ kiện...).</summary>
    public class DanhMuc
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100)]
        [Display(Name = "Tên danh mục")]
        public string TenDanhMuc { get; set; } = string.Empty;

        [StringLength(300)]
        public string? MoTa { get; set; }

        public ICollection<SanPham> SanPhams { get; set; } = new List<SanPham>();
    }

    /// <summary>Sản phẩm thời trang.</summary>
    public class SanPham
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên sản phẩm")]
        [StringLength(100, ErrorMessage = "Tên sản phẩm không được vượt quá 100 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string TenSanPham { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Mô tả")]
        public string? MoTa { get; set; }

        [StringLength(100)]
        [Display(Name = "Thương hiệu")]
        public string? ThuongHieu { get; set; }

        [Required]
        [Display(Name = "Danh mục")]
        public int DanhMucId { get; set; }
        public DanhMuc? DanhMuc { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá gốc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        public decimal GiaGoc { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        [Display(Name = "Giá khuyến mãi")]
        public decimal? GiaKhuyenMai { get; set; }

        [StringLength(400)]
        [Display(Name = "Ảnh đại diện")]
        public string? HinhAnhChinh { get; set; }

        [Display(Name = "Đang hiển thị")]
        public bool TrangThaiHienThi { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        [NotMapped]
        public decimal GiaHienThi => GiaKhuyenMai.HasValue && GiaKhuyenMai.Value > 0 && GiaKhuyenMai.Value < GiaGoc
            ? GiaKhuyenMai.Value
            : GiaGoc;

        [NotMapped]
        public bool DangGiamGia => GiaKhuyenMai.HasValue && GiaKhuyenMai.Value > 0 && GiaKhuyenMai.Value < GiaGoc;

        [NotMapped]
        public int TongSoLuongTon => BienThes?.Sum(b => b.SoLuongTon) ?? 0;

        [NotMapped]
        public double DiemDanhGiaTrungBinh => DanhGias != null && DanhGias.Count > 0
            ? Math.Round(DanhGias.Average(d => d.SoSao), 1)
            : 0;

        public ICollection<HinhAnhSanPham> HinhAnhs { get; set; } = new List<HinhAnhSanPham>();
        public ICollection<BienTheSanPham> BienThes { get; set; } = new List<BienTheSanPham>();
        public ICollection<DanhGia> DanhGias { get; set; } = new List<DanhGia>();
        public ICollection<YeuThich> YeuThichs { get; set; } = new List<YeuThich>();
    }

    /// <summary>Ảnh phụ của sản phẩm (ngoài ảnh đại diện).</summary>
    public class HinhAnhSanPham
    {
        public int Id { get; set; }

        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        [Required]
        [StringLength(400)]
        public string DuongDan { get; set; } = string.Empty;

        public int ThuTu { get; set; } = 0;

        public int? MauSacId { get; set; }
        public MauSac? MauSac { get; set; }
    }

    /// <summary>Danh mục màu sắc dùng chung cho toàn hệ thống.</summary>
    public class MauSac
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên màu")]
        [StringLength(50)]
        [Display(Name = "Tên màu")]
        public string TenMau { get; set; } = string.Empty;

        [StringLength(10)]
        [Display(Name = "Mã màu (hex)")]
        public string MaHex { get; set; } = "#000000";

        public ICollection<BienTheSanPham> BienThes { get; set; } = new List<BienTheSanPham>();
    }

    /// <summary>Danh mục kích thước dùng chung (S, M, L, XL...).</summary>
    public class KichThuoc
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên kích thước")]
        [StringLength(20)]
        [Display(Name = "Kích thước")]
        public string TenKichThuoc { get; set; } = string.Empty;

        [Display(Name = "Thứ tự hiển thị")]
        public int ThuTu { get; set; } = 0;

        public ICollection<BienTheSanPham> BienThes { get; set; } = new List<BienTheSanPham>();
    }

    /// <summary>
    /// Biến thể Sản phẩm x Màu sắc x Kích thước - đơn vị quản lý tồn kho thực tế.
    /// UC07/UC08: những biến thể hết hàng (SoLuongTon = 0) sẽ bị vô hiệu hóa trên giao diện.
    /// </summary>
    public class BienTheSanPham
    {
        public int Id { get; set; }

        public int SanPhamId { get; set; }
        public SanPham? SanPham { get; set; }

        public int MauSacId { get; set; }
        public MauSac? MauSac { get; set; }

        public int KichThuocId { get; set; }
        public KichThuoc? KichThuoc { get; set; }

        [Display(Name = "Số lượng tồn")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn không hợp lệ")]
        public int SoLuongTon { get; set; }

        [StringLength(400)]
        [Display(Name = "Ảnh biến thể")]
        public string? HinhAnh { get; set; }

        [NotMapped]
        public bool ConHang => SoLuongTon > 0;
    }
}

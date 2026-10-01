using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AMPFashionStore.Models
{
    /// <summary>Đơn hàng (UC09 Đặt hàng, UC010 Thanh toán, UC011 Cập nhật, UC012 Hủy).</summary>
    public class DonHang
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string MaDonHang { get; set; } = string.Empty;

        public int NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        public DateTime NgayDat { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        public string TenNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15)]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoaiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(300)]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string DiaChiNhan { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        public PhuongThucThanhToan PhuongThucThanhToan { get; set; }

        public TrangThaiThanhToan TrangThaiThanhToan { get; set; } = TrangThaiThanhToan.ChuaThanhToan;

        public TrangThaiDonHang TrangThaiDonHang { get; set; } = TrangThaiDonHang.ChoXacNhan;

        [Column(TypeName = "decimal(18,0)")]
        public decimal TamTinh { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal SoTienGiamGia { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal PhiVanChuyen { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal TongTien
        {
            get => _tongTien > 0 ? _tongTien : Math.Max(0, TamTinh + PhiVanChuyen - SoTienGiamGia);
            set => _tongTien = value;
        }
        private decimal _tongTien;

        public int? MaGiamGiaId { get; set; }
        public MaGiamGia? MaGiamGiaApDung { get; set; }

        [StringLength(300)]
        public string? LyDoHuy { get; set; }

        public DateTime? NgayXacNhan { get; set; }
        public DateTime? NgayGiao { get; set; }
        public DateTime? NgayHuy { get; set; }

        [NotMapped]
        public string TenTrangThai => TrangThaiDonHang switch
        {
            TrangThaiDonHang.ChoThanhToan => "Chờ thanh toán",
            TrangThaiDonHang.ChoXacNhan => "Chờ xác nhận",
            TrangThaiDonHang.DaXacNhan => "Đã xác nhận",
            TrangThaiDonHang.DangGiao => "Đang giao hàng",
            TrangThaiDonHang.DaGiao => "Đã giao hàng",
            TrangThaiDonHang.DaHuy => "Đã hủy",
            _ => TrangThaiDonHang.ToString()
        };

        /// <summary>Hậu tố class CSS (kebab-case) tương ứng, dùng cho khối trạng thái .amp-status-*.</summary>
        [NotMapped]
        public string CssTrangThai => TrangThaiDonHang switch
        {
            TrangThaiDonHang.ChoThanhToan => "cho-thanh-toan",
            TrangThaiDonHang.ChoXacNhan => "cho-xac-nhan",
            TrangThaiDonHang.DaXacNhan => "da-xac-nhan",
            TrangThaiDonHang.DangGiao => "dang-giao",
            TrangThaiDonHang.DaGiao => "da-giao",
            TrangThaiDonHang.DaHuy => "da-huy",
            _ => "cho-xac-nhan"
        };

        // UC011: chỉ được sửa thông tin nhận hàng khi đơn chưa bàn giao vận chuyển
        [NotMapped]
        public bool ChoPhepSuaThongTin => TrangThaiDonHang == TrangThaiDonHang.ChoXacNhan || TrangThaiDonHang == TrangThaiDonHang.DaXacNhan;

        // UC012: chỉ hủy được khi chưa vận chuyển
        [NotMapped]
        public bool ChoPhepHuy => TrangThaiDonHang == TrangThaiDonHang.ChoXacNhan || TrangThaiDonHang == TrangThaiDonHang.DaXacNhan;

        [NotMapped]
        public bool ChoPhepDanhGia => TrangThaiDonHang == TrangThaiDonHang.DaGiao;

        public ICollection<ChiTietDonHang> ChiTietDonHangs { get; set; } = new List<ChiTietDonHang>();
    }

    /// <summary>Chi tiết từng dòng sản phẩm trong đơn hàng (lưu snapshot tại thời điểm đặt).</summary>
    public class ChiTietDonHang
    {
        public int Id { get; set; }

        public int DonHangId { get; set; }
        public DonHang? DonHang { get; set; }

        // Có thể null nếu biến thể gốc đã bị xóa sau này - vẫn giữ được lịch sử đơn hàng nhờ các trường snapshot bên dưới
        public int? BienTheSanPhamId { get; set; }
        public BienTheSanPham? BienTheSanPham { get; set; }

        public int? SanPhamId { get; set; }

        [StringLength(200)]
        public string TenSanPham { get; set; } = string.Empty;

        [StringLength(50)]
        public string TenMau { get; set; } = string.Empty;

        [StringLength(20)]
        public string TenKichThuoc { get; set; } = string.Empty;

        [StringLength(400)]
        public string? HinhAnh { get; set; }

        public int SoLuong { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal DonGia { get; set; }

        [Column(TypeName = "decimal(18,0)")]
        public decimal ThanhTien { get; set; }
    }
}

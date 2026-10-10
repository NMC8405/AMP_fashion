using System.ComponentModel.DataAnnotations;
using AMPFashionStore.Models;

namespace AMPFashionStore.ViewModels
{
    /// <summary>Một dòng sản phẩm được chọn để thanh toán (lưu tạm trong Session cho tới khi đặt hàng xong).</summary>
    public class DongCheckout
    {
        public int BienTheSanPhamId { get; set; }
        public int SoLuong { get; set; }
    }

    /// <summary>UC09/UC010: trang thanh toán - nhập thông tin nhận hàng, chọn thanh toán, áp mã giảm giá.</summary>
    public class ThanhToanViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        public string TenNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số bắt đầu bằng số 0 (ví dụ: 0912345678).")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoaiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(300)]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string DiaChiNhan { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }

        [Display(Name = "Hình thức thanh toán")]
        public PhuongThucThanhToan PhuongThucThanhToan { get; set; } = PhuongThucThanhToan.ThanhToanKhiNhanHang;

        [Display(Name = "Mã giảm giá")]
        public string? MaGiamGia { get; set; }

        // ---- Chỉ dùng để hiển thị, không submit ----
        public List<ChiTietDonHang> DongHang { get; set; } = new();
        public decimal TamTinh { get; set; }
        public decimal PhiVanChuyen { get; set; } = 30000;
        public decimal SoTienGiamGia { get; set; }
        public decimal TongTien => TamTinh + PhiVanChuyen - SoTienGiamGia;
        public string? ThongBaoMa { get; set; }
        public bool MaHopLe { get; set; }
    }

    /// <summary>UC011/UC012: form khách hàng cập nhật thông tin nhận hàng hoặc hủy đơn.</summary>
    public class CapNhatDonHangViewModel
    {
        public int DonHangId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        [Display(Name = "Tên người nhận")]
        public string TenNguoiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ. Vui lòng nhập đúng 10 chữ số bắt đầu bằng số 0 (ví dụ: 0912345678).")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoaiNhan { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(300)]
        [Display(Name = "Địa chỉ nhận hàng")]
        public string DiaChiNhan { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Ghi chú")]
        public string? GhiChu { get; set; }
    }

    public class HuyDonHangRequest
    {
        public int DonHangId { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn lý do hủy đơn")]
        public string LyDoHuy { get; set; } = string.Empty;
    }

    /// <summary>Form gửi đánh giá sản phẩm sau khi đơn hàng đã giao (UC28).</summary>
    public class DanhGiaRequest
    {
        public int DonHangId { get; set; }
        public int SanPhamId { get; set; }

        [Range(1, 5, ErrorMessage = "Vui lòng chọn số sao từ 1 đến 5")]
        public int SoSao { get; set; } = 5;

        [Required(ErrorMessage = "Vui lòng nhập nội dung đánh giá")]
        [StringLength(1000)]
        public string NoiDung { get; set; } = string.Empty;
    }
}

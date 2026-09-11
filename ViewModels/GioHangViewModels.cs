using AMPFashionStore.Models;

namespace AMPFashionStore.ViewModels
{
    /// <summary>UC08: trang giỏ hàng - danh sách dòng sản phẩm + tổng tiền các dòng được chọn.</summary>
    public class GioHangViewModel
    {
        public List<GioHangItem> DongSanPhams { get; set; } = new();
        public decimal TongTien => DongSanPhams.Sum(x => x.ThanhTien);
        public int TongSoLuong => DongSanPhams.Sum(x => x.SoLuong);
    }

    /// <summary>UC07: request thêm sản phẩm vào giỏ hàng (submit từ trang chi tiết sản phẩm).</summary>
    public class ThemVaoGioRequest
    {
        public int SanPhamId { get; set; }
        public int MauSacId { get; set; }
        public int KichThuocId { get; set; }
        public int SoLuong { get; set; } = 1;

        /// <summary>Nếu true: chuyển thẳng sang trang thanh toán (Mua ngay) thay vì lưu vào giỏ.</summary>
        public bool MuaNgay { get; set; } = false;
    }

    /// <summary>UC08: request cập nhật số lượng một dòng trong giỏ hàng.</summary>
    public class CapNhatGioRequest
    {
        public int GioHangItemId { get; set; }
        public int SoLuong { get; set; }
    }
}

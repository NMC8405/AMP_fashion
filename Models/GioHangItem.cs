using System.ComponentModel.DataAnnotations.Schema;

namespace AMPFashionStore.Models
{
    /// <summary>
    /// Một dòng sản phẩm trong giỏ hàng của người dùng.
    /// Mỗi người dùng có một giỏ hàng "ảo" = tập hợp các GioHangItem của họ.
    /// </summary>
    public class GioHangItem
    {
        public int Id { get; set; }

        public int NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        public int BienTheSanPhamId { get; set; }
        public BienTheSanPham? BienTheSanPham { get; set; }

        public int SoLuong { get; set; }

        public DateTime NgayThem { get; set; } = DateTime.Now;

        [NotMapped]
        public decimal ThanhTien => (BienTheSanPham?.SanPham?.GiaHienThi ?? 0) * SoLuong;
    }
}

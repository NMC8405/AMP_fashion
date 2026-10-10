using System.ComponentModel.DataAnnotations;

namespace AMPFashionStore.Models
{
    public enum LoaiThongBao
    {
        DonHang = 1,
        DanhGia = 2,
        HeThong = 3
    }

    public class ThongBao
    {
        public int Id { get; set; }

        public int NguoiDungId { get; set; }
        public NguoiDung? NguoiDung { get; set; }

        [Required]
        [StringLength(200)]
        public string TieuDe { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string NoiDung { get; set; } = string.Empty;

        public LoaiThongBao LoaiThongBao { get; set; } = LoaiThongBao.DonHang;

        [StringLength(300)]
        public string? DuongDan { get; set; }

        public bool DaDoc { get; set; } = false;

        public DateTime NgayTao { get; set; } = DateTime.Now;
    }
}

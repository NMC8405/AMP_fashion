using AMPFashionStore.Models;

namespace AMPFashionStore.ViewModels
{
    /// <summary>UC14/UC15: danh sách sản phẩm có tìm kiếm + lọc + sắp xếp + phân trang.</summary>
    public class SanPhamDanhSachViewModel
    {
        public List<SanPham> SanPhams { get; set; } = new();
        public List<DanhMuc> DanhMucs { get; set; } = new();

        public string? TuKhoa { get; set; }
        public int? DanhMucId { get; set; }
        public decimal? GiaTu { get; set; }
        public decimal? GiaDen { get; set; }
        public string SapXep { get; set; } = "moi-nhat"; // moi-nhat | gia-tang | gia-giam | ban-chay

        public int TrangHienTai { get; set; } = 1;
        public int TongSoTrang { get; set; } = 1;
        public int TongSoSanPham { get; set; } = 0;
    }

    /// <summary>UC07/UC09: trang chi tiết sản phẩm, chọn màu/size để thêm giỏ hoặc mua ngay.</summary>
    public class SanPhamChiTietViewModel
    {
        public SanPham SanPham { get; set; } = null!;
        public List<MauSac> MauSacsCoSan { get; set; } = new();
        public List<KichThuoc> KichThuocsCoSan { get; set; } = new();

        /// <summary>Ma trận tồn kho: key "mauId-kichThuocId" -> số lượng tồn, dùng để JS bật/tắt lựa chọn.</summary>
        public Dictionary<string, int> TonKhoTheoBienThe { get; set; } = new();

        /// <summary>Ánh xạ mauSacId -> link ảnh để khi click chọn màu, ảnh chính lập tức đổi sang ảnh tương ứng.</summary>
        public Dictionary<int, string> AnhTheoMauSac { get; set; } = new();

        public List<SanPham> SanPhamLienQuan { get; set; } = new();
        public List<DanhGia> DanhGias { get; set; } = new();
        public bool DaYeuThich { get; set; }
        public bool CoTheDanhGia { get; set; }
    }

    /// <summary>Form thêm/sửa sản phẩm ở khu Quản lý.</summary>
    public class SanPhamFormViewModel
    {
        public SanPham SanPham { get; set; } = new();
        public List<DanhMuc> DanhMucs { get; set; } = new();
        public List<MauSac> TatCaMauSac { get; set; } = new();
        public List<KichThuoc> TatCaKichThuoc { get; set; } = new();

        /// <summary>Danh sách biến thể hiện có (khi sửa sản phẩm).</summary>
        public List<BienTheSanPham> BienThes { get; set; } = new();

        /// <summary>Danh sách link ảnh phụ, mỗi dòng một link (nhập nhanh, không cần upload file).</summary>
        public string? DuongDanAnhPhu { get; set; }
    }
}

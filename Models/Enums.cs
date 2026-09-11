namespace AMPFashionStore.Models
{
    /// <summary>Vai trò người dùng trong hệ thống.</summary>
    public enum VaiTro
    {
        KhachHang = 0,
        NhanVien = 1,      // Quản lý cửa hàng: sản phẩm, mã giảm giá, đơn hàng, thống kê
        QuanTriVien = 2    // Admin: có toàn bộ quyền của Nhân viên + quản lý tài khoản
    }

    /// <summary>Trạng thái tài khoản.</summary>
    public enum TrangThaiTaiKhoan
    {
        HoatDong = 0,
        BiKhoa = 1
    }

    /// <summary>Trạng thái xử lý của một đơn hàng (vòng đời đơn hàng).</summary>
    public enum TrangThaiDonHang
    {
        ChoThanhToan = 0,   // Thanh toán online nhưng chưa xác nhận giao dịch thành công
        ChoXacNhan = 1,     // Đã đặt / đã thanh toán - chờ nhân viên xác nhận
        DaXacNhan = 2,      // Nhân viên đã xác nhận, chuẩn bị hàng
        DangGiao = 3,       // Đã bàn giao cho đơn vị vận chuyển
        DaGiao = 4,         // Khách đã nhận hàng (khách tự xác nhận -> có thể đánh giá)
        DaHuy = 5
    }

    /// <summary>Hình thức thanh toán.</summary>
    public enum PhuongThucThanhToan
    {
        ThanhToanKhiNhanHang = 0, // COD
        ChuyenKhoanNganHang = 1   // QR/Chuyển khoản
    }

    /// <summary>Trạng thái thanh toán của đơn hàng (độc lập với trạng thái vận chuyển).</summary>
    public enum TrangThaiThanhToan
    {
        ChuaThanhToan = 0,
        DaThanhToan = 1,
        DaHoanTien = 2
    }

    /// <summary>Loại giá trị của mã giảm giá.</summary>
    public enum LoaiGiamGia
    {
        PhanTram = 0,     // giảm theo %
        SoTienCoDinh = 1  // giảm số tiền cố định
    }

    /// <summary>Mục đích sử dụng của một mã OTP.</summary>
    public enum LoaiOtp
    {
        DangKyTaiKhoan = 0,
        QuenMatKhau = 1
    }

    /// <summary>Giới tính (tùy chọn, hiển thị ở trang thông tin cá nhân).</summary>
    public enum GioiTinh
    {
        KhongMuonTietLo = 0,
        Nam = 1,
        Nu = 2,
        Khac = 3
    }
}

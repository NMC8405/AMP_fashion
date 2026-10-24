CREATE TABLE [DanhMucs] (
    [Id] int NOT NULL IDENTITY,
    [TenDanhMuc] nvarchar(100) NOT NULL,
    [MoTa] nvarchar(300) NULL,
    CONSTRAINT [PK_DanhMucs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [KichThuocs] (
    [Id] int NOT NULL IDENTITY,
    [TenKichThuoc] nvarchar(20) NOT NULL,
    [ThuTu] int NOT NULL,
    CONSTRAINT [PK_KichThuocs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [LienHes] (
    [Id] int NOT NULL IDENTITY,
    [HoTen] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [SoDienThoai] nvarchar(15) NULL,
    [NoiDung] nvarchar(1000) NOT NULL,
    [NgayGui] datetime2 NOT NULL,
    [DaXuLy] bit NOT NULL,
    CONSTRAINT [PK_LienHes] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MaGiamGias] (
    [Id] int NOT NULL IDENTITY,
    [Ma] nvarchar(30) NOT NULL,
    [MoTa] nvarchar(200) NULL,
    [LoaiGiamGia] int NOT NULL,
    [GiaTri] decimal(18,0) NOT NULL,
    [GiaTriDonHangToiThieu] decimal(18,0) NOT NULL,
    [SoTienGiamToiDa] decimal(18,0) NULL,
    [SoLuongToiDa] int NULL,
    [SoLuongDaDung] int NOT NULL,
    [NgayBatDau] datetime2 NOT NULL,
    [NgayKetThuc] datetime2 NOT NULL,
    [TrangThaiHoatDong] bit NOT NULL,
    CONSTRAINT [PK_MaGiamGias] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [MauSacs] (
    [Id] int NOT NULL IDENTITY,
    [TenMau] nvarchar(50) NOT NULL,
    [MaHex] nvarchar(10) NOT NULL,
    CONSTRAINT [PK_MauSacs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [NguoiDungs] (
    [Id] int NOT NULL IDENTITY,
    [HoTen] nvarchar(100) NOT NULL,
    [Email] nvarchar(150) NOT NULL,
    [MatKhauHash] nvarchar(max) NOT NULL,
    [SoDienThoai] nvarchar(15) NULL,
    [DiaChi] nvarchar(250) NULL,
    [AnhDaiDien] nvarchar(300) NULL,
    [NgaySinh] datetime2 NULL,
    [GioiTinh] int NOT NULL,
    [VaiTro] int NOT NULL,
    [TrangThai] int NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    [SoLanDangNhapSai] int NOT NULL,
    [KhoaDangNhapDenLuc] datetime2 NULL,
    CONSTRAINT [PK_NguoiDungs] PRIMARY KEY ([Id])
);
GO


CREATE TABLE [SanPhams] (
    [Id] int NOT NULL IDENTITY,
    [TenSanPham] nvarchar(100) NOT NULL,
    [MoTa] nvarchar(2000) NULL,
    [ThuongHieu] nvarchar(100) NULL,
    [DanhMucId] int NOT NULL,
    [GiaGoc] decimal(18,0) NOT NULL,
    [GiaKhuyenMai] decimal(18,0) NULL,
    [HinhAnhChinh] nvarchar(400) NULL,
    [TrangThaiHienThi] bit NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    CONSTRAINT [PK_SanPhams] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SanPhams_DanhMucs_DanhMucId] FOREIGN KEY ([DanhMucId]) REFERENCES [DanhMucs] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [DonHangs] (
    [Id] int NOT NULL IDENTITY,
    [MaDonHang] nvarchar(20) NOT NULL,
    [NguoiDungId] int NOT NULL,
    [NgayDat] datetime2 NOT NULL,
    [TenNguoiNhan] nvarchar(100) NOT NULL,
    [SoDienThoaiNhan] nvarchar(15) NOT NULL,
    [DiaChiNhan] nvarchar(300) NOT NULL,
    [GhiChu] nvarchar(500) NULL,
    [PhuongThucThanhToan] int NOT NULL,
    [TrangThaiThanhToan] int NOT NULL,
    [TrangThaiDonHang] int NOT NULL,
    [TamTinh] decimal(18,0) NOT NULL,
    [SoTienGiamGia] decimal(18,0) NOT NULL,
    [PhiVanChuyen] decimal(18,0) NOT NULL,
    [TongTien] decimal(18,0) NOT NULL,
    [MaGiamGiaId] int NULL,
    [LyDoHuy] nvarchar(300) NULL,
    [NgayXacNhan] datetime2 NULL,
    [NgayGiao] datetime2 NULL,
    [NgayHuy] datetime2 NULL,
    CONSTRAINT [PK_DonHangs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DonHangs_MaGiamGias_MaGiamGiaId] FOREIGN KEY ([MaGiamGiaId]) REFERENCES [MaGiamGias] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_DonHangs_NguoiDungs_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDungs] ([Id]) ON DELETE NO ACTION
);
GO


CREATE TABLE [ThongBaos] (
    [Id] int NOT NULL IDENTITY,
    [NguoiDungId] int NOT NULL,
    [TieuDe] nvarchar(200) NOT NULL,
    [NoiDung] nvarchar(1000) NOT NULL,
    [LoaiThongBao] int NOT NULL,
    [DuongDan] nvarchar(300) NULL,
    [DaDoc] bit NOT NULL,
    [NgayTao] datetime2 NOT NULL,
    CONSTRAINT [PK_ThongBaos] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ThongBaos_NguoiDungs_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDungs] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [BienTheSanPhams] (
    [Id] int NOT NULL IDENTITY,
    [SanPhamId] int NOT NULL,
    [MauSacId] int NOT NULL,
    [KichThuocId] int NOT NULL,
    [SoLuongTon] int NOT NULL,
    [HinhAnh] nvarchar(400) NULL,
    CONSTRAINT [PK_BienTheSanPhams] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_BienTheSanPhams_KichThuocs_KichThuocId] FOREIGN KEY ([KichThuocId]) REFERENCES [KichThuocs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BienTheSanPhams_MauSacs_MauSacId] FOREIGN KEY ([MauSacId]) REFERENCES [MauSacs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_BienTheSanPhams_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [HinhAnhSanPhams] (
    [Id] int NOT NULL IDENTITY,
    [SanPhamId] int NOT NULL,
    [DuongDan] nvarchar(400) NOT NULL,
    [ThuTu] int NOT NULL,
    [MauSacId] int NULL,
    CONSTRAINT [PK_HinhAnhSanPhams] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_HinhAnhSanPhams_MauSacs_MauSacId] FOREIGN KEY ([MauSacId]) REFERENCES [MauSacs] ([Id]),
    CONSTRAINT [FK_HinhAnhSanPhams_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [YeuThichs] (
    [Id] int NOT NULL IDENTITY,
    [NguoiDungId] int NOT NULL,
    [SanPhamId] int NOT NULL,
    [NgayThem] datetime2 NOT NULL,
    CONSTRAINT [PK_YeuThichs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_YeuThichs_NguoiDungs_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDungs] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_YeuThichs_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [DanhGias] (
    [Id] int NOT NULL IDENTITY,
    [SanPhamId] int NOT NULL,
    [NguoiDungId] int NOT NULL,
    [DonHangId] int NOT NULL,
    [SoSao] int NOT NULL,
    [NoiDung] nvarchar(1000) NOT NULL,
    [NgayDanhGia] datetime2 NOT NULL,
    [PhanHoi] nvarchar(1000) NULL,
    [NgayPhanHoi] datetime2 NULL,
    [TrangThai] int NOT NULL,
    [NhanCamXuc] nvarchar(20) NULL,
    [DoTinCay] float NULL,
    [BiCanhBaoTieuCuc] bit NOT NULL,
    [LyDoTuChoi] nvarchar(500) NULL,
    [NgayDuyet] datetime2 NULL,
    CONSTRAINT [PK_DanhGias] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_DanhGias_DonHangs_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHangs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DanhGias_NguoiDungs_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDungs] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_DanhGias_SanPhams_SanPhamId] FOREIGN KEY ([SanPhamId]) REFERENCES [SanPhams] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [ChiTietDonHangs] (
    [Id] int NOT NULL IDENTITY,
    [DonHangId] int NOT NULL,
    [BienTheSanPhamId] int NULL,
    [SanPhamId] int NULL,
    [TenSanPham] nvarchar(200) NOT NULL,
    [TenMau] nvarchar(50) NOT NULL,
    [TenKichThuoc] nvarchar(20) NOT NULL,
    [HinhAnh] nvarchar(400) NULL,
    [SoLuong] int NOT NULL,
    [DonGia] decimal(18,0) NOT NULL,
    [ThanhTien] decimal(18,0) NOT NULL,
    CONSTRAINT [PK_ChiTietDonHangs] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ChiTietDonHangs_BienTheSanPhams_BienTheSanPhamId] FOREIGN KEY ([BienTheSanPhamId]) REFERENCES [BienTheSanPhams] ([Id]) ON DELETE SET NULL,
    CONSTRAINT [FK_ChiTietDonHangs_DonHangs_DonHangId] FOREIGN KEY ([DonHangId]) REFERENCES [DonHangs] ([Id]) ON DELETE CASCADE
);
GO


CREATE TABLE [GioHangItems] (
    [Id] int NOT NULL IDENTITY,
    [NguoiDungId] int NOT NULL,
    [BienTheSanPhamId] int NOT NULL,
    [SoLuong] int NOT NULL,
    [NgayThem] datetime2 NOT NULL,
    CONSTRAINT [PK_GioHangItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_GioHangItems_BienTheSanPhams_BienTheSanPhamId] FOREIGN KEY ([BienTheSanPhamId]) REFERENCES [BienTheSanPhams] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_GioHangItems_NguoiDungs_NguoiDungId] FOREIGN KEY ([NguoiDungId]) REFERENCES [NguoiDungs] ([Id]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_BienTheSanPhams_KichThuocId] ON [BienTheSanPhams] ([KichThuocId]);
GO


CREATE INDEX [IX_BienTheSanPhams_MauSacId] ON [BienTheSanPhams] ([MauSacId]);
GO


CREATE UNIQUE INDEX [IX_BienTheSanPhams_SanPhamId_MauSacId_KichThuocId] ON [BienTheSanPhams] ([SanPhamId], [MauSacId], [KichThuocId]);
GO


CREATE INDEX [IX_ChiTietDonHangs_BienTheSanPhamId] ON [ChiTietDonHangs] ([BienTheSanPhamId]);
GO


CREATE INDEX [IX_ChiTietDonHangs_DonHangId] ON [ChiTietDonHangs] ([DonHangId]);
GO


CREATE UNIQUE INDEX [IX_DanhGias_DonHangId_SanPhamId] ON [DanhGias] ([DonHangId], [SanPhamId]);
GO


CREATE INDEX [IX_DanhGias_NguoiDungId] ON [DanhGias] ([NguoiDungId]);
GO


CREATE INDEX [IX_DanhGias_SanPhamId] ON [DanhGias] ([SanPhamId]);
GO


CREATE UNIQUE INDEX [IX_DonHangs_MaDonHang] ON [DonHangs] ([MaDonHang]);
GO


CREATE INDEX [IX_DonHangs_MaGiamGiaId] ON [DonHangs] ([MaGiamGiaId]);
GO


CREATE INDEX [IX_DonHangs_NguoiDungId] ON [DonHangs] ([NguoiDungId]);
GO


CREATE INDEX [IX_GioHangItems_BienTheSanPhamId] ON [GioHangItems] ([BienTheSanPhamId]);
GO


CREATE UNIQUE INDEX [IX_GioHangItems_NguoiDungId_BienTheSanPhamId] ON [GioHangItems] ([NguoiDungId], [BienTheSanPhamId]);
GO


CREATE INDEX [IX_HinhAnhSanPhams_MauSacId] ON [HinhAnhSanPhams] ([MauSacId]);
GO


CREATE INDEX [IX_HinhAnhSanPhams_SanPhamId] ON [HinhAnhSanPhams] ([SanPhamId]);
GO


CREATE UNIQUE INDEX [IX_MaGiamGias_Ma] ON [MaGiamGias] ([Ma]);
GO


CREATE UNIQUE INDEX [IX_NguoiDungs_Email] ON [NguoiDungs] ([Email]);
GO


CREATE INDEX [IX_SanPhams_DanhMucId] ON [SanPhams] ([DanhMucId]);
GO


CREATE INDEX [IX_ThongBaos_NguoiDungId] ON [ThongBaos] ([NguoiDungId]);
GO


CREATE UNIQUE INDEX [IX_YeuThichs_NguoiDungId_SanPhamId] ON [YeuThichs] ([NguoiDungId], [SanPhamId]);
GO


CREATE INDEX [IX_YeuThichs_SanPhamId] ON [YeuThichs] ([SanPhamId]);
GO



using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMPFashionStore.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DanhMucs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDanhMuc = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhMucs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KichThuocs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKichThuoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KichThuocs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LienHes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NgayGui = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaXuLy = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LienHes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaGiamGias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Ma = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    LoaiGiamGia = table.Column<int>(type: "int", nullable: false),
                    GiaTri = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    GiaTriDonHangToiThieu = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    SoTienGiamToiDa = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    SoLuongToiDa = table.Column<int>(type: "int", nullable: true),
                    SoLuongDaDung = table.Column<int>(type: "int", nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TrangThaiHoatDong = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaGiamGias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MauSacs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenMau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MaHex = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MauSacs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HoTen = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MatKhauHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SoDienThoai = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    DiaChi = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    AnhDaiDien = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NgaySinh = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GioiTinh = table.Column<int>(type: "int", nullable: false),
                    VaiTro = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<int>(type: "int", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SoLanDangNhapSai = table.Column<int>(type: "int", nullable: false),
                    KhoaDangNhapDenLuc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NguoiDungs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenSanPham = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ThuongHieu = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DanhMucId = table.Column<int>(type: "int", nullable: false),
                    GiaGoc = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    GiaKhuyenMai = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    HinhAnhChinh = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    TrangThaiHienThi = table.Column<bool>(type: "bit", nullable: false),
                    NgayTao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SanPhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SanPhams_DanhMucs_DanhMucId",
                        column: x => x.DanhMucId,
                        principalTable: "DanhMucs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DonHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaDonHang = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    NgayDat = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenNguoiNhan = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SoDienThoaiNhan = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    DiaChiNhan = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PhuongThucThanhToan = table.Column<int>(type: "int", nullable: false),
                    TrangThaiThanhToan = table.Column<int>(type: "int", nullable: false),
                    TrangThaiDonHang = table.Column<int>(type: "int", nullable: false),
                    TamTinh = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    SoTienGiamGia = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    PhiVanChuyen = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    TongTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    MaGiamGiaId = table.Column<int>(type: "int", nullable: true),
                    LyDoHuy = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    NgayXacNhan = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayGiao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayHuy = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DonHangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DonHangs_MaGiamGias_MaGiamGiaId",
                        column: x => x.MaGiamGiaId,
                        principalTable: "MaGiamGias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_DonHangs_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BienTheSanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    MauSacId = table.Column<int>(type: "int", nullable: false),
                    KichThuocId = table.Column<int>(type: "int", nullable: false),
                    SoLuongTon = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BienTheSanPhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BienTheSanPhams_KichThuocs_KichThuocId",
                        column: x => x.KichThuocId,
                        principalTable: "KichThuocs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BienTheSanPhams_MauSacs_MauSacId",
                        column: x => x.MauSacId,
                        principalTable: "MauSacs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BienTheSanPhams_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HinhAnhSanPhams",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    DuongDan = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    ThuTu = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HinhAnhSanPhams", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HinhAnhSanPhams_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "YeuThichs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuThichs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_YeuThichs_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YeuThichs_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DanhGias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SanPhamId = table.Column<int>(type: "int", nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    DonHangId = table.Column<int>(type: "int", nullable: false),
                    SoSao = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    NgayDanhGia = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DanhGias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DanhGias_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGias_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DanhGias_SanPhams_SanPhamId",
                        column: x => x.SanPhamId,
                        principalTable: "SanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChiTietDonHangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DonHangId = table.Column<int>(type: "int", nullable: false),
                    BienTheSanPhamId = table.Column<int>(type: "int", nullable: true),
                    SanPhamId = table.Column<int>(type: "int", nullable: true),
                    TenSanPham = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TenMau = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenKichThuoc = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HinhAnh = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    DonGia = table.Column<decimal>(type: "decimal(18,0)", nullable: false),
                    ThanhTien = table.Column<decimal>(type: "decimal(18,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChiTietDonHangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_BienTheSanPhams_BienTheSanPhamId",
                        column: x => x.BienTheSanPhamId,
                        principalTable: "BienTheSanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ChiTietDonHangs_DonHangs_DonHangId",
                        column: x => x.DonHangId,
                        principalTable: "DonHangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GioHangItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    BienTheSanPhamId = table.Column<int>(type: "int", nullable: false),
                    SoLuong = table.Column<int>(type: "int", nullable: false),
                    NgayThem = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GioHangItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GioHangItems_BienTheSanPhams_BienTheSanPhamId",
                        column: x => x.BienTheSanPhamId,
                        principalTable: "BienTheSanPhams",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GioHangItems_NguoiDungs_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "NguoiDungs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BienTheSanPhams_KichThuocId",
                table: "BienTheSanPhams",
                column: "KichThuocId");

            migrationBuilder.CreateIndex(
                name: "IX_BienTheSanPhams_MauSacId",
                table: "BienTheSanPhams",
                column: "MauSacId");

            migrationBuilder.CreateIndex(
                name: "IX_BienTheSanPhams_SanPhamId_MauSacId_KichThuocId",
                table: "BienTheSanPhams",
                columns: new[] { "SanPhamId", "MauSacId", "KichThuocId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_BienTheSanPhamId",
                table: "ChiTietDonHangs",
                column: "BienTheSanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_ChiTietDonHangs_DonHangId",
                table: "ChiTietDonHangs",
                column: "DonHangId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_DonHangId_SanPhamId",
                table: "DanhGias",
                columns: new[] { "DonHangId", "SanPhamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_NguoiDungId",
                table: "DanhGias",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_DanhGias_SanPhamId",
                table: "DanhGias",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_MaDonHang",
                table: "DonHangs",
                column: "MaDonHang",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_MaGiamGiaId",
                table: "DonHangs",
                column: "MaGiamGiaId");

            migrationBuilder.CreateIndex(
                name: "IX_DonHangs_NguoiDungId",
                table: "DonHangs",
                column: "NguoiDungId");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangItems_BienTheSanPhamId",
                table: "GioHangItems",
                column: "BienTheSanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_GioHangItems_NguoiDungId_BienTheSanPhamId",
                table: "GioHangItems",
                columns: new[] { "NguoiDungId", "BienTheSanPhamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPhams_SanPhamId",
                table: "HinhAnhSanPhams",
                column: "SanPhamId");

            migrationBuilder.CreateIndex(
                name: "IX_MaGiamGias_Ma",
                table: "MaGiamGias",
                column: "Ma",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NguoiDungs_Email",
                table: "NguoiDungs",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SanPhams_DanhMucId",
                table: "SanPhams",
                column: "DanhMucId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_NguoiDungId_SanPhamId",
                table: "YeuThichs",
                columns: new[] { "NguoiDungId", "SanPhamId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_YeuThichs_SanPhamId",
                table: "YeuThichs",
                column: "SanPhamId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChiTietDonHangs");

            migrationBuilder.DropTable(
                name: "DanhGias");

            migrationBuilder.DropTable(
                name: "GioHangItems");

            migrationBuilder.DropTable(
                name: "HinhAnhSanPhams");

            migrationBuilder.DropTable(
                name: "LienHes");

            migrationBuilder.DropTable(
                name: "YeuThichs");

            migrationBuilder.DropTable(
                name: "DonHangs");

            migrationBuilder.DropTable(
                name: "BienTheSanPhams");

            migrationBuilder.DropTable(
                name: "MaGiamGias");

            migrationBuilder.DropTable(
                name: "NguoiDungs");

            migrationBuilder.DropTable(
                name: "KichThuocs");

            migrationBuilder.DropTable(
                name: "MauSacs");

            migrationBuilder.DropTable(
                name: "SanPhams");

            migrationBuilder.DropTable(
                name: "DanhMucs");
        }
    }
}

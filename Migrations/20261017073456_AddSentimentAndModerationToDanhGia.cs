using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMPFashionStore.Migrations
{
    /// <inheritdoc />
    public partial class AddSentimentAndModerationToDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "BiCanhBaoTieuCuc",
                table: "DanhGias",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "DoTinCay",
                table: "DanhGias",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LyDoTuChoi",
                table: "DanhGias",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayDuyet",
                table: "DanhGias",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NhanCamXuc",
                table: "DanhGias",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrangThai",
                table: "DanhGias",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BiCanhBaoTieuCuc",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "DoTinCay",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "LyDoTuChoi",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "NgayDuyet",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "NhanCamXuc",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "TrangThai",
                table: "DanhGias");
        }
    }
}

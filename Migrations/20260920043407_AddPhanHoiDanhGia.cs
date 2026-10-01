using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMPFashionStore.Migrations
{
    /// <inheritdoc />
    public partial class AddPhanHoiDanhGia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "NgayPhanHoi",
                table: "DanhGias",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhanHoi",
                table: "DanhGias",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NgayPhanHoi",
                table: "DanhGias");

            migrationBuilder.DropColumn(
                name: "PhanHoi",
                table: "DanhGias");
        }
    }
}

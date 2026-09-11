using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AMPFashionStore.Migrations
{
    /// <inheritdoc />
    public partial class AddAnhBienThe : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MauSacId",
                table: "HinhAnhSanPhams",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HinhAnh",
                table: "BienTheSanPhams",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HinhAnhSanPhams_MauSacId",
                table: "HinhAnhSanPhams",
                column: "MauSacId");

            migrationBuilder.AddForeignKey(
                name: "FK_HinhAnhSanPhams_MauSacs_MauSacId",
                table: "HinhAnhSanPhams",
                column: "MauSacId",
                principalTable: "MauSacs",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HinhAnhSanPhams_MauSacs_MauSacId",
                table: "HinhAnhSanPhams");

            migrationBuilder.DropIndex(
                name: "IX_HinhAnhSanPhams_MauSacId",
                table: "HinhAnhSanPhams");

            migrationBuilder.DropColumn(
                name: "MauSacId",
                table: "HinhAnhSanPhams");

            migrationBuilder.DropColumn(
                name: "HinhAnh",
                table: "BienTheSanPhams");
        }
    }
}

using AMPFashionStore.Models;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<NguoiDung> NguoiDungs => Set<NguoiDung>();
        public DbSet<DanhMuc> DanhMucs => Set<DanhMuc>();
        public DbSet<SanPham> SanPhams => Set<SanPham>();
        public DbSet<HinhAnhSanPham> HinhAnhSanPhams => Set<HinhAnhSanPham>();
        public DbSet<MauSac> MauSacs => Set<MauSac>();
        public DbSet<KichThuoc> KichThuocs => Set<KichThuoc>();
        public DbSet<BienTheSanPham> BienTheSanPhams => Set<BienTheSanPham>();
        public DbSet<GioHangItem> GioHangItems => Set<GioHangItem>();
        public DbSet<DonHang> DonHangs => Set<DonHang>();
        public DbSet<ChiTietDonHang> ChiTietDonHangs => Set<ChiTietDonHang>();
        public DbSet<MaGiamGia> MaGiamGias => Set<MaGiamGia>();
        public DbSet<DanhGia> DanhGias => Set<DanhGia>();
        public DbSet<YeuThich> YeuThichs => Set<YeuThich>();
        public DbSet<LienHe> LienHes => Set<LienHe>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ===== NguoiDung =====
            modelBuilder.Entity<NguoiDung>(e =>
            {
                e.HasIndex(x => x.Email).IsUnique();
            });

            // ===== SanPham / DanhMuc =====
            modelBuilder.Entity<DanhMuc>()
                .HasMany(d => d.SanPhams)
                .WithOne(s => s.DanhMuc)
                .HasForeignKey(s => s.DanhMucId)
                .OnDelete(DeleteBehavior.Restrict); // Không cho xóa danh mục còn sản phẩm

            modelBuilder.Entity<SanPham>()
                .HasMany(s => s.HinhAnhs)
                .WithOne(h => h.SanPham)
                .HasForeignKey(h => h.SanPhamId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SanPham>()
                .HasMany(s => s.BienThes)
                .WithOne(b => b.SanPham)
                .HasForeignKey(b => b.SanPhamId)
                .OnDelete(DeleteBehavior.Cascade);

            // ===== BienTheSanPham (SanPham x MauSac x KichThuoc) =====
            modelBuilder.Entity<BienTheSanPham>(e =>
            {
                e.HasOne(b => b.MauSac)
                 .WithMany(m => m.BienThes)
                 .HasForeignKey(b => b.MauSacId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(b => b.KichThuoc)
                 .WithMany(k => k.BienThes)
                 .HasForeignKey(b => b.KichThuocId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasIndex(b => new { b.SanPhamId, b.MauSacId, b.KichThuocId }).IsUnique();
            });

            // ===== GioHangItem =====
            modelBuilder.Entity<GioHangItem>(e =>
            {
                e.HasOne(g => g.NguoiDung)
                 .WithMany(n => n.GioHangItems)
                 .HasForeignKey(g => g.NguoiDungId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(g => g.BienTheSanPham)
                 .WithMany()
                 .HasForeignKey(g => g.BienTheSanPhamId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(g => new { g.NguoiDungId, g.BienTheSanPhamId }).IsUnique();
            });

            // ===== DonHang =====
            modelBuilder.Entity<DonHang>(e =>
            {
                e.HasIndex(d => d.MaDonHang).IsUnique();

                e.HasOne(d => d.NguoiDung)
                 .WithMany(n => n.DonHangs)
                 .HasForeignKey(d => d.NguoiDungId)
                 .OnDelete(DeleteBehavior.Restrict); // Giữ lịch sử đơn hàng, không xóa theo tài khoản

                e.HasOne(d => d.MaGiamGiaApDung)
                 .WithMany(m => m.DonHangs)
                 .HasForeignKey(d => d.MaGiamGiaId)
                 .OnDelete(DeleteBehavior.SetNull);

                e.HasMany(d => d.ChiTietDonHangs)
                 .WithOne(c => c.DonHang)
                 .HasForeignKey(c => c.DonHangId)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChiTietDonHang>()
                .HasOne(c => c.BienTheSanPham)
                .WithMany()
                .HasForeignKey(c => c.BienTheSanPhamId)
                .OnDelete(DeleteBehavior.SetNull); // Vẫn giữ chi tiết đơn hàng (bản snapshot) nếu biến thể gốc bị xóa

            // ===== MaGiamGia =====
            modelBuilder.Entity<MaGiamGia>()
                .HasIndex(m => m.Ma).IsUnique();

            // ===== DanhGia =====
            modelBuilder.Entity<DanhGia>(e =>
            {
                e.HasOne(d => d.SanPham)
                 .WithMany(s => s.DanhGias)
                 .HasForeignKey(d => d.SanPhamId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(d => d.NguoiDung)
                 .WithMany(n => n.DanhGias)
                 .HasForeignKey(d => d.NguoiDungId)
                 .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(d => d.DonHang)
                 .WithMany()
                 .HasForeignKey(d => d.DonHangId)
                 .OnDelete(DeleteBehavior.Restrict);

                // Mỗi đơn hàng chỉ đánh giá 1 lần cho mỗi sản phẩm
                e.HasIndex(d => new { d.DonHangId, d.SanPhamId }).IsUnique();
            });

            // ===== YeuThich =====
            modelBuilder.Entity<YeuThich>(e =>
            {
                e.HasOne(y => y.NguoiDung)
                 .WithMany(n => n.YeuThichs)
                 .HasForeignKey(y => y.NguoiDungId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(y => y.SanPham)
                 .WithMany(s => s.YeuThichs)
                 .HasForeignKey(y => y.SanPhamId)
                 .OnDelete(DeleteBehavior.Cascade);

                e.HasIndex(y => new { y.NguoiDungId, y.SanPhamId }).IsUnique();
            });
        }
    }
}

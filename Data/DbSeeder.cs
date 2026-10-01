using AMPFashionStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Data
{
    /// <summary>
    /// Khởi tạo dữ liệu mẫu để chạy thử hệ thống ngay sau khi tạo database.
    /// Gọi DbSeeder.SeedAsync(app) một lần trong Program.cs.
    /// </summary>
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();

            var hasher = new PasswordHasher<NguoiDung>();

            // ---------- Tài khoản mẫu ----------
            if (!await db.NguoiDungs.AnyAsync())
            {
                var admin = new NguoiDung
                {
                    HoTen = "Quản trị viên",
                    Email = "admin@ampfashion.vn",
                    SoDienThoai = "0900000001",
                    VaiTro = VaiTro.QuanTriVien,
                    TrangThai = TrangThaiTaiKhoan.HoatDong
                };
                admin.MatKhauHash = hasher.HashPassword(admin, "Admin@123");

                var nhanVien = new NguoiDung
                {
                    HoTen = "Nhân viên Cửa hàng",
                    Email = "nhanvien@ampfashion.vn",
                    SoDienThoai = "0900000002",
                    VaiTro = VaiTro.NhanVien,
                    TrangThai = TrangThaiTaiKhoan.HoatDong
                };
                nhanVien.MatKhauHash = hasher.HashPassword(nhanVien, "NhanVien@123");

                var khachHang = new NguoiDung
                {
                    HoTen = "Nguyễn Văn Khách",
                    Email = "khachhang@gmail.com",
                    SoDienThoai = "0900000003",
                    DiaChi = "123 Đường Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    VaiTro = VaiTro.KhachHang,
                    TrangThai = TrangThaiTaiKhoan.HoatDong
                };
                khachHang.MatKhauHash = hasher.HashPassword(khachHang, "KhachHang@123");

                db.NguoiDungs.AddRange(admin, nhanVien, khachHang);
                await db.SaveChangesAsync();
            }

            if (await db.DanhMucs.AnyAsync())
            {
                await CapNhatAnhThoiTrangThatAsync(db);
                await SeedThemDuLieuFigmaAsync(db, hasher);
                return;
            }

            // ---------- Danh mục ----------
            var aoNam = new DanhMuc { TenDanhMuc = "Áo Nam", MoTa = "Áo sơ mi, áo thun, áo khoác nam" };
            var aoNu = new DanhMuc { TenDanhMuc = "Áo Nữ", MoTa = "Áo kiểu, blazer, croptop nữ" };
            var quan = new DanhMuc { TenDanhMuc = "Quần", MoTa = "Quần jeans, quần tây, quần jogger" };
            var vayDam = new DanhMuc { TenDanhMuc = "Váy - Đầm", MoTa = "Chân váy và đầm các loại" };
            var phuKien = new DanhMuc { TenDanhMuc = "Phụ Kiện", MoTa = "Thắt lưng, túi xách, mũ nón" };
            db.DanhMucs.AddRange(aoNam, aoNu, quan, vayDam, phuKien);
            await db.SaveChangesAsync();

            // ---------- Màu sắc ----------
            var mauDen = new MauSac { TenMau = "Đen", MaHex = "#1A1A1A" };
            var mauTrang = new MauSac { TenMau = "Trắng", MaHex = "#FFFFFF" };
            var mauXam = new MauSac { TenMau = "Xám", MaHex = "#9A9A9A" };
            var mauBe = new MauSac { TenMau = "Be", MaHex = "#D9C7A3" };
            var mauNavy = new MauSac { TenMau = "Xanh Navy", MaHex = "#1F2A44" };
            var mauDoDo = new MauSac { TenMau = "Đỏ Đô", MaHex = "#6B2337" };
            db.MauSacs.AddRange(mauDen, mauTrang, mauXam, mauBe, mauNavy, mauDoDo);

            // ---------- Kích thước ----------
            var sizeS = new KichThuoc { TenKichThuoc = "S", ThuTu = 1 };
            var sizeM = new KichThuoc { TenKichThuoc = "M", ThuTu = 2 };
            var sizeL = new KichThuoc { TenKichThuoc = "L", ThuTu = 3 };
            var sizeXL = new KichThuoc { TenKichThuoc = "XL", ThuTu = 4 };
            db.KichThuocs.AddRange(sizeS, sizeM, sizeL, sizeXL);
            await db.SaveChangesAsync();

            // ---------- Sản phẩm mẫu ----------
            var sanPhams = new List<SanPham>
            {
                Tao("Áo Sơ Mi Trắng Basic", aoNam.Id, "AMP Basics", 359000, null,
                    "Áo sơ mi trắng form regular, chất liệu cotton thoáng mát, dễ phối đồ công sở lẫn dạo phố.", "aomi-trang-1"),
                Tao("Áo Thun Cotton Đen", aoNam.Id, "AMP Basics", 199000, 159000,
                    "Áo thun cotton 100% form unisex, in logo tối giản, mềm mại và thấm hút mồ hôi tốt.", "aothun-den-1"),
                Tao("Áo Khoác Denim Nam", aoNam.Id, "AMP Denim", 549000, null,
                    "Áo khoác denim rửa nhẹ, form rộng rãi, cá tính, mặc được 3 mùa.", "khoac-denim-1"),
                Tao("Áo Blazer Nữ Công Sở", aoNu.Id, "AMP Office", 689000, 549000,
                    "Blazer form vừa, vải dày dặn không nhăn, tôn dáng, phù hợp môi trường công sở.", "blazer-nu-1"),
                Tao("Áo Kiểu Nữ Tay Bồng", aoNu.Id, "AMP Feminine", 299000, null,
                    "Áo kiểu tay bồng nữ tính, chất liệu voan lụa nhẹ nhàng, thoáng mát.", "aokieu-nu-1"),
                Tao("Áo Croptop Nữ", aoNu.Id, "AMP Casual", 179000, null,
                    "Croptop form ôm nhẹ, năng động, dễ phối cùng quần cạp cao.", "croptop-nu-1"),
                Tao("Quần Jeans Slim Fit", quan.Id, "AMP Denim", 459000, 399000,
                    "Quần jeans form slim fit, vải denim co giãn nhẹ, tôn dáng và thoải mái vận động.", "jeans-slim-1"),
                Tao("Quần Tây Ống Suông", quan.Id, "AMP Office", 429000, null,
                    "Quần tây ống suông thanh lịch, phù hợp đi làm, đi học.", "quantay-1"),
                Tao("Quần Jogger Thể Thao", quan.Id, "AMP Active", 299000, null,
                    "Quần jogger nỉ bo gấu, form gọn gàng, phù hợp tập luyện và mặc hàng ngày.", "jogger-1"),
                Tao("Đầm Maxi Hoa Nhí", vayDam.Id, "AMP Feminine", 459000, 379000,
                    "Đầm maxi họa tiết hoa nhí, chất liệu voan 2 lớp, bay bổng và nữ tính.", "daymaxi-1"),
                Tao("Chân Váy Xếp Ly", vayDam.Id, "AMP Office", 259000, null,
                    "Chân váy xếp ly dáng chữ A, dễ phối cùng áo sơ mi hoặc áo kiểu.", "chanvay-1"),
                Tao("Đầm Body Dự Tiệc", vayDam.Id, "AMP Evening", 599000, null,
                    "Đầm body ôm dáng, chất liệu co giãn 4 chiều, sang trọng cho các buổi tiệc.", "daymtiec-1"),
                Tao("Thắt Lưng Da Nam", phuKien.Id, "AMP Leather", 249000, null,
                    "Thắt lưng da thật, khóa kim loại chống gỉ, bảo hành 12 tháng.", "thatlung-1"),
                Tao("Túi Xách Tote Nữ", phuKien.Id, "AMP Leather", 399000, 329000,
                    "Túi tote da PU cao cấp, sức chứa lớn, phù hợp đi làm và đi học.", "tui-tote-1"),
                Tao("Mũ Lưỡi Trai", phuKien.Id, "AMP Casual", 129000, null,
                    "Mũ lưỡi trai form basic, điều chỉnh size dễ dàng, nhiều màu lựa chọn.", "muluoitrai-1"),
            };

            db.SanPhams.AddRange(sanPhams);
            await db.SaveChangesAsync();

            var mauSacs = new[] { mauDen, mauTrang, mauXam, mauBe, mauNavy, mauDoDo };
            var rnd = new Random(2026);
            foreach (var sp in sanPhams)
            {
                // Mỗi sản phẩm có 2-3 màu ngẫu nhiên (ổn định theo seed) x 4 size
                var soMau = 2 + (sp.Id % 2);
                var mauChon = mauSacs.OrderBy(_ => rnd.Next()).Take((int)soMau).ToList();
                foreach (var mau in mauChon)
                {
                    foreach (var size in new[] { sizeS, sizeM, sizeL, sizeXL })
                    {
                        db.BienTheSanPhams.Add(new BienTheSanPham
                        {
                            SanPhamId = sp.Id,
                            MauSacId = mau.Id,
                            KichThuocId = size.Id,
                            // Một vài biến thể cố tình hết hàng để demo logic vô hiệu hóa (UC07/UC08)
                            SoLuongTon = (sp.Id + mau.Id + size.Id) % 7 == 0 ? 0 : rnd.Next(3, 40)
                        });
                    }
                }
            }
            await db.SaveChangesAsync();

            // ---------- Mã giảm giá ----------
            db.MaGiamGias.AddRange(
                new MaGiamGia
                {
                    Ma = "WELCOME10",
                    MoTa = "Giảm 10% cho đơn hàng đầu tiên",
                    LoaiGiamGia = LoaiGiamGia.PhanTram,
                    GiaTri = 10,
                    GiaTriDonHangToiThieu = 200000,
                    SoTienGiamToiDa = 100000,
                    SoLuongToiDa = 500,
                    NgayBatDau = DateTime.Today.AddDays(-30),
                    NgayKetThuc = DateTime.Today.AddMonths(6),
                    TrangThaiHoatDong = true
                },
                new MaGiamGia
                {
                    Ma = "FREESHIP",
                    MoTa = "Miễn phí vận chuyển cho đơn từ 300.000đ",
                    LoaiGiamGia = LoaiGiamGia.SoTienCoDinh,
                    GiaTri = 30000,
                    GiaTriDonHangToiThieu = 300000,
                    SoLuongToiDa = null,
                    NgayBatDau = DateTime.Today.AddDays(-30),
                    NgayKetThuc = DateTime.Today.AddMonths(6),
                    TrangThaiHoatDong = true
                },
                new MaGiamGia
                {
                    Ma = "SALE50K",
                    MoTa = "Giảm ngay 50.000đ cho đơn từ 500.000đ",
                    LoaiGiamGia = LoaiGiamGia.SoTienCoDinh,
                    GiaTri = 50000,
                    GiaTriDonHangToiThieu = 500000,
                    SoLuongToiDa = 200,
                    NgayBatDau = DateTime.Today.AddDays(-10),
                    NgayKetThuc = DateTime.Today.AddMonths(3),
                    TrangThaiHoatDong = true
                }
            );

            await db.SaveChangesAsync();
            await CapNhatAnhThoiTrangThatAsync(db);
        }

        private static SanPham Tao(string ten, int danhMucId, string thuongHieu, decimal giaGoc, decimal? giaKhuyenMai, string moTa, string anhSeed)
        {
            var anhChinh = BoAnhThoiTrang.TryGetValue(ten, out var data) 
                ? data.AnhChinh 
                : $"https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?q=80&w=900&auto=format&fit=crop";

            return new SanPham
            {
                TenSanPham = ten,
                DanhMucId = danhMucId,
                ThuongHieu = thuongHieu,
                GiaGoc = giaGoc,
                GiaKhuyenMai = giaKhuyenMai,
                MoTa = moTa,
                HinhAnhChinh = anhChinh,
                TrangThaiHienThi = true,
                NgayTao = DateTime.Now
            };
        }

        private static async Task CapNhatAnhThoiTrangThatAsync(ApplicationDbContext db)
        {
            var sanPhams = await db.SanPhams
                .Include(s => s.BienThes).ThenInclude(b => b.MauSac)
                .Include(s => s.HinhAnhs)
                .ToListAsync();

            var tatCaMau = await db.MauSacs.ToListAsync();

            foreach (var sp in sanPhams)
            {
                if (BoAnhThoiTrang.TryGetValue(sp.TenSanPham, out var data))
                {
                    sp.HinhAnhChinh = data.AnhChinh;

                    // Cập nhật ảnh cho từng biến thể màu
                    foreach (var bt in sp.BienThes)
                    {
                        var tenMau = bt.MauSac?.TenMau ?? "";
                        if (data.AnhMau.TryGetValue(tenMau, out var anhMau))
                        {
                            bt.HinhAnh = anhMau;
                        }
                        else
                        {
                            bt.HinhAnh = data.AnhChinh;
                        }
                    }

                    // Đồng bộ HinhAnhs của sản phẩm theo từng màu
                    foreach (var kv in data.AnhMau)
                    {
                        var mau = tatCaMau.FirstOrDefault(m => m.TenMau == kv.Key);
                        if (mau != null && !sp.HinhAnhs.Any(h => h.MauSacId == mau.Id))
                        {
                            sp.HinhAnhs.Add(new HinhAnhSanPham
                            {
                                SanPhamId = sp.Id,
                                DuongDan = kv.Value,
                                MauSacId = mau.Id,
                                ThuTu = mau.Id
                            });
                        }
                    }
                }
            }

            await db.SaveChangesAsync();
        }

        private static readonly Dictionary<string, (string AnhChinh, Dictionary<string, string> AnhMau)> BoAnhThoiTrang = new()
        {
            ["Áo Sơ Mi Trắng Basic"] = (
                "https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Trắng"] = "https://images.unsplash.com/photo-1602810318383-e386cc2a3ccf?q=80&w=900&auto=format&fit=crop",
                    ["Đen"] = "https://images.unsplash.com/photo-1603252109303-2751441dd157?q=80&w=900&auto=format&fit=crop",
                    ["Xanh Navy"] = "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1589310243389-96a5483213a8?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1598033129183-c4f50c736f10?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Áo Thun Cotton Đen"] = (
                "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1521572267360-ee0c2909d518?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1581655353564-df123a1eb820?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1618354691373-d851c5c3a990?q=80&w=900&auto=format&fit=crop",
                    ["Xanh Navy"] = "https://images.unsplash.com/photo-1576566588028-4147f3842f27?q=80&w=900&auto=format&fit=crop",
                    ["Đỏ Đô"] = "https://images.unsplash.com/photo-1503342217505-b0a15ec3261c?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Áo Khoác Denim Nam"] = (
                "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Xanh Navy"] = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?q=80&w=900&auto=format&fit=crop",
                    ["Đen"] = "https://images.unsplash.com/photo-1551028719-00167b16eac5?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1548883354-7622d03aca27?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1520975954732-35dd22299614?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Áo Blazer Nữ Công Sở"] = (
                "https://images.unsplash.com/photo-1584273143981-41c073dfe8f8?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1584273143981-41c073dfe8f8?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1591047139829-d91aecb6caea?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1539109136881-3be0616acf4b?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1594633312681-425c7b97ccd1?q=80&w=900&auto=format&fit=crop",
                    ["Đỏ Đô"] = "https://images.unsplash.com/photo-1550614000-4895a10e1bfd?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Áo Kiểu Nữ Tay Bồng"] = (
                "https://images.unsplash.com/photo-1564257631407-4deb1f99d992?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Trắng"] = "https://images.unsplash.com/photo-1564257631407-4deb1f99d992?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1551803091-e20673f15770?q=80&w=900&auto=format&fit=crop",
                    ["Đen"] = "https://images.unsplash.com/photo-1509631179647-0177331693ae?q=80&w=900&auto=format&fit=crop",
                    ["Đỏ Đô"] = "https://images.unsplash.com/photo-1496747611176-843222e1e57c?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Áo Croptop Nữ"] = (
                "https://images.unsplash.com/photo-1503342217505-b0a15ec3261c?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Trắng"] = "https://images.unsplash.com/photo-1503342217505-b0a15ec3261c?q=80&w=900&auto=format&fit=crop",
                    ["Đen"] = "https://images.unsplash.com/photo-1529139574466-a303027c1d8b?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1508427953056-b00b8d78ebf5?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Quần Jeans Slim Fit"] = (
                "https://images.unsplash.com/photo-1542272604-780c96856592?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Xanh Navy"] = "https://images.unsplash.com/photo-1542272604-780c96856592?q=80&w=900&auto=format&fit=crop",
                    ["Đen"] = "https://images.unsplash.com/photo-1541099649105-f69ad21f3246?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1624378439575-d8705ad7ae80?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Quần Tây Ống Suông"] = (
                "https://images.unsplash.com/photo-1594633312681-425c7b97ccd1?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1594633312681-425c7b97ccd1?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1506630448388-4e683c67ddb0?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1552374196-1ab2a1c593e8?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Quần Jogger Thể Thao"] = (
                "https://images.unsplash.com/photo-1552902865-b72c031ac5ea?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Xám"] = "https://images.unsplash.com/photo-1552902865-b72c031ac5ea?q=80&w=900&auto=format&fit=crop",
                    ["Đen"] = "https://images.unsplash.com/photo-1517445312882-bc9910d016b7?q=80&w=900&auto=format&fit=crop",
                    ["Xanh Navy"] = "https://images.unsplash.com/photo-1584865288642-42078afe6942?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Đầm Maxi Hoa Nhí"] = (
                "https://images.unsplash.com/photo-1496747611176-843222e1e57c?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đỏ Đô"] = "https://images.unsplash.com/photo-1496747611176-843222e1e57c?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1515372039744-b8f02a3ae446?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Chân Váy Xếp Ly"] = (
                "https://images.unsplash.com/photo-1583496661160-fb5886a0aaaa?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1583496661160-fb5886a0aaaa?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1509551388413-e18d0ac5d495?q=80&w=900&auto=format&fit=crop",
                    ["Xám"] = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Đầm Body Dự Tiệc"] = (
                "https://images.unsplash.com/photo-1566174053879-31528523f8ae?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1566174053879-31528523f8ae?q=80&w=900&auto=format&fit=crop",
                    ["Đỏ Đô"] = "https://images.unsplash.com/photo-1539109136881-3be0616acf4b?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1518895949257-7621c3c786d7?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1572804013309-59a88b7e92f1?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Thắt Lưng Da Nam"] = (
                "https://images.unsplash.com/photo-1624222247344-550fb60583dc?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1624222247344-550fb60583dc?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1553062407-98eeb64c6a62?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Túi Xách Tote Nữ"] = (
                "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?q=80&w=900&auto=format&fit=crop",
                    ["Be"] = "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1591561954557-26941169b49e?q=80&w=900&auto=format&fit=crop"
                }
            ),
            ["Mũ Lưỡi Trai"] = (
                "https://images.unsplash.com/photo-1588850561407-ed78c282e89b?q=80&w=900&auto=format&fit=crop",
                new Dictionary<string, string>
                {
                    ["Đen"] = "https://images.unsplash.com/photo-1588850561407-ed78c282e89b?q=80&w=900&auto=format&fit=crop",
                    ["Trắng"] = "https://images.unsplash.com/photo-1575428652377-a2d80e2277fc?q=80&w=900&auto=format&fit=crop",
                    ["Xanh Navy"] = "https://images.unsplash.com/photo-1521369909029-2afed882baee?q=80&w=900&auto=format&fit=crop"
                }
            )
        };

        private static async Task SeedThemDuLieuFigmaAsync(ApplicationDbContext db, PasswordHasher<NguoiDung> hasher)
        {
            // Seed thêm nhân viên chuẩn Figma nếu chưa có
            if (!await db.NguoiDungs.AnyAsync(n => n.Email == "minhthuc@ampfashion.vn"))
            {
                var staffList = new List<NguoiDung>
                {
                    new NguoiDung { HoTen = "Lê Minh Thức", Email = "minhthuc@ampfashion.vn", SoDienThoai = "0901234567", VaiTro = VaiTro.QuanTriVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-120) },
                    new NguoiDung { HoTen = "Nguyễn Ngọc Yến Nhi", Email = "yennhi@ampfashion.vn", SoDienThoai = "0902345678", VaiTro = VaiTro.QuanTriVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-100) },
                    new NguoiDung { HoTen = "Tô Ngọc Châu", Email = "ngocchau@ampfashion.vn", SoDienThoai = "0903456789", VaiTro = VaiTro.NhanVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-90) },
                    new NguoiDung { HoTen = "Hứa Châu Kha", Email = "chaukha@ampfashion.vn", SoDienThoai = "0904567890", VaiTro = VaiTro.NhanVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-80) },
                    new NguoiDung { HoTen = "Đào Thanh Hòa", Email = "thanhhoa@ampfashion.vn", SoDienThoai = "0905678901", VaiTro = VaiTro.QuanTriVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-75) },
                    new NguoiDung { HoTen = "Châu Ngọc Ẩn", Email = "ngocan@ampfashion.vn", SoDienThoai = "0906789012", VaiTro = VaiTro.NhanVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-60) },
                    new NguoiDung { HoTen = "Trần Ngọc Nam Chi", Email = "namchi@ampfashion.vn", SoDienThoai = "0907890123", VaiTro = VaiTro.NhanVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-50) },
                    new NguoiDung { HoTen = "Trần Trầm Châu", Email = "tramchau@ampfashion.vn", SoDienThoai = "0908901234", VaiTro = VaiTro.NhanVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-40) },
                    new NguoiDung { HoTen = "Thới Yến", Email = "thoiyen@ampfashion.vn", SoDienThoai = "0909012345", VaiTro = VaiTro.NhanVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-30) },
                    new NguoiDung { HoTen = "Nguyễn Tùng Hoàng", Email = "tunghoang@ampfashion.vn", SoDienThoai = "0909123456", VaiTro = VaiTro.NhanVien, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-20) }
                };

                foreach (var s in staffList)
                {
                    s.MatKhauHash = hasher.HashPassword(s, "123456");
                    db.NguoiDungs.Add(s);
                }
                await db.SaveChangesAsync();
            }

            // Seed thêm khách hàng chuẩn Figma nếu chưa có
            if (!await db.NguoiDungs.AnyAsync(n => n.Email == "xuantruong@gmail.com"))
            {
                var customers = new List<NguoiDung>
                {
                    new NguoiDung { HoTen = "Võ Xuân Trường", Email = "xuantruong@gmail.com", SoDienThoai = "0981112233", DiaChi = "45 Lê Duẩn, Quận 1, TP.HCM", VaiTro = VaiTro.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-110) },
                    new NguoiDung { HoTen = "Lê Phương Thảo", Email = "phuongthao@gmail.com", SoDienThoai = "0982223344", DiaChi = "12 Trần Hưng Đạo, Quận 5, TP.HCM", VaiTro = VaiTro.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-85) },
                    new NguoiDung { HoTen = "Nguyễn Ngọc Yến Nhi", Email = "yennhi_customer@gmail.com", SoDienThoai = "0983334455", DiaChi = "88 Hai Bà Trưng, Quận 3, TP.HCM", VaiTro = VaiTro.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-70) },
                    new NguoiDung { HoTen = "Trần Hoài Nam", Email = "hoainam@gmail.com", SoDienThoai = "0984445566", DiaChi = "99 Nguyễn Oanh, Gò Vấp, TP.HCM", VaiTro = VaiTro.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-65) },
                    new NguoiDung { HoTen = "Lê Khải Phong", Email = "khaiphong@gmail.com", SoDienThoai = "0985556677", DiaChi = "15 Nguyễn Trãi, Quận 1, TP.HCM", VaiTro = VaiTro.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-55) },
                    new NguoiDung { HoTen = "Châu Ngọc Ẩn", Email = "ngocan_customer@gmail.com", SoDienThoai = "0986667788", DiaChi = "33 Cách Mạng Tháng 8, Quận 10, TP.HCM", VaiTro = VaiTro.KhachHang, TrangThai = TrangThaiTaiKhoan.HoatDong, NgayTao = DateTime.Today.AddDays(-45) }
                };

                foreach (var c in customers)
                {
                    c.MatKhauHash = hasher.HashPassword(c, "123456");
                    db.NguoiDungs.Add(c);
                }
                await db.SaveChangesAsync();
            }

            // Seed đơn hàng mẫu nếu chưa có
            if (!await db.DonHangs.AnyAsync())
            {
                var customers = await db.NguoiDungs.Where(n => n.VaiTro == VaiTro.KhachHang).ToListAsync();
                var products = await db.SanPhams.Include(s => s.BienThes).ToListAsync();

                if (customers.Any() && products.Any())
                {
                    var c1 = customers[0];
                    var p1 = products[0];
                    var p2 = products.Count > 1 ? products[1] : products[0];

                    var orders = new List<DonHang>
                    {
                        new DonHang
                        {
                            MaDonHang = "AMP260912",
                            NguoiDungId = c1.Id,
                            TenNguoiNhan = c1.HoTen,
                            SoDienThoaiNhan = c1.SoDienThoai ?? "0981112233",
                            DiaChiNhan = c1.DiaChi ?? "TP. Hồ Chí Minh",
                            PhuongThucThanhToan = PhuongThucThanhToan.ThanhToanKhiNhanHang,
                            TrangThaiThanhToan = TrangThaiThanhToan.DaThanhToan,
                            TrangThaiDonHang = TrangThaiDonHang.DaGiao,
                            NgayDat = DateTime.Today.AddDays(-8),
                            TongTien = 2480000,
                            ChiTietDonHangs = new List<ChiTietDonHang>
                            {
                                new ChiTietDonHang { SanPhamId = p1.Id, TenSanPham = p1.TenSanPham, DonGia = p1.GiaHienThi, SoLuong = 2, ThanhTien = p1.GiaHienThi * 2, HinhAnh = p1.HinhAnhChinh }
                            }
                        },
                        new DonHang
                        {
                            MaDonHang = "AMP260913",
                            NguoiDungId = customers.Count > 1 ? customers[1].Id : c1.Id,
                            TenNguoiNhan = customers.Count > 1 ? customers[1].HoTen : "Nguyễn Ngọc Yến Nhi",
                            SoDienThoaiNhan = "0983334455",
                            DiaChiNhan = "Quận 3, TP.HCM",
                            PhuongThucThanhToan = PhuongThucThanhToan.ChuyenKhoanNganHang,
                            TrangThaiThanhToan = TrangThaiThanhToan.ChuaThanhToan,
                            TrangThaiDonHang = TrangThaiDonHang.ChoXacNhan,
                            NgayDat = DateTime.Today.AddDays(-2),
                            TongTien = 2980000,
                            ChiTietDonHangs = new List<ChiTietDonHang>
                            {
                                new ChiTietDonHang { SanPhamId = p2.Id, TenSanPham = p2.TenSanPham, DonGia = p2.GiaHienThi, SoLuong = 2, ThanhTien = p2.GiaHienThi * 2, HinhAnh = p2.HinhAnhChinh }
                            }
                        },
                        new DonHang
                        {
                            MaDonHang = "AMP260914",
                            NguoiDungId = customers.Count > 2 ? customers[2].Id : c1.Id,
                            TenNguoiNhan = customers.Count > 2 ? customers[2].HoTen : "Lê Phương Thảo",
                            SoDienThoaiNhan = "0982223344",
                            DiaChiNhan = "Quận 1, TP.HCM",
                            PhuongThucThanhToan = PhuongThucThanhToan.ThanhToanKhiNhanHang,
                            TrangThaiThanhToan = TrangThaiThanhToan.ChuaThanhToan,
                            TrangThaiDonHang = TrangThaiDonHang.DaXacNhan,
                            NgayDat = DateTime.Today.AddDays(-3),
                            TongTien = 2580000,
                            ChiTietDonHangs = new List<ChiTietDonHang>
                            {
                                new ChiTietDonHang { SanPhamId = p1.Id, TenSanPham = p1.TenSanPham, DonGia = p1.GiaHienThi, SoLuong = 1, ThanhTien = p1.GiaHienThi, HinhAnh = p1.HinhAnhChinh }
                            }
                        },
                        new DonHang
                        {
                            MaDonHang = "AMP260915",
                            NguoiDungId = c1.Id,
                            TenNguoiNhan = "Hồ Đăng Khoa",
                            SoDienThoaiNhan = "0987778899",
                            DiaChiNhan = "Bình Thạnh, TP.HCM",
                            PhuongThucThanhToan = PhuongThucThanhToan.ThanhToanKhiNhanHang,
                            TrangThaiThanhToan = TrangThaiThanhToan.DaThanhToan,
                            TrangThaiDonHang = TrangThaiDonHang.DaGiao,
                            NgayDat = DateTime.Today.AddDays(-1),
                            TongTien = 2680000,
                            ChiTietDonHangs = new List<ChiTietDonHang>
                            {
                                new ChiTietDonHang { SanPhamId = p2.Id, TenSanPham = p2.TenSanPham, DonGia = p2.GiaHienThi, SoLuong = 1, ThanhTien = p2.GiaHienThi, HinhAnh = p2.HinhAnhChinh }
                            }
                        },
                        new DonHang
                        {
                            MaDonHang = "AMP260916",
                            NguoiDungId = c1.Id,
                            TenNguoiNhan = "Võ Xuân Trường",
                            SoDienThoaiNhan = "0981112233",
                            DiaChiNhan = "Quận 1, TP.HCM",
                            PhuongThucThanhToan = PhuongThucThanhToan.ChuyenKhoanNganHang,
                            TrangThaiThanhToan = TrangThaiThanhToan.ChuaThanhToan,
                            TrangThaiDonHang = TrangThaiDonHang.DangGiao,
                            NgayDat = DateTime.Today.AddDays(-1),
                            TongTien = 2490000,
                            ChiTietDonHangs = new List<ChiTietDonHang>
                            {
                                new ChiTietDonHang { SanPhamId = p1.Id, TenSanPham = p1.TenSanPham, DonGia = p1.GiaHienThi, SoLuong = 1, ThanhTien = p1.GiaHienThi, HinhAnh = p1.HinhAnhChinh }
                            }
                        }
                    };

                    db.DonHangs.AddRange(orders);
                    await db.SaveChangesAsync();
                }
            }

            // Seed đánh giá mẫu theo chuẩn Figma nếu chưa có
            if (await db.DanhGias.CountAsync() < 4)
            {
                var deliveredOrder = await db.DonHangs.FirstOrDefaultAsync(d => d.TrangThaiDonHang == TrangThaiDonHang.DaGiao);
                var user = await db.NguoiDungs.FirstOrDefaultAsync(u => u.VaiTro == VaiTro.KhachHang);
                var spList = await db.SanPhams.Take(5).ToListAsync();

                if (deliveredOrder != null && user != null && spList.Any())
                {
                    var reviews = new List<DanhGia>
                    {
                        new DanhGia
                        {
                            DonHangId = deliveredOrder.Id,
                            NguoiDungId = user.Id,
                            SanPhamId = spList[0].Id,
                            SoSao = 5,
                            NoiDung = "Form áo đẹp, đường may rất chỉn chu, chất liệu vải cao cấp mặc lên rất tôn dáng!",
                            NgayDanhGia = DateTime.Today.AddDays(-2),
                            PhanHoi = "Dạ AMP xin chân thành cảm ơn bạn đã yêu thích và tin tưởng sản phẩm của thương hiệu ạ!"
                        },
                        new DanhGia
                        {
                            DonHangId = deliveredOrder.Id,
                            NguoiDungId = user.Id,
                            SanPhamId = spList.Count > 1 ? spList[1].Id : spList[0].Id,
                            SoSao = 4,
                            NoiDung = "Giao hàng hơi chậm 2 ngày so với dự kiến. Túi da đẹp chuẩn phom dáng nhưng phục vụ của bên vận chuyển chưa tương xứng phân khúc.",
                            NgayDanhGia = DateTime.Today.AddDays(-3),
                            PhanHoi = null
                        },
                        new DanhGia
                        {
                            DonHangId = deliveredOrder.Id,
                            NguoiDungId = user.Id,
                            SanPhamId = spList.Count > 2 ? spList[2].Id : spList[0].Id,
                            SoSao = 2,
                            NoiDung = "Đường chỉ may ở tay áo hơi lỏng lẻo, vải mỏng hơn so với đợt trước mua. Cần thương hiệu kiểm tra lại chất lượng đầu ra.",
                            NgayDanhGia = DateTime.Today.AddDays(-5),
                            PhanHoi = null
                        },
                        new DanhGia
                        {
                            DonHangId = deliveredOrder.Id,
                            NguoiDungId = user.Id,
                            SanPhamId = spList.Count > 3 ? spList[3].Id : spList[0].Id,
                            SoSao = 5,
                            NoiDung = "Đơn hàng tuyệt cà là vời!!!!!! Chất lượng đỉnh chóp!",
                            NgayDanhGia = DateTime.Today.AddDays(-6),
                            PhanHoi = "Dạ cảm ơn bạn nhiều nhiều nha, chúc bạn luôn xinh đẹp cùng AMP ạ!"
                        }
                    };

                    db.DanhGias.AddRange(reviews);
                    await db.SaveChangesAsync();
                }
            }

            // Tự động chuẩn hóa tổng tiền cho các đơn hàng cũ (nếu có đơn hàng nào lưu TongTien = 0 trong khi có TamTinh)
            var donHangLoiTongTien = await db.DonHangs.Where(d => d.TongTien == 0 && d.TamTinh > 0).ToListAsync();
            if (donHangLoiTongTien.Count > 0)
            {
                foreach (var d in donHangLoiTongTien)
                {
                    d.TongTien = Math.Max(0, d.TamTinh + d.PhiVanChuyen - d.SoTienGiamGia);
                }
                await db.SaveChangesAsync();
            }
        }
    }
}

using AMPFashionStore.Data;
using AMPFashionStore.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ===== MVC =====
builder.Services.AddControllersWithViews();

// ===== EF Core + SQL Server =====
// LƯU Ý: cần chạy `dotnet add package Microsoft.EntityFrameworkCore.SqlServer`
// và `dotnet add package Microsoft.EntityFrameworkCore.Tools` (xem README.md) trước khi build.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== Cấu hình đọc từ appsettings.json =====
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.Configure<BankSettings>(builder.Configuration.GetSection("BankSettings"));
builder.Services.AddScoped<IEmailService, EmailService>();

// ===== Session (lưu tạm thông tin đăng ký / OTP / giỏ hàng phía server) =====
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ===== Xác thực bằng Cookie (thay cho ASP.NET Identity đầy đủ để tùy biến luồng OTP) =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/TaiKhoan/DangNhap";
        options.LogoutPath = "/TaiKhoan/DangXuat";
        options.AccessDeniedPath = "/TaiKhoan/KhongCoQuyen";
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
        options.Cookie.Name = "AMPFashionStore.Auth";
    });

builder.Services.AddAuthorization();

// ===== Tác vụ nền: tự động hủy đơn quá 24h chưa thanh toán =====
builder.Services.AddHostedService<DonHangCleanupService>();

var app = builder.Build();

// ===== Khởi tạo / cập nhật database + dữ liệu mẫu khi ứng dụng khởi động =====
using (var scope = app.Services.CreateScope())
{
    await DbSeeder.SeedAsync(app.Services);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Loi");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Route cho khu vực (Area) "QuanLy" - phải khai báo trước route mặc định
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Services
{
    /// <summary>
    /// UC09 (dòng sự kiện phụ): "Trong 24 giờ nếu khách hàng không thực hiện thanh toán
    /// thì đơn hàng sẽ được hủy bỏ." Quét định kỳ và tự động hủy các đơn ở trạng thái
    /// "Chờ thanh toán" quá 24 giờ.
    /// </summary>
    public class DonHangCleanupService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DonHangCleanupService> _logger;
        private static readonly TimeSpan ChuKyQuet = TimeSpan.FromMinutes(30);

        public DonHangCleanupService(IServiceProvider services, ILogger<DonHangCleanupService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _services.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var moc = DateTime.Now.AddHours(-24);
                    var donHetHan = await db.DonHangs
                        .Where(d => d.TrangThaiDonHang == TrangThaiDonHang.ChoThanhToan && d.NgayDat <= moc)
                        .ToListAsync(stoppingToken);

                    if (donHetHan.Count > 0)
                    {
                        foreach (var don in donHetHan)
                        {
                            don.TrangThaiDonHang = TrangThaiDonHang.DaHuy;
                            don.LyDoHuy = "Tự động hủy do quá 24 giờ chưa thanh toán";
                            don.NgayHuy = DateTime.Now;
                        }
                        await db.SaveChangesAsync(stoppingToken);
                        _logger.LogInformation("Đã tự động hủy {SoLuong} đơn hàng quá hạn thanh toán.", donHetHan.Count);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Lỗi khi quét đơn hàng quá hạn thanh toán.");
                }

                try
                {
                    await Task.Delay(ChuKyQuet, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Ứng dụng đang dừng - thoát vòng lặp một cách êm ái
                }
            }
        }
    }
}

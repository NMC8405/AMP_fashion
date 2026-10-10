using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Services
{
    public interface IThongBaoService
    {
        Task GuiThongBaoAsync(int nguoiDungId, string tieuDe, string noiDung, LoaiThongBao loai, string? duongDan = null);
        Task<int> DemThongBaoChuaDocAsync(int nguoiDungId);
        Task<List<ThongBao>> LayThongBaoCuaToiAsync(int nguoiDungId, int take = 50);
        Task DanhDauDaDocAsync(int thongBaoId, int nguoiDungId);
        Task DanhDauTatCaDaDocAsync(int nguoiDungId);
    }

    public class ThongBaoService : IThongBaoService
    {
        private readonly ApplicationDbContext _db;

        public ThongBaoService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task GuiThongBaoAsync(int nguoiDungId, string tieuDe, string noiDung, LoaiThongBao loai, string? duongDan = null)
        {
            if (nguoiDungId <= 0) return;

            var thongBao = new ThongBao
            {
                NguoiDungId = nguoiDungId,
                TieuDe = tieuDe,
                NoiDung = noiDung,
                LoaiThongBao = loai,
                DuongDan = duongDan,
                DaDoc = false,
                NgayTao = DateTime.Now
            };

            _db.ThongBaos.Add(thongBao);
            await _db.SaveChangesAsync();
        }

        public async Task<int> DemThongBaoChuaDocAsync(int nguoiDungId)
        {
            if (nguoiDungId <= 0) return 0;
            return await _db.ThongBaos.CountAsync(t => t.NguoiDungId == nguoiDungId && !t.DaDoc);
        }

        public async Task<List<ThongBao>> LayThongBaoCuaToiAsync(int nguoiDungId, int take = 50)
        {
            if (nguoiDungId <= 0) return new();
            return await _db.ThongBaos
                .Where(t => t.NguoiDungId == nguoiDungId)
                .OrderByDescending(t => t.NgayTao)
                .Take(take)
                .ToListAsync();
        }

        public async Task DanhDauDaDocAsync(int thongBaoId, int nguoiDungId)
        {
            var tb = await _db.ThongBaos.FirstOrDefaultAsync(t => t.Id == thongBaoId && t.NguoiDungId == nguoiDungId);
            if (tb != null && !tb.DaDoc)
            {
                tb.DaDoc = true;
                await _db.SaveChangesAsync();
            }
        }

        public async Task DanhDauTatCaDaDocAsync(int nguoiDungId)
        {
            var danhSach = await _db.ThongBaos
                .Where(t => t.NguoiDungId == nguoiDungId && !t.DaDoc)
                .ToListAsync();

            foreach (var tb in danhSach)
            {
                tb.DaDoc = true;
            }
            await _db.SaveChangesAsync();
        }
    }
}

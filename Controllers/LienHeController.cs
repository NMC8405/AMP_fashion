using AMPFashionStore.Data;
using AMPFashionStore.Models;
using Microsoft.AspNetCore.Mvc;

namespace AMPFashionStore.Controllers
{
    public class LienHeController : Controller
    {
        private readonly ApplicationDbContext _db;

        public LienHeController(ApplicationDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index() => View(new LienHe());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(LienHe model)
        {
            if (!ModelState.IsValid) return View(model);

            model.NgayGui = DateTime.Now;
            model.DaXuLy = false;
            _db.LienHes.Add(model);
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Cảm ơn bạn đã liên hệ với AMP Fashion Store! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            return RedirectToAction(nameof(Index));
        }
    }
}

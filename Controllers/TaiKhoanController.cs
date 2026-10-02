using System.Security.Claims;
using AMPFashionStore.Data;
using AMPFashionStore.Models;
using AMPFashionStore.Services;
using AMPFashionStore.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AMPFashionStore.Controllers
{
    public class TaiKhoanController : Controller
    {
        private const int SoLanSaiToiDa = 5;
        private const int PhutKhoaTaiKhoan = 15;
        private const int PhutHetHanOtp = 5;
        private const int SoLanSaiOtpToiDa = 5;

        private readonly ApplicationDbContext _db;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _env;
        private readonly PasswordHasher<NguoiDung> _hasher = new();

        public TaiKhoanController(ApplicationDbContext db, IEmailService emailService, IWebHostEnvironment env)
        {
            _db = db;
            _emailService = emailService;
            _env = env;
        }

        // ================= UC01: ĐĂNG KÝ TÀI KHOẢN =================
        [HttpGet]
        public IActionResult DangKy()
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View(new DangKyViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangKy(DangKyViewModel model)
        {
            if (!model.DongYDieuKhoan)
            {
                ModelState.AddModelError(nameof(model.DongYDieuKhoan), "Vui lòng chọn chấp nhận Điều khoản dịch vụ và Chính sách bảo mật để tiếp tục.");
            }

            var emailNormalized = model.Email?.Trim().ToLower() ?? "";
            if (string.IsNullOrWhiteSpace(emailNormalized) || !emailNormalized.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError(nameof(model.Email), "Hệ thống chỉ chấp nhận địa chỉ email @gmail.com (ví dụ: example@gmail.com).");
            }

            if (!ModelState.IsValid) return View(model);

            var daTonTai = await _db.NguoiDungs.AnyAsync(n => n.Email == emailNormalized);
            if (daTonTai)
            {
                ModelState.AddModelError(nameof(model.Email), "Email này đã được đăng ký. Vui lòng dùng email khác hoặc đăng nhập.");
                return View(model);
            }

            var pending = new PendingRegistration
            {
                HoTen = model.HoTen.Trim(),
                Email = emailNormalized,
                SoDienThoai = model.SoDienThoai.Trim(),
                MatKhauHash = _hasher.HashPassword(null!, model.MatKhau)
            };
            HttpContext.Session.SetObject("PendingRegistration", pending);

            await TaoVaGuiOtpAsync(pending.Email, pending.HoTen, "register", "đăng ký tài khoản");

            return RedirectToAction(nameof(XacThucOtp), new { muc = "register" });
        }

        // ================= XÁC THỰC OTP (dùng chung Đăng ký & Quên mật khẩu) =================
        [HttpGet]
        public IActionResult XacThucOtp(string muc = "register")
        {
            var otp = HttpContext.Session.GetObject<PendingOtp>(SessionKeyOtp(muc));
            if (otp == null)
            {
                TempData["Loi"] = "Phiên xác thực đã hết hạn. Vui lòng thực hiện lại.";
                return RedirectToAction(muc == "forgot" ? nameof(QuenMatKhau) : nameof(DangKy));
            }

            ViewBag.Email = otp.Email;
            ViewBag.Muc = muc;
            ViewBag.DevOtp = _env.IsDevelopment() ? otp.MaOtp : null; // tiện demo khi chưa cấu hình SMTP thật
            return View(new XacThucOtpViewModel { Muc = muc });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacThucOtp(XacThucOtpViewModel model)
        {
            var otp = HttpContext.Session.GetObject<PendingOtp>(SessionKeyOtp(model.Muc));
            if (otp == null)
            {
                TempData["Loi"] = "Phiên xác thực đã hết hạn. Vui lòng thực hiện lại.";
                return RedirectToAction(model.Muc == "forgot" ? nameof(QuenMatKhau) : nameof(DangKy));
            }

            ViewBag.Email = otp.Email;
            ViewBag.Muc = model.Muc;
            ViewBag.DevOtp = _env.IsDevelopment() ? otp.MaOtp : null;

            if (!ModelState.IsValid) return View(model);

            if (DateTime.Now > otp.HetHan)
            {
                ModelState.AddModelError(nameof(model.MaOtp), "Mã OTP đã hết hạn. Vui lòng bấm gửi lại mã.");
                return View(model);
            }

            if (otp.MaOtp != model.MaOtp.Trim())
            {
                otp.SoLanNhapSai++;
                HttpContext.Session.SetObject(SessionKeyOtp(model.Muc), otp);

                if (otp.SoLanNhapSai >= SoLanSaiOtpToiDa)
                {
                    HttpContext.Session.Remove(SessionKeyOtp(model.Muc));
                    TempData["Loi"] = "Bạn đã nhập sai mã OTP quá số lần cho phép. Vui lòng thực hiện lại từ đầu.";
                    return RedirectToAction(model.Muc == "forgot" ? nameof(QuenMatKhau) : nameof(DangKy));
                }

                ModelState.AddModelError(nameof(model.MaOtp), "Mã OTP không chính xác. Vui lòng kiểm tra lại.");
                return View(model);
            }

            // OTP hợp lệ
            if (model.Muc == "forgot")
            {
                otp.DaXacThuc = true;
                HttpContext.Session.SetObject(SessionKeyOtp(model.Muc), otp);
                return RedirectToAction(nameof(DatLaiMatKhau));
            }

            // muc == "register": tạo tài khoản chính thức
            var pending = HttpContext.Session.GetObject<PendingRegistration>("PendingRegistration");
            if (pending == null)
            {
                TempData["Loi"] = "Không tìm thấy thông tin đăng ký. Vui lòng đăng ký lại.";
                return RedirectToAction(nameof(DangKy));
            }

            var nguoiDungMoi = new NguoiDung
            {
                HoTen = pending.HoTen,
                Email = pending.Email,
                SoDienThoai = pending.SoDienThoai,
                MatKhauHash = pending.MatKhauHash,
                VaiTro = VaiTro.KhachHang,
                TrangThai = TrangThaiTaiKhoan.HoatDong
            };
            _db.NguoiDungs.Add(nguoiDungMoi);
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove("PendingRegistration");
            HttpContext.Session.Remove(SessionKeyOtp(model.Muc));

            await DangNhapBangCookieAsync(nguoiDungMoi, false);
            TempData["ThongBao"] = "Đăng ký tài khoản thành công! Chào mừng bạn đến với AMP Fashion Store.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GuiLaiOtp(string muc)
        {
            var otp = HttpContext.Session.GetObject<PendingOtp>(SessionKeyOtp(muc));
            if (otp == null)
            {
                TempData["Loi"] = "Phiên xác thực đã hết hạn. Vui lòng thực hiện lại.";
                return RedirectToAction(muc == "forgot" ? nameof(QuenMatKhau) : nameof(DangKy));
            }

            var hoTen = muc == "forgot"
                ? (await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == otp.Email))?.HoTen ?? "bạn"
                : HttpContext.Session.GetObject<PendingRegistration>("PendingRegistration")?.HoTen ?? "bạn";

            await TaoVaGuiOtpAsync(otp.Email, hoTen, muc, muc == "forgot" ? "khôi phục mật khẩu" : "đăng ký tài khoản");
            TempData["ThongBao"] = "Đã gửi lại mã OTP mới tới email của bạn.";
            return RedirectToAction(nameof(XacThucOtp), new { muc });
        }

        // ================= UC02: ĐĂNG NHẬP =================
        [HttpGet]
        public IActionResult DangNhap(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true) return RedirectToAction("Index", "Home");
            return View(new DangNhapViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangNhap(DangNhapViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == model.Email.Trim().ToLower());

            // UC02 4-a: sai tài khoản/mật khẩu
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            // UC02 4-c: đang tạm khóa do nhập sai quá số lần quy định
            if (user.KhoaDangNhapDenLuc.HasValue && user.KhoaDangNhapDenLuc.Value > DateTime.Now)
            {
                var conLai = (int)Math.Ceiling((user.KhoaDangNhapDenLuc.Value - DateTime.Now).TotalMinutes);
                ModelState.AddModelError(string.Empty, $"Tài khoản tạm khóa do nhập sai quá {SoLanSaiToiDa} lần. Vui lòng thử lại sau khoảng {conLai} phút.");
                return View(model);
            }

            // UC02 4-b: tài khoản bị khóa bởi quản trị viên
            if (user.TrangThai == TrangThaiTaiKhoan.BiKhoa)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn hiện không khả dụng. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            var ketQua = _hasher.VerifyHashedPassword(user, user.MatKhauHash, model.MatKhau);
            if (ketQua == PasswordVerificationResult.Failed)
            {
                user.SoLanDangNhapSai++;
                if (user.SoLanDangNhapSai >= SoLanSaiToiDa)
                {
                    user.KhoaDangNhapDenLuc = DateTime.Now.AddMinutes(PhutKhoaTaiKhoan);
                    user.SoLanDangNhapSai = 0;
                    await _db.SaveChangesAsync();
                    ModelState.AddModelError(string.Empty, $"Bạn đã nhập sai quá {SoLanSaiToiDa} lần. Tài khoản tạm khóa trong {PhutKhoaTaiKhoan} phút.");
                    return View(model);
                }
                await _db.SaveChangesAsync();
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            user.SoLanDangNhapSai = 0;
            user.KhoaDangNhapDenLuc = null;
            await _db.SaveChangesAsync();

            await DangNhapBangCookieAsync(user, model.GhiNho);

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return user.VaiTro switch
            {
                VaiTro.QuanTriVien or VaiTro.NhanVien => RedirectToAction("Index", "Dashboard", new { area = "QuanLy" }),
                _ => RedirectToAction("Index", "Home")
            };
        }

        // ================= Đăng xuất =================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult KhongCoQuyen() => View();

        // ================= UC03: QUÊN MẬT KHẨU =================
        [HttpGet]
        public IActionResult QuenMatKhau() => View(new QuenMatKhauViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuenMatKhau(QuenMatKhauViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var email = model.Email.Trim().ToLower();
            var user = await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == email);
            if (user == null)
            {
                ModelState.AddModelError(nameof(model.Email), "Không tìm thấy tài khoản nào với email này.");
                return View(model);
            }

            await TaoVaGuiOtpAsync(email, user.HoTen, "forgot", "khôi phục mật khẩu");
            return RedirectToAction(nameof(XacThucOtp), new { muc = "forgot" });
        }

        [HttpGet]
        public IActionResult DatLaiMatKhau()
        {
            var otp = HttpContext.Session.GetObject<PendingOtp>(SessionKeyOtp("forgot"));
            if (otp == null || !otp.DaXacThuc)
            {
                TempData["Loi"] = "Vui lòng xác thực OTP trước khi đặt lại mật khẩu.";
                return RedirectToAction(nameof(QuenMatKhau));
            }
            return View(new DatLaiMatKhauViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DatLaiMatKhau(DatLaiMatKhauViewModel model)
        {
            var otp = HttpContext.Session.GetObject<PendingOtp>(SessionKeyOtp("forgot"));
            if (otp == null || !otp.DaXacThuc)
            {
                TempData["Loi"] = "Phiên khôi phục mật khẩu đã hết hạn. Vui lòng thực hiện lại.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            if (!ModelState.IsValid) return View(model);

            var user = await _db.NguoiDungs.FirstOrDefaultAsync(n => n.Email == otp.Email);
            if (user == null)
            {
                TempData["Loi"] = "Không tìm thấy tài khoản.";
                return RedirectToAction(nameof(QuenMatKhau));
            }

            user.MatKhauHash = _hasher.HashPassword(user, model.MatKhauMoi);
            user.SoLanDangNhapSai = 0;
            user.KhoaDangNhapDenLuc = null;
            await _db.SaveChangesAsync();

            HttpContext.Session.Remove(SessionKeyOtp("forgot"));

            TempData["ThongBao"] = "Đặt lại mật khẩu thành công. Vui lòng đăng nhập bằng mật khẩu mới.";
            return RedirectToAction(nameof(DangNhap));
        }

        // ================= UC04: SỬA THÔNG TIN CÁ NHÂN =================
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> ThongTinCaNhan()
        {
            var user = await _db.NguoiDungs.FindAsync(User.GetUserId());
            if (user == null) return NotFound();

            return View(new ThongTinCaNhanViewModel
            {
                HoTen = user.HoTen,
                Email = user.Email,
                SoDienThoai = user.SoDienThoai ?? "",
                DiaChi = user.DiaChi,
                AnhDaiDien = user.AnhDaiDien,
                NgaySinh = user.NgaySinh,
                GioiTinh = user.GioiTinh
            });
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThongTinCaNhan(ThongTinCaNhanViewModel model)
        {
            var user = await _db.NguoiDungs.FindAsync(User.GetUserId());
            if (user == null) return NotFound();

            model.Email = user.Email;
            if (!ModelState.IsValid) return View(model);

            user.HoTen = model.HoTen.Trim();
            user.SoDienThoai = model.SoDienThoai.Trim();
            user.DiaChi = model.DiaChi?.Trim();
            user.NgaySinh = model.NgaySinh;
            user.GioiTinh = model.GioiTinh;
            if (!string.IsNullOrWhiteSpace(model.AnhDaiDien)) user.AnhDaiDien = model.AnhDaiDien.Trim();

            await _db.SaveChangesAsync();

            // Cập nhật lại tên hiển thị trong Claims mà không cần đăng xuất
            await DangNhapBangCookieAsync(user, false);

            TempData["ThongBao"] = "Cập nhật thông tin cá nhân thành công.";
            return RedirectToAction(nameof(ThongTinCaNhan));
        }

        // ================= UC05: ĐỔI MẬT KHẨU =================
        [Authorize]
        [HttpGet]
        public IActionResult DoiMatKhau() => View(new DoiMatKhauViewModel());

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DoiMatKhau(DoiMatKhauViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _db.NguoiDungs.FindAsync(User.GetUserId());
            if (user == null) return NotFound();

            var ketQua = _hasher.VerifyHashedPassword(user, user.MatKhauHash, model.MatKhauCu);
            if (ketQua == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError(nameof(model.MatKhauCu), "Mật khẩu hiện tại không chính xác.");
                return View(model);
            }

            user.MatKhauHash = _hasher.HashPassword(user, model.MatKhauMoi);
            await _db.SaveChangesAsync();

            TempData["ThongBao"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(ThongTinCaNhan));
        }

        // ================= Hàm hỗ trợ dùng nội bộ =================
        private static string SessionKeyOtp(string muc) => $"PendingOtp_{muc}";

        private async Task TaoVaGuiOtpAsync(string email, string hoTen, string muc, string mucDichHienThi)
        {
            var maOtp = Random.Shared.Next(100000, 999999).ToString();
            var otp = new PendingOtp
            {
                Email = email,
                MaOtp = maOtp,
                HetHan = DateTime.Now.AddMinutes(PhutHetHanOtp),
                SoLanNhapSai = 0,
                DaXacThuc = false
            };
            HttpContext.Session.SetObject(SessionKeyOtp(muc), otp);
            await _emailService.GuiOtpAsync(email, hoTen, maOtp, mucDichHienThi);
        }

        private async Task DangNhapBangCookieAsync(NguoiDung user, bool ghiNho)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.HoTen),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.VaiTro.ToString())
            };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, new AuthenticationProperties
            {
                IsPersistent = ghiNho,
                ExpiresUtc = ghiNho ? DateTimeOffset.UtcNow.AddDays(30) : null
            });
        }
    }
}

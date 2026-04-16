using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using QuanLyQuanCaPhe.DAL;
using QuanLyQuanCaPhe.Models;
using QuanLyQuanCaPhe.Utils;
using System.Security.Claims;

namespace QuanLyQuanCaPhe.Controllers
{
    public class LoginController : Controller
    {
        // ================================================================
        // ĐĂNG NHẬP
        // ================================================================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            if (TempData["Success"] is string msg)
                ViewBag.SuccessMessage = msg;

            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = UserDAL.Authenticate(model.Email, model.Password);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            string roleName = user.Role ? "Manager" : "Employee";
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name,           user.FullName),
                new(ClaimTypes.Email,          user.Email),
                new(ClaimTypes.Role,           roleName)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24) });

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        // ================================================================
        // BƯỚC 1: Quên mật khẩu — Nhập email → Gửi OTP
        // ================================================================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");
            return View(new ForgotPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = UserDAL.GetByEmail(model.Email);

            // Nếu email không tồn tại → vẫn chuyển trang (tránh email enumeration)
            if (user == null)
            {
                TempData["OtpEmail"]   = model.Email;
                TempData["OtpSent"]    = "Nếu email tồn tại, mã OTP đã được gửi.";
                TempData["OtpExpiry"]  = DateTime.Now.AddMinutes(5).ToString("o");
                return RedirectToAction(nameof(VerifyOtp));
            }

            // Kiểm tra cooldown 60 giây chống spam
            var (isInCooldown, secondsRemaining) = UserOtpDAL.CheckCooldown(user.Id);
            if (isInCooldown)
            {
                ModelState.AddModelError(string.Empty,
                    $"Bạn vừa gửi yêu cầu OTP. Vui lòng chờ {secondsRemaining} giây trước khi thử lại.");
                return View(model);
            }

            // Sinh OTP → lưu DB → gửi email
            string   otpCode   = OtpUtil.Generate();
            DateTime createdAt = DateTime.Now;
            DateTime expiresAt = createdAt.AddMinutes(OtpUtil.ExpiryMinutesValue);

            UserOtpDAL.Create(user.Id, otpCode, createdAt, expiresAt);

            try
            {
                string body = EmailUtil.BuildOtpEmail(user.FullName, otpCode, OtpUtil.ExpiryMinutesValue);
                await EmailUtil.SendAsync(user.Email, user.FullName,
                                          "Mã OTP đặt lại mật khẩu - Cafe Manager", body);
            }
            catch (Exception ex)
            {
                // Log lỗi để debug
                Console.WriteLine($"[EmailError] Gửi OTP thất bại: {ex.Message}");
                ModelState.AddModelError(string.Empty,
                    "Không thể gửi email OTP. Vui lòng kiểm tra cấu hình email hoặc thử lại sau.");
                return View(model);
            }

            TempData["OtpEmail"]    = model.Email;
            TempData["OtpSent"]     = "Nếu email tồn tại, mã OTP đã được gửi.";
            TempData["OtpExpiry"]   = expiresAt.ToString("o");
            TempData["OtpSentAt"]   = createdAt.ToString("o");   // ← cho resend cooldown
            return RedirectToAction(nameof(VerifyOtp));
        }

        // ================================================================
        // GỬI LẠI OTP — AJAX POST endpoint
        // ================================================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendOtp([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Json(new { success = false, message = "Email không hợp lệ." });

            var user = UserDAL.GetByEmail(email);

            // Nếu email không tồn tại → vẫn trả success (tránh email enumeration)
            if (user == null)
            {
                return Json(new
                {
                    success = true,
                    message = "Nếu email tồn tại, mã OTP mới đã được gửi.",
                    expiry  = DateTime.Now.AddMinutes(5).ToString("o"),
                    sentAt  = DateTime.Now.ToString("o")
                });
            }

            // Kiểm tra cooldown 60 giây
            var (isInCooldown, secondsRemaining) = UserOtpDAL.CheckCooldown(user.Id);
            if (isInCooldown)
            {
                return Json(new
                {
                    success   = false,
                    message   = $"Vui lòng chờ {secondsRemaining} giây trước khi gửi lại.",
                    cooldown  = secondsRemaining
                });
            }

            // Sinh OTP mới → lưu DB → gửi email
            string   otpCode   = OtpUtil.Generate();
            DateTime createdAt = DateTime.Now;
            DateTime expiresAt = createdAt.AddMinutes(OtpUtil.ExpiryMinutesValue);

            UserOtpDAL.Create(user.Id, otpCode, createdAt, expiresAt);

            try
            {
                string body = EmailUtil.BuildOtpEmail(user.FullName, otpCode, OtpUtil.ExpiryMinutesValue);
                await EmailUtil.SendAsync(user.Email, user.FullName,
                                          "Mã OTP đặt lại mật khẩu - Cafe Manager", body);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailError] Gửi lại OTP thất bại: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = "Không thể gửi email OTP. Vui lòng kiểm tra cấu hình email hoặc thử lại sau."
                });
            }

            return Json(new
            {
                success = true,
                message = "Mã OTP mới đã được gửi đến email của bạn.",
                expiry  = expiresAt.ToString("o"),
                sentAt  = createdAt.ToString("o")
            });
        }

        // ================================================================
        // BƯỚC 2: Xác nhận OTP — Nhập 6 chữ số
        // ================================================================

        [HttpGet]
        public IActionResult VerifyOtp()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            string? email = TempData["OtpEmail"]?.ToString();
            if (string.IsNullOrEmpty(email))
                return RedirectToAction(nameof(ForgotPassword));

            // Đọc trước khi Keep — đảm bảo giá trị có sẵn
            string? otpSent   = TempData["OtpSent"]?.ToString();
            string? otpExpiry = TempData["OtpExpiry"]?.ToString();
            string? otpSentAt = TempData["OtpSentAt"]?.ToString();

            // Keep cho lần đọc tiếp (POST fail → redirect)
            TempData.Keep("OtpEmail");
            TempData.Keep("OtpExpiry");
            TempData.Keep("OtpSentAt");

            ViewBag.OtpSentMessage = otpSent;
            ViewBag.OtpExpiry      = otpExpiry ?? "";
            ViewBag.OtpSentAt      = otpSentAt ?? "";
            ViewBag.OtpEmail       = email;

            return View(new VerifyOtpViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyOtp(VerifyOtpViewModel model)
        {
            // Phục hồi ViewBag từ TempData cho trường hợp validation fail
            string? otpExpiry = TempData["OtpExpiry"]?.ToString();
            string? otpSentAt = TempData["OtpSentAt"]?.ToString();
            TempData.Keep("OtpEmail");
            TempData.Keep("OtpExpiry");
            TempData.Keep("OtpSentAt");

            ViewBag.OtpExpiry = otpExpiry ?? "";
            ViewBag.OtpSentAt = otpSentAt ?? "";
            ViewBag.OtpEmail  = model.Email;

            if (!ModelState.IsValid)
                return View(model);

            var user = UserDAL.GetByEmail(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email không tồn tại trong hệ thống.");
                return View(model);
            }

            // Validate OTP (tồn tại + chưa dùng + chưa hết hạn + mã đúng)
            var (isValid, otp, errorMsg) = UserOtpDAL.Validate(user.Id, model.OtpCode);
            if (!isValid)
            {
                ModelState.AddModelError(nameof(model.OtpCode), errorMsg);
                return View(model);
            }

            // OTP hợp lệ → chuyển sang bước 3 (đặt mật khẩu mới)
            // Truyền OtpId qua TempData để xác nhận ở bước 3
            TempData["ResetEmail"] = model.Email;
            TempData["ResetOtpId"] = otp!.Id.ToString();
            return RedirectToAction(nameof(ResetPassword));
        }

        // ================================================================
        // BƯỚC 3: Đặt lại mật khẩu mới
        // ================================================================

        [HttpGet]
        public IActionResult ResetPassword()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            string? email  = TempData["ResetEmail"]?.ToString();
            string? otpId  = TempData["ResetOtpId"]?.ToString();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(otpId))
                return RedirectToAction(nameof(ForgotPassword));

            TempData.Keep("ResetEmail");
            TempData.Keep("ResetOtpId");

            return View(new ResetPasswordViewModel
            {
                Email = email,
                OtpId = int.Parse(otpId)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // Xác minh lại OTP id vẫn còn hợp lệ (chưa dùng)
            var user = UserDAL.GetByEmail(model.Email);
            if (user == null)
                return RedirectToAction(nameof(ForgotPassword));

            // Cập nhật mật khẩu mới
            user.Password = model.NewPassword;
            UserDAL.Update(user);

            // Đánh dấu OTP đã sử dụng
            UserOtpDAL.MarkUsed(model.OtpId);

            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập bằng mật khẩu mới.";
            return RedirectToAction(nameof(Login));
        }
    }
}

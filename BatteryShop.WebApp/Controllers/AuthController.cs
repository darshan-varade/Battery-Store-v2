using System;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.ViewModels;
using BatteryShop.WebApp.Infrastructure;

namespace BatteryShop.WebApp.Controllers
{
    [AllowAnonymous]
    public class AuthController : BaseController
    {
        [HttpGet]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                AuthDAL dal = new AuthDAL();
                var owner = dal.OwnerLogin(vm.Email);

                if (owner == null || !BCrypt.Net.BCrypt.Verify(vm.Password, owner.PasswordHash))
                {
                    if (owner == null && dal.OwnerCheckEmail(vm.Email))
                    {
                        byte? status = dal.GetApprovalStatus(vm.Email);
                        if (status == 0)
                            ModelState.AddModelError("", "Your account has been rejected. Contact admin.");
                        else
                            ModelState.AddModelError("", "Your account is pending admin approval.");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Invalid email or password.");
                    }

                    return View(vm);
                }

                string accessToken = JwtHelper.GenerateAccessToken(owner.OwnerId, owner.OwnerName, owner.OwnerEmail, owner.RoleName, owner.ProfileImage, owner.OwnerPhone);
                string refreshToken = JwtHelper.GenerateRefreshToken();
                string refreshTokenHash = JwtHelper.HashRefreshToken(refreshToken);
                DateTime refreshExpiry = DateTime.Now.AddDays(int.Parse(ConfigurationManager.AppSettings["JwtRefreshTokenExpiryDays"] ?? "7"));

                dal.CreateRefreshToken(owner.OwnerId, refreshTokenHash, refreshExpiry);

                DateTime accessExpiry = JwtHelper.GetAccessTokenExpiry(vm.RememberMe);

                Response.Cookies.Add(new HttpCookie("access_token", accessToken)
                {
                    HttpOnly = true,
                    Secure = false,
                    Path = "/",
                    Expires = accessExpiry
                });

                Response.Cookies.Add(new HttpCookie("refresh_token", refreshToken)
                {
                    HttpOnly = true,
                    Secure = false,
                    Path = "/",
                    Expires = refreshExpiry
                });

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in Login POST");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(vm);
            }
        }

        [HttpGet]
        public ActionResult Signup()
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Signup(SignupViewModel vm)
        {
            AuthDAL dal = new AuthDAL();

            if (vm.SignupStep == "otp")
            {
                ModelState.Remove("OwnerName");
                ModelState.Remove("OwnerPhone");
                ModelState.Remove("OwnerEmail");
                ModelState.Remove("Password");
                ModelState.Remove("ConfirmPassword");

                if (string.IsNullOrEmpty(vm.OtpCode) || vm.OtpCode.Length != 6)
                {
                    ModelState.AddModelError("OtpCode", "Enter the 6-digit code.");
                    return View(vm);
                }

                try
                {
                    int? otpId = dal.ValidateOtpByEmail(vm.OtpEmail, vm.OtpCode);
                    if (otpId == null)
                    {
                        ModelState.AddModelError("OtpCode", "Invalid or expired code.");
                        return View(vm);
                    }

                    dal.MarkOtpUsed(otpId.Value);
                    int ownerId = dal.OwnerRegister(vm.OwnerName, vm.OwnerPhone, vm.OwnerEmail, vm.PasswordHash);

                    if (!string.IsNullOrEmpty(vm.ProfileImage) && ownerId > 0)
                    {
                        try
                        {
                            string tempPhysicalPath = ImagePathHelper.GetTempProfilePhysicalPath(vm.ProfileImage);
                            if (System.IO.File.Exists(tempPhysicalPath))
                            {
                                string profilesDir = Server.MapPath(ConfigurationManager.AppSettings["ProfileImagePath"]);
                                if (!Directory.Exists(profilesDir))
                                    Directory.CreateDirectory(profilesDir);

                                string finalFileName = Guid.NewGuid().ToString("N") + ".jpg";
                                string finalPath = Path.Combine(profilesDir, finalFileName);
                                System.IO.File.Move(tempPhysicalPath, finalPath);

                                dal.UpdateProfile(ownerId, vm.OwnerName, vm.OwnerPhone, vm.OwnerEmail, finalFileName);
                            }
                        }
                        catch (Exception ex)
                        {
                            Serilog.Log.Error(ex, "Error moving profile image during signup");
                        }
                    }

                    TempData["info"] = "Account created! An admin will review and activate your account.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Error in Signup OTP verification");
                    ModelState.AddModelError("", "An error occurred. Please try again.");
                    return View(vm);
                }
            }

            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                if (dal.OwnerCheckEmail(vm.OwnerEmail))
                {
                    ModelState.AddModelError("OwnerEmail", "This email is already registered.");
                    return View(vm);
                }

                string passwordHash = BCrypt.Net.BCrypt.HashPassword(vm.Password);
                string otpCode = new Random().Next(100000, 999999).ToString();
                DateTime otpExpiresAt = DateTime.Now.AddMinutes(int.Parse(ConfigurationManager.AppSettings["OtpExpiryMinutes"] ?? "5"));

                string tempProfileImage = null;
                if (Request.Files.Count > 0)
                {
                    var file = Request.Files[0];
                    if (file != null && file.ContentLength > 0)
                    {
                        string ext = Path.GetExtension(file.FileName).ToLower();
                        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png")
                        {
                            string tempDir = Server.MapPath(ConfigurationManager.AppSettings["ProfileImageTempPath"]);
                            if (!Directory.Exists(tempDir))
                                Directory.CreateDirectory(tempDir);

                            string tempFileName = Guid.NewGuid().ToString("N") + ext;
                            string tempPath = Path.Combine(tempDir, tempFileName);
                            file.SaveAs(tempPath);
                            tempProfileImage = tempFileName;
                        }
                    }
                }

                dal.CreateOtpByEmail(vm.OwnerEmail, otpCode, otpExpiresAt);
                EmailService.SendOtp(vm.OwnerEmail, otpCode);

                vm.PasswordHash = passwordHash;
                vm.OtpEmail = vm.OwnerEmail;
                vm.SignupStep = "otp";
                vm.ProfileImage = tempProfileImage;
                vm.Password = null;
                vm.ConfirmPassword = null;
                ModelState.Clear();

                return View(vm);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in Signup POST");
                ModelState.AddModelError("", "An error occurred. Please try again.");
                return View(vm);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult ResendOtp(string email)
        {
            try
            {
                AuthDAL dal = new AuthDAL();

                DateTime? lastOtp = dal.GetLatestOtpTimeByEmail(email);
                if (lastOtp.HasValue && (DateTime.Now - lastOtp.Value).TotalSeconds < 60)
                {
                    return Json(new { success = false, error = "Please wait 60 seconds before requesting a new code." });
                }

                string otpCode = new Random().Next(100000, 999999).ToString();
                DateTime otpExpiresAt = DateTime.Now.AddMinutes(int.Parse(ConfigurationManager.AppSettings["OtpExpiryMinutes"] ?? "5"));

                dal.CreateOtpByEmail(email, otpCode, otpExpiresAt);
                EmailService.SendOtp(email, otpCode);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error in ResendOtp");
                return Json(new { success = false, error = "An error occurred. Please try again." });
            }
        }

        [HttpGet]
        public ActionResult Logout()
        {
            string refreshToken = Request.Cookies["refresh_token"]?.Value;
            if (!string.IsNullOrEmpty(refreshToken))
            {
                string hash = JwtHelper.HashRefreshToken(refreshToken);
                var record = new AuthDAL().GetRefreshTokenByHash(hash);
                if (record != null)
                    new AuthDAL().RevokeRefreshToken(record.RefreshTokenId);
            }

            Response.Cookies.Add(new HttpCookie("access_token", "") { Expires = DateTime.Now.AddDays(-1), Path = "/" });
            Response.Cookies.Add(new HttpCookie("refresh_token", "") { Expires = DateTime.Now.AddDays(-1), Path = "/" });

            return RedirectToAction("Login");
        }
    }
}

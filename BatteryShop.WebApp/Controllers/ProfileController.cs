using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
using System.Web.Mvc;
using BatteryShop.DataAccess.DAL;
using BatteryShop.DataAccess.ViewModels;
using BatteryShop.WebApp.Infrastructure;

namespace BatteryShop.WebApp.Controllers
{
    [Authorize]
    public class ProfileController : BaseController
    {
        [HttpGet]
        public ActionResult Index()
        {
            var vm = new OwnerProfileViewModel
            {
                OwnerId = CurrentOwnerId,
                OwnerName = CurrentOwnerName,
                OwnerPhone = CurrentOwnerPhone,
                OwnerEmail = CurrentOwnerEmail,
                ProfileImage = CurrentProfileImage
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(OwnerProfileViewModel vm, HttpPostedFileBase profileImageFile)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string newProfileImage = vm.ProfileImage;

            if (profileImageFile != null && profileImageFile.ContentLength > 0)
            {
                string ext = Path.GetExtension(profileImageFile.FileName).ToLower();
                if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                {
                    ModelState.AddModelError("profileImageFile", "Only JPG and PNG images are allowed.");
                    return View(vm);
                }

                try
                {
                    string profilesDir = Server.MapPath(ConfigurationManager.AppSettings["ProfileImagePath"]);
                    if (!Directory.Exists(profilesDir))
                        Directory.CreateDirectory(profilesDir);

                    string fileName = Guid.NewGuid().ToString("N") + ".jpg";
                    string filePath = Path.Combine(profilesDir, fileName);

                    using (var original = Image.FromStream(profileImageFile.InputStream))
                    {
                        int maxSize = 300;
                        int width = original.Width;
                        int height = original.Height;

                        if (width > maxSize || height > maxSize)
                        {
                            double ratio = Math.Min((double)maxSize / width, (double)maxSize / height);
                            width = (int)(width * ratio);
                            height = (int)(height * ratio);
                        }

                        using (var resized = new Bitmap(width, height))
                        using (var graphics = Graphics.FromImage(resized))
                        {
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            graphics.DrawImage(original, 0, 0, width, height);

                            var jpegCodec = ImageCodecInfo.GetImageEncoders()[1];
                            var encoderParams = new EncoderParameters(1);
                            encoderParams.Param[0] = new EncoderParameter(Encoder.Quality, 85L);
                            resized.Save(filePath, jpegCodec, encoderParams);
                        }
                    }

                    newProfileImage = fileName;
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Error uploading profile image");
                    ModelState.AddModelError("", "Error uploading image. Please try again.");
                    return View(vm);
                }
            }

            AuthDAL dal = new AuthDAL();
            dal.UpdateProfile(vm.OwnerId, vm.OwnerName, vm.OwnerPhone, vm.OwnerEmail, newProfileImage);

            ReIssueJwtCookies(vm.OwnerId, vm.OwnerName, vm.OwnerEmail, newProfileImage);

            TempData["success"] = "Profile updated successfully.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public JsonResult RemovePhoto()
        {
            try
            {
                if (!string.IsNullOrEmpty(CurrentProfileImage))
                {
                    string physicalPath = ImagePathHelper.GetProfilePhysicalPath(CurrentProfileImage);
                    if (System.IO.File.Exists(physicalPath))
                        System.IO.File.Delete(physicalPath);
                }

                AuthDAL dal = new AuthDAL();
                dal.UpdateProfile(CurrentOwnerId, CurrentOwnerName, CurrentOwnerPhone, CurrentOwnerEmail, null);

                ReIssueJwtCookies(CurrentOwnerId, CurrentOwnerName, CurrentOwnerEmail, null);

                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error removing profile photo");
                return Json(new { success = false, message = "An error occurred." });
            }
        }

        private void ReIssueJwtCookies(int ownerId, string ownerName, string ownerEmail, string profileImage)
        {
            string roleName = CurrentRoleName ?? "Owner";
            string phone = CurrentOwnerPhone;

            string accessToken = JwtHelper.GenerateAccessToken(ownerId, ownerName, ownerEmail, roleName, profileImage, phone);
            string refreshToken = JwtHelper.GenerateRefreshToken();
            string refreshTokenHash = JwtHelper.HashRefreshToken(refreshToken);
            DateTime refreshExpiry = DateTime.Now.AddDays(int.Parse(ConfigurationManager.AppSettings["JwtRefreshTokenExpiryDays"] ?? "7"));

            AuthDAL dal = new AuthDAL();
            dal.CreateRefreshToken(ownerId, refreshTokenHash, refreshExpiry);

            DateTime accessExpiry = DateTime.Now.AddMinutes(int.Parse(ConfigurationManager.AppSettings["JwtAccessTokenExpiryMinutes"] ?? "15"));

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
        }
    }
}

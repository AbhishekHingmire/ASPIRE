using DocumentFormat.OpenXml.Spreadsheet;
using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class SettingsController : Controller
    {
        private readonly ISettings _settingRepository;
        private readonly ILogger<SettingsController> _logger;

        public SettingsController(ISettings settingRepository, ILogger<SettingsController> logger)
        {
            _settingRepository = settingRepository;
            _logger = logger;
        }

        [Authorize(Roles = "Administrator, NormalUser, SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var (roleClaim, _) = UserHelper.GetRoleAndScreenRights(User);
                long userId = UserHelper.GetUserId(User);
                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.Setting);

                if (screenRight != null)
                {
                    ViewBag.ScreenRightRank = (int)screenRight;

                    ViewBag.Role = roleClaim;
                    ViewBag.Name = name;

                    var allowedScreens = await UserHelper.GetScreensForUserAsync(userId, _settingRepository);

                    ViewBag.AllowedScreens = allowedScreens;

                    var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                    ViewBag.MFICBO = mfiList;

                    string selectedMFI = Request.Cookies["SelectedMFI"] ?? "";
                    string selectedMFIName = Request.Cookies["SelectedMFIName"] ?? "";

                    ViewBag.SelectedMFI = selectedMFI;
                    ViewBag.SelectedMFIName = selectedMFIName;

                    return View();
                }

                return RedirectToAction("Errors404Basic", "Authentication");

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Index method");
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, NormalUser, SuperAdmin")]
        public IActionResult SetSettings(long SelectedMFI, string SelectedMFIName)
        {
            SetCookie("SelectedMFI", SelectedMFI.ToString());
            SetCookie("SelectedMFIName", SelectedMFIName);

            return Json(new { data = "Ok" });
        }

        private void SetCookie(string key, string value)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(1)
            };

            Response.Cookies.Append(key, value, cookieOptions);
        }
    }
}

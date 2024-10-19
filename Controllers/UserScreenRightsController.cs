using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class UserScreenRightsController : Controller
    {
        private readonly IUserScreenRights _userScreenRights;
        private readonly ILogger<UserScreenRightsController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public UserScreenRightsController(IUserScreenRights userRepository, ILogger<UserScreenRightsController> logger, ISettings settingRepository, IConfiguration configuration)
        {
            _userScreenRights = userRepository;
            _logger = logger;
            _settingRepository = settingRepository;
            _configuration = configuration;

            _defaultPageNumber = _configuration.GetValue<int>("PaginationCount:PageNumber");
            _defaultPageSize = _configuration.GetValue<int>("PaginationCount:PageSize");
        }

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Index(int pageNumber, int pageSize)
        {
            try
            {
                var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);

                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userId = UserHelper.GetUserId(User);

                var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                ViewBag.MFICBO = mfiList;
                ViewBag.Role = roleClaim;
                ViewBag.Name = name;

                pageNumber = pageNumber > 0 ? pageNumber : _defaultPageNumber;
                pageSize = pageSize > 0 ? pageSize : _defaultPageSize;

                var screens = _userScreenRights.GetScreen();
                var screenRights = _userScreenRights.GetScreenRights();
                var userDetails = _userScreenRights.GetUserDetails();
                var userScreenRightsList = _userScreenRights.GetAllUserScreenRights(pageNumber, pageSize);

                ViewData["Screens"] = screens.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name }).ToList();
                ViewData["ScreenRights"] = screenRights.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name }).ToList();
                ViewData["UserDetails"] = userDetails.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Username }).ToList();
                ViewData["UserScreenRightsDetails"] = userScreenRightsList.Select(r => new UserScreenRightsViewModel
                {
                    Id = r.Id,
                    UserName = r.UserName,
                    UserID = r.UserID,
                    Rights = r.Rights,
                    ScreenRightsID = r.ScreenRightsID,
                    Screen = r.Screen,
                    ScreenID = r.ScreenID
                }).ToList();
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching screen and Screen Rights: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while fetching screen and Screen Rights.");
                return View();
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult UserScreenRights([FromBody] UserScreenRightsModel model)
        {
            try
            {
                bool isSuccess = _userScreenRights.CreateOrEditUserScreenRights(model);

                if (isSuccess)
                {
                    TempData["SuccessMessage"] = "User Screen Rights successfully added.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while saving data. This combination of UserID and ScreenID might already exist.");
                    return View();
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving data: " + ex.Message);
                return View(model);
            }
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult UpdateScreenRights(UpdateUserScreenRightsModel model)
        {
            try
            {
                bool isSuccess = _userScreenRights.UpdateScreenRights(model);
                if (isSuccess)
                {
                    TempData["SuccessMessage"] = "User Screen Rights update successfully added.";
                    return RedirectToAction("Index");
                }
                else
                {
                    ModelState.AddModelError("", "An error occurred while update the data. This combination of UserID and ScreenID might already exist.");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating user details: {ex.Message}");
                ModelState.AddModelError("", "Error updating user details.");
                return RedirectToAction("Index");
            }
        }

        [HttpDelete]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult DeleteScreenRight(long id)
        {
            try
            {
                _userScreenRights.DeleteScreenRights(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                TempData["ErrorMessage"] = "Error deleting user.";
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult GetAllDropdownData()
        {
            try
            {
                var userDetails = _userScreenRights.GetUserDetails();
                var users = userDetails.Select(u => new SelectListItem { Value = u.ID.ToString(), Text = u.Username }).ToList();

                var screens = _userScreenRights.GetScreen();
                var screenOptions = screens.Select(s => new SelectListItem { Value = s.ID.ToString(), Text = s.Name }).ToList();

                var screenRights = _userScreenRights.GetScreenRights();
                var screenRightOptions = screenRights.Select(sr => new SelectListItem { Value = sr.ID.ToString(), Text = sr.Name }).ToList();

                var dropdownData = new
                {
                    Users = users,
                    Screens = screenOptions,
                    ScreenRights = screenRightOptions
                };

                return Json(dropdownData);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching dropdown data: {ex.Message}");
                return BadRequest("Error fetching dropdown data");
            }
        }

    }
}

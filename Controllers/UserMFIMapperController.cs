using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS2.Controllers
{
    public class UserMFIMapperController : Controller
    {
        private readonly IUserMFIMapper _userMFIMapperRepository;
        private readonly ILogger<UserMFIMapperController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public UserMFIMapperController(IUserMFIMapper userMFIMapperRepository, ILogger<UserMFIMapperController> logger, ISettings settingRepository, IConfiguration configuration)
        {
            _userMFIMapperRepository = userMFIMapperRepository;
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

                var userMFIList = _userMFIMapperRepository.GetAllAdminUserMFIList(pageNumber, pageSize);
                var partners = await _userMFIMapperRepository.GetPartnersAsync();
                var userDetails = _userMFIMapperRepository.GetUserDetails();

                var MFIList = partners.Select(p => new SelectListItem
                {
                    Text = p.Name,
                    Value = p.ID.ToString()
                }).ToList();

                ViewData["UserDetails"] = userDetails.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Username }).ToList();

                ViewData["MFICBO"] = MFIList;

                ViewData["UserMFIMapperList"] = userMFIList.Select(r => new UserMFIMapperModel
                {
                    DT_RowId = r.DT_RowId,
                    UserName = r.UserName,
                    MFI = r.MFI,
                    UserID = r.UserID,
                    PartnerID = r.PartnerID
                }).ToList();

                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching user MFI mapper: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while fetching user MFI mapper.");
                return View();
            }
        }
        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult UserMFIMapperCreateAndEdit([FromBody] UserMFIMapperCreateAndEditModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    ModelState.AddModelError("", "Model is not valid.");
                    return View(model);
                }

                if (model.Operation == "Create")
                {
                    model.ID = -1;
                    bool isSuccess = _userMFIMapperRepository.CreateOrEditUserUserMFIScreen(model);
                    if (isSuccess)
                    {
                        TempData["SuccessMessage"] = "User MFI Mapper successfully added.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while saving data. This combination of UserID and PartnerID might already exist.");
                        return View(model);
                    }
                }
                else if (model.Operation == "Edit")
                {
                    bool isSuccess = _userMFIMapperRepository.CreateOrEditUserUserMFIScreen(model);
                    if (isSuccess)
                    {
                        TempData["SuccessMessage"] = "User MFI Mapper edited successfully.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        ModelState.AddModelError("", "An error occurred while editing data. This combination of UserID and PartnerID might already exist.");
                        return View(model);
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid operation.");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Error saving data: " + ex.Message);
                return View(model);
            }
        }

        [HttpDelete]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult DeleteUserMFIMapper(long id)
        {
            try
            {
                _userMFIMapperRepository.DeleteUserMFIMapper(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                TempData["ErrorMessage"] = "Error deleting user.";
            }

            return RedirectToAction("Index");
        }
    }
}

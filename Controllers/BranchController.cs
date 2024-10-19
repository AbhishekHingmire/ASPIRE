using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class BranchController : Controller
    {
        private readonly IBranch _branchRepository;
        private readonly ILogger<BranchController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public BranchController(IBranch branchRepository, ILogger<BranchController> logger, ISettings settingRepository, IConfiguration configuration)
        {
            _branchRepository = branchRepository;
            _logger = logger;
            _settingRepository = settingRepository;
            _configuration = configuration;
            _defaultPageNumber = _configuration.GetValue<int>("PaginationCount:PageNumber");
            _defaultPageSize = _configuration.GetValue<int>("PaginationCount:PageSize");
        }

        [Authorize(Roles = "Administrator, NormalUser")]
        public async Task<IActionResult> Index(int pageNumber, int pageSize)
        {
            try
            {
                var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                long userId = UserHelper.GetUserId(User);
                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.Branch);

                if (screenRight != null)
                {
                    ViewBag.ScreenRightRank = (int)screenRight;
                    ViewBag.Role = roleClaim;

                    var allowedScreens = await UserHelper.GetScreensForUserAsync(userId, _settingRepository);

                    ViewBag.AllowedScreens = allowedScreens;

                    var partnerIdCookie = Request.Cookies["SelectedMFI"];
                    if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                    {
                        return BadRequest("Invalid PartnerID cookie.");
                    }

                    pageNumber = pageNumber > 0 ? pageNumber : _defaultPageNumber;
                    pageSize = pageSize > 0 ? pageSize : _defaultPageSize;

                    var branchItems = await _branchRepository.GetBranchListAsync(partnerId, pageNumber, pageSize);
                    var branchType = await _branchRepository.GetBranchAsync();

                    var branchMasterModels = branchItems.Select(r => new BranchModel
                    {
                        DT_RowId = r.DT_RowId,
                        BranchTypeID = r.BranchTypeID,
                        ParentID = r.ParentID,
                        ParentBranchType = r.ParentBranchType,
                        ParentCode = r.ParentCode,
                        ParentName = r.ParentName,
                        BranchType = r.BranchType,
                        Code = r.Code,
                        Name = r.Name,
                        AddressLine1 = r.AddressLine1,
                        AddressLine2 = r.AddressLine2,
                        CityID = r.CityID,
                        City = r.City,
                        StockistID = r.StockistID,
                        Pincode = r.Pincode,
                        Phone = r.Phone,
                        UserName = r.UserName,
                        Password = r.Password
                    }).ToList();
                    ViewBag.PageNumber = pageNumber;
                    ViewBag.PageSize = pageSize;

                    var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                    ViewBag.MFICBO = mfiList;
                    ViewBag.Name = name;
                    ViewData["BranchTypesList"] = branchType.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name }).ToList();
                    ViewData["BranchList"] = branchMasterModels;

                    return View();
                }

                return RedirectToAction("Errors404Basic", "Authentication");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Branch Index page");
                return StatusCode(500, "Internal server error.");
            }
        }


        [Authorize(Roles = "Administrator, NormalUser")]
        [HttpGet]
        public async Task<IActionResult> GetParentBranches(long branchTypeID)
        {
            try
            {
                var partnerIdCookie = Request.Cookies["SelectedMFI"];
                if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                {
                    return BadRequest("Invalid PartnerID cookie.");
                }
                var parentBranches = await _branchRepository.GetParentBranches(branchTypeID, partnerId);

                return Json(parentBranches);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving parent branches: {ex.Message}");
                return BadRequest("Error retrieving parent branches.");
            }
        }

        [Authorize(Roles = "Administrator, NormalUser")]
        [HttpPost]
        public async Task<IActionResult> CreateBranch(BranchMasterModel model, string action)
        {
            try
            {
                var claims = User.Claims;

                var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;
                var partnerIdCookie = Request.Cookies["SelectedMFI"];

                model.PartnerID = Convert.ToInt64(partnerIdCookie);
                model.AdminUserID = Convert.ToInt64(userId);

                if (action == "Create")
                {
                    model.DT_RowId = -1;
                    long branchId = await _branchRepository.CreateUserAndProcessBranchAsync(model);
                    return RedirectToAction("Index", branchId);
                }

                else if (action == "Edit")
                {
                    model.UserID = -1;
                    long branchId = await _branchRepository.CreateUserAndProcessBranchAsync(model);
                    return RedirectToAction("Index", branchId);
                }

                return RedirectToAction("Index");

                //return Ok(new { BranchId = branchId }); 
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [Authorize(Roles = "Administrator, NormalUser")]
        [HttpDelete]
        public IActionResult DeleteBranch(long id)
        {
            try
            {
                _branchRepository.DeleteUser(id);
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

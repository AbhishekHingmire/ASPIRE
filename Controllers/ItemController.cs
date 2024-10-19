using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Velzon.Models;

namespace IMS2.Controllers
{
    public class ItemController : Controller
    {
        private readonly IItemMaster _itemMasterRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ItemController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public ItemController(IItemMaster itemMasterRepository, ILogger<ItemController> logger, ApplicationDbContext context, ISettings settingRepository, IConfiguration configuration)
        {
            _itemMasterRepository = itemMasterRepository;
            _logger = logger;
            _context = context;
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
                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.Item);
                long userId = UserHelper.GetUserId(User);

                if (screenRight != null)
                {
                    ViewBag.ScreenRightRank = (int)screenRight;

                    ViewBag.Role = roleClaim;

                    var allowedScreens = await UserHelper.GetScreensForUserAsync(userId, _settingRepository);

                    ViewBag.AllowedScreens = allowedScreens;

                    pageNumber = pageNumber > 0 ? pageNumber : _defaultPageNumber;
                    pageSize = pageSize > 0 ? pageSize : _defaultPageSize;

                    var itemListResponse = await _itemMasterRepository.GetItemMasterListAsync(pageNumber, pageSize);
                    var categoriesResponse = await _itemMasterRepository.GetCategoriesAsync();

                    if (!itemListResponse.Success || !categoriesResponse.Success)
                    {
                        ViewBag.ErrorMessage = itemListResponse.Message + " " + categoriesResponse.Message;
                        return View();
                    }
                    
                    var itemMasterModels = itemListResponse.Data.Select(r => new GetItemMasterModel
                    {
                        DT_RowId = r.DT_RowId,
                        Category = r.Category,
                        CategoryID = r.CategoryID,
                        Code = r.Code,
                        Name = r.Name,
                        ShortDesc = r.ShortDesc,
                        HSNCode = r.HSNCode,
                    }).ToList();
                    ViewBag.PageNumber = pageNumber;
                    ViewBag.PageSize = pageSize;

                    ViewBag.Name = name;
                    ViewData["ItemList"] = itemMasterModels;
                    ViewBag.Categories = categoriesResponse.Data.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name }).ToList();

                    var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                    ViewBag.MFICBO = mfiList;

                    return View();
                }
                return RedirectToAction("Errors404Basic", "Authentication");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading item master index page");
                return View("Error");
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrator, NormalUser")]
        public async Task<IActionResult> CreateUpdateItemMaster(ItemMasterModel model, string action)
        {
            try
            {
                var claims = User.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User ID claim not found.");
                }

                long adminUserId = Convert.ToInt64(userId);
                ServiceResponse<bool> response;

                if (action == "Create")
                {
                    model.ID = -1;
                    if (model.Code != null)
                    {
                        response = await _itemMasterRepository.CreateEditItemMasterAsync(model, adminUserId);
                    }
                    else
                    {
                        response = new ServiceResponse<bool> { Success = false, Message = "Code cannot be null." };
                    }
                }
                else if (action == "Edit")
                {
                    response = await _itemMasterRepository.CreateEditItemMasterAsync(model, adminUserId);
                }
                else
                {
                    response = new ServiceResponse<bool> { Success = false, Message = "Invalid action." };
                }

                if (response.Success)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating/updating item master: {ex.Message}");
                ModelState.AddModelError("", "Error creating/updating item master.");
                return RedirectToAction("Index");
            }
        }

        [HttpDelete]
        [Authorize]
        public async Task<IActionResult> DeleteItem(long Id)
        {
            try
            {
                var claims = User.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User ID claim not found.");
                }

                long adminUserId = Convert.ToInt64(userId);
                var response = await _itemMasterRepository.DeleteItemAsync(Id, adminUserId);

                if (response.Success)
                {
                    TempData["SuccessMessage"] = response.Message;
                }
                else
                {
                    TempData["ErrorMessage"] = response.Message;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting Item: {ex.Message}");
                TempData["ErrorMessage"] = "Error deleting Item.";
            }

            return RedirectToAction("Index");
        }
    }
}

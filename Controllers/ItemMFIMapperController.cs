using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS2.Controllers
{
    public class ItemMFIMapperController : Controller
    {
        private readonly IItemMFIMapper _itemMFIMapperRepository;
        private readonly ILogger<ItemMFIMapperController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public ItemMFIMapperController(IItemMFIMapper itemMFIMapperRepository, ILogger<ItemMFIMapperController> logger, ISettings settingRepository, IConfiguration configuration)
        {
            _itemMFIMapperRepository = itemMFIMapperRepository;
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
                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.ItemMFIMapper);
                long userId = UserHelper.GetUserId(User);

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

                    var partnerItems = await _itemMFIMapperRepository.GetItemMFIMapperListAsync(partnerId, pageNumber, pageSize);
                    var itemCBOs = await _itemMFIMapperRepository.GetItemCBOsAsync();

                    var itemMasterModels = partnerItems.Select(r => new ItemMFIMapperModel
                    {
                        DT_RowId = r.DT_RowId,
                        Code = r.Code,
                        Item = r.Item,
                        MRP = r.MRP,
                        Price = r.Price,
                        GSTPercent = r.GSTPercent,
                        IsLMD = r.IsLMD,
                        ItemID = r.ItemID,
                    }).ToList();
                    ViewBag.PageNumber = pageNumber;
                    ViewBag.PageSize = pageSize;

                    var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                    ViewBag.MFICBO = mfiList;
                    ViewBag.Name = name;
                    ViewData["ItemList"] = itemMasterModels;
                    ViewData["ItemCBOs"] = itemCBOs.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name }).ToList();

                    return View();
                }

                return RedirectToAction("Errors404Basic", "Authentication");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Index page");
                return StatusCode(500, "Internal server error.");
            }
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, NormalUser")]
        public IActionResult ManageItemMaster(ItemMFIData model, string action)
        {
            try
            {
                var claims = User.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("User ID claim not found.");
                }

                var partnerIdCookie = Request.Cookies["SelectedMFI"];

                if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                {
                    return BadRequest("Invalid PartnerID cookie.");
                }

                long adminUserId = Convert.ToInt64(userId);

                bool operationResult = false;

                if (action == "Create")
                {
                    model.DT_RowId = -1;
                    operationResult = _itemMFIMapperRepository.CreateAndEditItemMFIMapperAsync(model, adminUserId, partnerId);

                    if (operationResult)
                    {
                        TempData["SuccessMessage"] = "Item created successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError("", "An item with this PartnerID and ItemID combination already exists.");
                    }
                }
                else if (action == "Edit")
                {
                    operationResult = _itemMFIMapperRepository.CreateAndEditItemMFIMapperAsync(model, adminUserId, partnerId);

                    if (operationResult)
                    {
                        TempData["SuccessMessage"] = "Item updated successfully.";
                    }
                    else
                    {
                        ModelState.AddModelError("", "An item with this PartnerID and ItemID combination already exists.");
                    }
                }

                if (!operationResult)
                {
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error managing item master: {ex.Message}");
                ModelState.AddModelError("", "Error managing item master.");
                return RedirectToAction("Index");
            }
        }



        [HttpDelete]
        [Authorize(Roles = "Administrator, NormalUser")]
        public IActionResult DeleteItem(long dtRowId)
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

                _itemMFIMapperRepository.DeleteItem(dtRowId, adminUserId);
                TempData["SuccessMessage"] = "Item deleted successfully.";
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

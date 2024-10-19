using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class PurchaseOrderController : Controller
    {
        private readonly IPurchaseOrder _PurchaseOrderRepository;
        private readonly ILogger<PurchaseOrderController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public PurchaseOrderController(IPurchaseOrder PurchaseOrderRepository, ILogger<PurchaseOrderController> logger, ISettings settingRepository, IConfiguration configuration)
        {
            _PurchaseOrderRepository = PurchaseOrderRepository;
            _logger = logger;
            _settingRepository = settingRepository;
            _configuration = configuration;

            _defaultPageNumber = _configuration.GetValue<int>("PaginationCount:PageNumber");
            _defaultPageSize = _configuration.GetValue<int>("PaginationCount:PageSize");
        }

        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> Index(int pageNumber, int pageSize)
        {
            try
            {
                var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                long userId = UserHelper.GetUserId(User);
                var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                ViewBag.MFICBO = mfiList;
                ViewBag.Name = name;
                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.PO);

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


                    var purchaseOrder = await _PurchaseOrderRepository.GetPurchaseOrderListAsync(partnerId, pageNumber, pageSize);
                    var itemDetails = await _PurchaseOrderRepository.GetItemsForPartnerAsync(partnerId);
                    var poTypeList = await _PurchaseOrderRepository.GetPOTypeList();
                    var companyList = await _PurchaseOrderRepository.GetPOCompanyList();
                    var supplierList = await _PurchaseOrderRepository.GetSupplierList();

                    var selectSupplierList = supplierList
                        .Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name })
                        .ToList();
                    var selectItemList = itemDetails
                        .Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name })
                        .ToList();
                    var selectPOTypeList = poTypeList
                        .Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name })
                        .ToList();
                    var selectPOCompanyList = companyList
                        .Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name })
                        .ToList();

                    ViewData["SupplierList"] = selectSupplierList;
                    ViewData["POCompanyList"] = selectPOCompanyList;
                    ViewData["POTypeList"] = selectPOTypeList;
                    ViewData["SelectedItemList"] = selectItemList;

                    var itempurchaseOrder = purchaseOrder.Select(r => new PurchaseOrderModel
                    {
                        DT_RowId = r.DT_RowId,
                        PO_No = r.PO_No,
                        PORef = r.PORef,
                        OrderDate = r.OrderDate,
                        POType = r.POType,
                        POTypeID = r.POTypeID,
                        Supplier = r.Supplier,
                        SupplierID = r.SupplierID,
                        Item = r.Item,
                        ItemID = r.ItemID,
                        Qty = r.Qty,
                        ReceivedQty = r.ReceivedQty,
                        AvailableQty = r.AvailableQty,
                        TotalAmount = r.TotalAmount,
                        BillTo = r.BillTo,
                        ShipTo = r.ShipTo,
                        Partner = r.Partner,
                        PaymentTerms = r.PaymentTerms,
                        PO_Company = r.PO_Company,
                        POCompanyID = r.POCompanyID,
                    }).ToList();

                    ViewData["ItemList"] = itempurchaseOrder;

                    ViewBag.PageNumber = pageNumber;
                    ViewBag.PageSize = pageSize;

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



        [Authorize(Roles = "Administrator, NormalUser")]
        [HttpPost]
        public async Task<IActionResult> CreateEditPO(PurchaseOrders model, string action)
        {
            try
            {
                var claims = User.Claims;

                var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;
                var partnerIdCookie = Request.Cookies["SelectedMFI"];

                model.PartnerID = Convert.ToInt64(partnerIdCookie);
                model.AdminUserID = Convert.ToInt64(userId);

                model.OrderDate = DateTime.Parse(model.OrderDate).ToString("yyyyMMdd");

                if (action == "Create")
                {
                    model.DT_RowId = -1;
                    _PurchaseOrderRepository.CreateOrEditPO(model);
                    return RedirectToAction("Index");
                }

                else if (action == "Edit")
                {
                    _PurchaseOrderRepository.CreateOrEditPO(model);
                    return RedirectToAction("Index");
                }

                return RedirectToAction("Index");

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete]
        [Authorize(Roles = "Administrator, NormalUser")]
        public IActionResult DeletePO(long id)
        {
            try
            {
                var claims = User.Claims;
                var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;

                long AdminUserID = Convert.ToInt64(userId);

                _PurchaseOrderRepository.DeletePo(id, AdminUserID);
                return Json(new { success = true, message = "User deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                return Json(new { success = false, message = "Error deleting user." });
            }
        }
    }
}

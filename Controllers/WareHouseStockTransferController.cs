using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class WareHouseStockTransferController : Controller
    {
        private readonly IWareHouseStockTransfer _wareHouseStockTransfer;
        private readonly ILogger<WareHouseStockTransferController> _logger;
        private readonly ISettings _settingRepository;

        public WareHouseStockTransferController(IWareHouseStockTransfer wareHouseStockTransfer, ILogger<WareHouseStockTransferController> logger, ISettings settingRepository)
        {
            _wareHouseStockTransfer = wareHouseStockTransfer;
            _logger = logger;
            _settingRepository = settingRepository;
        }

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Index()
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

                var wareHouseList = await _wareHouseStockTransfer.GetWareHouseList();
                var ItemList = await _wareHouseStockTransfer.GetItemList();

                var selectwareHouseList = wareHouseList
                    .Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Code })
                    .ToList();

                var selectItemList = ItemList
                    .Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Name })
                    .ToList();

                ViewData["WareHouseCode"] = selectwareHouseList;
                ViewData["ItemList"] = selectItemList;

                return View();
            }
            catch (Exception)
            {

                throw;
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost("GetStock")]
        public async Task<IActionResult> GetStock(long wareHouseID, long itemID)
        {
            try
            {
                var stock = await _wareHouseStockTransfer.GetStockAsync(wareHouseID, itemID);
                return Ok(new { Stock = stock });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the stock.");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("transfer")]
        public async Task<IActionResult> TransferStock([FromForm] WareHouseStockTransferModel model)
        {
            try
            {
                var claims = User.Claims;
                var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;

                model.AdminUserID = Convert.ToInt64(userId);

                model.InvoiceFilePath = "invoice/path/invoice.pdf";

                var result = await _wareHouseStockTransfer.DoWareHouseStockTransfer(model);
                if (result)
                {
                    return Ok(new { message = "Stock transfer successful" });
                }
                return BadRequest(new { message = "Stock transfer failed" });
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

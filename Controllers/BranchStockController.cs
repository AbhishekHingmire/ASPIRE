using DocumentFormat.OpenXml.Wordprocessing;
using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using IMS2.Repository.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class BranchStockController : Controller
    {
        private readonly IBranchStock _branchStockRepository;
        private readonly ILogger<BranchStockController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;
        private readonly IImportSalesOrder _importSalesOrderRepository;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public BranchStockController(IBranchStock branchStockRepository, ILogger<BranchStockController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ISettings settingRepository, ApplicationDbContext context, IImportSalesOrder importSalesOrderRepository)
        {
            _branchStockRepository = branchStockRepository;
            _logger = logger;
            _configuration = configuration;
            _env = webHostEnvironment;
            _settingRepository = settingRepository;
            _importSalesOrderRepository = importSalesOrderRepository;
            _defaultPageNumber = _configuration.GetValue<int>("PaginationCount:PageNumber");
            _defaultPageSize = _configuration.GetValue<int>("PaginationCount:PageSize");
        }

        [Authorize(Roles = "Branch")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                long userId = UserHelper.GetUserId(User);

                if (string.IsNullOrEmpty(name))
                {
                    return BadRequest("User name is missing.");
                }

                var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

                ViewBag.Role = roleClaim;

                var partnerIdCookie = Request.Cookies["SelectedMFI"];
                if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                {
                    return BadRequest("Invalid PartnerID cookie.");
                }

                var branches = await _branchStockRepository.GetBranchAsync(partnerId, branchTypeID, roleClaim, name, branchID,userId);
                ViewData["BranchTypesList"] = branches.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Code }).ToList();

                var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);

                var items = await _branchStockRepository.GetItemNamesAsync(partnerId);
                ViewData["ItemList"] = items;

                ViewBag.MFICBO = mfiList;
                ViewBag.Name = name;

                long BranchTypeID = branchTypeID;
                ViewBag.BranchType = BranchTypeID;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Branch Index page");
                return StatusCode(500, "Internal server error.");
            }
        }



        [HttpPost]
        public async Task<IActionResult> GetBranchCodes(long regionId)
        {
            try
            {
                var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                var partnerIdCookie = Request.Cookies["SelectedMFI"];
                if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                {
                    return BadRequest("Invalid PartnerID cookie.");
                }

                var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

                var branches = await _branchStockRepository.GetBranchCodesByTypeAsync(partnerId, regionId, roleClaim, name, branchTypeID, branchID);

                var branchOptions = branches.Select(b => new { b.ID, b.Code }).ToList();
                return Json(new { Result = "OK", Options = branchOptions });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching branch codes");
                return StatusCode(500, "Internal server error.");
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetBranchStock(string branchCode, long itemId, string datestamp, string regionCode, int pageNumber, int pageSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(datestamp))
                {
                    datestamp = ""; 
                }

                var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                var partnerIdCookie = Request.Cookies["SelectedMFI"];
                if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
                {
                    return BadRequest("Invalid PartnerID cookie.");
                }

                pageNumber = pageNumber > 0 ? pageNumber : _defaultPageNumber;
                pageSize = pageSize > 0 ? pageSize : _defaultPageSize;

                var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

                var branchStockTableData = await _branchStockRepository.GetAllTableData(branchCode, itemId, datestamp, regionCode, branchTypeID, branchID, partnerId, pageNumber, pageSize);

                var branchStockData = branchStockTableData.Select(b => new BranchStockModel
                {
                    SlNo = b.SlNo,
                    Branch = b.Branch,
                    Code = b.Code,
                    Item = b.Item,
                    SKU = b.SKU,
                    Qty = b.Qty,
                    DateTimeStamp = b.DateTimeStamp
                }).ToList();

                return Json(new { Result = "OK", Options = branchStockData });

            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}

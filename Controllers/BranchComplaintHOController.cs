using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class BranchComplaintHOController : Controller
    {
        private readonly IBranchComplaintHO _branchComplaintHORepository;
        private readonly IBranchStock _branchStockRepository;
        private readonly ILogger<BranchComplaintHOController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;
        private readonly IImportSalesOrder _importSalesOrderRepository;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public BranchComplaintHOController(IBranchStock branchStockRepository, ILogger<BranchComplaintHOController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ISettings settingRepository, ApplicationDbContext context, IImportSalesOrder importSalesOrderRepository, IBranchComplaintHO branchComplaintHORepository)
        {
            _branchStockRepository = branchStockRepository;
            _logger = logger;
            _configuration = configuration;
            _env = webHostEnvironment;
            _settingRepository = settingRepository;
            _importSalesOrderRepository = importSalesOrderRepository;
            _defaultPageNumber = _configuration.GetValue<int>("PaginationCount:PageNumber");
            _defaultPageSize = _configuration.GetValue<int>("PaginationCount:PageSize");
            _branchComplaintHORepository = branchComplaintHORepository;
        }

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

                var branches = await _branchStockRepository.GetBranchAsync(partnerId, branchTypeID, roleClaim, name, branchID, userId);
                ViewData["BranchTypesList"] = branches.Select(r => new SelectListItem { Value = r.ID.ToString(), Text = r.Code }).ToList();

                var items = await _branchComplaintHORepository.GetItemNamesAsync(partnerId);
                ViewData["ItemList"] = items;

                var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
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
        public async Task<IActionResult> GetBranchComplaintHOList(string branchCode, long itemId, string regionCode, int pageNumber, int pageSize)
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

                pageNumber = pageNumber > 0 ? pageNumber : _defaultPageNumber;
                pageSize = pageSize > 0 ? pageSize : _defaultPageSize;

                var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

                var branchStockTableData = await _branchComplaintHORepository.GetBranchComplaintHO(branchCode, itemId, regionCode, branchTypeID, branchID, partnerId, pageNumber, pageSize);

                var branchStockData = branchStockTableData.Select(b => new BranchComplaintHOModel
                {
                    SlNo = b.SlNo,
                    ComplaintCode = b.ComplaintCode,
                    ComplaintType = b.ComplaintType,
                    SupplierRefNo = b.SupplierRefNo,
                    State = b.State,
                    Region = b.Region,
                    BranchCode = b.BranchCode,
                    BranchName = b.BranchName,
                    AMBMContactName = b.AMBMContactName,
                    AMBMContactNo = b.AMBMContactNo,
                    Item = b.Item,
                    BranchOrCustomerAddress = b.BranchOrCustomerAddress,
                    BranchOrCustomerContactNo = b.BranchOrCustomerContactNo,
                    NoOfDefectiveUnits = b.NoOfDefectiveUnits,
                    Problem = b.Problem,
                    CreatedOn = b.CreatedOn,
                    Status = b.Status,
                    Resolved = b.Resolved,
                    ResolvedOn = b.ResolvedOn,
                    LastRemark = b.LastRemark,
                    LastRemarkOn = b.LastRemarkOn
                }).ToList();

                return Json(new { Result = "OK", Options = branchStockData });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error. Please try again later.");
            }
        }

    }
}


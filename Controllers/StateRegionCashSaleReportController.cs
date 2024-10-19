using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using DocumentFormat.OpenXml.Spreadsheet;

namespace IMS2.Controllers
{
    public class StateRegionCashSaleReportController : Controller
    {
        private readonly IStateRegionCashSaleReport _stateRegionCashSaleReportRepository;
        private readonly IImportSalesOrder _importSalesOrderRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;
        private readonly ISettings _settingRepository;

        public StateRegionCashSaleReportController(
            IStateRegionCashSaleReport stateRegionCashSaleReportRepository,
            IImportSalesOrder importSalesOrderRepository,
            IConfiguration configuration,
            ISettings _settingsRepository)
        {
            _stateRegionCashSaleReportRepository = stateRegionCashSaleReportRepository;
            _importSalesOrderRepository = importSalesOrderRepository;
            _configuration = configuration;
            _defaultPageNumber = _configuration.GetValue<int>("PaginationCount:PageNumber");
            _defaultPageSize = _configuration.GetValue<int>("PaginationCount:PageSize");
            _settingRepository = _settingsRepository;
        }

        public async Task<IActionResult> Index()
        {
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            long userId = UserHelper.GetUserId(User);

            var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

            var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);

            ViewBag.MFICBO = mfiList;
            ViewBag.Name = name;

            long BranchTypeID = branchTypeID;
            ViewBag.BranchType = BranchTypeID;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetCashSaleReport(int pageNumber, int pageSize)
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

                var StateRegionCashSaleData = await _stateRegionCashSaleReportRepository.GetAllTableData(partnerId, branchTypeID, branchID, pageNumber, pageSize);

                var StateRegionCashSale = StateRegionCashSaleData.Select(b => new StateRegionCashSaleReportModel
                {
                    Branch = b.Branch,
                    Item = b.Item,
                    CustomerName = b.CustomerName,
                    Phone = b.Phone,
                    Address = b.Address,
                    AddressZip = b.AddressZip,
                    EmployeeCode = b.EmployeeCode,
                    CashReciptNo = b.CashReciptNo,
                    OrderStatus = b.OrderStatus,
                    AadharNo = b.AadharNo,
                    CashSaleDate = b.CashSaleDate,
                    State = b.State,
                    Region = b.Region,
                    InvoiceDate = b.InvoiceDate,
                    InvoiceNo = b.InvoiceNo,
                    DeliveryDate = b.DeliveryDate,
                    DisbursementDate = b.DisbursementDate,
                    TotalAmount = b.TotalAmount
                }).ToList();

                return Json(new { Result = "OK", Options = StateRegionCashSale });
            }
            catch (Exception ex)
            {
                // Optionally log the exception or handle it as needed
                return StatusCode(500, "Internal server error");
            }
        }
    }
}

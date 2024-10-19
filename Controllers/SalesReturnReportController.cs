using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class SalesReturnReportController : Controller
    {
        private readonly ISalesReturnReport _SalesReturnReportRepository;
        private readonly ILogger<SalesReturnReportController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;
        public SalesReturnReportController(ISalesReturnReport SalesReturnReportRepository, ILogger<SalesReturnReportController> logger, ISettings settingRepository, IConfiguration configuration)
        {
            _SalesReturnReportRepository = SalesReturnReportRepository;
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
                var userId = UserHelper.GetUserId(User);

                ViewBag.Name = name;
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

                var salesReturnReport = await _SalesReturnReportRepository.GetSalesReturnReport(partnerId, pageNumber, pageSize);

                var salesReturnReportList = salesReturnReport.Select(r => new SalesReturnReportModel
                {
                    OrderNo = r.OrderNo,
                    LoanAppNo = r.LoanAppNo,
                    CustomerID = r.CustomerID,
                    CustomerName = r.CustomerName,
                    SKU = r.SKU,
                    ProductName = r.ProductName,
                    Qty = r.Qty,
                    MRP = r.MRP,
                    BasicRate = r.BasicRate,
                    GSTPercent = r.GSTPercent,
                    CGST = r.CGST,
                    SGST = r.SGST,
                    IGST = r.IGST,
                    SaleRate = r.SaleRate,
                    TotalAmount = r.TotalAmount,
                    BranchID = r.BranchID,
                    BranchName = r.BranchName,
                    OrderDate = r.OrderDate,
                    OrderStatus = r.OrderStatus,
                    AgainstOriginalInvoiceNo = r.AgainstOriginalInvoiceNo,
                    InvoiceNo = r.InvoiceNo,
                    InvoiceDate = r.InvoiceDate
                }).ToList();
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;

                var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                ViewBag.MFICBO = mfiList;

                ViewData["ItemList"] = salesReturnReportList;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading Sales Return Report Index page");
                return StatusCode(500, "Internal server error.");
            }
        }

    }
}

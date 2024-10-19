using ClosedXML.Excel;
using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class ImportGRNController : Controller
    {
        private readonly IImportGRN _importGRNRepository;
        private readonly ILogger<ImportGRNController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;

        public ImportGRNController(IImportGRN importGRNRepository, ILogger<ImportGRNController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ISettings settingRepository)
        {
            _importGRNRepository = importGRNRepository;
            _logger = logger;
            _configuration = configuration;
            _env = webHostEnvironment;
            _settingRepository = settingRepository;
        }

        [Authorize(Roles = "Administrator, NormalUser")]
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

                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.ImportGRN);
                if (screenRight != null)
                {
                    ViewBag.ScreenRightRank = (int)screenRight;

                    var allowedScreens = await UserHelper.GetScreensForUserAsync(userId, _settingRepository);

                    ViewBag.AllowedScreens = allowedScreens;

                    return View();
                }

                return RedirectToAction("Errors404Basic", "Authentication");
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, NormalUser")]
        public async Task<IActionResult> ImportGRNData(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File not selected or empty.");
                return BadRequest("File not selected or empty.");
            }

            int maxTransactionCount = _configuration.GetValue<int>("TransactionSettings:MaxTransactionCount");
            var claims = User.Claims;
            var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;
            var partnerIdCookie = Request.Cookies["SelectedMFI"];

            if (userId == null || partnerIdCookie == null)
            {
                _logger.LogWarning("User ID or Partner ID not found in claims or cookies.");
                return Unauthorized("User ID or Partner ID not found.");
            }

            var transactions = new List<ImportGRNModel>();

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        int totalRecordCount = worksheet.RowsUsed().Count() - 1;

                        for (int row = 2; row <= totalRecordCount + 1; row++)
                        {
                            var transaction = new ImportGRNModel
                            {
                                AdminUserID = Convert.ToInt64(userId),
                                PartnerID = Convert.ToInt64(partnerIdCookie),
                                PORef = worksheet.Cell(row, 1).GetValue<string>(),
                                InvoiceNo = worksheet.Cell(row, 2).GetValue<string>(),
                                InvoiceDate = worksheet.Cell(row, 3).GetValue<string>(),
                                Month = worksheet.Cell(row, 4).GetValue<string>(),
                                Supplier = worksheet.Cell(row, 5).GetValue<string>(),
                                City = worksheet.Cell(row, 6).GetValue<string>(),
                                WareHouseCode = worksheet.Cell(row, 7).GetValue<string>(),
                                WareHouseName = worksheet.Cell(row, 8).GetValue<string>(),
                                ProductName = worksheet.Cell(row, 9).GetValue<string>(),
                                SKU = worksheet.Cell(row, 10).GetValue<string>(),
                                Qty = worksheet.Cell(row, 11).GetValue<int>(),
                                DeliveryDate = worksheet.Cell(row, 12).GetValue<string>(),
                                IMEI_No = worksheet.Cell(row, 13).GetValue<string>(),
                                InvoiceFile = worksheet.Cell(row, 14).GetValue<string>(),
                                InvQty = worksheet.Cell(row, 15).GetValue<int>(),
                                DiffRemarks = worksheet.Cell(row, 16).GetValue<string>(),
                            };

                            transactions.Add(transaction);
                        }
                    }
                }

                if (transactions.Count > maxTransactionCount)
                {
                    _logger.LogError($"Transaction count exceeds the maximum limit of {maxTransactionCount}.");
                    return BadRequest($"Transaction count exceeds the maximum limit of {maxTransactionCount}.");
                }

                var failedTransactions = await _importGRNRepository.ImportGRNs(transactions);

                var successCount = transactions.Count - failedTransactions.Count;
                var totalCount = transactions.Count;

                var response = new
                {
                    TotalCount = totalCount,
                    SuccessCount = successCount,
                    FailedCount = failedTransactions.Count,
                    Message = failedTransactions.Count > 0
                        ? "File uploaded with some errors"
                        : "File uploaded and data saved successfully",
                    ErrorFile = failedTransactions.Count > 0
                        ? Url.Content("~/error-files/" + await CreateErrorExcelFile(file, failedTransactions))
                        : null
                };

                return Ok(response);
            }
            catch (FormatException ex)
            {
                _logger.LogError(ex, "Invalid data format in the uploaded file.");
                return BadRequest("Invalid data format in the uploaded file.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the file.");
                return StatusCode(500, "Internal server error");
            }
        }

        private async Task<string> CreateErrorExcelFile(IFormFile file, List<ImportGRNModel> failedTransactions)
        {
            var tempPath = Path.Combine(_env.WebRootPath, "error-files", $"{Path.GetFileNameWithoutExtension(file.FileName)}_Errors_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Errors");

                worksheet.Cell(1, 1).Value = "PORef";
                worksheet.Cell(1, 2).Value = "InvoiceNo";
                worksheet.Cell(1, 3).Value = "InvoiceDate";
                worksheet.Cell(1, 4).Value = "Month";
                worksheet.Cell(1, 5).Value = "Supplier";
                worksheet.Cell(1, 6).Value = "City";
                worksheet.Cell(1, 7).Value = "WareHouseCode";
                worksheet.Cell(1, 8).Value = "WareHouseName";
                worksheet.Cell(1, 9).Value = "ProductName";
                worksheet.Cell(1, 10).Value = "SKU";
                worksheet.Cell(1, 11).Value = "Qty";
                worksheet.Cell(1, 12).Value = "DeliveryDate";
                worksheet.Cell(1, 13).Value = "IMEI_No";
                worksheet.Cell(1, 14).Value = "InvoiceFile";
                worksheet.Cell(1, 15).Value = "InvQty";
                worksheet.Cell(1, 16).Value = "DiffRemarks";

                for (int i = 0; i < failedTransactions.Count; i++)
                {
                    var transaction = failedTransactions[i];
                    worksheet.Cell(i + 2, 1).Value = transaction.PORef;
                    worksheet.Cell(i + 2, 2).Value = transaction.InvoiceNo;
                    worksheet.Cell(i + 2, 3).Value = transaction.InvoiceDate;
                    worksheet.Cell(i + 2, 4).Value = transaction.Month;
                    worksheet.Cell(i + 2, 5).Value = transaction.Supplier;
                    worksheet.Cell(i + 2, 6).Value = transaction.City;
                    worksheet.Cell(i + 2, 7).Value = transaction.WareHouseCode;
                    worksheet.Cell(i + 2, 8).Value = transaction.WareHouseName;
                    worksheet.Cell(i + 2, 9).Value = transaction.ProductName;
                    worksheet.Cell(i + 2, 10).Value = transaction.SKU;
                    worksheet.Cell(i + 2, 11).Value = transaction.Qty;
                    worksheet.Cell(i + 2, 12).Value = transaction.DeliveryDate;
                    worksheet.Cell(i + 2, 13).Value = transaction.IMEI_No;
                    worksheet.Cell(i + 2, 14).Value = transaction.InvoiceFile;
                    worksheet.Cell(i + 2, 15).Value = transaction.InvQty;
                    worksheet.Cell(i + 2, 16).Value = transaction.DiffRemarks;
                }

                workbook.SaveAs(tempPath);
            }

            return Path.GetFileName(tempPath);
        }
    }
}

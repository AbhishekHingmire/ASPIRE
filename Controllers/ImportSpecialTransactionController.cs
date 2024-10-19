using ClosedXML.Excel;
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
    public class ImportSpecialTransactionController : Controller
    {
        private readonly IImportSpecialTransaction _importSpetialTransactionRepository;
        private readonly ILogger<ImportSpecialTransactionController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;

        public ImportSpecialTransactionController(IImportSpecialTransaction importSpetialTransactionRepository, ILogger<ImportSpecialTransactionController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ISettings settingRepository)
        {
            _importSpetialTransactionRepository = importSpetialTransactionRepository;
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

                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.ImportSpecialSO);
                if (screenRight != null)
                {
                    ViewBag.ScreenRightRank = (int)screenRight;

                    var allowedScreens = await UserHelper.GetScreensForUserAsync(userId, _settingRepository);

                    ViewBag.AllowedScreens = allowedScreens;

                    return View();
                }

                return RedirectToAction("Errors404Basic", "Authentication");
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, NormalUser")]
        public async Task<IActionResult> UploadSpecialTransactions(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("File not selected or empty.");
                return BadRequest("File not selected or empty.");
            }

            int maxTransactionCount = _configuration.GetValue<int>("TransactionSettings:MaxTransactionCount");

            if (maxTransactionCount <= 0)
            {
                _logger.LogError("Invalid maximum transaction count configured.");
                return StatusCode(500, "Internal server error");
            }

            var claims = User.Claims;
            var roleClaim = claims.FirstOrDefault(c => c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
            var userId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;
            var partnerIdCookie = Request.Cookies["SelectedMFI"];

            if (userId == null || partnerIdCookie == null)
            {
                _logger.LogWarning("User ID or Partner ID not found in claims or cookies.");
                return Unauthorized("User ID or Partner ID not found.");
            }

            var transactions = new List<ImportSpecialTransactionModel>();

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
                            var transaction = new ImportSpecialTransactionModel
                            {
                                AdminUserID = Convert.ToInt64(userId),
                                PartnerID = Convert.ToInt64(partnerIdCookie),
                                StateCode = worksheet.Cell(row, 1).GetValue<string>(),
                                StateName = worksheet.Cell(row, 2).GetValue<string>(),
                                WareHouseCode = worksheet.Cell(row, 3).GetValue<string>(),
                                WareHouseName = worksheet.Cell(row, 4).GetValue<string>(),
                                OrderNo = worksheet.Cell(row, 5).GetValue<string>(),
                                LoanAppNo = worksheet.Cell(row, 6).GetValue<string>(),
                                CustomerID = worksheet.Cell(row, 7).GetValue<string>(),
                                CustomerName = worksheet.Cell(row, 8).GetValue<string>(),
                                SKU = worksheet.Cell(row, 9).GetValue<string>(),
                                ProductName = worksheet.Cell(row, 10).GetValue<string>(),
                                Qty = worksheet.Cell(row, 11).GetValue<int>(),
                                Region = worksheet.Cell(row, 12).GetValue<string>(),
                                BranchCode = worksheet.Cell(row, 13).GetValue<string>(),
                                BranchName = worksheet.Cell(row, 14).GetValue<string>(),
                                BillToAddressStateCode = worksheet.Cell(row, 15).GetValue<string>(),
                                BillToAddress = worksheet.Cell(row, 16).GetValue<string>(),
                                ContactNo = worksheet.Cell(row, 17).GetValue<string>(),
                                SpouseName = worksheet.Cell(row, 18).GetValue<string>(),
                                GSTNo = worksheet.Cell(row, 19).GetValue<string>(),
                                DP = worksheet.Cell(row, 20).GetValue<int>(),
                                IMEI_No = worksheet.Cell(row, 21).GetValue<string>(),
                                DPStatus = worksheet.Cell(row, 22).GetValue<string>(),
                                MRP = worksheet.Cell(row, 23).GetValue<decimal>(),
                                Price = worksheet.Cell(row, 24).GetValue<decimal>(),
                                GSTPercent = worksheet.Cell(row, 25).GetValue<decimal>(),
                                ShipToAddress = worksheet.Cell(row, 26).GetValue<string>(),
                                ShipToGSTNo = worksheet.Cell(row, 27).GetValue<string>()
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

                var failedTransactions = await _importSpetialTransactionRepository.AddTransactionsAsync(transactions);

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
                        ? Url.Content($"~/error-files/{await CreateErrorExcelFile(file, failedTransactions)}")
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

        private async Task<string> CreateErrorExcelFile(IFormFile file, List<FailedTransactionModel> failedTransactions)
        {
            var tempPath = Path.Combine(_env.WebRootPath, "error-files", $"{Path.GetFileNameWithoutExtension(file.FileName)}_Errors_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

            try
            {
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Errors");

                    worksheet.Cell(1, 1).Value = "LoanAppNo";
                    worksheet.Cell(1, 2).Value = "Reason";

                    for (int i = 0; i < failedTransactions.Count; i++)
                    {
                        var transaction = failedTransactions[i];
                        worksheet.Cell(i + 2, 1).Value = transaction.LoanAppNo;
                        worksheet.Cell(i + 2, 2).Value = transaction.Reason;
                    }

                    workbook.SaveAs(tempPath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the error file.");
                throw; 
            }

            return Path.GetFileName(tempPath);
        }


    }
}

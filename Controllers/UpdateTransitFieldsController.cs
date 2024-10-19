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
    public class UpdateTransitFieldsController : Controller
    {
        private readonly IUpdateTransitFields _updateTransitFieldsRepository;
        private readonly ILogger<UpdateTransitFieldsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;

        public UpdateTransitFieldsController(IUpdateTransitFields updateTransitFieldsRepository, ILogger<UpdateTransitFieldsController> logger, IConfiguration configuration, IWebHostEnvironment webHostEnvironment, ISettings settingRepository)
        {
            _updateTransitFieldsRepository = updateTransitFieldsRepository;
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

                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.Updatetransitfields);
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
                _logger.LogError(ex, "An error occurred while processing the Index action.");
                throw;
            }
        }


        [HttpPost]
        [Authorize(Roles = "Administrator, NormalUser")]
        public async Task<IActionResult> UploadTransitFieldsData(IFormFile file)
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

            var transactions = new List<UpdateTransitFieldsModel>();

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
                            var transaction = new UpdateTransitFieldsModel
                            {
                                AdminUserID = Convert.ToInt64(userId),
                                PartnerID = Convert.ToInt64(partnerIdCookie),
                                ApplicationNo = worksheet.Cell(row, 1).GetValue<string>(),
                                GR_No = worksheet.Cell(row, 2).GetValue<string>(),
                                EDD = worksheet.Cell(row, 3).GetValue<string>(),
                                TransporterName = worksheet.Cell(row, 4).GetValue<string>(),
                                Mode = worksheet.Cell(row, 5).GetValue<string>(),
                                GrossWeight = worksheet.Cell(row, 6).GetValue<string>(),
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

                var failedTransactions = await _updateTransitFieldsRepository.UpdateTransitData(transactions);

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

        private async Task<string> CreateErrorExcelFile(IFormFile file, List<UpdateTransitFieldsModel> failedTransactions)
        {
            var tempPath = Path.Combine(_env.WebRootPath, "error-files", $"{Path.GetFileNameWithoutExtension(file.FileName)}_Errors_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Errors");

                worksheet.Cell(1, 1).Value = "ApplicationNo";
                worksheet.Cell(1, 2).Value = "GR_No";
                worksheet.Cell(1, 3).Value = "EDD";
                worksheet.Cell(1, 4).Value = "TransporterName";
                worksheet.Cell(1, 5).Value = "Mode";
                worksheet.Cell(1, 6).Value = "GrossWeight";

                for (int i = 0; i < failedTransactions.Count; i++)
                {
                    var transaction = failedTransactions[i];
                    worksheet.Cell(i + 2, 1).Value = transaction.ApplicationNo;
                    worksheet.Cell(i + 2, 2).Value = transaction.GR_No;
                    worksheet.Cell(i + 2, 3).Value = transaction.EDD;
                    worksheet.Cell(i + 2, 4).Value = transaction.TransporterName;
                    worksheet.Cell(i + 2, 5).Value = transaction.Mode;
                    worksheet.Cell(i + 2, 6).Value = transaction.GrossWeight;
                }

                workbook.SaveAs(tempPath);
            }

            return Path.GetFileName(tempPath);
        }

    }
}

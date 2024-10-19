using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml.Style;
using System.Data;
using Microsoft.AspNetCore.Http;
using System.Text;
using OfficeOpenXml;
using ClosedXML.Excel;
using IMS2.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class ImportSalesOrderController : Controller
    {
        private readonly IImportSalesOrder _importSalesOrderRepository;
        private readonly ILogger<ImportSalesOrderController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;

        public ImportSalesOrderController(IImportSalesOrder importSalesOrderRepository, ILogger<ImportSalesOrderController> logger, ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment env, ISettings settingRepository)
        {
            _importSalesOrderRepository = importSalesOrderRepository;
            _logger = logger;
            _configuration = configuration;
            _env = env;
            _settingRepository = settingRepository;
        }

        public ImportSalesOrderController(ControllerContext context)
        {
            this.ControllerContext = context;
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

                var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.ImportSalesOrder);
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
        public async Task<IActionResult> UploadSalesOrder(IFormFile file)
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

            var transactions = new List<TransactionModel>();
            int totalRecordCount = 0;

            try
            {
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    stream.Position = 0;

                    using (var workbook = new XLWorkbook(stream))
                    {
                        var worksheet = workbook.Worksheet(1);
                        totalRecordCount = worksheet.RowsUsed().Count() - 1;

                        for (int row = 2; row <= totalRecordCount + 1; row++)
                        {
                            var transaction = new TransactionModel
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
                                Qty = int.TryParse(worksheet.Cell(row, 11).Value.ToString(), out int qty) ? qty : 0,
                                Region = worksheet.Cell(row, 12).GetValue<string>(),
                                BranchCode = worksheet.Cell(row, 13).GetValue<string>(),
                                BranchName = worksheet.Cell(row, 14).GetValue<string>(),
                                BillToAddressStateCode = worksheet.Cell(row, 15).GetValue<string>(),
                                BillToAddress = worksheet.Cell(row, 16).GetValue<string>(),
                                ContactNo = worksheet.Cell(row, 17).GetValue<string>(),
                                SpouseName = worksheet.Cell(row, 18).GetValue<string>(),
                                GSTNo = worksheet.Cell(row, 19).GetValue<string>(),
                                DP = int.TryParse(worksheet.Cell(row, 20).Value.ToString(), out int dp) ? dp : 0,
                                IMEI_No = worksheet.Cell(row, 21).GetValue<string>(),
                                DPStatus = worksheet.Cell(row, 22).GetValue<string>(),
                                AltContactNo = worksheet.Cell(row, 23).GetValue<string>(),
                                ShipToAddress = worksheet.Cell(row, 24).GetValue<string>(),
                                ShipToGSTNo = worksheet.Cell(row, 25).GetValue<string>()
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

                var failedTransactions = await _importSalesOrderRepository.AddTransactionsAsync(transactions);

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
                    ErrorFile = failedTransactions.Count > 0 ? Url.Content($"~/error-files/{await CreateErrorExcelFile(file, failedTransactions)}") : null
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
            var errorDirectory = Path.Combine(_env.WebRootPath, "error-files");
            Directory.CreateDirectory(errorDirectory);
            var tempPath = Path.Combine(errorDirectory, $"{Path.GetFileNameWithoutExtension(file.FileName)}_Errors_{DateTime.Now:yyyyMMddHHmmss}.xlsx");

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Errors");

                worksheet.Cell(1, 1).Value = "OrderNo";
                worksheet.Cell(1, 2).Value = "Reason";

                for (int i = 0; i < failedTransactions.Count; i++)
                {
                    var transaction = failedTransactions[i];
                    worksheet.Cell(i + 2, 1).Value = transaction.OrderNo;
                    worksheet.Cell(i + 2, 2).Value = transaction.Reason;  
                }

                workbook.SaveAs(tempPath);
            }

            return Path.GetFileName(tempPath);
        }


        [HttpGet]
        public void ExportTemplate(long ID, bool IsWithData, params object[] args)
        {
            if (args != null)
                foreach (var str in args) { if (_importSalesOrderRepository.IsSQLInjection(str.ToString())) { Response.Redirect("/Home/Error"); return; } }
            ExcelPackage epackage = GetExportTemplate(ID, IsWithData, args);
            DownloadExportTemplate(ID, epackage);
        }

        public ExcelPackage GetExportTemplate(long ID, bool IsWithData, params object[] args)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var FrozenRows = 1;
            List<DataTable> lDTs = null;
            var etlConfig = GetEtlConfig(ID);
            string Procedure = ""; if (IsWithData) Procedure = etlConfig.ExportStoredProc;
            if (!string.IsNullOrEmpty(Procedure))
            {
                var ProcParts = _importSalesOrderRepository.GetProcedureSignatureInParts(Procedure).Split('|');
                var sqlDataExtract = ProcParts[0];
                if (ProcParts.Length == 2 && !string.IsNullOrEmpty(ProcParts[1])) sqlDataExtract += String.Format(ProcParts[1], args);
                lDTs = _importSalesOrderRepository.ExecSQL(sqlDataExtract);
                FrozenRows = lDTs.Count;
            }
            ExcelPackage epackage = new ExcelPackage();
            epackage.Workbook.Properties.Author = GetAuthor(ID);
            ExcelWorksheet excel = epackage.Workbook.Worksheets.Add(string.IsNullOrEmpty(etlConfig.ExcelTabName) ? "Sheet1" : etlConfig.ExcelTabName);

            var lTitles = etlConfig.ParamTitles.Split('|').Where(x => !String.IsNullOrEmpty(x)).Select(x => x.Split('≡')[1]).ToList();
            var lWidths = etlConfig.ParamColWidth.Split('|').Where(x => !String.IsNullOrEmpty(x)).Select(x => x.Split('≡')[1]).ToList();
            for (var i = 0; i < lTitles.Count; i++)
            {
                excel.Cells[GetExcelColumnName(i + 1) + FrozenRows].Value = lTitles[i];
                var strWidth = lWidths[i]; var iWidth = String.IsNullOrEmpty(strWidth) ? 0 : Convert.ToInt32(strWidth);
                if (iWidth == 0) excel.Column(i + 1).Hidden = true; else excel.Column(i + 1).Width = iWidth;
                if (lTitles[i].ToLower().Contains("date")) excel.Column(i + 1).Style.Numberformat.Format = "@";
            }

            if (lDTs != null && lDTs.Count == 2) //Export Data Header Row
            {
                var cellRange = excel.Cells["A1:" + GetExcelColumnName(lWidths.Count) + "1"]; cellRange.Merge = true;
                var cell = excel.Cells["A1"]; cell.Style.Font.Bold = true; cell.Value = lDTs[0].Rows[0][0];
            }
            if (lDTs != null) excel.Cells["A" + (FrozenRows + 1)].LoadFromDataTable(lDTs[FrozenRows - 1], false);  //Export Data Rows

            excel.Cells["1:" + FrozenRows].Style.Font.Bold = true;
            excel.Cells["1:" + FrozenRows].Style.WrapText = true;
            excel.Cells["1:" + FrozenRows].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            excel.Cells["1:" + FrozenRows].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            excel.Cells["1:" + FrozenRows].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;

            excel.View.FreezePanes(FrozenRows + 1, etlConfig.FrozonCols + 1);

            if (!String.IsNullOrEmpty(etlConfig.ExcelPassword))
            {
                excel.Cells["A:" + GetExcelColumnName(lTitles.Count)].Style.Locked = false;
                excel.Cells["1:" + FrozenRows].Style.Locked = true;
                excel.Protection.SetPassword(etlConfig.ExcelPassword);
                excel.Protection.AllowFormatColumns = true;
                excel.Protection.AllowFormatRows = true;
                excel.Protection.AllowDeleteRows = true;
                excel.Protection.AllowFormatCells = false;
                excel.Protection.AllowInsertRows = true;
                excel.Protection.AllowSelectLockedCells = true;
                excel.Protection.AllowSelectUnlockedCells = true;
                excel.Protection.AllowSort = true;
                excel.Protection.AllowAutoFilter = true;
                excel.Protection.IsProtected = true;
            }
            return epackage;
        }

        private ImportSalesOrder GetEtlConfig(long ID)
        {
            var dt = _importSalesOrderRepository.ExecSQL(string.Format("SELECT * FROM ETLConfig WHERE ID={0}", ID))[0];
            var etlConfig = new ImportSalesOrder { };
            if (dt.Rows.Count > 0)
            {
                etlConfig.ID = Convert.ToInt64(dt.Rows[0]["ID"]);
                etlConfig.Title = Convert.ToString(dt.Rows[0]["Title"]);
                etlConfig.StoredProc = Convert.ToString(dt.Rows[0]["StoredProc"]);
                etlConfig.ParamTitles = Convert.ToString(dt.Rows[0]["ParamTitles"]);
                etlConfig.ParamColWidth = Convert.ToString(dt.Rows[0]["ParamColWidth"]);
                etlConfig.FrozonCols = Convert.ToInt32(dt.Rows[0]["FrozonCols"]);
                etlConfig.ExcelTabName = Convert.ToString(dt.Rows[0]["ExcelTabName"]);
                etlConfig.ExcelPassword = Convert.ToString(dt.Rows[0]["ExcelPassword"]);
                etlConfig.ExportStoredProc = Convert.ToString(dt.Rows[0]["ExportStoredProc"]);
            }
            return etlConfig;
        }

        private string GetExcelColumnName(int columnNumber)
        {
            int dividend = columnNumber;
            string columnName = String.Empty;
            int modulo;
            while (dividend > 0)
            {
                modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo).ToString() + columnName;
                dividend = (int)((dividend - modulo) / 26);
            }
            return columnName;
        }

        private string GetAuthor(long ID)
        {
            var host = Request.Host.Value;
            return $"Intech_{host}_{ID}";
        }

        public void DownloadExportTemplate(long ID, ExcelPackage epackage)
        {
            string strTitle = Convert.ToString(_importSalesOrderRepository.ExecSQL(string.Format("SELECT Title FROM ETLConfig WHERE ID={0}", ID))[0].Rows[0]["Title"]);
            string fileName = System.Text.RegularExpressions.Regex.Replace(strTitle.Trim().Replace(" ", ""), "[^0-9a-zA-Z]+", "");
            string attachment = "attachment; filename=" + fileName + ".xlsx";
            Response.Clear();
            Response.Headers.Clear();
            Response.Headers.Add("content-disposition", attachment);
            Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            Response.Body.WriteAsync(epackage.GetAsByteArray());
            epackage.Dispose();
        }
    }
}

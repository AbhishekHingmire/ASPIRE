using IMS2.Helpers;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using System.Security.Claims;
using System.Data;
using System.Drawing;

namespace IMS2.Controllers
{
    public class GITReportController : Controller
    {
        private readonly ILogger<GITReportController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;
        private readonly IImportSalesOrder _importSalesOrderRepository;
        private readonly IGITReport _gITReportRepository;

        public GITReportController(ILogger<GITReportController> logger, ApplicationDbContext context, IConfiguration configuration, IWebHostEnvironment env, ISettings settingRepository, IImportSalesOrder importSalesOrderRepository, IGITReport gITReportRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _env = env;
            _settingRepository = settingRepository;
            _importSalesOrderRepository = importSalesOrderRepository;
            _gITReportRepository = gITReportRepository;
        }

        public async Task<IActionResult> Index()
        {
            var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var userId = UserHelper.GetUserId(User);
            var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.SalesReport);

            var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);

            ViewBag.MFICBO = mfiList;
            ViewBag.Role = roleClaim;
            ViewBag.Name = name;

            var partnerIdCookie = Request.Cookies["SelectedMFI"];
            if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
            {
                return BadRequest("Invalid PartnerID cookie.");
            }

            var provinceNames = await _gITReportRepository.GetProvinceNames(roleClaim, partnerId, name);
            ViewData["StateList"] = provinceNames.Select(r => new SelectListItem { Value = r, Text = r }).ToList();

            var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

            var branches = await _gITReportRepository.GetBranchAsync(partnerId);
            ViewData["BranchTypesList"] = branches.Select(r => new SelectListItem { Value = r, Text = r }).ToList();

            var items = await _gITReportRepository.GetItemNamesAsync(partnerId);
            ViewData["ItemList"] = items;

            long BranchTypeID = branchTypeID;
            ViewBag.BranchType = BranchTypeID;

            return View();

        }

        public void ExportSalesReport(string State, string RegionCode, string FromDate, string ToDate)
        {
            try
            {
                FromDate = string.IsNullOrWhiteSpace(FromDate) ? "" : FromDate;
                ToDate = string.IsNullOrWhiteSpace(ToDate) ? "" : ToDate;
                State = string.IsNullOrWhiteSpace(State) ? "-" : State.Trim();
                RegionCode = string.IsNullOrWhiteSpace(RegionCode) ? "" : RegionCode.Trim();

                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

                //var etlCtrl = new ExportService(this.ControllerContext);
                var ETLID = 9;

                ExportTemplate(ETLID, true, new object[]
                {
                    Request.Cookies["SelectedMFI"], State, RegionCode, FromDate, ToDate, name
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while exporting the sales report.");
                throw;
            }
        }

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

            if (lDTs != null && lDTs.Count == 2)
            {
                var cellRange = excel.Cells["A1:" + GetExcelColumnName(lWidths.Count) + "1"]; cellRange.Merge = true;
                var cell = excel.Cells["A1"]; cell.Style.Font.Bold = true; cell.Value = lDTs[0].Rows[0][0];
            }
            if (lDTs != null) excel.Cells["A" + (FrozenRows + 1)].LoadFromDataTable(lDTs[FrozenRows - 1], false);

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

        private Models.ImportSalesOrder GetEtlConfig(long ID)
        {
            var dt = _importSalesOrderRepository.ExecSQL(string.Format("SELECT * FROM ETLConfig WHERE ID={0}", ID))[0];
            var etlConfig = new Models.ImportSalesOrder { };
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

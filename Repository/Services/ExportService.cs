using System.Data;
using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Text;
using IMS2;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using IMS2.Repository.Interfaces;
using Azure.Core;
using Azure;
using OfficeOpenXml.Style;
using OfficeOpenXml;
using IMS2.Models;
using Microsoft.AspNetCore.Mvc;

public class ExportService : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ExportService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public ExportService(
    ApplicationDbContext context,
    ILogger<ExportService> logger,
    IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("DefaultConnection") ?? "";
        _logger.LogInformation("Connection String: {ConnectionString}", _connectionString);

    }

    public ExportService(ControllerContext context)
    {
        this.ControllerContext = context;
    }

    public void ExportTemplate(long ID, bool IsWithData, params object[] args)
    {
        if (args != null)
            foreach (var str in args) { if (IsSQLInjection(str.ToString())) { Response.Redirect("/Home/Error"); return; } }
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
            var ProcParts = GetProcedureSignatureInParts(Procedure).Split('|');
            var sqlDataExtract = ProcParts[0];
            if (ProcParts.Length == 2 && !string.IsNullOrEmpty(ProcParts[1])) sqlDataExtract += String.Format(ProcParts[1], args);
            lDTs = ExecSQL(sqlDataExtract);
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

    private ImportSalesOrder GetEtlConfig(long ID)
    {
        var dt = ExecSQL(string.Format("SELECT * FROM ETLConfig WHERE ID={0}", ID))[0];
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
        string strTitle = Convert.ToString(ExecSQL(string.Format("SELECT Title FROM ETLConfig WHERE ID={0}", ID))[0].Rows[0]["Title"]);
        string fileName = Regex.Replace(strTitle.Trim().Replace(" ", ""), "[^0-9a-zA-Z]+", "");
        string attachment = "attachment; filename=" + fileName + ".xlsx";
        Response.Clear();
        Response.Headers.Clear();
        Response.Headers.Add("content-disposition", attachment);
        Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        Response.Body.WriteAsync(epackage.GetAsByteArray());
        epackage.Dispose();
    }

    public List<DataTable> ExecSQL(string commandText)
    {
        var resultTables = new List<DataTable>();

        var connectionString = _connectionString;

        try
        {
            using (var connection = new SqlConnection(connectionString))
            using (var command = new SqlCommand(commandText, connection))
            using (var adapter = new SqlDataAdapter(command))
            {
                command.CommandTimeout = 0;

                var dataSet = new DataSet();
                adapter.Fill(dataSet);

                foreach (DataTable table in dataSet.Tables)
                {
                    resultTables.Add(table);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing SQL command.");
        }

        return resultTables;
    }

    public string GetProcedureSignatureInParts(string procedure)
    {
        var sqlFirst = "EXEC " + procedure;
        var sqlSecond = GetProcedureSignature(procedure).Substring(sqlFirst.Length + 1);

        sqlSecond = Regex.Replace(sqlSecond, @"\{ (\d+) \}", m => $"{{{m.Groups[1].Value}}}");

        return sqlFirst + " | " + sqlSecond;
    }

    public string GetProcedureSignature(string procedure)
    {
        var sql = new StringBuilder();
        sql.Append("EXEC ").Append(procedure).Append(" ");

        var dtParams = GetProceduresParameters(procedure, false);
        var offset = 0;

        foreach (DataRow row in dtParams.Rows)
        {
            var name = row["ParameterName"].ToString();
            var dataType = row["ParameterDataType"].ToString();
            var size = Convert.ToInt16(row["ParameterMaxBytes"]);

            var parameterString = dataType switch
            {
                "bigint" or "int" or "tinyint" or "decimal" or "bit" => $"{name}={{ {(offset++)} }}",
                _ => $"{name}='{{ {(offset++)} }}'"
            };

            sql.Append((offset > 1 ? ", " : "") + parameterString);
        }

        return sql.ToString();
    }

    public DataTable GetProceduresParameters(string procedure, bool ignoreSpecialCols)
    {
        var query = new StringBuilder();
        query.AppendLine("SELECT ")
             .AppendLine("    P.name AS [ParameterName],")
             .AppendLine("    TYPE_NAME(P.user_type_id) AS [ParameterDataType],")
             .AppendLine("    P.max_length AS [ParameterMaxBytes]")
             .AppendLine("FROM Sys.Objects AS SO")
             .AppendLine("INNER JOIN Sys.Parameters AS P ON SO.OBJECT_ID = P.OBJECT_ID")
             .AppendLine($"WHERE SO.Name = '{procedure}'");

        if (ignoreSpecialCols)
        {
            query.AppendLine("    AND P.name NOT IN ('@OrgID', '@UserID')");
        }

        query.AppendLine("ORDER BY P.Parameter_ID");

        return ExecSQL(query.ToString())[0];
    }

    public DataTable ReadAsDataTable(Stream inputStream, int tableRowStartIndex)
    {
        OfficeOpenXml.ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;

        using (var pck = new OfficeOpenXml.ExcelPackage(inputStream))
        {
            var ws = pck.Workbook.Worksheets.First();
            DataTable tbl = new DataTable();

            foreach (var firstRowCell in ws.Cells[tableRowStartIndex, 1, tableRowStartIndex, ws.Dimension.End.Column])
            {
                tbl.Columns.Add(firstRowCell.Text);
            }
            var startRow = tableRowStartIndex + 1;
            for (var rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
            {
                var wsRow = ws.Cells[rowNum, 1, rowNum, ws.Dimension.End.Column];
                var row = tbl.NewRow();
                foreach (var cell in wsRow)
                {
                    row[cell.Start.Column - 1] = cell.Text;
                }
                tbl.Rows.Add(row);
            }
            return tbl;
        }
    }

    public string FormatDatepickerToDatestamp(string str)
    {
        if (String.IsNullOrEmpty(str)) return "";
        DateTime tempDate = TryParseDate(str);
        return tempDate != null ? tempDate.ToString("yyyyMMdd") : string.Empty;
    }

    private static DateTime TryParseDate(string str)
    {
        string[] formats = { "dd/MM/yyyy", "dd-MM-yyyy" };
        DateTime tempDate; DateTime.TryParseExact(str, formats, new CultureInfo("en-US"), DateTimeStyles.None, out tempDate);
        return tempDate;
    }

    public int ExecNonQuery(string commandText)
    {
        var connectionString = _context.Database.GetConnectionString();

        using (var connection = new SqlConnection(connectionString))
        using (var command = new SqlCommand(commandText, connection))
        {
            command.CommandType = CommandType.Text;
            command.CommandTimeout = 6000;

            try
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing non-query SQL command.");
                throw;
            }
        }
    }

    public bool IsSQLInjection(string DataSQL)
    {
        var lKeywords = new List<string>(){
                "DBCC","DESC","DISK","DROP","DUMP","ELSE","EXEC","EXIT","GOTO","JOIN","KILL","LOAD","NULL","OPEN","OVER","PLAN","PROC","READ","THEN","TRAN","USER","VIEW","WHEN","ALTER","BEGIN","BREAK","CLOSE",
                "CROSS","FETCH","FLOAT","GRANT","GROUP","INDEX","INNER","MERGE","ORDER","OUTER","PIVOT","PRINT","TABLE","UNION","WHERE","WHILE","BACKUP","BIGINT","BROWSE","COLUMN","COMMIT","CREATE","CURSOR",
                "DELETE","DOUBLE","ERRLVL","ESCAPE","EXCEPT","EXISTS","HAVING","INSERT","LINENO","NULLIF","OPTION","PUBLIC","RETURN","REVERT","REVOKE","SCHEMA","SELECT","UNIQUE","UPDATE","VALUES","BETWEEN",
                "CASCADE","COLLATE","COMPUTE","CONVERT","CURRENT","DECIMAL","DECLARE","DEFAULT","EXECUTE","FOREIGN","NOCHECK","OFFSETS","OPENXML","PERCENT","PRIMARY","RESTORE","SETUSER","TRIGGER","TSEQUAL",
                "UNPIVOT","VARCHAR","VARYING","WAITFOR","COALESCE","CONTAINS","CONTINUE","DATABASE","DATETIME","DISTINCT","EXTERNAL","FREETEXT","FUNCTION","HOLDLOCK","IDENTITY","NATIONAL","READTEXT",
                "RESTRICT","ROLLBACK","ROWCOUNT","SHUTDOWN","TEXTSIZE","TRUNCATE","CLUSTERED","INTERSECT","OPENQUERY","PRECISION","PROCEDURE","RAISERROR","WRITETEXT","CHECKPOINT","CONSTRAINT","DEALLOCATE",
                "FILLFACTOR","OPENROWSET","REFERENCES","ROWGUIDCOL","STATISTICS","UPDATETEXT","DISTRIBUTED","IDENTITYCOL","RECONFIGURE","REPLICATION","SYSTEM_USER","TABLESAMPLE","TRANSACTION","CURRENT_DATE",
                "CURRENT_TIME","CURRENT_USER","NONCLUSTERED","SESSION_USER","AUTHORIZATION","CONTAINSTABLE","FREETEXTTABLE","SECURITYAUDIT","OPENDATASOURCE","IDENTITY_INSERT","CURRENT_TIMESTAMP"
            };
        foreach (string keyword in lKeywords) { if (DataSQL.Contains(keyword)) return true; }
        return false;
    }
}

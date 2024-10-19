using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace IMS2.Repository.Services
{
    public class ImportSalesOrder : IImportSalesOrder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImportSalesOrder> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;
        private readonly string _connectionString;

        public ImportSalesOrder(ApplicationDbContext context, ILogger<ImportSalesOrder> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _batchSize = int.Parse(_configuration["BatchSettings:BatchSize"] ??"");
            _connectionString = _configuration.GetConnectionString("DefaultConnection")??"";
        }

        public async Task<List<FailedTransactionModel>> AddTransactionsAsync(List<TransactionModel> transactions)
        {
            var failedTransactions = new List<FailedTransactionModel>();

            if (transactions == null || transactions.Count == 0)
            {
                _logger.LogWarning("No transactions to process.");
                throw new ArgumentException("Transaction list is empty.");
            }

            try
            {
                for (int i = 0; i < transactions.Count; i += _batchSize)
                {
                    var batch = transactions.Skip(i).Take(_batchSize).ToList();
                    var dataTable = CreateDataTable();

                    foreach (var transaction in batch)
                    {
                        try
                        {
                            if (string.IsNullOrWhiteSpace(transaction.LoanAppNo))
                            {
                                _logger.LogWarning("LoanAppNo is empty for transaction with OrderNo: {OrderNo}", transaction.OrderNo);
                                failedTransactions.Add(new FailedTransactionModel
                                {
                                    OrderNo = transaction.OrderNo,
                                    Reason = "LoanAppNo is empty"
                                });
                                continue;
                            }

                            transaction.LoanAppNo = transaction.LoanAppNo.Replace(" ", "").Replace("\t", "");

                            if (!transaction.BranchCode.StartsWith("B"))
                            {
                                transaction.BranchCode = "B" + transaction.BranchCode;
                            }

                            dataTable.Rows.Add(
                                dataTable.Rows.Count + 1,
                                transaction.StateCode,
                                transaction.StateName,
                                transaction.WareHouseCode,
                                transaction.WareHouseName,
                                transaction.OrderNo,
                                transaction.LoanAppNo,
                                transaction.CustomerID,
                                transaction.CustomerName,
                                transaction.SKU,
                                transaction.ProductName,
                                transaction.Qty == 0 ? 1 : transaction.Qty,
                                transaction.Region,
                                transaction.BranchCode,
                                transaction.BranchName,
                                transaction.BillToAddressStateCode,
                                transaction.BillToAddress,
                                transaction.ContactNo,
                                transaction.SpouseName,
                                transaction.GSTNo,
                                transaction.DP,
                                transaction.IMEI_No,
                                transaction.DPStatus,
                                transaction.AltContactNo,
                                transaction.ShipToAddress,
                                transaction.ShipToGSTNo
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process transaction with OrderNo: {OrderNo}", transaction.OrderNo);
                            failedTransactions.Add(new FailedTransactionModel
                            {
                                OrderNo = transaction.OrderNo,
                                Reason = "Processing error"
                            });
                        }
                    }

                    if (dataTable.Rows.Count == 0)
                    {
                        _logger.LogWarning("No valid transactions to process in batch.");
                        continue;
                    }

                    var parameters = new[]
                    {
                        new SqlParameter("@AdminUserID", SqlDbType.BigInt) { Value = batch[0].AdminUserID },
                        new SqlParameter("@PartnerID", SqlDbType.BigInt) { Value = batch[0].PartnerID },
                        new SqlParameter
                        {
                            ParameterName = "@ImportSO",
                            SqlDbType = SqlDbType.Structured,
                            TypeName = "dbo.T_ImportSO",
                            Value = dataTable
                        }
                    };

                    try
                    {
                        var failedRecords = new List<FailedTransactionModel>();

                        using (var command = _context.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = "EXEC usp_ETL_Transactions_Batch @AdminUserID, @PartnerID, @ImportSO";
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddRange(parameters);

                            await _context.Database.OpenConnectionAsync();
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    failedRecords.Add(new FailedTransactionModel
                                    {
                                        OrderNo = reader["OrderNo"].ToString(),
                                        Reason = reader["Reason"].ToString()
                                    });
                                }
                            }
                        }

                        foreach (var record in failedRecords)
                        {
                            var failedTransaction = batch.FirstOrDefault(t => t.OrderNo == record.OrderNo);
                            if (failedTransaction != null)
                            {
                                failedTransactions.Add(new FailedTransactionModel
                                {
                                    OrderNo = failedTransaction.OrderNo,
                                    Reason = record.Reason
                                });
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        _logger.LogError(ex, "SQL error occurred while adding transactions.");
                        foreach (var transaction in batch)
                        {
                            failedTransactions.Add(new FailedTransactionModel
                            {
                                OrderNo = transaction.OrderNo,
                                Reason = "SQL error"
                            });
                        }
                    }
                }

                _logger.LogInformation("Transactions processed in batches.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while adding transactions.");
                throw;
            }

            foreach (var failedTransaction in failedTransactions)
            {
                _logger.LogWarning("Failed transaction: {OrderNo}, Reason: {Reason}", failedTransaction.OrderNo, failedTransaction.Reason);
            }

            return failedTransactions;
        }

        private DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("StateCode", typeof(string));
            dataTable.Columns.Add("StateName", typeof(string));
            dataTable.Columns.Add("WareHouseCode", typeof(string));
            dataTable.Columns.Add("WareHouseName", typeof(string));
            dataTable.Columns.Add("OrderNo", typeof(string));
            dataTable.Columns.Add("LoanAppNo", typeof(string));
            dataTable.Columns.Add("CustomerID", typeof(string));
            dataTable.Columns.Add("CustomerName", typeof(string));
            dataTable.Columns.Add("SKU", typeof(string));
            dataTable.Columns.Add("ProductName", typeof(string));
            dataTable.Columns.Add("Qty", typeof(int));
            dataTable.Columns.Add("Region", typeof(string));
            dataTable.Columns.Add("BranchCode", typeof(string));
            dataTable.Columns.Add("BranchName", typeof(string));
            dataTable.Columns.Add("BillToAddressStateCode", typeof(string));
            dataTable.Columns.Add("BillToAddress", typeof(string));
            dataTable.Columns.Add("ContactNo", typeof(string));
            dataTable.Columns.Add("SpouseName", typeof(string));
            dataTable.Columns.Add("GSTNo", typeof(string));
            dataTable.Columns.Add("DP", typeof(decimal));
            dataTable.Columns.Add("IMEI_No", typeof(string));
            dataTable.Columns.Add("DPStatus", typeof(string));
            dataTable.Columns.Add("AltContactNo", typeof(string));
            dataTable.Columns.Add("ShipToAddress", typeof(string));
            dataTable.Columns.Add("ShipToGSTNo", typeof(string));

            return dataTable;
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
}

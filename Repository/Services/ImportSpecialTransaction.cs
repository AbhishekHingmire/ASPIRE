using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IMS2.Repository.Services
{
    public class ImportSpecialTransaction : IImportSpecialTransaction
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImportSpecialTransaction> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;

        public ImportSpecialTransaction(ApplicationDbContext context, ILogger<ImportSpecialTransaction> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _batchSize = int.Parse(_configuration["BatchSettings:BatchSize"]);
        }

        public async Task<List<FailedTransactionModel>> AddTransactionsAsync(List<ImportSpecialTransactionModel> transactions)
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
                                    LoanAppNo = transaction.OrderNo,
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
                                transaction.Qty > 0 ? transaction.Qty : 1,
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
                                transaction.MRP,
                                transaction.Price,
                                transaction.GSTPercent,
                                transaction.ShipToAddress,
                                transaction.ShipToGSTNo
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process transaction with OrderNo: {OrderNo}", transaction.OrderNo);
                            failedTransactions.Add(new FailedTransactionModel
                            {
                                LoanAppNo = transaction.OrderNo,
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
                        new SqlParameter("@AdminUserID", SqlDbType.BigInt) { Value = transactions[0].AdminUserID },
                        new SqlParameter("@PartnerID", SqlDbType.BigInt) { Value = transactions[0].PartnerID },
                        new SqlParameter("@ImportSO", SqlDbType.Structured)
                        {
                            TypeName = "dbo.ST_ImportSO",
                            Value = dataTable
                        }
                    };

                    try
                    {
                        using (var connection = _context.Database.GetDbConnection())
                        {
                            await connection.OpenAsync();
                            using (var command = connection.CreateCommand())
                            {
                                command.CommandText = "EXEC usp_ETL_SpecialTransactions_Batch @AdminUserID, @PartnerID, @ImportSO";
                                command.CommandType = CommandType.Text;
                                command.Parameters.AddRange(parameters);

                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    var failedRecords = new List<FailedTransactionModel>();

                                    while (await reader.ReadAsync())
                                    {
                                        failedRecords.Add(new FailedTransactionModel
                                        {
                                            LoanAppNo = reader["LoanAppNo"].ToString(),
                                            Reason = reader["Reason"].ToString()
                                        });
                                    }

                                    failedTransactions.AddRange(failedRecords);
                                }
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        _logger.LogError(ex, "SQL error occurred while adding transactions.");
                        failedTransactions.AddRange(batch.Select(t => new FailedTransactionModel
                        {
                            LoanAppNo = t.LoanAppNo,
                            Reason = "SQL error"
                        }));
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
                _logger.LogWarning("Failed transaction: {OrderNo}, Reason: {Reason}", failedTransaction.LoanAppNo, failedTransaction.Reason ?? "Processing error or SQL error");
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
            dataTable.Columns.Add("DP", typeof(int));
            dataTable.Columns.Add("IMEI_No", typeof(string));
            dataTable.Columns.Add("DPStatus", typeof(string));
            dataTable.Columns.Add("MRP", typeof(decimal));
            dataTable.Columns.Add("Price", typeof(decimal));
            dataTable.Columns.Add("GSTPercent", typeof(decimal));
            dataTable.Columns.Add("ShipToAddress", typeof(string));
            dataTable.Columns.Add("ShipToGSTNo", typeof(string));

            return dataTable;
        }

    }
}

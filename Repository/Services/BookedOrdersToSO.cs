using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IMS2.Repository.Services
{
    public class BookedOrdersToSO : IBookedOrdersToSO
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookedOrdersToSO> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;

        public BookedOrdersToSO(ApplicationDbContext context, ILogger<BookedOrdersToSO> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _batchSize = int.Parse(_configuration["BatchSettings:BatchSize"] ??"");
        }

        public async Task<List<BookedOrdersToSOModel>> UpdateBookedOrdersSO(List<BookedOrdersToSOModel> transactions)
        {
            var failedTransactions = new List<BookedOrdersToSOModel>();

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
                            if (string.IsNullOrWhiteSpace(transaction.BookedLoanAppNo))
                            {
                                _logger.LogWarning("LoanAppNo is empty for transaction with AppNo: {AppNo}", transaction.BookedLoanAppNo);
                                failedTransactions.Add(transaction);
                                continue;
                            }

                            dataTable.Rows.Add(
                                dataTable.Rows.Count + 1,
                                transaction.StateCode,
                                transaction.StateName,
                                transaction.WareHouseCode,
                                transaction.WareHouseName,
                                transaction.BookedLoanAppNo,
                                transaction.SKU,
                                transaction.ProductName,
                                transaction.Qty,
                                transaction.BranchCode,
                                transaction.BranchName,
                                transaction.BillToAddressStateCode,
                                transaction.GSTNo,
                                transaction.DP,
                                transaction.IMEI_No,
                                transaction.DPStatus,
                                transaction.AltContactNo
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process transaction with AppNo: {AppNo}", transaction.BookedLoanAppNo);
                            failedTransactions.Add(transaction);
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
                        new SqlParameter("@BookedOrders", SqlDbType.Structured)
                        {
                            TypeName = "dbo.T_UpdateSOTransits",
                            Value = dataTable
                        }
                    };

                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC dbo.usp_ETL_BookedOrdersToSO_Batch @AdminUserID, @PartnerID, @BookedOrders",
                            parameters);
                    }
                    catch (SqlException ex)
                    {
                        _logger.LogError(ex, "SQL error occurred while adding transactions.");
                        failedTransactions.AddRange(batch);
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
                _logger.LogWarning("Failed transaction: {AppNo}, Reason: Processing error or SQL error", failedTransaction.BookedLoanAppNo);
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
            dataTable.Columns.Add("BookedLoanAppNo", typeof(string));
            dataTable.Columns.Add("SKU", typeof(string));
            dataTable.Columns.Add("ProductName", typeof(string));
            dataTable.Columns.Add("Qty", typeof(int));
            dataTable.Columns.Add("BranchCode", typeof(string));
            dataTable.Columns.Add("BranchName", typeof(string));
            dataTable.Columns.Add("BillToAddressStateCode", typeof(string));
            dataTable.Columns.Add("GSTNo", typeof(string));
            dataTable.Columns.Add("DP", typeof(int));
            dataTable.Columns.Add("IMEI_No", typeof(string));
            dataTable.Columns.Add("DPStatus", typeof(string));
            dataTable.Columns.Add("AltContactNo", typeof(string));

            return dataTable;
        }


    }
}

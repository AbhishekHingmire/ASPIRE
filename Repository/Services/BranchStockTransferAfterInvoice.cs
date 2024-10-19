using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IMS2.Repository.Services
{
    public class BranchStockTransferAfterInvoice : IBranchStockTransferAfterInvoice
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BranchStockTransferAfterInvoice> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;

        public BranchStockTransferAfterInvoice(ApplicationDbContext context, ILogger<BranchStockTransferAfterInvoice> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _batchSize = int.Parse(_configuration["BatchSettings:BatchSize"]);
        }

        public async Task<List<BranchStockTransferAfterInvoiceModel>> BranchStockTransfer(List<BranchStockTransferAfterInvoiceModel> transactions)
        {
            var failedTransactions = new List<BranchStockTransferAfterInvoiceModel>();

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
                            if (string.IsNullOrWhiteSpace(transaction.ApplicationNo))
                            {
                                _logger.LogWarning("LoanAppNo is empty for transaction with AppNo: {AppNo}", transaction.ApplicationNo);
                                failedTransactions.Add(transaction);
                                continue;
                            }

                            dataTable.Rows.Add(
                                dataTable.Rows.Count + 1,
                                transaction.ApplicationNo,
                                transaction.ToBranchCode
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process transaction with AppNo: {AppNo}", transaction.ApplicationNo);
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
                        new SqlParameter("@BranchStockTransfers", SqlDbType.Structured)
                        {
                            TypeName = "dbo.T_BranchStockTransfer",
                            Value = dataTable
                        }
                    };

                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC dbo.usp_ETL_BranchStockTransferAfterInvoice_Batch @AdminUserID, @PartnerID, @BranchStockTransfers",
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
                _logger.LogWarning("Failed transaction: {AppNo}, Reason: Processing error or SQL error", failedTransaction.ApplicationNo);
            }

            return failedTransactions;
        }

        private DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("ApplicationNo", typeof(string));
            dataTable.Columns.Add("ToBranchCode", typeof(string));

            return dataTable;
        }

    }
}

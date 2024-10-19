using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IMS2.Repository.Services
{
    public class ImportGRN : IImportGRN
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ImportGRN> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;

        public ImportGRN(ApplicationDbContext context, ILogger<ImportGRN> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _batchSize = int.Parse(_configuration["BatchSettings:BatchSize"]);
        }

        public async Task<List<ImportGRNModel>> ImportGRNs(List<ImportGRNModel> transactions)
        {
            var failedTransactions = new List<ImportGRNModel>();

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
                            dataTable.Rows.Add(
                                dataTable.Rows.Count + 1,
                                transaction.PORef,
                                transaction.InvoiceNo,
                                transaction.InvoiceDate,
                                transaction.Month,
                                transaction.Supplier,
                                transaction.City,
                                transaction.WareHouseCode,
                                transaction.WareHouseName,
                                transaction.ProductName,
                                transaction.SKU,
                                transaction.Qty,
                                transaction.DeliveryDate,
                                transaction.IMEI_No,
                                transaction.InvoiceFile,
                                transaction.InvQty,
                                transaction.DiffRemarks
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning("LoanAppNo is empty for transaction with Invoice: {InvoiceNo}", transaction.InvoiceNo);
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
                        new SqlParameter("@ETLPOs", SqlDbType.Structured)
                        {
                            TypeName = "dbo.T_ImportETLPO",
                            Value = dataTable
                        }
                    };

                    try
                    {
                        await _context.Database.ExecuteSqlRawAsync(
                            "EXEC dbo.usp_ETL_PurchaseOrder_Batch @AdminUserID, @PartnerID, @ETLPOs",
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
                _logger.LogWarning("Failed transaction: {InvoiceNo}, Reason: Processing error or SQL error", failedTransaction.InvoiceNo);
            }

            return failedTransactions;
        }

        private DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("PORef", typeof(string));
            dataTable.Columns.Add("InvoiceNo", typeof(string));
            dataTable.Columns.Add("InvoiceDate", typeof(string));
            dataTable.Columns.Add("Month", typeof(string));
            dataTable.Columns.Add("Supplier", typeof(string));
            dataTable.Columns.Add("City", typeof(string));
            dataTable.Columns.Add("WareHouseCode", typeof(string));
            dataTable.Columns.Add("WareHouseName", typeof(string));
            dataTable.Columns.Add("ProductName", typeof(string));
            dataTable.Columns.Add("SKU", typeof(string));
            dataTable.Columns.Add("Qty", typeof(int));
            dataTable.Columns.Add("DeliveryDate", typeof(string));
            dataTable.Columns.Add("IMEI_No", typeof(string));
            dataTable.Columns.Add("InvoiceFile", typeof(string));
            dataTable.Columns.Add("InvQty", typeof(int));
            dataTable.Columns.Add("DiffRemarks", typeof(string));

            return dataTable;
        }
    }
}

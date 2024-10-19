using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IMS2.Repository.Services
{
    public class UpdateOrderStatus : IUpdateOrderStatus
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdateOrderStatus> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;

        public UpdateOrderStatus(ApplicationDbContext context, ILogger<UpdateOrderStatus> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _batchSize = int.Parse(_configuration["BatchSettings:BatchSize"] ??"");
        }

        public async Task<List<FailedTransactionModel>> AddOrderStatusAsync(List<UpdateOrderStatusModel> transactions)
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
                            if (string.IsNullOrWhiteSpace(transaction.AppNo))
                            {
                                _logger.LogWarning("LoanAppNo is empty for transaction with OrderNo: {OrderNo}", transaction.AppNo);
                                failedTransactions.Add(new FailedTransactionModel
                                {
                                    LoanAppNo = transaction.AppNo,
                                    Reason = "LoanAppNo is empty"
                                });
                                continue;
                            }

                            dataTable.Rows.Add(
                                dataTable.Rows.Count + 1,
                                transaction.AppNo,
                                transaction.StatusCode,
                                transaction.Remarks
                            );
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to process transaction with OrderNo: {OrderNo}", transaction.AppNo);
                            failedTransactions.Add(new FailedTransactionModel
                            {
                                LoanAppNo = transaction.AppNo,
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
                        new SqlParameter("@OrderStatusTable", SqlDbType.Structured)
                        {
                            TypeName = "dbo.U_OrderStatus",
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
                                command.CommandText = "EXEC dbo.usp_ETL_UpdateOrderStatus_Batch @AdminUserID, @PartnerID, @OrderStatusTable";
                                command.CommandType = CommandType.Text;
                                command.Parameters.AddRange(parameters);

                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    var failedRecords = new List<FailedTransactionModel>();

                                    // Read the result set from the stored procedure
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
                            LoanAppNo = t.AppNo,
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
                _logger.LogWarning("Failed transaction: {LoanAppNo}, Reason: {Reason}", failedTransaction.LoanAppNo, failedTransaction.Reason);
            }

            return failedTransactions;
        }

        private DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("AppNo", typeof(string));
            dataTable.Columns.Add("StatusCode", typeof(string));
            dataTable.Columns.Add("Remarks", typeof(string));

            return dataTable;
        }

    }
}

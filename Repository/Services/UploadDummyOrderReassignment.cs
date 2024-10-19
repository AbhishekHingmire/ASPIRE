using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Data;

namespace IMS2.Repository.Services
{
    public class UploadDummyOrderReassignment : IUploadDummyOrderReassignment
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UploadDummyOrderReassignment> _logger;
        private readonly IConfiguration _configuration;
        private readonly int _batchSize;

        public UploadDummyOrderReassignment(ApplicationDbContext context, ILogger<UploadDummyOrderReassignment> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _batchSize = int.Parse(_configuration["BatchSettings:BatchSize"]);
        }

        public async Task<List<UploadDummyOrderReassignmentModel>> UploadDummyOrder(List<UploadDummyOrderReassignmentModel> transactions)
        {
            var failedTransactions = new List<UploadDummyOrderReassignmentModel>();

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
                        if (string.IsNullOrWhiteSpace(transaction.OldLoanAppNo))
                        {
                            _logger.LogWarning("LoanAppNo is empty for transaction with AppNo: {OldLoanAppNo}", transaction.OldLoanAppNo);
                            failedTransactions.Add(transaction);
                            continue;
                        }

                        dataTable.Rows.Add(
                            dataTable.Rows.Count + 1,
                            transaction.OldLoanAppNo,
                            transaction.NewLoanAppNo,
                            transaction.CustomerName
                        );
                    }

                    if (dataTable.Rows.Count == 0)
                    {
                        _logger.LogWarning("No valid transactions to process in batch.");
                        continue;
                    }

                    var parameters = new[]
                    {
                        new SqlParameter("@AdminUserID", SqlDbType.BigInt) { Value = transactions[0].AdminUserID },
                        new SqlParameter("@Reassignments", SqlDbType.Structured)
                        {
                            TypeName = "dbo.T_DummyOrderReassignment",
                            Value = dataTable
                        }
                    };

                    try
                    {
                        var failedRecords = new List<UploadDummyOrderReassignmentModel>();

                        using (var command = _context.Database.GetDbConnection().CreateCommand())
                        {
                            command.CommandText = "EXEC dbo.usp_ETL_DummyOrderReassignment_Batch @AdminUserID, @Reassignments";
                            command.CommandType = CommandType.Text;
                            command.Parameters.AddRange(parameters);

                            await _context.Database.OpenConnectionAsync();
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    failedRecords.Add(new UploadDummyOrderReassignmentModel
                                    {
                                        OldLoanAppNo = reader["LoanAppNo"].ToString(),
                                        Reason = reader["Reason"].ToString()
                                    });
                                }
                            }
                        }

                        foreach (var record in failedRecords)
                        {
                            var failedTransaction = batch.FirstOrDefault(t => t.OldLoanAppNo == record.OldLoanAppNo);
                            if (failedTransaction != null)
                            {
                                failedTransaction.Reason = record.Reason;
                                failedTransactions.Add(failedTransaction);
                            }
                        }
                    }
                    catch (SqlException ex)
                    {
                        _logger.LogError(ex, "SQL error occurred while adding transactions.");
                        foreach (var transaction in batch)
                        {
                            transaction.Reason = "SQL error";
                            failedTransactions.Add(transaction);
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

            return failedTransactions;
        }


        private DataTable CreateDataTable()
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("ID", typeof(int));
            dataTable.Columns.Add("OldLoanAppNo", typeof(string));
            dataTable.Columns.Add("NewLoanAppNo", typeof(string));
            dataTable.Columns.Add("CustomerName", typeof(string));

            return dataTable;
        }
    }
}

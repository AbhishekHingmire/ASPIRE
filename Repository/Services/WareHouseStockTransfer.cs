using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace IMS2.Repository.Services
{
    public class WareHouseStockTransfer : IWareHouseStockTransfer
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<WareHouseStockTransfer> _logger;

        public WareHouseStockTransfer(ApplicationDbContext context, ILogger<WareHouseStockTransfer> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<WareHouseModel>> GetWareHouseList()
        {
            try
            {
                var result = await _context.WareHouse
                .Select(e => new WareHouseModel { ID = e.ID, Code = e.Code })
                .OrderBy(e => e.ID)
                .ToListAsync();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ItemMasterModel>> GetItemList()
        {
            try
            {
                var result = await _context.Item
                .Where(e => e.ID != -1)
                .OrderBy(e => e.ID)
                .Select(e => new ItemMasterModel {ID = e.ID, Name = e.Name })
                .ToListAsync();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<long> GetStockAsync(long wareHouseID, long itemID)
        {
            try
            {
                var stock = await _context.WareHouseStock
                    .Where(ws => ws.WareHouseID == wareHouseID && ws.ItemID == itemID)
                    .SumAsync(ws => (long?)ws.Qty) ?? 0;

                return stock;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving stock quantity", ex);
            }
        }

        public async Task<bool> DoWareHouseStockTransfer(WareHouseStockTransferModel model)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@FromWareHouseID", model.FromWareHouseID),
                    new SqlParameter("@ItemID", model.ItemID),
                    new SqlParameter("@ToWareHouseID", model.ToWareHouseID),
                    new SqlParameter("@Stock", model.Stock),
                    new SqlParameter("@IMEINos", model.IMEINos),
                    new SqlParameter("@Remarks", model.Remarks ?? string.Empty),
                    new SqlParameter("@AdminUserID", model.AdminUserID),
                    new SqlParameter("@InvoiceNo", model.InvoiceNo),
                    new SqlParameter("@InvoiceFilePath", model.InvoiceFilePath)
                };

                var result = await _context.Database.ExecuteSqlRawAsync(
                    "EXEC [dbo].[usp_admin_DoWareHouseStockTransfer] @FromWareHouseID, @ItemID, @ToWareHouseID, @Stock, @IMEINos, @Remarks, @AdminUserID, @InvoiceNo, @InvoiceFilePath",
                    parameters
                );

                return result > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing warehouse stock transfer");
                throw;
            }
        }
    }
}

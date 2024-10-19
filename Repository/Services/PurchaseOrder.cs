using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository.Services
{
    public class PurchaseOrder : IPurchaseOrder
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PurchaseOrder> _logger;

        public PurchaseOrder(ApplicationDbContext context, ILogger<PurchaseOrder> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<PurchaseOrderModel>> GetPurchaseOrderListAsync(long partnerId, int pageNumber, int pageSize)
        {
            try
            {
                var result = await _context.PurchaseOrderList
                    .FromSqlRaw("EXEC dbo.api_Admin_PO_List_New @PartnerID = {0}, @PageNumber = {1}, @PageSize = {2}", partnerId, pageNumber, pageSize)
                    .ToListAsync();

                if (result == null)
                {
                    _logger.LogWarning("No data returned for PartnerID {PartnerID}", partnerId);
                    return new List<PurchaseOrderModel>();
                }

                return result;
            }
            catch (SqlException ex) when (ex.Number == 0)
            {
                _logger.LogError(ex, "SQL error while fetching partner items for PartnerID {PartnerID}", partnerId);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching partner items for PartnerID {PartnerID}", partnerId);
                throw;
            }
        }


        public async Task<List<POTypes>> GetPOTypeList()
        {
            try
            {
                var result = await _context.POType
                .Select(e => new POTypes { ID = e.ID, Name = e.Name })
                .ToListAsync();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<POCompanys>> GetPOCompanyList()
        {
            try
            {
                var result = await _context.POCompany
                .Select(e => new POCompanys { ID = e.ID, Name = e.Name })
                .ToListAsync();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Suppliers>> GetSupplierList()
        {
            try
            {
                var result = await _context.Supplier
                .Select(e => new Suppliers { ID = e.ID, Name = e.Name })
                .ToListAsync();

                return result;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ItemViewModel>> GetItemsForPartnerAsync(long partnerId)
        {
            try
            {
                var result = await _context.ItemMaster
                    .FromSqlRaw("EXEC dbo.api_Admin_PO_ItemList @PartnerID = {0}", partnerId)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching items for PartnerID {PartnerID}", partnerId);
                throw;
            }
        }

        public void CreateOrEditPO(PurchaseOrders model)
        {
            try
            {
                model.PriceExcludingGST = 0;
                model.GSTPercent = 0;
                var parameters = new[]
                {
                    new SqlParameter("@ID", model.DT_RowId),
                    new SqlParameter("@PO_No", model.PONo),
                    new SqlParameter("@PORef", model.PORef),
                    new SqlParameter("@OrderDate", model.OrderDate),
                    new SqlParameter("@POTypeID", model.POTypeID),
                    new SqlParameter("@SupplierID", model.SupplierID),
                    new SqlParameter("@Receiver_GST", model.ReceiverGST),
                    new SqlParameter("@BillTo", model.BillTo),
                    new SqlParameter("@ShipTo", model.ShipTo),
                    new SqlParameter("@ItemID", model.ItemID),
                    new SqlParameter("@Description", model.Description),
                    new SqlParameter("@Qty", model.Qty),
                    new SqlParameter("@Unit", model.Unit),
                    new SqlParameter("@PriceExcludingGST", model.PriceExcludingGST),
                    new SqlParameter("@GSTPercent", model.GSTPercent),
                    new SqlParameter("@PriceIncludingGST", model.PriceIncludingGST),
                    new SqlParameter("@TotalAmount", model.TotalAmount),
                    new SqlParameter("@PartnerID", model.PartnerID),
                    new SqlParameter("@PaymentTerms", model.PaymentTerms),
                    new SqlParameter("@AdminUserID", model.AdminUserID),
                    new SqlParameter("@POCompanyID", model.POCompanyID)
                };

                _logger.LogInformation("Attempting to create or edit PO: {@PurchaseOrderModel}", model);
                var rowsAffected = _context.Database.ExecuteSqlRaw("EXEC [dbo].[api_Admin_PO_Edit] @ID, @PO_No, @PORef, @OrderDate, @POTypeID, @SupplierID, @Receiver_GST, @BillTo, @ShipTo, @ItemID, @Description, @Qty, @Unit, @PriceExcludingGST, @GSTPercent, @PriceIncludingGST, @TotalAmount, @PartnerID, @PaymentTerms, @AdminUserID, @POCompanyID", parameters);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("PO created or updated successfully");
                }
                else
                {
                    _logger.LogWarning("No rows affected while creating or updating PO");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating or updating the PO: {ex.Message}");
                throw;
            }
        }

        public void DeletePo(long id, long AdminUserID)
        {
            try
            {
                SqlParameter paramId = new SqlParameter("@ID", id);
                SqlParameter paramAdminUserID = new SqlParameter("@AdminUserID", AdminUserID);

                _context.Database.ExecuteSqlRaw("EXEC api_Admin_PO_Delete @ID, @AdminUserID", paramId, paramAdminUserID);
                _logger.LogInformation($"User with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user with ID {id}: {ex.Message}");
                throw;
            }
        }

    }
}

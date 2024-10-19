using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace IMS2.Repository.Services
{
    public class Branch : IBranch
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<Branch> _logger;

        public Branch(ApplicationDbContext context, ILogger<Branch> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<BranchModel>> GetBranchListAsync(long partnerId, int pageNumber, int pageSize)
        {
            try
            {
                var result = await _context.BranchList
                    .FromSqlRaw("EXEC dbo.api_Admin_Branch_List @PartnerID = {0}, @PageNumber = {1}, @PageSize = {2}", partnerId, pageNumber, pageSize)
                    .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching partner items for PartnerID {PartnerID}", partnerId);
                throw;
            }
        }

        public async Task<IEnumerable<BranchTypeModel>> GetBranchAsync()
        {
            try
            {
                var branches = await _context.BranchType
                    .Where(b => b.ID != -1)
                    .Select(b => new BranchTypeModel
                    {
                        ID = b.ID,
                        Name = b.Name
                    })
                    .ToListAsync();

                return branches;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching branches: {ex.Message}");
                throw;
            }
        }

        public async Task<IEnumerable<ParentBranchModel>> GetParentBranches(long branchTypeID, long partnerID)
        {
            try
            {
                var parentBranches = await _context.getBranchType
                    .FromSqlRaw("EXEC [dbo].[api_Admin_GetParentBranchCBO] @BranchTypeID={0}, @PartnerID={1}", branchTypeID, partnerID)
                    .ToListAsync();

                return parentBranches;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                throw new Exception("Error retrieving parent branches.", ex);
            }
        }

        public async Task<long> CreateUserAndProcessBranchAsync(BranchMasterModel model)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    if (model.DT_RowId == -1)
                    {
                        var branchType = _context.BranchType
                          .Where(bt => bt.ID == model.BranchTypeID)
                          .Select(bt => bt.Name)
                          .FirstOrDefault();

                        var user = new Users
                        {
                            Username = model.Username,
                            Password = model.Password,
                            Role = branchType,
                            IsActive = true,
                            IsApproved = true
                        };

                        _context.User.Add(user);
                        await _context.SaveChangesAsync();

                        model.UserID = user.ID;
                    }

                    long BranchTypeID = model.BranchTypeID ?? -1;
                    long CityID = model.CityID ?? -1;
                    long StockistID = -1;
                    long ParentID = model.ParentID ?? -1;

                    var parameters = new List<SqlParameter>
                    {
                        new SqlParameter("@ID", model.DT_RowId ?? (object)DBNull.Value),
                        new SqlParameter("@BranchTypeID", BranchTypeID),
                        new SqlParameter("@Code", model.Code ?? (object)DBNull.Value),
                        new SqlParameter("@Name", model.Name ?? (object)DBNull.Value),
                        new SqlParameter("@AddressLine1", model.AddressLine1 ?? (object)DBNull.Value),
                        new SqlParameter("@AddressLine2", model.AddressLine2 ?? (object)DBNull.Value),
                        new SqlParameter("@CityID", CityID),
                        new SqlParameter("@StockistID", StockistID),
                        new SqlParameter("@Pincode", model.Pincode ?? (object)DBNull.Value),
                        new SqlParameter("@Phone", model.Phone ?? (object)DBNull.Value),
                        new SqlParameter("@UserID", model.UserID),
                        new SqlParameter("@ParentID", ParentID),
                        new SqlParameter("@PartnerID", model.PartnerID ?? (object)DBNull.Value),
                        new SqlParameter("@AdminUserID", model.AdminUserID ?? (object)DBNull.Value),
                        new SqlParameter("@ID_OUT", SqlDbType.BigInt)
                        {
                            Direction = ParameterDirection.Output
                        }
                    };

                    var sql = "EXEC @ID_OUT = api_Admin_Branch_Edit @ID, @BranchTypeID, @Code, @Name, @AddressLine1, @AddressLine2, @CityID, @StockistID, @Pincode, @Phone, @UserID, @ParentID, @PartnerID, @AdminUserID";
                    await _context.Database.ExecuteSqlRawAsync(sql, parameters.ToArray());

                    long branchId = Convert.ToInt64(parameters.Last().Value);

                    await transaction.CommitAsync();

                    return branchId;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("Error creating user and processing branch.", ex);
                }
            }
        }

        public void DeleteUser(long id)
        {
            try
            {
                SqlParameter paramId = new SqlParameter("@ID", id);
                _context.Database.ExecuteSqlRaw("EXEC api_Admin_Branch_Delete @ID", paramId);
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

using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IMS2.Repository.Services
{
    public class UserMFIMapper : IUserMFIMapper
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserMFIMapper> _logger;

        public UserMFIMapper(ApplicationDbContext context, ILogger<UserMFIMapper> logger)
        {
            _context = context;
            _logger = logger;
        }

        public List<UserMFIMapperModel> GetAllAdminUserMFIList(int pageNumber, int pageSize)
        {
            try
            {
                pageNumber = pageNumber > 0 ? pageNumber : 1;
                pageSize = pageSize > 0 ? pageSize : int.MaxValue;

                var query = from um in _context.UserMFI
                            join u in _context.User on um.UserID equals u.ID
                            join p in _context.Partner on um.PartnerID equals p.ID
                            orderby um.ID descending
                            select new UserMFIMapperModel
                            {
                                DT_RowId = um.ID,
                                UserName = u.Username,
                                MFI = p.Name,
                                UserID = um.UserID,
                                PartnerID = um.PartnerID
                            };

                int skip = (pageNumber - 1) * pageSize;
                var paginatedResults = query.Skip(skip).Take(pageSize).ToList();

                return paginatedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while getting the user MFI list: {ex.Message}");
                throw;
            }
        }


        public async Task<List<MFIList>> GetPartnersAsync()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch partners list from database");

                var partnersList = await _context.Partner
                    .Where(p => p.ID != -1)
                    .OrderBy(p => p.ID)
                    .ToListAsync();

                _logger.LogInformation("Partners list fetched successfully");

                return partnersList;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while getting the partners list: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<UserDetails> GetUserDetails()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch screen from database");

                var userDetails = _context.UserDetails
                    .FromSqlInterpolated($"EXEC [dbo].[usp_GetUsersDetails]")
                    .ToList();

                _logger.LogInformation("User Details fetched successfully");

                return userDetails;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching roles: {ex.Message}");
                throw;
            }
        }

        public bool CreateOrEditUserUserMFIScreen(UserMFIMapperCreateAndEditModel model)
        {
            try
            {
                for (int i = 0; i < model.MFI.Count; i++)
                {
                    var existingItem = _context.UserMFI
                        .FirstOrDefault(item => item.UserID == model.UserID[i] && item.PartnerID == model.MFI[i]);

                    if (existingItem != null)
                    {
                        _logger.LogWarning($"Combination of UserID: {model.UserID} and ScreenID: {model.MFI[i]} already exists.");
                        return false;
                    }
                }

                for (int i = 0; i < model.MFI.Count; i++)
                {
                    try
                    {
                        var parameter = new[]
                        {
                            new SqlParameter("@ID", model.ID),
                            new SqlParameter("@UserID", model.UserID[i]),
                            new SqlParameter("@PartnerID", model.MFI[i]),
                        };

                        _logger.LogInformation("Attempting to create/edit user User MFI Mapper for UserID: {UserID}, MFI: {MFIID}", model.UserID, model.MFI[i]);
                        var rowsAffected = _context.Database.ExecuteSqlRaw("EXEC api_Admin_UserMFI_Edit @ID, @UserID, @PartnerID", parameter);
                    }
                    catch (SqlException sqlEx)
                    {
                        _logger.LogError(sqlEx, "SQL error occurred while processing UserID: {UserID}, MFI: {MFIID}", model.UserID, model.MFI[i]);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while processing UserID: {UserID}, MFI: {MFIID}", model.UserID, model.MFI[i]);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while creating/editing user screen rights for UserID: {UserID}", model.UserID);
                return false;
            }
        }

        public void DeleteUserMFIMapper(long id)
        {
            try
            {
                SqlParameter paramId = new SqlParameter("@ID", id);
                _context.Database.ExecuteSqlRaw("EXEC api_Admin_UserMFI_Delete @ID", paramId);
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

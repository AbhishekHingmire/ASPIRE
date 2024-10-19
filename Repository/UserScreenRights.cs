using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace IMS2.Repository
{
    public class UserScreenRights : IUserScreenRights
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserScreenRights> _logger;

        public UserScreenRights(ApplicationDbContext context, ILogger<UserScreenRights> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<UserScreenRight> GetScreen()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch screen from database");

                var screens = _context.Screens
                    .FromSqlInterpolated($"EXEC [dbo].[usp_GetScreens]")
                    .ToList();

                _logger.LogInformation("Screens fetched successfully");

                return screens;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching roles: {ex.Message}");
                throw;
            }
        }

        public IEnumerable<UserScreen> GetScreenRights()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch screen from database");

                var screenRights = _context.ScreenRights
                    .FromSqlInterpolated($"EXEC [dbo].[usp_GetScreenRights]")
                    .ToList();

                _logger.LogInformation("Screens fetched successfully");

                return screenRights;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching roles: {ex.Message}");
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

        public bool CreateOrEditUserScreenRights(UserScreenRightsModel model)
        {
            try
            {
                for (int i = 0; i < model.ScreenIds.Count; i++)
                {
                    var existingItem = _context.UserScreenRights
                        .FirstOrDefault(item => item.UserID == model.UserID && item.ScreenID == model.ScreenIds[i]);

                    if (existingItem != null)
                    {
                        _logger.LogWarning($"Combination of UserID: {model.UserID} and ScreenID: {model.ScreenIds[i]} already exists.");
                        return false;
                    }
                }

                for (int i = 0; i < model.ScreenIds.Count; i++)
                {
                    try
                    {
                        var parameter = new[]
                        {
                            new SqlParameter("@ID", -1),
                            new SqlParameter("@UserID", model.UserID),
                            new SqlParameter("@ScreenID", model.ScreenIds[i]),
                            new SqlParameter("@ScreenRightsID", model.ScreenRightsIds[i])
                        };

                        _logger.LogInformation("Attempting to create/edit user screen rights for UserID: {UserID}, ScreenID: {ScreenID}", model.UserID, model.ScreenIds[i]);
                        var rowsAffected = _context.Database.ExecuteSqlRaw("EXEC usp_CreateEditUserScreenRightsMapping @ID, @UserID, @ScreenID, @ScreenRightsID", parameter);
                    }
                    catch (SqlException sqlEx)
                    {
                        _logger.LogError(sqlEx, "SQL error occurred while processing UserID: {UserID}, ScreenID: {ScreenID}", model.UserID, model.ScreenIds[i]);
                        return false;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "An error occurred while processing UserID: {UserID}, ScreenID: {ScreenID}", model.UserID, model.ScreenIds[i]);
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



        public bool UpdateScreenRights(UpdateUserScreenRightsModel model)
        {
            try
            {
                _context.Database.ExecuteSqlRaw(
                    "EXEC usp_CreateEditUserScreenRightsMapping @ID, @UserID, @ScreenID, @ScreenRightsID",
                    new[]
                    {
                        new SqlParameter("@ID", model.ID),
                        new SqlParameter("@UserID", model.UserID),
                        new SqlParameter("@ScreenID", model.ScreenID),
                        new SqlParameter("@ScreenRightsID", model.ScreenRightsID),
                    });
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error updating user details: {ex.Message}");
            }
        }

        public List<UserScreenRightsViewModel> GetAllUserScreenRights(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Attempting to fetch user screen rights from the database");

                pageNumber = pageNumber > 0 ? pageNumber : 1;
                pageSize = pageSize > 0 ? pageSize : int.MaxValue;

                var query = from usr in _context.UserScreenRights
                            join u in _context.User on usr.UserID equals u.ID
                            join s in _context.Screens on usr.ScreenID equals s.ID
                            join r in _context.ScreenRights on usr.ScreenRightsID equals r.ID
                            where u.IsActive == true && s.IsActive == true && r.IsActive == true
                            orderby usr.ID descending
                            select new UserScreenRightsViewModel
                            {
                                Id = usr.ID,
                                UserID = u.ID,
                                UserName = u.Username ?? "",
                                ScreenID = s.ID,
                                Screen = s.Name,
                                ScreenRightsID = r.ID,
                                Rights = r.Name ??""
                            };

                int skip = (pageNumber - 1) * pageSize;
                var paginatedResults = query.Skip(skip).Take(pageSize).ToList();

                _logger.LogInformation("User screen rights details fetched successfully");

                return paginatedResults;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching user rights: {ex.Message}");
                throw;
            }
        }


        public void DeleteScreenRights(long id)
        {
            try
            {
                SqlParameter paramId = new SqlParameter("@ID", id);
                _context.Database.ExecuteSqlRaw("EXEC usp_DeleteUserScreenRights @ID", paramId);
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

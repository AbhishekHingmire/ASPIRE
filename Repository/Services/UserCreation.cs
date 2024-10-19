using IMS2.Models;
using IMS2;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using IMS2.Repository.Interfaces;

namespace IMS2.Repository.Services
{
    public class UserCreation : IUserCreation
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UserCreation> _logger;

        public UserCreation(ApplicationDbContext context, ILogger<UserCreation> logger)
        {
            _context = context;
            _logger = logger;
        }

        public IEnumerable<RoleModel> GetRoles()
        {
            try
            {
                _logger.LogInformation("Attempting to fetch roles from database");

                var roles = _context.Roles
                    .FromSqlInterpolated($"EXEC [dbo].[usp_GetRoles]")
                    .ToList();

                _logger.LogInformation("Roles fetched successfully");

                return roles;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching roles: {ex.Message}");
                throw;
            }
        }

        public void CreateAndUpdateUser(UserCreationModel userModel)
        {
            try
            {
                var parameters = new[]
                {
                    new SqlParameter("@ID", userModel.Id),
                    new SqlParameter("@Username", userModel.UserName),
                    new SqlParameter("@Password", userModel.Password),
                    new SqlParameter("@RoleName", userModel.Role),
                    new SqlParameter("@Phone", userModel.Phone),
                    new SqlParameter("@Name", userModel.Name),
                    new SqlParameter("@Email", userModel.Email)
                };

                _logger.LogInformation("Attempting to create user: {@UserModel}", userModel);
                var rowsAffected = _context.Database.ExecuteSqlRaw("EXEC usp_CreateEditSystemUser @ID, @Username, @Password, @RoleName, @Phone, @Name, @Email", parameters);

                if (rowsAffected > 0)
                {
                    _logger.LogInformation("User created successfully");
                }
                else
                {
                    _logger.LogWarning("No rows affected while creating user");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating the user: {ex.Message}");
                throw;
            }
        }


        public List<UserCreationGetDetailsModel> GetAllUserCreation(int pageNumber, int pageSize)
        {
            try
            {
                _logger.LogInformation("Attempting to fetch user details from the database.");

                var getUserCreation = _context.UserCreationList
                    .FromSqlInterpolated($"EXEC [dbo].[Usp_getusers] @PageNumber = {pageNumber}, @PageSize = {pageSize}")
                    .ToList();

                _logger.LogInformation("User details fetched successfully.");

                return getUserCreation;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public void DeleteUser(long id)
        {
            try
            {
                SqlParameter paramId = new SqlParameter("@ID", id);
                _context.Database.ExecuteSqlRaw("EXEC usp_DeleteUser @ID", paramId);
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
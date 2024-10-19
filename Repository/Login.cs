using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace IMS2.Repository
{
    public class Login : ILogin
    {
        private readonly IConfiguration _configuration;
        private readonly IUserCreation _userRepository;

        private readonly ApplicationDbContext _context;
        private readonly ILogger<Login> _logger;

        public Login(ApplicationDbContext context, ILogger<Login> logger, IConfiguration configuration, IUserCreation userRepository)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
            _userRepository = userRepository;
        }

        public Users GetUserByUsernameAndPassword(string username, string password)
        {
            try
            {
                var user = _context.User.FirstOrDefault(u => u.Username == username && u.Password == password);

                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username and password");
                throw;
            }
        }

        public Dictionary<string, string> GetUserRightsByScreens(long UserID)
        {
            Dictionary<string, string> userRights = new Dictionary<string, string>();
            try
            {
                var userScreenRights = _context.UserScreenRight
                    .FromSqlRaw("EXECUTE dbo.usp_GetUserScreenRightsByUserId @UserID", new SqlParameter("@UserID", UserID))
                    .ToList();

                foreach (var rights in userScreenRights)
                {
                    if (!userRights.ContainsKey(rights.Screen))
                    {
                        userRights[rights.Screen] = rights.Rights;
                    }
                }

            }
            catch (Exception)
            {

                throw;
            }
            return userRights;
        }
    }
}

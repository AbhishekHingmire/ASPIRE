using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;

namespace IMS2.Controllers
{
    public class UsersController : Controller
    {
        private readonly IUserCreation _userRepository;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UsersController> _logger;
        private readonly ISettings _settingRepository;
        private readonly IConfiguration _configuration;
        private readonly int _defaultPageNumber;
        private readonly int _defaultPageSize;

        public UsersController(IUserCreation userRepository, ILogger<UsersController> logger, ApplicationDbContext context, ISettings settingRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _logger = logger;
            _context = context;
            _settingRepository = settingRepository;
            _configuration = configuration;

            _defaultPageNumber = _configuration.GetValue<int>("PaginationCount:PageNumber");
            _defaultPageSize = _configuration.GetValue<int>("PaginationCount:PageSize");
        }

        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Index(int pageNumber, int pageSize)
        {
            try
            {
                var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
                var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                var userId = UserHelper.GetUserId(User);

                var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);
                ViewBag.MFICBO = mfiList;
                ViewBag.Role = roleClaim;
                ViewBag.Name = name;

                pageNumber = pageNumber > 0 ? pageNumber : _defaultPageNumber;
                pageSize = pageSize > 0 ? pageSize : _defaultPageSize;

                var roles = _userRepository.GetRoles();
                var userDetailsList = _userRepository.GetAllUserCreation(pageNumber, pageSize);

                ViewData["Roles"] = roles.Select(r => new SelectListItem { Value = r.Name, Text = r.Name });
                ViewData["userDetailsList"] = userDetailsList.Select(r => new UserCreationGetDetailsModel
                {
                    Id = r.Id,
                    UserName = r.UserName,
                    Role = r.Role,
                    Phone = r.Phone,
                    Name = r.Name,
                    Email = r.Email
                }).ToList();
                ViewBag.PageNumber = pageNumber;
                ViewBag.PageSize = pageSize;

                return View();

            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while fetching roles: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while fetching roles.");
                return View();
            }
        }


        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult CreateAndEditUser(UserCreationModel userModel, string action)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (action == "Create")
                    {
                        var existingUser = _context.User.FirstOrDefault(u => u.Username == userModel.UserName);

                        if (existingUser != null)
                        {
                            TempData["SuccessMessage"] = "Username already exist.";
                            return RedirectToAction("Index", userModel);
                        }

                        userModel.Id = -1;
                        _userRepository.CreateAndUpdateUser(userModel);
                        TempData["SuccessMessage"] = "User created successfully.";
                        return RedirectToAction("Index");

                    }
                    else if (action == "Edit")
                    {
                        userModel.Password = "null";
                        _userRepository.CreateAndUpdateUser(userModel);
                        TempData["SuccessMessage"] = "User updated successfully.";
                        return RedirectToAction("Index");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Invalid data.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while creating the user: {ex.Message}");
                ModelState.AddModelError("", "An error occurred while creating the user.");
            }

            return View("Index", userModel);
        }

        [HttpDelete]
        [Authorize(Roles = "SuperAdmin")]
        public IActionResult DeleteUser(long id)
        {
            try
            {
                _userRepository.DeleteUser(id);
                TempData["SuccessMessage"] = "User deleted successfully.";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting user: {ex.Message}");
                TempData["ErrorMessage"] = "Error deleting user.";
            }

            return RedirectToAction("Index");
        }
    }
}

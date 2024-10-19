using DocumentFormat.OpenXml.Spreadsheet;
using IMS2.Helpers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using IMS2.Session;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Velzon.Controllers
{
    public class DashBoardController : Controller
    {
        private readonly ILogger<DashBoardController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ISettings _settingRepository;
        private readonly IImportSalesOrder _importSalesOrderRepository;

        public DashBoardController(ILogin userRepository, IConfiguration configuration, ILogger<DashBoardController> logger, ISettings settingRepository, IImportSalesOrder importSalesOrderRepository)
        {
            _configuration = configuration;
            _logger = logger;
            _settingRepository = settingRepository;
            _importSalesOrderRepository = importSalesOrderRepository;
        }

        [Authorize(Roles = "Administrator, NormalUser, SuperAdmin, Branch")]
        [HttpGet]
        public async Task<IActionResult> BaseDashboard()
        {
            try
            {
                var claims = User.Claims;

                if (User.Identity.IsAuthenticated)
                {
                    var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                    var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
                    var UserId = claims.FirstOrDefault(c => c.Type == "ID")?.Value;
                    long UserID = Convert.ToInt64(UserId);
                    ViewBag.Role = roleClaim;

                    var partners = await _settingRepository.GetPartnersAsync(UserID);
                    var MFIList = partners.Select(p => new SelectListItem
                    {
                        Text = p.Name,
                        Value = p.ID.ToString()
                    }).ToList();

                    var allowedScreens = await UserHelper.GetScreensForUserAsync(UserID, _settingRepository);

                    ViewBag.AllowedScreens = allowedScreens;
                    ViewBag.MFICBO = MFIList;
                    ViewBag.Role = roleClaim;
                    ViewBag.Name = name;

                    var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

                    long BranchTypeID = branchTypeID;
                    ViewBag.BranchType = BranchTypeID;
                }

                return View();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "An error occurred in BaseDashboard action.");
                throw;
            }
        }

        [ActionName("Analytics")]
        public IActionResult Analytics()
        {
            return View();
        }

        [ActionName("CRM")]
        public IActionResult CRM()
        {
            return View();
        }

        [ActionName("Crypto")]
        public IActionResult Crypto()
        {
            return View();
        }

        [ActionName("Projects")]
        public IActionResult Projects()
        {
            return View();
        }

        [ActionName("NFT")]
        public IActionResult NFT()
        {
            return View();
        }

        [ActionName("Job")]
        public IActionResult Job()
        {
            return View();
        }

    }
}

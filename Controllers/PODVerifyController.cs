using ICSharpCode.SharpZipLib.Zip;
using IMS2.Helpers;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IMS2.Controllers
{
    public class PODVerifyController : Controller
    {
        private readonly IPODVerify _pODVerifyRepository;
        private readonly ILogger<PODVerifyController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _env;
        private readonly ISettings _settingRepository;
        private readonly IImportSalesOrder _importSalesOrderRepository;
        private readonly IGITReport _gITReportRepository;

        public PODVerifyController
            (
                ILogger<PODVerifyController> logger,
                IConfiguration configuration,
                IWebHostEnvironment env,
                ISettings settingRepository,
                IImportSalesOrder importSalesOrderRepository,
                IGITReport gITReportRepository,
                IPODVerify pODVerifyRepository
            )
            {
                _logger = logger;
                _configuration = configuration;
                _env = env;
                _settingRepository = settingRepository;
                _importSalesOrderRepository = importSalesOrderRepository;
                _gITReportRepository = gITReportRepository;
                _pODVerifyRepository = pODVerifyRepository;
            }

        public async Task<IActionResult> Index()
        {
            var (roleClaim, lstScreenRights) = UserHelper.GetRoleAndScreenRights(User);
            var name = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
            var userId = UserHelper.GetUserId(User);
            var screenRight = UserHelper.GetScreenAndRight(User, EnumScreenNames.SalesReport);

            var mfiList = await UserHelper.GetMFIListAsync(userId, _settingRepository);

            ViewBag.MFICBO = mfiList;
            ViewBag.Role = roleClaim;
            ViewBag.Name = name;

            var partnerIdCookie = Request.Cookies["SelectedMFI"];
            if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
            {
                return BadRequest("Invalid PartnerID cookie.");
            }

            var partners = await _settingRepository.GetPartnersAsync(userId);
            var MFIList = partners.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.ID.ToString()
            }).ToList();

            ViewBag.Partners = MFIList;

            var (branchTypeID, branchID) = await UserHelper.GetUserBranchDetailsAsync(name, _importSalesOrderRepository);

            long BranchTypeID = branchTypeID;
            ViewBag.BranchType = BranchTypeID;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> GetOrderDetailsForPODVerify(string applicationNo)
        {
            var partnerIdCookie = Request.Cookies["SelectedMFI"];
            if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
            {
                return BadRequest(new { Result = "Error", Message = "Invalid PartnerID cookie." });
            }

            if (string.IsNullOrEmpty(applicationNo))
            {
                return BadRequest(new { Result = "Error", Message = "ApplicationNo is required." });
            }

            try
            {
                var result = await _pODVerifyRepository.GetOrderDetailsForPODVerifyAsync(partnerId, applicationNo);

                if (result == null || !result.Any())
                {
                    return NotFound(new { Result = "Error", Message = "No data found." });
                }

                return Json(new { Result = "OK", Data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching order details for POD verification.");
                return StatusCode(500, new { Result = "Error", Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DownloadPODFilesByAppNo(string ApplicationNo)
        {
            var partnerIdCookie = Request.Cookies["SelectedMFI"];
            if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
            {
                _logger.LogWarning("Invalid PartnerID cookie.");
                return BadRequest(new { Result = "Error", Message = "Invalid PartnerID cookie." });
            }

            if (string.IsNullOrEmpty(ApplicationNo))
            {
                _logger.LogWarning("ApplicationNo is required.");
                return BadRequest(new { Result = "Error", Message = "ApplicationNo is required." });
            }

            try
            {
                var fileUrls = await _pODVerifyRepository.GetFilesURLsByApplicationNoAsync(partnerId, ApplicationNo);
                if (fileUrls == null || !fileUrls.Any())
                {
                    _logger.LogWarning("No files found for ApplicationNo: {ApplicationNo}.", ApplicationNo);
                    return NotFound(new { Result = "Error", Message = "No files found." });
                }

                var tempDownloadPath = Path.Combine(_env.WebRootPath, "TempDownload");
                if (!Directory.Exists(tempDownloadPath))
                {
                    Directory.CreateDirectory(tempDownloadPath);
                }

                var directory = new DirectoryInfo(tempDownloadPath);
                foreach (FileInfo file in directory.GetFiles())
                {
                    file.Delete();
                }

                foreach (var fileUrl in fileUrls)
                {
                    if (!string.IsNullOrEmpty(fileUrl.Url))
                    {
                        var sourceFilePath = fileUrl.Url;
                        _logger.LogInformation("Attempting to copy file from source: {SourceFilePath}", sourceFilePath);

                        try
                        {
                            var fileInfo = new FileInfo(sourceFilePath);
                            if (fileInfo.Exists)
                            {
                                var destinationFilePath = Path.Combine(tempDownloadPath, fileInfo.Name);
                                System.IO.File.Copy(sourceFilePath, destinationFilePath, true);
                                _logger.LogInformation("File copied successfully from {SourceFilePath} to {DestinationFilePath}", sourceFilePath, destinationFilePath);
                            }
                            else
                            {
                                _logger.LogWarning("File not found: {SourceFilePath}", sourceFilePath);
                            }
                        }
                        catch (IOException ioEx)
                        {
                            _logger.LogError(ioEx, "IO error occurred while copying file: {SourceFilePath}", fileUrl.Url);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Error occurred while processing file: {SourceFilePath}", fileUrl.Url);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Invalid file URL: {FileUrl}", fileUrl.Url);
                    }
                }

                var zipFileName = $"{ApplicationNo}_POD.zip";

                using (var memoryStream = new MemoryStream())
                {
                    using (var zipStream = new ZipOutputStream(memoryStream))
                    {
                        zipStream.SetLevel(9);
                        var buffer = new byte[4096];

                        foreach (var file in directory.GetFiles())
                        {
                            var entry = new ZipEntry(file.Name)
                            {
                                DateTime = DateTime.Now
                            };
                            zipStream.PutNextEntry(entry);

                            using (var fileStream = file.OpenRead())
                            {
                                int sourceBytes;
                                while ((sourceBytes = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                                {
                                    zipStream.Write(buffer, 0, sourceBytes);
                                }
                            }
                        }

                        zipStream.Finish();
                    }

                    var zipBytes = memoryStream.ToArray();

                    if (zipBytes == null || !zipBytes.Any())
                    {
                        throw new Exception("No files found to zip.");
                    }

                    // Return the file directly with proper content disposition
                    return File(zipBytes, "application/zip", zipFileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating the zip file.");
                return StatusCode(500, new { Result = "Error", Message = "An error occurred while creating the zip file.", Details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> MarkOrderDelivered(string applicationNo)
        {
            var partnerIdCookie = Request.Cookies["SelectedMFI"];
            if (string.IsNullOrEmpty(partnerIdCookie) || !long.TryParse(partnerIdCookie, out long partnerId))
            {
                _logger.LogWarning("Invalid PartnerID cookie.");
                return BadRequest(new { Result = "Error", Message = "Invalid PartnerID cookie." });
            }

            if (string.IsNullOrEmpty(applicationNo))
            {
                _logger.LogWarning("ApplicationNo is required.");
                return BadRequest(new { Result = "Error", Message = "ApplicationNo is required." });
            }

            try
            {
                await _pODVerifyRepository.MarkOrderDeliveredAsync(applicationNo);

                return Json(new { Result = "OK", Message = "Order marked as delivered successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while marking order as delivered.");
                return StatusCode(500, new { Result = "Error", Message = "An error occurred while processing your request.", Details = ex.Message });
            }
        }

    }
}

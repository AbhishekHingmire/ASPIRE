using IMS2.Controllers;
using IMS2.Models;
using IMS2.Repository.Interfaces;
using IMS2.Session;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Velzon.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogin _loginRepository;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationController(ILogin userRepository, IConfiguration configuration, ILogger<AuthenticationController> logger)
        {
            _loginRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        [ActionName("SignInBasic")]
        public IActionResult SignInBasic()
        {
            return View();
        }

        [ActionName("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(Users model)
        {
            try
            {
                var user = _loginRepository.GetUserByUsernameAndPassword(model.Username, model.Password);

                if (user != null)
                {
                    var dictUserScreenRights = _loginRepository.GetUserRightsByScreens(user.ID);

                    var claims = new List<Claim>
                    {
                         new Claim(ClaimTypes.Name, user.Username??""),
                         new Claim(ClaimTypes.GivenName, user.Username ?? ""),
                         new Claim("ID", user.ID.ToString()),
                         new Claim(ClaimTypes.Role, user.Role ?? "")
                    };

                    foreach(var item in dictUserScreenRights)
                    {
                        claims.Add(new Claim("ScreenRights",$"{item.Key}|{item.Value}"));
                    }

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity));

                    SetCookie("SelectedMFI", "1");
                    SetCookie("SelectedMFIName", "Satin BD");

                    return RedirectToAction("BaseDashboard", "Dashboard");
                }
                else
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);
                }
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
                throw;
            }
        }

        private void SetCookie(string key, string value)
        {
            var cookieOptions = new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(1)
            };

            Response.Cookies.Append(key, value, cookieOptions);
        }

        private string GenerateJwtToken(Users user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]??"");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Username??""),
                    new Claim(ClaimTypes.GivenName, user.Username ?? ""),
                    new Claim(ClaimTypes.Role, user.Role ?? "")
                }),
                IssuedAt = DateTime.UtcNow,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return token.ToString()??"";
        }

        [ActionName("LogoutBasic")]
        public IActionResult LogoutBasic()
        {
            try
            {
                if (Request.Cookies != null)
                {
                    foreach (var cookie in Request.Cookies.Keys)
                    {
                        Response.Cookies.Append(cookie, "", new CookieOptions
                        {
                            Expires = DateTimeOffset.UtcNow.AddDays(-1)
                        });
                    }
                }

                HttpContext.Session.Clear();

                HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return View();
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while processing your request.");
                throw;
            }
        }


        [ActionName("SignUpBasic")]
        public IActionResult SignUpBasic()
        {
            return View();
        }

        [ActionName("SignUpCover")]
        public IActionResult SignUpCover()
        {
            return View();
        }

		[ActionName("PasswordChangeBasic")]
        public IActionResult PasswordChangeBasic()
        {
            return View();
        }

        [ActionName("PasswordChangeCover")]
        public IActionResult PasswordChangeCover()
        {
            return View();
        }

        [ActionName("PasswordResetBasic")]
        public IActionResult PasswordResetBasic()
        {
            return View();
        }

        [ActionName("PasswordResetCover")]
        public IActionResult PasswordResetCover()
        {
            return View();
        }

        [ActionName("LockScreenBasic")]
        public IActionResult LockScreenBasic()
        {
            return View();
        }

        [ActionName("LockScreenCover")]
        public IActionResult LockScreenCover()
        {
            return View();
        }

        [ActionName("LogoutCover")]
        public IActionResult LogoutCover()
        {
            return View();
        }

        [ActionName("SuccessMessageBasic")]
        public IActionResult SuccessMessageBasic()
        {
            return View();
        }

        [ActionName("SuccessMessageCover")]
        public IActionResult SuccessMessageCover()
        {
            return View();
        }

        [ActionName("TwoStepVerificationBasic")]
        public IActionResult TwoStepVerificationBasic()
        {
            return View();
        }

        [ActionName("TwoStepVerificationCover")]
        public IActionResult TwoStepVerificationCover()
        {
            return View();
        }

        [ActionName("Errors404Basic")]
        public IActionResult Errors404Basic()
        {
            return View();
        }

        [ActionName("Errors404Cover")]
        public IActionResult Errors404Cover()
        {
            return View();
        }

        [ActionName("Errors404Alt")]
        public IActionResult Errors404Alt()
        {
            return View();
        }

        [ActionName("Errors500")]
        public IActionResult Errors500()
        {
            return View();
        }

		[ActionName("Offline")]
        public IActionResult Offline()
        {
            return View();
        }

    }
}

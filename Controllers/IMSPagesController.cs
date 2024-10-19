using IMS2.Models;
using IMS2.Repository;
using IMS2.Repository.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IMS2.Controllers
{
    public class IMSPagesController : Controller
    {
        private readonly IUserCreation _userRepository;
        private readonly ILogger<IMSPagesController> _logger;

        public IMSPagesController(IUserCreation userRepository, ILogger<IMSPagesController> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        [ActionName("AppBookedOrders")]
        public IActionResult AppBookedOrders()
        {
            return View();
        }
    }
}

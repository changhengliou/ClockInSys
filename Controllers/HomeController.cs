using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using ReactSpa.Data;

namespace ReactSpa.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly UserManager<UserInfo> _userManager;

        public HomeController(UserManager<UserInfo> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(s => s.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));
            var roles = await _userManager.GetRolesAsync(user);
            
            return View(roles);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
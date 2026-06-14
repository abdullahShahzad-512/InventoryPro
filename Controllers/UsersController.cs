using IMS.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
namespace IMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class UsersController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersController(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }
        public IActionResult UserManagement() 
        {
            var users = _userManager.Users.ToList();

            return View(users);

        }
        [Authorize(Policy = "AdminAccess")]
        public IActionResult AddUser()
        {
            return RedirectToPage("/Account/Register", new { area = "Identity" });
        }
        public async Task<IActionResult> BlockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEnabledAsync(user, true);

                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.MaxValue);
            }
            return RedirectToAction("UserManagement");
        } 
        public  async Task<IActionResult> UnblockUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
            }
            return RedirectToAction("UserManagement");
        }
      
    }
}
 
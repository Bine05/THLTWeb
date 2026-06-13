using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(string msg = null, string err = null)
        {
            ViewBag.Message = msg;
            ViewBag.Error = err;
            var users = await _userManager.Users.ToListAsync();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> LockUnlock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return RedirectToAction(nameof(Index), new { err = "Không tìm thấy tài khoản." });
            }

            var enableResult = await _userManager.SetLockoutEnabledAsync(user, true);
            if (!enableResult.Succeeded)
            {
                return RedirectToAction(nameof(Index), new { err = "Lỗi khi bật LockoutEnabled: " + string.Join(", ", enableResult.Errors.Select(e => e.Description)) });
            }

            IdentityResult result;
            string successMsg = "";
            if (user.LockoutEnd != null && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Mở khóa
                result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow);
                successMsg = "Mở khóa thành công!";
            }
            else
            {
                // Khóa 100 năm
                result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                successMsg = "Khóa tài khoản thành công!";
            }

            if (!result.Succeeded)
            {
                return RedirectToAction(nameof(Index), new { err = "Lỗi khi khóa/mở khóa: " + string.Join(", ", result.Errors.Select(e => e.Description)) });
            }

            return RedirectToAction(nameof(Index), new { msg = successMsg });
        }
    }
}

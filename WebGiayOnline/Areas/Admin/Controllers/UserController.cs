using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebGiayOnline.Models;

namespace WebGiayOnline.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var currentEmail = User.Identity?.Name;
            var users = _userManager.Users.ToList();
            var userRolesViewModel = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                // ✅ Chỉ ẩn người dùng đang đăng nhập
                if (user.Email == currentEmail)
                    continue;

                userRolesViewModel.Add(new UserRoleViewModel
                {
                    UserId = user.Id,
                    Email = user.Email,
                    CurrentRole = roles.FirstOrDefault() ?? "None",
                    IsEnabled = user.IsEnabled
                });
            }

            // ✅ Ẩn vai trò "Company" và "Employee" khỏi danh sách gán vai trò
            ViewBag.AllRoles = await _roleManager.Roles
                .Where(r => r.Name != "Company" && r.Name != "Employee")
                .Select(r => r.Name)
                .ToListAsync();

            return View(userRolesViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(string userId)
        {
            var currentEmail = User.Identity?.Name;
            var user = await _userManager.FindByIdAsync(userId);

            // ✅ Không cho phép tự thay đổi trạng thái của chính mình
            if (user == null || user.Email == currentEmail)
            {
                TempData["Error"] = "Không thể cập nhật trạng thái người dùng này.";
                return RedirectToAction("Index");
            }

            user.IsEnabled = !user.IsEnabled;
            var result = await _userManager.UpdateAsync(user);

            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? $"Tài khoản {(user.IsEnabled ? "đã được kích hoạt" : "đã bị vô hiệu hóa")}."
                : "Có lỗi xảy ra khi cập nhật trạng thái.";

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeUserRole(string userId, string newRole)
        {
            var currentEmail = User.Identity?.Name;
            var user = await _userManager.FindByIdAsync(userId);

            // ✅ Không cho phép tự thay đổi vai trò của chính mình
            if (user == null || user.Email == currentEmail)
            {
                TempData["Error"] = "Không thể thay đổi vai trò người dùng này.";
                return RedirectToAction("Index");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!currentRoles.Contains(newRole))
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                {
                    TempData["Error"] = "Không thể xóa vai trò cũ.";
                    return RedirectToAction("Index");
                }

                var addResult = await _userManager.AddToRoleAsync(user, newRole);
                if (!addResult.Succeeded)
                {
                    TempData["Error"] = "Không thể gán vai trò mới.";
                    return RedirectToAction("Index");
                }

                TempData["Success"] = "Cập nhật vai trò thành công.";
            }
            else
            {
                TempData["Success"] = "Vai trò không thay đổi.";
            }

            return RedirectToAction("Index");
        }
    }

    public class UserRoleViewModel
    {
        public string UserId { get; set; }
        public string Email { get; set; }
        public string CurrentRole { get; set; }
        public bool IsEnabled { get; set; }
    }
}
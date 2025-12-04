using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(UserManager<IdentityUser> userManager,
                               RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();

            var model = new List<(IdentityUser User, IList<string> Roles)>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add((user, roles));
            }

            ViewBag.AllRoles = new[] { "Customer", "Engineer", "Admin" };

            return View(model);
        }

        // POST: /Admin/SetRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var allRoles = new[] { "Customer", "Engineer", "Admin" };

            // Remove user from all known roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles.Intersect(allRoles));

            // Add to selected role
            if (allRoles.Contains(role))
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

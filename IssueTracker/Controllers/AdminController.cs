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

        // POST: /Admin/AddRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Invalid user or role.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                TempData["ErrorMessage"] = "User not found.";
                return NotFound();
            }

            var validRoles = new[] { "Customer", "Engineer", "Admin" };
            if (!validRoles.Contains(role))
            {
                TempData["ErrorMessage"] = "Invalid role.";
                return RedirectToAction(nameof(Index));
            }

            // Ensure role exists
            if (!await _roleManager.RoleExistsAsync(role))
            {
                await _roleManager.CreateAsync(new IdentityRole(role));
            }

            // Add user to role if not already in it
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Successfully added {role} role to {user.Email}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to add role to user.";
                }
            }
            else
            {
                TempData["InfoMessage"] = $"User already has {role} role.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/RemoveRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Invalid user or role.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                TempData["ErrorMessage"] = "User not found.";
                return NotFound();
            }

            // Remove user from role if they have it
            if (await _userManager.IsInRoleAsync(user, role))
            {
                var result = await _userManager.RemoveFromRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Successfully removed {role} role from {user.Email}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to remove role from user.";
                }
            }
            else
            {
                TempData["InfoMessage"] = $"User does not have {role} role.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /Admin/SetRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetRole(string userId, string role)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                TempData["ErrorMessage"] = "Invalid user or role.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) 
            {
                TempData["ErrorMessage"] = "User not found.";
                return NotFound();
            }

            var allRoles = new[] { "Customer", "Engineer", "Admin" };

            // Remove user from all known roles
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles.Intersect(allRoles));

            // Add to selected role
            if (allRoles.Contains(role))
            {
                // Ensure role exists
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                }

                var result = await _userManager.AddToRoleAsync(user, role);
                if (result.Succeeded)
                {
                    TempData["SuccessMessage"] = $"Successfully set {user.Email} role to {role}.";
                }
                else
                {
                    TempData["ErrorMessage"] = "Failed to set user role.";
                }
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Admin/Issues
        public IActionResult Issues(string? status, string? assignedTo)
        {
            var issues = HttpContext.RequestServices.GetRequiredService<IssueTracker.Data.AppDbContext>()
                .Issues.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                issues = issues.Where(i => i.Status == status);
            }

            if (!string.IsNullOrEmpty(assignedTo))
            {
                issues = issues.Where(i => i.AssignedToUserId == assignedTo);
            }

            var issuesList = issues.OrderBy(i => i.Priority)
                                 .ThenByDescending(i => i.CreatedAt)
                                 .ToList();

            // Get user list for assignment filter
            var users = _userManager.Users.ToList();
            ViewBag.Users = users.Select(u => new { u.Id, u.Email }).ToList();
            
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentAssignedTo = assignedTo;
            ViewBag.StatusOptions = new[] { "Open", "In Progress", "Waiting for User", "Resolved", "Closed" };

            return View(issuesList);
        }

        // POST: /Admin/BulkAssign
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkAssign(int[] issueIds, string assignToUserId)
        {
            if (issueIds == null || !issueIds.Any())
            {
                TempData["ErrorMessage"] = "No issues selected.";
                return RedirectToAction(nameof(Issues));
            }

            var appContext = HttpContext.RequestServices.GetRequiredService<IssueTracker.Data.AppDbContext>();
            var issues = appContext.Issues.Where(i => issueIds.Contains(i.Id)).ToList();

            foreach (var issue in issues)
            {
                issue.AssignedToUserId = string.IsNullOrEmpty(assignToUserId) ? null : assignToUserId;
                if (!string.IsNullOrEmpty(assignToUserId) && issue.Status == "Open")
                {
                    issue.Status = "In Progress";
                }
            }

            appContext.SaveChanges();
            TempData["SuccessMessage"] = $"Successfully updated {issues.Count} issues.";

            return RedirectToAction(nameof(Issues));
        }

        // POST: /Admin/BulkUpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult BulkUpdateStatus(int[] issueIds, string newStatus)
        {
            if (issueIds == null || !issueIds.Any())
            {
                TempData["ErrorMessage"] = "No issues selected.";
                return RedirectToAction(nameof(Issues));
            }

            var validStatuses = new[] { "Open", "In Progress", "Waiting for User", "Resolved", "Closed" };
            if (!validStatuses.Contains(newStatus))
            {
                TempData["ErrorMessage"] = "Invalid status selected.";
                return RedirectToAction(nameof(Issues));
            }

            var appContext = HttpContext.RequestServices.GetRequiredService<IssueTracker.Data.AppDbContext>();
            var issues = appContext.Issues.Where(i => issueIds.Contains(i.Id)).ToList();

            foreach (var issue in issues)
            {
                issue.Status = newStatus;
            }

            appContext.SaveChanges();
            TempData["SuccessMessage"] = $"Successfully updated status of {issues.Count} issues to {newStatus}.";

            return RedirectToAction(nameof(Issues));
        }

        // POST: /Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                TempData["ErrorMessage"] = "Invalid user ID.";
                return RedirectToAction(nameof(Index));
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction(nameof(Index));
            }

            // Prevent deletion of the current admin user
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null && currentUser.Id == userId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction(nameof(Index));
            }

            // Check if user has created any issues or comments
            var appContext = HttpContext.RequestServices.GetRequiredService<IssueTracker.Data.AppDbContext>();
            var hasIssues = appContext.Issues.Any(i => i.CreatedByUserId == userId || i.AssignedToUserId == userId);
            var hasComments = appContext.Comments.Any(c => c.CreatedByUserId == userId);

            if (hasIssues || hasComments)
            {
                TempData["ErrorMessage"] = "Cannot delete user who has created issues or comments. Please reassign or delete their content first.";
                return RedirectToAction(nameof(Index));
            }

            // Remove user from all roles first
            var userRoles = await _userManager.GetRolesAsync(user);
            if (userRoles.Any())
            {
                await _userManager.RemoveFromRolesAsync(user, userRoles);
            }

            // Delete the user
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"Successfully deleted user {user.Email}.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user. " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

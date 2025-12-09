using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace IssueTracker.Controllers
{
    [Authorize]
    public class IssuesController : Controller
    {
        private readonly AppDbContext _context;

        public IssuesController(AppDbContext context)
        {
            _context = context;
        }

        // GET: /Issues
        public IActionResult Index(string? status, string? search, bool showAll = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEngineerOrAdmin = User.IsInRole("Engineer") || User.IsInRole("Admin");

            var baseQuery = _context.Issues.AsQueryable();

            // For dashboard counts, Engineers/Admins see all; others only their own.
            if (!isEngineerOrAdmin && userId != null)
            {
                baseQuery = baseQuery.Where(i => i.CreatedByUserId == userId);
            }

            var total = baseQuery.Count();

            int countOpen = baseQuery.Count(i => i.Status == "Open");
            int countInProgress = baseQuery.Count(i => i.Status == "In Progress");
            int countWaiting = baseQuery.Count(i => i.Status == "Waiting for User");
            int countResolved = baseQuery.Count(i => i.Status == "Resolved");
            int countClosed = baseQuery.Count(i => i.Status == "Closed");

            ViewBag.CountOpen = countOpen;
            ViewBag.CountInProgress = countInProgress;
            ViewBag.CountWaiting = countWaiting;
            ViewBag.CountResolved = countResolved;
            ViewBag.CountClosed = countClosed;
            ViewBag.TotalIssues = total;

            double pct(int c) => total == 0 ? 0 : Math.Round((double)c * 100 / total);

            ViewBag.PctOpen = pct(countOpen);
            ViewBag.PctInProgress = pct(countInProgress);
            ViewBag.PctWaiting = pct(countWaiting);
            ViewBag.PctResolved = pct(countResolved);
            ViewBag.PctClosed = pct(countClosed);

            // Apply filters for table
            var query = baseQuery;

            if ((!isEngineerOrAdmin || !showAll) && userId != null)
            {
                query = query.Where(i => i.CreatedByUserId == userId);
            }

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(i => i.Status == status);
            }

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i => i.Title.Contains(search));
            }

            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.ShowAll = showAll;

            query = query.OrderBy(i => i.Priority)
                         .ThenByDescending(i => i.CreatedAt);

            var issues = query.ToList();
            return View(issues);
        }

        // GET: /Issues/Create - Only Customers can create issues
        [Authorize(Roles = "Customer")]
        public IActionResult Create()
        {            
            return View();
        }

        // POST: /Issues/Create - Only Customers can create issues
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Customer")]
        public IActionResult Create(Issue issue)
        {
            if (ModelState.IsValid)
            {
                // Set the user who created the issue
                issue.CreatedByUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                issue.CreatedAt = DateTime.UtcNow;

                _context.Issues.Add(issue);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Issue created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(issue);
        }

        // GET: /Issues/Details/5
        public IActionResult Details(int id)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEngineerOrAdmin = User.IsInRole("Engineer") || User.IsInRole("Admin");

            // Check if user can view this issue
            if (!isEngineerOrAdmin && issue.CreatedByUserId != userId)
            {
                return Forbid();
            }

            // Get comments for this issue
            var comments = _context.Comments
                .Where(c => c.IssueId == id)
                .OrderBy(c => c.CreatedAt)
                .ToList();

            ViewBag.Comments = comments;
            ViewBag.CanEdit = isEngineerOrAdmin || issue.CreatedByUserId == userId;

            return View(issue);
        }

        // GET: /Issues/Edit/5
        public IActionResult Edit(int id)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEngineerOrAdmin = User.IsInRole("Engineer") || User.IsInRole("Admin");
            var isAdmin = User.IsInRole("Admin");

            // Check if user can edit this issue
            if (!isEngineerOrAdmin && issue.CreatedByUserId != userId)
            {
                return Forbid();
            }

            // Engineers can only edit status of issues assigned to themselves (except admins)
            if (!isAdmin && User.IsInRole("Engineer") && issue.AssignedToUserId != userId)
            {
                TempData["ErrorMessage"] = "Engineers can only edit issues assigned to themselves. Please assign this issue to yourself first to update its status.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            return View(issue);
        }

        // POST: /Issues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Issue issue)
        {
            if (id != issue.Id)
            {
                return NotFound();
            }

            var existingIssue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (existingIssue == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEngineerOrAdmin = User.IsInRole("Engineer") || User.IsInRole("Admin");
            var isAdmin = User.IsInRole("Admin");

            // Check if user can edit this issue
            if (!isEngineerOrAdmin && existingIssue.CreatedByUserId != userId)
            {
                return Forbid();
            }

            // Engineers can only edit status of issues assigned to themselves (except admins)
            if (!isAdmin && User.IsInRole("Engineer") && existingIssue.AssignedToUserId != userId)
            {
                TempData["ErrorMessage"] = "Engineers can only edit issues assigned to themselves. Please assign this issue to yourself first to update its status.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            if (ModelState.IsValid)
            {
                // Engineers can only change status, not other fields
                if (User.IsInRole("Engineer") && !User.IsInRole("Admin"))
                {
                    // Engineers can only update status of their assigned issues
                    existingIssue.Status = issue.Status;
                }
                else
                {
                    // Admins and Customers can update all fields
                    existingIssue.Title = issue.Title;
                    existingIssue.Description = issue.Description;
                    existingIssue.Priority = issue.Priority;
                    
                    // Only Engineers/Admins can change status
                    if (isEngineerOrAdmin)
                    {
                        existingIssue.Status = issue.Status;
                    }
                }

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Issue updated successfully!";
                return RedirectToAction(nameof(Details), new { id = issue.Id });
            }

            return View(issue);
        }

        // POST: /Issues/PostComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PostComment(int issueId, string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                TempData["ErrorMessage"] = "Comment text is required.";
                return RedirectToAction(nameof(Details), new { id = issueId });
            }

            var issue = _context.Issues.FirstOrDefault(i => i.Id == issueId);
            if (issue == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEngineerOrAdmin = User.IsInRole("Engineer") || User.IsInRole("Admin");

            // Check if user can comment on this issue
            if (!isEngineerOrAdmin && issue.CreatedByUserId != userId)
            {
                return Forbid();
            }

            var comment = new Comment
            {
                IssueId = issueId,
                Text = text,
                CreatedByUserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            _context.SaveChanges();

            TempData["SuccessMessage"] = "Comment added successfully!";
            return RedirectToAction(nameof(Details), new { id = issueId });
        }

        // GET: /Issues/Delete/5
        [Authorize(Roles = "Admin")]
        public IActionResult Delete(int id)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            return View(issue);
        }

        // POST: /Issues/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteConfirmed(int id)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue != null)
            {
                // Delete associated comments first
                var comments = _context.Comments.Where(c => c.IssueId == id);
                _context.Comments.RemoveRange(comments);
                
                _context.Issues.Remove(issue);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Issue deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /Issues/AssignedToMe
        [Authorize(Roles = "Engineer,Admin")]
        public IActionResult AssignedToMe()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            var assignedIssues = _context.Issues
                .Where(i => i.AssignedToUserId == userId)
                .OrderBy(i => i.Priority)
                .ThenByDescending(i => i.CreatedAt)
                .ToList();

            ViewBag.PageTitle = "Issues Assigned to Me";
            ViewBag.IsAssignedView = true;

            return View("Index", assignedIssues);
        }

        // POST: /Issues/AssignToMe/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Engineer,Admin")]
        public IActionResult AssignToMe(int id)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            // Prevent engineers from assigning closed issues to themselves
            if (issue.Status == "Closed")
            {
                TempData["ErrorMessage"] = "Cannot assign closed issues. Please reopen the issue first if work is needed.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            issue.AssignedToUserId = userId;

            // If assigning to self, likely starting work
            if (issue.Status == "Open")
            {
                issue.Status = "In Progress";
            }

            _context.SaveChanges();
            TempData["SuccessMessage"] = "Issue assigned to you successfully!";

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: /Issues/Unassign/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Engineer,Admin")]
        public IActionResult Unassign(int id)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            issue.AssignedToUserId = null;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Issue unassigned successfully!";

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: /Issues/UpdateStatus/5 - For engineers to update status on self-assigned issues
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Engineer,Admin")]
        public IActionResult UpdateStatus(int id, string status)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isAdmin = User.IsInRole("Admin");

            // Only allow engineers to update status on their assigned issues or admins on any
            if (!isAdmin && issue.AssignedToUserId != userId)
            {
                return Forbid();
            }

            // Validate status
            var validStatuses = new[] { "Open", "In Progress", "Waiting for User", "Resolved", "Closed" };
            if (!validStatuses.Contains(status))
            {
                TempData["ErrorMessage"] = "Invalid status selected.";
                return RedirectToAction(nameof(Details), new { id = id });
            }

            issue.Status = status;
            _context.SaveChanges();
            TempData["SuccessMessage"] = "Issue status updated successfully!";

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // POST: /Issues/CloseIssue/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CloseIssue(int id)
        {
            var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
            if (issue == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var isEngineerOrAdmin = User.IsInRole("Engineer") || User.IsInRole("Admin");

            // Only the creator (customer) or engineers/admins can close an issue
            if (issue.CreatedByUserId != userId && !isEngineerOrAdmin)
            {
                return Forbid();
            }

            // Only allow closing if issue is Resolved or in certain states
            if (issue.Status == "Resolved" || issue.Status == "Waiting for User" || isEngineerOrAdmin)
            {
                issue.Status = "Closed";
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Issue closed successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Issue can only be closed when it's resolved or waiting for user action.";
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}

using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Authorization;
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

        // the rest of the controller (Details, PostComment, Create, Edit, Delete) stays as we already set up
    }
}

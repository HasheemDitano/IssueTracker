using System.Diagnostics;
using IssueTracker.Data;
using IssueTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var total = _context.Issues.Count();
            var open = _context.Issues.Count(i => i.Status == "Open");
            var inProgress = _context.Issues.Count(i => i.Status == "In Progress");
            var resolvedClosed = _context.Issues.Count(i =>
                i.Status == "Resolved" || i.Status == "Closed");

            ViewBag.TotalIssues = total;
            ViewBag.OpenIssues = open;
            ViewBag.InProgressIssues = inProgress;
            ViewBag.ResolvedClosedIssues = resolvedClosed;

            return View();
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

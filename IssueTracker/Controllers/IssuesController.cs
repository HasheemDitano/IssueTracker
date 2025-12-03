using IssueTracker.Models;
using Microsoft.AspNetCore.Mvc;

namespace IssueTracker.Controllers;


    public class IssuesController : Controller
    {
        private readonly AppDbContext _context;

        public IssuesController(AppDbContext context)
        {
            _context = context;
        }

    public IActionResult Edit(int id)
    {
        var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
        if (issue == null) return NotFound();

        return View(issue);
    }

    [HttpPost]
    public IActionResult Edit(int id, Issue updatedIssue)
    {
        if (!ModelState.IsValid) return View(updatedIssue);

        var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
        if (issue == null) return NotFound();

        issue.Title = updatedIssue.Title;
        _context.SaveChanges();

        return RedirectToAction(nameof(Index));
    }

    public IActionResult Delete(int id)
    {
        var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
        if (issue == null) return NotFound();

        return View(issue);
    }

    [HttpPost, ActionName("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var issue = _context.Issues.FirstOrDefault(i => i.Id == id);
        if (issue == null) return NotFound();

        _context.Issues.Remove(issue);
        _context.SaveChanges();
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Index()
        {
            var issues = _context.Issues.ToList();
            return View(issues);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Issue issue)
        {
            if (!ModelState.IsValid) return View(issue);

            _context.Issues.Add(issue);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }
    }

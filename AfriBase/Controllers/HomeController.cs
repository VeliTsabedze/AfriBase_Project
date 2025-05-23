using AfriBase.Data;
using AfriBase.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AfriBase.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get featured artifacts (currently just showing the most recent artifacts)
            var featuredArtifacts = await _context.Artifacts
                .Include(a => a.Owner)
                .Include(a => a.Bids)
                .Where(a => a.IsAvailableForBidding)
                .OrderByDescending(a => a.DateAdded)
                .Take(6)
                .ToListAsync();

            // Get recent bids for the activity section
            var recentBids = await _context.Bids
                .Include(b => b.User)
                .Include(b => b.Artifact)
                .OrderByDescending(b => b.BidDate)
                .Take(5)
                .ToListAsync();

            // Pass the data to the view using ViewData
            ViewData["FeaturedArtifacts"] = featuredArtifacts;
            ViewData["RecentBids"] = recentBids;

            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ExploreCategories()
        {
            return View();
        }

        public IActionResult ExploreRegions()
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
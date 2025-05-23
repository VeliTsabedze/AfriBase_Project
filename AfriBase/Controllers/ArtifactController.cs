using AfriBase.Data;
using AfriBase.Models;
using AfriBase.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AfriBase.Controllers
{
    /// <summary>
    /// Controller handling artifact operations
    /// </summary>
    public class ArtifactController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment webHostEnvironment,
        ILogger<ArtifactController> logger) : Controller
    {
        private readonly ApplicationDbContext _context = context;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;
        private readonly ILogger<ArtifactController> _logger = logger;

        /// <summary>
        /// Display a list of artifacts
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(string sortOrder, string searchString, string currentFilter, int? pageNumber)
        {
            try
            {
                ViewData["CurrentSort"] = sortOrder;
                ViewData["NameSortParm"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
                ViewData["DateSortParm"] = sortOrder == "Date" ? "date_desc" : "Date";
                ViewData["ValueSortParm"] = sortOrder == "Value" ? "value_desc" : "Value";
                ViewData["OriginSortParm"] = sortOrder == "Origin" ? "origin_desc" : "Origin";

                if (searchString != null)
                {
                    pageNumber = 1;
                }
                else
                {
                    searchString = currentFilter;
                }

                ViewData["CurrentFilter"] = searchString;

                // Get all artifacts from the database
                var artifacts = from a in _context.Artifacts
                                select a;

                // Apply search filter if provided
                if (!string.IsNullOrEmpty(searchString))
                {
                    artifacts = artifacts.Where(a =>
                        a.Name.Contains(searchString) ||
                        a.Description.Contains(searchString) ||
                        a.Origin.Contains(searchString));
                }

                // Apply sorting
                artifacts = sortOrder switch
                {
                    "name_desc" => artifacts.OrderByDescending(a => a.Name),
                    "Date" => artifacts.OrderBy(a => a.DateAdded),
                    "date_desc" => artifacts.OrderByDescending(a => a.DateAdded),
                    "Value" => artifacts.OrderBy(a => a.EstimatedValue),
                    "value_desc" => artifacts.OrderByDescending(a => a.EstimatedValue),
                    "Origin" => artifacts.OrderBy(a => a.Origin),
                    "origin_desc" => artifacts.OrderByDescending(a => a.Origin),
                    _ => artifacts.OrderBy(a => a.Name),
                };

                // Load the artifacts with related data
                var artifactsList = await artifacts
                    .Include(a => a.Owner)
                    .Include(a => a.Bids)
                    .AsNoTracking()
                    .ToListAsync();

                // Determine if the current user can bid on artifacts
                bool canBid = User.Identity != null && User.Identity.IsAuthenticated && User.IsInRole("Bidder");

                return View(artifactsList);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving artifacts list");
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Display detailed information about a specific artifact
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get the artifact with related data
                var artifact = await _context.Artifacts
                    .Include(a => a.Owner)
                    .Include(a => a.Bids)
                        .ThenInclude(b => b.User)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.ArtifactId == id);

                if (artifact == null)
                {
                    return NotFound();
                }

                // Get current user ID for authorization checks
                var currentUserId = await GetCurrentUserId();

                // Calculate highest bid amount separately to avoid null propagation in LINQ
                var highestBid = artifact.Bids.OrderByDescending(b => b.BidAmount).FirstOrDefault();
                decimal currentHighestBid = highestBid != null ? highestBid.BidAmount : 0;

                // Create the view model
                var viewModel = new ArtifactDetailsViewModel
                {
                    Artifact = artifact,
                    HighestBid = highestBid,
                    OwnerInfo = new ArtifactOwnerViewModel
                    {
                        UserId = artifact.OwnerId,
                        FullName = artifact.Owner.FullName,
                        Country = artifact.Owner.Country,
                        ProfileImageUrl = artifact.Owner.ProfileImageUrl,
                        IsVerified = artifact.Owner.IsVerified,
                        Bio = artifact.Owner.Bio,
                        RegistrationDate = artifact.Owner.RegistrationDate,
                        ArtifactCount = _context.Artifacts.Count(a => a.OwnerId == artifact.OwnerId)
                    },
                    RecentBids = artifact.Bids
                        .OrderByDescending(b => b.BidDate)
                        .Take(5)
                        .Select(b => new BidSummaryViewModel
                        {
                            BidId = b.BidId,
                            ArtifactId = b.ArtifactId,
                            ArtifactName = artifact.Name,
                            BidderName = b.User.FullName,
                            BidAmount = b.BidAmount,
                            BidDate = b.BidDate.ToString("MMM dd, yyyy HH:mm"),
                            Status = b.Status
                        })
                        .ToList(),
                    // Create a new bid view model for the user to place a bid
                    NewBidViewModel = new BidViewModel
                    {
                        ArtifactId = artifact.ArtifactId,
                        ArtifactName = artifact.Name,
                        ArtifactImageUrl = artifact.ImageUrl,
                        StartingBid = artifact.StartingBid ?? 0,
                        CurrentHighestBid = currentHighestBid
                    },
                    // Determine if the current user can edit this artifact
                    CanEdit = User.Identity != null &&
                              User.Identity.IsAuthenticated &&
                              (User.IsInRole("Admin") ||
                              (User.IsInRole("Seller") && currentUserId == artifact.OwnerId)),
                    // Determine if the current user can bid on this artifact
                    CanBid = User.Identity != null &&
                             User.Identity.IsAuthenticated &&
                             User.IsInRole("Bidder") &&
                             artifact.IsAvailableForBidding &&
                             currentUserId != artifact.OwnerId
                };

                // Get similar artifacts based on category and region
                viewModel.SimilarArtifacts = await _context.Artifacts
                    .Where(a => a.ArtifactId != artifact.ArtifactId &&
                           (a.Category == artifact.Category || a.Region == artifact.Region))
                    .OrderBy(a => Guid.NewGuid()) // Randomize the order
                    .Take(3)
                    .Select(a => new ArtifactSummaryViewModel
                    {
                        ArtifactId = a.ArtifactId,
                        Name = a.Name,
                        Origin = a.Origin,
                        EstimatedYear = a.EstimatedYear,
                        StartingBid = a.StartingBid ?? 0,
                        // Replace inline calculation with a materialized query to avoid null propagation issues
                        CurrentHighestBid = a.Bids.Any() ? a.Bids.Max(b => b.BidAmount) : 0,
                        BidCount = a.Bids.Count,
                        ImageUrl = a.ImageUrl,
                        Category = a.Category
                    })
                    .ToListAsync();

                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving artifact details for id {ArtifactId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Display the create artifact form
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Process a create artifact request
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ArtifactViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Get the current user
                    var userId = await GetCurrentUserId();
                    if (userId == null)
                    {
                        return RedirectToAction("Login", "Account");
                    }

                    // Create a new artifact
                    var artifact = new Artifact
                    {
                        Name = model.Name,
                        Description = model.Description,
                        Origin = model.Origin,
                        EstimatedYear = model.EstimatedYear,
                        EstimatedValue = model.EstimatedValue,
                        Category = model.Category,
                        Condition = model.Condition,
                        StartingBid = model.StartingBid,
                        IsAvailableForBidding = model.IsAvailableForBidding,
                        OwnerId = userId,
                        DateAdded = DateTime.Now,
                        Region = model.Region,
                        // Default image URL to avoid null checks later
                        ImageUrl = "/images/artifacts/no-image.jpg"
                    };

                    // If an image was uploaded, save it
                    if (model.ImageFile != null)
                    {
                        // Generate a unique file name
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                        var artifactImagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "artifacts");
                        var filePath = Path.Combine(artifactImagesPath, fileName);

                        // Create the directory if it doesn't exist
                        Directory.CreateDirectory(artifactImagesPath);

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        // Set the artifact's image URL
                        artifact.ImageUrl = $"/images/artifacts/{fileName}";
                    }

                    // Add the artifact to the database
                    _context.Add(artifact);
                    await _context.SaveChangesAsync();

                    // Redirect to the details page
                    return RedirectToAction(nameof(Details), new { id = artifact.ArtifactId });
                }
                catch (Exception ex)
                {
                    // Log the exception and display a generic error message
                    _logger.LogError(ex, "Error creating artifact");
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Display the edit artifact form
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get the artifact
                var artifact = await _context.Artifacts.FindAsync(id);
                if (artifact == null)
                {
                    return NotFound();
                }

                // Check if the current user is authorized to edit this artifact
                var userId = await GetCurrentUserId();
                if (userId != artifact.OwnerId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Create a view model with the artifact's information
                var model = new ArtifactViewModel
                {
                    ArtifactId = artifact.ArtifactId,
                    Name = artifact.Name,
                    Description = artifact.Description,
                    Origin = artifact.Origin,
                    EstimatedYear = artifact.EstimatedYear,
                    EstimatedValue = artifact.EstimatedValue,
                    Category = artifact.Category,
                    Condition = artifact.Condition,
                    CurrentImageUrl = artifact.ImageUrl,
                    StartingBid = artifact.StartingBid,
                    IsAvailableForBidding = artifact.IsAvailableForBidding,
                    Region = artifact.Region
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving artifact for editing, id: {ArtifactId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Process an edit artifact request
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ArtifactViewModel model)
        {
            if (id != model.ArtifactId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the artifact
                    var artifact = await _context.Artifacts.FindAsync(id);
                    if (artifact == null)
                    {
                        return NotFound();
                    }

                    // Check if the current user is authorized to edit this artifact
                    var userId = await GetCurrentUserId();
                    if (userId != artifact.OwnerId && !User.IsInRole("Admin"))
                    {
                        return RedirectToAction("AccessDenied", "Account");
                    }

                    // Update the artifact's information
                    artifact.Name = model.Name;
                    artifact.Description = model.Description;
                    artifact.Origin = model.Origin;
                    artifact.EstimatedYear = model.EstimatedYear;
                    artifact.EstimatedValue = model.EstimatedValue;
                    artifact.Category = model.Category;
                    artifact.Condition = model.Condition;
                    artifact.StartingBid = model.StartingBid;
                    artifact.IsAvailableForBidding = model.IsAvailableForBidding;
                    artifact.Region = model.Region;

                    // If a new image was uploaded, save it
                    if (model.ImageFile != null)
                    {
                        // Generate a unique file name
                        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";
                        var artifactImagesPath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "artifacts");
                        var filePath = Path.Combine(artifactImagesPath, fileName);

                        // Create the directory if it doesn't exist
                        Directory.CreateDirectory(artifactImagesPath);

                        // Save the file
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }

                        // Delete the old image if it's not the default
                        if (!string.IsNullOrEmpty(artifact.ImageUrl) &&
                            artifact.ImageUrl != "/images/artifacts/no-image.jpg")
                        {
                            string oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, artifact.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldFilePath))
                            {
                                System.IO.File.Delete(oldFilePath);
                            }
                        }

                        // Set the artifact's image URL
                        artifact.ImageUrl = $"/images/artifacts/{fileName}";
                    }

                    // Update the artifact in the database
                    _context.Update(artifact);
                    await _context.SaveChangesAsync();

                    // Redirect to the details page
                    return RedirectToAction(nameof(Details), new { id = artifact.ArtifactId });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!ArtifactExists(id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency exception while updating artifact {ArtifactId}", id);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception and display a generic error message
                    _logger.LogError(ex, "Error updating artifact {ArtifactId}", id);
                    ModelState.AddModelError(string.Empty, "An error occurred while processing your request. Please try again later.");
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        /// <summary>
        /// Display the delete artifact confirmation page
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Seller")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                // Get the artifact with related data
                var artifact = await _context.Artifacts
                    .Include(a => a.Owner)
                    .FirstOrDefaultAsync(a => a.ArtifactId == id);

                if (artifact == null)
                {
                    return NotFound();
                }

                // Check if the current user is authorized to delete this artifact
                var userId = await GetCurrentUserId();
                if (userId != artifact.OwnerId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                return View(artifact);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving artifact for deletion, id: {ArtifactId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Process a delete artifact request
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Seller")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                // Get the artifact
                var artifact = await _context.Artifacts.FindAsync(id);
                if (artifact == null)
                {
                    return NotFound();
                }

                // Check if the current user is authorized to delete this artifact
                var userId = await GetCurrentUserId();
                if (userId != artifact.OwnerId && !User.IsInRole("Admin"))
                {
                    return RedirectToAction("AccessDenied", "Account");
                }

                // Delete the artifact's image if it's not the default
                if (!string.IsNullOrEmpty(artifact.ImageUrl) &&
                    artifact.ImageUrl != "/images/artifacts/no-image.jpg")
                {
                    string filePath = Path.Combine(_webHostEnvironment.WebRootPath, artifact.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }

                // Delete the artifact
                _context.Artifacts.Remove(artifact);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error deleting artifact {ArtifactId}", id);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Display artifacts by category
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByCategory(ArtifactCategory category)
        {
            try
            {
                // Get artifacts in the specified category
                var artifacts = await _context.Artifacts
                    .Where(a => a.Category == category)
                    .Include(a => a.Owner)
                    .Include(a => a.Bids)
                    .AsNoTracking()
                    .ToListAsync();

                ViewData["CategoryName"] = category.ToString();

                return View(artifacts);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving artifacts by category: {Category}", category);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Display artifacts by region
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ByRegion(AfricanRegion region)
        {
            try
            {
                // Get artifacts from the specified region
                var artifacts = await _context.Artifacts
                    .Where(a => a.Region == region)
                    .Include(a => a.Owner)
                    .Include(a => a.Bids)
                    .AsNoTracking()
                    .ToListAsync();

                ViewData["RegionName"] = region.ToString();

                return View(artifacts);
            }
            catch (Exception ex)
            {
                // Log the exception and redirect to an error page
                _logger.LogError(ex, "Error retrieving artifacts by region: {Region}", region);
                return RedirectToAction("Error", "Home");
            }
        }

        /// <summary>
        /// Helper method to get the current user's ID
        /// </summary>
        private async Task<string?> GetCurrentUserId()
        {
            var user = await _userManager.GetUserAsync(User);
            return user?.Id;
        }

        /// <summary>
        /// Helper method to check if an artifact exists
        /// </summary>
        private bool ArtifactExists(int id)
        {
            return _context.Artifacts.Any(e => e.ArtifactId == id);
        }
    }
}
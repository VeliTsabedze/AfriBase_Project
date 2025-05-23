using AfriBase.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AfriBase.Data
{
    /// <summary>
    /// Initializes the database with seed data
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Seeds the database with initial data
        /// </summary>
        /// <param name="context">The application database context</param>
        /// <param name="userManager">The ASP.NET Core Identity user manager</param>
        /// <param name="roleManager">The ASP.NET Core Identity role manager</param>
        public static async Task InitializeAsync(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Look for any users
            if (await userManager.Users.AnyAsync())
            {
                return; // DB has been seeded
            }

            // Create roles
            var roles = new List<IdentityRole>
            {
                new() { Name = "Admin", NormalizedName = "ADMIN" },
                new() { Name = "User", NormalizedName = "USER" },
                new() { Name = "Seller", NormalizedName = "SELLER" },
                new() { Name = "Bidder", NormalizedName = "BIDDER" }
            };

            foreach (var role in roles)
            {
                await roleManager.CreateAsync(role);
            }

            // Create admin user
            var adminUser = new ApplicationUser
            {
                UserName = "admin@afribase.com",
                Email = "admin@afribase.com",
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                Country = "South Africa",
                IsVerified = true,
                PhoneNumber = "+27123456789",
                ProfileImageUrl = "/images/default-profile.jpg",
                Bio = "System administrator for the AfriBase platform."
            };

            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // Create regular user
            var regularUser = new ApplicationUser
            {
                UserName = "user@afribase.com",
                Email = "user@afribase.com",
                EmailConfirmed = true,
                FirstName = "Regular",
                LastName = "User",
                Country = "Kenya",
                IsVerified = true,
                PhoneNumber = "+254123456789",
                ProfileImageUrl = "/images/default-profile.jpg",
                Bio = "Enthusiast and collector of African artifacts."
            };

            result = await userManager.CreateAsync(regularUser, "User123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(regularUser, "User");
                await userManager.AddToRoleAsync(regularUser, "Bidder");
            }

            // Create seller user
            var sellerUser = new ApplicationUser
            {
                UserName = "seller@afribase.com",
                Email = "seller@afribase.com",
                EmailConfirmed = true,
                FirstName = "Artifact",
                LastName = "Seller",
                Country = "Nigeria",
                IsVerified = true,
                PhoneNumber = "+234123456789",
                ProfileImageUrl = "/images/default-profile.jpg",
                Bio = "Experienced curator and seller of authentic African artifacts with over 10 years of experience."
            };

            result = await userManager.CreateAsync(sellerUser, "Seller123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(sellerUser, "User");
                await userManager.AddToRoleAsync(sellerUser, "Seller");
            }

            // Create sample artifacts
            var artifacts = new List<Artifact>
            {
                new()
                {
                    Name = "Dogon Mask",
                    Description = "Traditional ceremonial mask from the Dogon people of Mali. Used in important religious ceremonies and dances to connect with ancestral spirits.",
                    Origin = "Mali",
                    EstimatedYear = 1890,
                    EstimatedValue = 4500.00M,
                    Category = ArtifactCategory.Mask,
                    Condition = ArtifactCondition.VeryGood,
                    ImageUrl = "/images/artifacts/dogon-mask.jpg",
                    DateAdded = DateTime.Now.AddDays(-30),
                    StartingBid = 2000.00M,
                    IsAvailableForBidding = true,
                    OwnerId = sellerUser.Id,
                    Region = AfricanRegion.WestAfrica
                },
                new()
                {
                    Name = "Zulu Beaded Necklace",
                    Description = "Intricately beaded necklace from the Zulu tribe of South Africa. The colors and patterns have specific cultural meanings.",
                    Origin = "South Africa",
                    EstimatedYear = 1950,
                    EstimatedValue = 850.00M,
                    Category = ArtifactCategory.Jewelry,
                    Condition = ArtifactCondition.Excellent,
                    ImageUrl = "/images/artifacts/zulu-necklace.jpg",
                    DateAdded = DateTime.Now.AddDays(-25),
                    StartingBid = 400.00M,
                    IsAvailableForBidding = true,
                    OwnerId = sellerUser.Id,
                    Region = AfricanRegion.SouthernAfrica
                },
                new()
                {
                    Name = "Ethiopian Orthodox Cross",
                    Description = "Hand-crafted Ethiopian Orthodox cross, showing the distinctive patterns of Ethiopian Christianity. Used for religious ceremonies.",
                    Origin = "Ethiopia",
                    EstimatedYear = 1880,
                    EstimatedValue = 3200.00M,
                    Category = ArtifactCategory.ReligiousItem,
                    Condition = ArtifactCondition.Good,
                    ImageUrl = "/images/artifacts/ethiopian-cross.jpg",
                    DateAdded = DateTime.Now.AddDays(-20),
                    StartingBid = 1500.00M,
                    IsAvailableForBidding = true,
                    OwnerId = sellerUser.Id,
                    Region = AfricanRegion.EastAfrica
                },
                new()
                {
                    Name = "Djembe Drum",
                    Description = "Traditional West African djembe drum with goatskin head and hardwood body. Used in traditional ceremonies and music performances.",
                    Origin = "Guinea",
                    EstimatedYear = 1970,
                    EstimatedValue = 950.00M,
                    Category = ArtifactCategory.MusicalInstrument,
                    Condition = ArtifactCondition.VeryGood,
                    ImageUrl = "/images/artifacts/djembe-drum.jpg",
                    DateAdded = DateTime.Now.AddDays(-15),
                    StartingBid = 500.00M,
                    IsAvailableForBidding = true,
                    OwnerId = sellerUser.Id,
                    Region = AfricanRegion.WestAfrica
                },
                new()
                {
                    Name = "Maasai Shield",
                    Description = "Traditional Maasai warrior shield made of buffalo hide and painted with natural pigments. Used for both protection and ceremonies.",
                    Origin = "Kenya",
                    EstimatedYear = 1930,
                    EstimatedValue = 2800.00M,
                    Category = ArtifactCategory.Weapon,
                    Condition = ArtifactCondition.Good,
                    ImageUrl = "/images/artifacts/maasai-shield.jpg",
                    DateAdded = DateTime.Now.AddDays(-10),
                    StartingBid = 1200.00M,
                    IsAvailableForBidding = true,
                    OwnerId = sellerUser.Id,
                    Region = AfricanRegion.EastAfrica
                }
            };

            // Add artifacts to database
            await context.Artifacts.AddRangeAsync(artifacts);
            await context.SaveChangesAsync();

            // Create sample bids
            var bids = new List<Bid>
            {
                new()
                {
                    UserId = regularUser.Id,
                    ArtifactId = artifacts[0].ArtifactId,
                    BidAmount = 2200.00M,
                    BidDate = DateTime.Now.AddDays(-25),
                    Status = BidStatus.Outbid,
                    Comments = "Beautiful mask, would love to add it to my collection."
                },
                new()
                {
                    UserId = adminUser.Id,
                    ArtifactId = artifacts[0].ArtifactId,
                    BidAmount = 2500.00M,
                    BidDate = DateTime.Now.AddDays(-20),
                    Status = BidStatus.Pending,
                    Comments = "Incredible craftsmanship, very interested."
                },
                new()
                {
                    UserId = regularUser.Id,
                    ArtifactId = artifacts[1].ArtifactId,
                    BidAmount = 500.00M,
                    BidDate = DateTime.Now.AddDays(-15),
                    Status = BidStatus.Pending,
                    Comments = "The colors and patterns are amazing."
                }
            };

            // Add bids to database
            await context.Bids.AddRangeAsync(bids);
            await context.SaveChangesAsync();
        }
    }
}
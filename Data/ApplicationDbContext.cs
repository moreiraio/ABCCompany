using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ABCCompanyService.Models;

namespace ABCCompanyService.Data
{


    /// <summary>
    /// ApplicationDbContext
    /// </summary>

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        /// <summary>
        /// AbsenceRequests
        /// </summary>
        public DbSet<AbsenceRequest> AbsenceRequests { get; set; }


        /// <summary>
        /// ApplicationDbContext
        /// </summary>
        /// <param name="options"></param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{

        //    optionsBuilder.UseSqlite("Filename=MyDatabase.db");
        //}


        /// <summary>
        /// Customize the ASP.NET Identity model and override the defaults if needed.
        /// </summary>
        /// <param name="builder"></param>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
        }


        /// <summary>
        /// Create the default application roles
        /// </summary>
        /// <param name="userMgr"></param>
        /// <param name="roleMgr"></param>
        /// <returns></returns>
        public async Task<bool> EnsureSeedData(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userMgr, Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleMgr)
        {
            var adminRole = await roleMgr.FindByNameAsync("Admin");
            if (adminRole == null)
            {
                adminRole = new IdentityRole("Admin");
                await roleMgr.CreateAsync(adminRole);
            }

            var userRole = await roleMgr.FindByNameAsync("User");
            if (userRole == null)
            {
                userRole = new IdentityRole("User");
                await roleMgr.CreateAsync(userRole);
            }
            return true;
        }
    }
}

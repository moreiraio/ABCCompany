using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ABCCompanyService.Models;

namespace ABCCompanyService.Data
{
   


    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<AbsenceRequest> AbsenceRequests { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{

        //    optionsBuilder.UseSqlite("Filename=MyDatabase.db");
        //}

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);



        }

        public async Task<bool> EnsureSeedData(Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userMgr, Microsoft.AspNetCore.Identity.RoleManager<IdentityRole> roleMgr)
        {
            //if (!this.Users.Any(u => u.UserName == "admin@mydomain.com"))
            //{
            // Add 'admin' role
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

            // create admin user
            //var adminUser = new ApplicationUser();
            //adminUser.UserName = "admin@mydomain.com";
            //adminUser.Email = "admin@mydomain.com";

            //await userMgr.CreateAsync(adminUser, "MYP@55word");

            //await userMgr.SetLockoutEnabledAsync(adminUser, false);
            //await userMgr.AddToRoleAsync(adminUser, "admin");
            //}
            return true;
        }



    }
}

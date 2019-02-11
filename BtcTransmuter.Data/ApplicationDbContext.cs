using System;
using System.Collections.Generic;
using System.Text;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BtcTransmuter.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<ExternalServiceData> ExternalServices { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        public DbSet<RecipeInvocation> RecipeInvocations { get; set; }
        public DbSet<RecipeTrigger> RecipeTriggers { get; set; }
        public DbSet<RecipeAction> RecipeActions { get; set; }
    }
}

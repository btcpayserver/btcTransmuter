using System;
using System.Collections.Generic;
using System.Text;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

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
        public DbSet<RecipeActionGroup> RecipeActionGroups { get; set; }
        public DbSet<RecipeInvocation> RecipeInvocations { get; set; }
        public DbSet<RecipeTrigger> RecipeTriggers { get; set; }
        public DbSet<RecipeAction> RecipeActions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Recipe>()
                .HasMany(l => l.RecipeActions)
                .WithOne(action => action.Recipe)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Recipe>()
                .HasOne(l => l.RecipeTrigger)
                .WithOne(action => action.Recipe)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.Entity<Recipe>()
                .HasMany(l => l.RecipeInvocations)
                .WithOne(action => action.Recipe)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<Recipe>()
                .HasMany(l => l.RecipeActionGroups)
                .WithOne(action => action.Recipe)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<RecipeActionGroup>()
                .HasMany(l => l.RecipeActions)
                .WithOne(action => action.RecipeActionGroup)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
    
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=mydb.db");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}

using BtcTransmuter.Data.Encryption;
using BtcTransmuter.Data.Entities;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BtcTransmuter.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IBtcTransmuterOptions _btcTransmuterOptions;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IDataProtectionProvider dataProtectionProvider, IBtcTransmuterOptions btcTransmuterOptions)
            : base(options)
        {
            _dataProtectionProvider = dataProtectionProvider;
            _btcTransmuterOptions = btcTransmuterOptions;
        }

        public DbSet<ExternalServiceData> ExternalServices { get; set; }
        public DbSet<Recipe> Recipes { get; set; }
        
        public DbSet<RecipeActionGroup> RecipeActionGroups { get; set; }
        public DbSet<RecipeInvocation> RecipeInvocations { get; set; }
        public DbSet<RecipeTrigger> RecipeTriggers { get; set; }
        public DbSet<RecipeAction> RecipeActions { get; set; }
        public DbSet<Settings> Settings { get; set; }

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

            builder.Entity<Settings>()
                .HasIndex(settings => settings.Key)
                .IsUnique();
                
            if (_dataProtectionProvider != null && (_btcTransmuterOptions?.UseDatabaseColumnEncryption ?? false))
            {
                builder.AddEncryptionValueConvertersToDecoratedEncryptedColumns(_dataProtectionProvider.CreateProtector("ApplicationDbContext"));
            }
        }
    }

    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseSqlite("Data Source=mydb.db");

            return new ApplicationDbContext(optionsBuilder.Options, null, null);
        }
    }
}
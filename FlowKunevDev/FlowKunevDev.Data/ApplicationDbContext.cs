using FlowKunevDev.Data.Models;
using FlowKunevDev.Data.Seeding;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FlowKunevDev.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets за всички модели
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<PlannedTransaction> PlannedTransactions { get; set; }
        public DbSet<AccountTransfer> AccountTransfers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Budget> Budgets { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // Важно за Identity таблиците

            // Конфигурации
            ConfigureConstraintsAndIndexes(builder);
            ConfigureCascadeDeletes(builder);

            // Всички seed операции
            DatabaseSeeder.SeedAll(builder);

            // Update check constraints using ToTable
            builder.Entity<AccountTransfer>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_AccountTransfer_DifferentAccounts", "[FromAccountId] <> [ToAccountId]");
            });

            builder.Entity<Budget>().ToTable(t =>
            {
                t.HasCheckConstraint("CK_Budget_ValidDateRange", "[StartDate] <= [EndDate]");
            });
        }

        private void ConfigureConstraintsAndIndexes(ModelBuilder builder)
        {
            // Уникални ограничения
            builder.Entity<Account>()
                .HasIndex(a => new { a.UserId, a.Name })
                .IsUnique()
                .HasDatabaseName("IX_Accounts_UserId_Name_Unique");

            builder.Entity<Category>()
                .HasIndex(c => c.Name)
                .IsUnique()
                .HasDatabaseName("IX_Categories_Name_Unique");

            // Check constraints moved to ToTable
            // Performance индекси
            builder.Entity<Transaction>()
                .HasIndex(t => new { t.UserId, t.Date })
                .HasDatabaseName("IX_Transactions_UserId_Date");

            builder.Entity<Transaction>()
                .HasIndex(t => new { t.AccountId, t.Date })
                .HasDatabaseName("IX_Transactions_AccountId_Date");

            builder.Entity<PlannedTransaction>()
                .HasIndex(pt => new { pt.UserId, pt.Status, pt.PlannedDate })
                .HasDatabaseName("IX_PlannedTransactions_UserId_Status_PlannedDate");

            builder.Entity<AccountTransfer>()
                .HasIndex(at => new { at.UserId, at.Date })
                .HasDatabaseName("IX_AccountTransfers_UserId_Date");

            builder.Entity<Budget>()
                .HasIndex(b => new { b.UserId, b.IsActive, b.StartDate, b.EndDate })
                .HasDatabaseName("IX_Budgets_UserId_Active_Dates");
        }

        private void ConfigureCascadeDeletes(ModelBuilder builder)
        {
            // Ограничаваме каскадните изтривания за да пазим интегритета на данните
            builder.Entity<Account>(entity =>
            {
                entity.HasMany(a => a.Transactions)
                      .WithOne(t => t.Account)
                      .HasForeignKey(t => t.AccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.PlannedTransactions)
                      .WithOne(pt => pt.Account)
                      .HasForeignKey(pt => pt.AccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.TransfersFrom)
                      .WithOne(at => at.FromAccount)
                      .HasForeignKey(at => at.FromAccountId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(a => a.TransfersTo)
                      .WithOne(at => at.ToAccount)
                      .HasForeignKey(at => at.ToAccountId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Category>(entity =>
            {
                entity.HasMany(c => c.Transactions)
                      .WithOne(t => t.Category)
                      .HasForeignKey(t => t.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.PlannedTransactions)
                      .WithOne(pt => pt.Category)
                      .HasForeignKey(pt => pt.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasMany(c => c.Budgets)
                      .WithOne(b => b.Category)
                      .HasForeignKey(b => b.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

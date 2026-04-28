# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project

Personal finance management web app (`flow.kunev.dev`). ASP.NET Core 8 MVC + Razor Identity, EF Core 8 with SQL Server, AutoMapper. UI text and most code comments are in Bulgarian — keep that convention when adding user-facing strings or commentary in existing files.

## Solution layout

The solution lives in `FlowKunevDev/FlowKunevDev.sln` (one level below repo root). Five projects with strict layering — do not introduce reverse references:

- **FlowKunevDev.Common** — enums (`AccountType`, `TransactionType`, `CategoryType`, `RecurrenceType`, `PlannedTransactionStatus`, `ComparisonType`) and `TimeHelper`. No other project deps.
- **FlowKunevDev.Data.Models** — EF entities (`Account`, `Transaction`, `PlannedTransaction`, `AccountTransfer`, `Category`, `Budget`, `ApplicationUser`). Depends on Common.
- **FlowKunevDev.Data** — `ApplicationDbContext` (extends `IdentityDbContext<ApplicationUser>`), `Migrations/`, `Seeding/DatabaseSeeder.cs`. Depends on Common + Data.Models.
- **FlowKunevDev.Services** — `Interfaces/`, `Implementations/` (one service per aggregate: Account, Transaction, Category, AccountTransfer, PlannedTransaction, Budget; plus `EmailSender`), `DTOs/`, `BackgroundServices/`. Depends on Common + Data.Models + Data.
- **FlowKunevDev.Web** — Controllers, Views, `Areas/Identity/` (scaffolded Identity Razor Pages), `ViewModels/`, `Mapping/MappingProfile.cs`, `Program.cs`. Depends on all of the above.

## Common commands

Run all from the `FlowKunevDev/` directory (where the `.sln` lives), unless noted.

```bash
dotnet restore                                  # also restores local tool: dotnet-ef 9.0.6 (.config/dotnet-tools.json)
dotnet build                                    # builds whole solution
dotnet run --project FlowKunevDev.Web           # http://localhost:5057, https://localhost:7251

# EF Core migrations — startup project is Web, migrations live in Data
dotnet ef migrations add <Name>   --project FlowKunevDev.Data --startup-project FlowKunevDev.Web
dotnet ef database update         --project FlowKunevDev.Data --startup-project FlowKunevDev.Web

# Connection string is read from "DefaultConnection". appsettings*.json is gitignored —
# expect it (or user-secrets, id `aspnet-FlowKunevDev-94586953-9ac2-4a4b-9b0a-c7476ab0f1cc`) to provide it.
```

There is no test project in the solution.

## Architecture notes

**Request flow.** Controllers (`[Authorize]` on every controller except `HomeController`) resolve the current user via `UserManager<ApplicationUser>.GetUserId(User)` and pass `userId` into service methods. Services return DTOs, never entities. Controllers map DTOs ↔ ViewModels for the views. AutoMapper (`MappingProfile`) handles Entity ↔ DTO; Entity ↔ ViewModel mapping is done manually in controllers.

**Authorization model.** All data is per-user — every entity has a `UserId` foreign key, and every service method takes `userId` and filters on it. When adding new queries, always scope by `userId`; never expose another user's data.

**Cascade delete policy.** `ApplicationDbContext.ConfigureCascadeDeletes` sets `DeleteBehavior.Restrict` on Account → Transactions/PlannedTransactions/Transfers and Category → Transactions/PlannedTransactions/Budgets. Deletions of Accounts/Categories must be soft (toggle `IsActive`) or preceded by removing dependent rows; do not relax these to cascade.

**DB constraints worth knowing.**
- `Accounts(UserId, Name)` is unique.
- `Categories(Name)` is globally unique (categories are seeded shared data, not per-user).
- `AccountTransfers` enforces `FromAccountId <> ToAccountId` via check constraint.
- `Budgets` enforces `StartDate <= EndDate` via check constraint.
- Performance indexes exist on `(UserId, Date)` for transactions/transfers and `(UserId, Status, PlannedDate)` for planned transactions — keep filter queries aligned with these.

**Seeded data.** `DatabaseSeeder.SeedCategories` populates the `Category` table with fixed-Id Bulgarian categories. Adding/changing categories means a new migration. Don't renumber existing category ids.

**Time handling.** Always use `FlowKunevDev.Common.TimeHelper.UtcNow` / `LocalNow` instead of `DateTime.UtcNow` / `DateTime.Now`. It strips seconds (minute precision) and converts via the Sofia timezone (`FLE Standard Time` on Windows, `Europe/Sofia` on Linux). Mixing raw `DateTime.UtcNow` will produce off-by-seconds bugs in equality and grouping.

**Background work.** `PlannedTransactionBackgroundService` (registered as `IHostedService` in `Program.cs`) wakes hourly, calls `IPlannedTransactionService.ExecuteDueTransactionsAsync` then `ProcessRecurringTransactionsAsync`. It creates its own DI scope per tick — follow this pattern (`IServiceScopeFactory.CreateScope()`) for any new background worker that touches the DbContext.

**Identity.** `ApplicationUser : IdentityUser` lives in `FlowKunevDev.Data.Models`. `Program.cs` allows Cyrillic characters in usernames (`options.User.AllowedUserNameCharacters`) and uses relaxed password rules (no uppercase/non-alphanumeric required). Identity UI is the default scaffolded Razor Pages under `Areas/Identity/Pages/`. Auth-required redirects in controllers go to `Account/Login` in area `Identity`.

**Routing.** Default `{controller=Home}/{action=Index}/{id?}` plus an explicit `/dashboard → DashboardController.Index`. `HomeController.Index` redirects authenticated users to the Dashboard, so the Home view is the public landing page only.

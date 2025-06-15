using FlowKunevDev.Common;
using FlowKunevDev.Data.Models;
using Microsoft.EntityFrameworkCore;


namespace FlowKunevDev.Data.Seeding
{
    public static class DatabaseSeeder
    {
        public static void SeedAll(ModelBuilder builder)
        {
            SeedCategories(builder);
            // В бъдеще можем да добавим:
            // SeedDefaultAccounts(builder);
            // SeedCurrencies(builder);
            // SeedUserRoles(builder);
        }

        private static void SeedCategories(ModelBuilder builder)
        {
            builder.Entity<Category>().HasData(GetCategories());
        }

        private static List<Category> GetCategories()
        {
            return new List<Category>
            {
                // Категории за разходи
                new Category
                {
                    Id = 1,
                    Name = "Храна и ресторанти",
                    Description = "Покупка на храна, ресторанти, доставки на храна",
                    Color = "#ff6b6b",
                    Icon = "utensils",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 2,
                    Name = "Транспорт",
                    Description = "Гориво, градски транспорт, такси, поддръжка на автомобил",
                    Color = "#4ecdc4",
                    Icon = "car",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 3,
                    Name = "Пазаруване",
                    Description = "Дрехи, електроника, подаръци, различни покупки",
                    Color = "#45b7d1",
                    Icon = "shopping-bag",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 4,
                    Name = "Развлечения",
                    Description = "Кино, театър, игри, спорт, хобита, излизания",
                    Color = "#f9ca24",
                    Icon = "gamepad-2",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 5,
                    Name = "Сметки и комунални услуги",
                    Description = "Ток, вода, газ, интернет, телефон, наем, ТВ",
                    Color = "#f0932b",
                    Icon = "zap",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 6,
                    Name = "Здравеопазване",
                    Description = "Лекар, лекарства, болница, дентален преглед, очила",
                    Color = "#eb4d4b",
                    Icon = "heart",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 7,
                    Name = "Образование",
                    Description = "Курсове, книги, обучения, университет, сертификати",
                    Color = "#6c5ce7",
                    Icon = "book",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 8,
                    Name = "Домакинство",
                    Description = "Почистване, поддръжка, мебели, домакински стоки",
                    Color = "#a29bfe",
                    Icon = "home",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 9,
                    Name = "Застраховки",
                    Description = "Автомобилна, домашна, здравна, живот застраховка",
                    Color = "#fd79a8",
                    Icon = "shield",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 10,
                    Name = "Облекло и аксесоари",
                    Description = "Дрехи, обувки, чанти, бижута, часовници",
                    Color = "#fdcb6e",
                    Icon = "shirt",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 11,
                    Name = "Красота и грижи",
                    Description = "Фризьор, козметика, SPA, маникюр, педикюр",
                    Color = "#e17055",
                    Icon = "sparkles",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 12,
                    Name = "Домашни любимци",
                    Description = "Храна за животни, ветеринар, играчки, грижи",
                    Color = "#00cec9",
                    Icon = "heart-hand",
                    Type = CategoryType.ExpenseOnly,
                    IsActive = true
                },

                // Категории за приходи
                new Category
                {
                    Id = 20,
                    Name = "Заплата",
                    Description = "Основна месечна заплата, бонуси, надбавки",
                    Color = "#00b894",
                    Icon = "briefcase",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 21,
                    Name = "Свободна практика",
                    Description = "Фрийланс проекти, консултации, временна работа",
                    Color = "#00cec9",
                    Icon = "laptop",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 22,
                    Name = "Инвестиции и лихви",
                    Description = "Дивиденти, печалби от акции, лихви от депозити",
                    Color = "#81ecec",
                    Icon = "trending-up",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 23,
                    Name = "Подаръци",
                    Description = "Получени подаръци в пари, наследства",
                    Color = "#74b9ff",
                    Icon = "gift",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 24,
                    Name = "Възстановявания",
                    Description = "Връщане на заеми, възстановени разходи, гаранции",
                    Color = "#a29bfe",
                    Icon = "refresh-cw",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 25,
                    Name = "Продажби",
                    Description = "Продажба на стоки, услуги, имущество",
                    Color = "#fd79a8",
                    Icon = "tag",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 26,
                    Name = "Награди и премии",
                    Description = "Спечелени награди, премии, състезания",
                    Color = "#fdcb6e",
                    Icon = "trophy",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },
                new Category
                {
                    Id = 27,
                    Name = "Наем и рента",
                    Description = "Приходи от наем на имот, рента, авторски права",
                    Color = "#e17055",
                    Icon = "building",
                    Type = CategoryType.IncomeOnly,
                    IsActive = true
                },

                // Универсални категории
                new Category
                {
                    Id = 90,
                    Name = "Друго",
                    Description = "Други некатегоризирани приходи и разходи",
                    Color = "#636e72",
                    Icon = "more-horizontal",
                    Type = CategoryType.Both,
                    IsActive = true
                },
                new Category
                {
                    Id = 91,
                    Name = "Трансфер между сметки",
                    Description = "Прехвърляне на средства между собствени сметки",
                    Color = "#2d3436",
                    Icon = "arrow-right-left",
                    Type = CategoryType.Both,
                    IsActive = true
                }
            };
        }

        // Готовност за бъдещи seed операции
        private static void SeedDefaultAccounts(ModelBuilder builder)
        {
            // За бъдещо разширение - default сметки за нови потребители
            /*
            builder.Entity<Account>().HasData(
                new Account 
                { 
                    Id = -1, // Темплейт ID
                    Name = "Основна сметка",
                    Type = AccountType.Current,
                    InitialBalance = 0,
                    Currency = "BGN",
                    Color = "#007bff",
                    UserId = "TEMPLATE", // Ще се замести при създаване на потребител
                    IsActive = true
                }
            );
            */
        }

        private static void SeedCurrencies(ModelBuilder builder)
        {
            // За бъдещо разширение - поддържани валути
            /*
            builder.Entity<Currency>().HasData(
                new Currency { Code = "BGN", Name = "Български лев", Symbol = "лв." },
                new Currency { Code = "EUR", Name = "Евро", Symbol = "€" },
                new Currency { Code = "USD", Name = "Американски долар", Symbol = "$" }
            );
            */
        }
    }
}


IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [Accounts] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [InitialBalance] decimal(18,2) NOT NULL,
        [Type] int NOT NULL,
        [Currency] nvarchar(3) NOT NULL,
        [Color] nvarchar(7) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Accounts] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoles] (
        [Id] nvarchar(450) NOT NULL,
        [Name] nvarchar(256) NULL,
        [NormalizedName] nvarchar(256) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUsers] (
        [Id] nvarchar(450) NOT NULL,
        [UserName] nvarchar(256) NULL,
        [NormalizedUserName] nvarchar(256) NULL,
        [Email] nvarchar(256) NULL,
        [NormalizedEmail] nvarchar(256) NULL,
        [EmailConfirmed] bit NOT NULL,
        [PasswordHash] nvarchar(max) NULL,
        [SecurityStamp] nvarchar(max) NULL,
        [ConcurrencyStamp] nvarchar(max) NULL,
        [PhoneNumber] nvarchar(max) NULL,
        [PhoneNumberConfirmed] bit NOT NULL,
        [TwoFactorEnabled] bit NOT NULL,
        [LockoutEnd] datetimeoffset NULL,
        [LockoutEnabled] bit NOT NULL,
        [AccessFailedCount] int NOT NULL,
        CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [Categories] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(100) NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Color] nvarchar(7) NOT NULL,
        [Icon] nvarchar(50) NOT NULL,
        [Type] int NOT NULL,
        [IsActive] bit NOT NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AccountTransfers] (
        [Id] int NOT NULL IDENTITY,
        [Amount] decimal(18,2) NOT NULL,
        [FromAccountId] int NOT NULL,
        [ToAccountId] int NOT NULL,
        [Date] datetime2 NOT NULL,
        [Description] nvarchar(500) NOT NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_AccountTransfers] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_AccountTransfer_DifferentAccounts] CHECK ([FromAccountId] <> [ToAccountId]),
        CONSTRAINT [FK_AccountTransfers_Accounts_FromAccountId] FOREIGN KEY ([FromAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_AccountTransfers_Accounts_ToAccountId] FOREIGN KEY ([ToAccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetRoleClaims] (
        [Id] int NOT NULL IDENTITY,
        [RoleId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserClaims] (
        [Id] int NOT NULL IDENTITY,
        [UserId] nvarchar(450) NOT NULL,
        [ClaimType] nvarchar(max) NULL,
        [ClaimValue] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserLogins] (
        [LoginProvider] nvarchar(128) NOT NULL,
        [ProviderKey] nvarchar(128) NOT NULL,
        [ProviderDisplayName] nvarchar(max) NULL,
        [UserId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
        CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserRoles] (
        [UserId] nvarchar(450) NOT NULL,
        [RoleId] nvarchar(450) NOT NULL,
        CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
        CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [AspNetUserTokens] (
        [UserId] nvarchar(450) NOT NULL,
        [LoginProvider] nvarchar(128) NOT NULL,
        [Name] nvarchar(128) NOT NULL,
        [Value] nvarchar(max) NULL,
        CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
        CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [Budgets] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [CategoryId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [StartDate] datetime2 NOT NULL,
        [EndDate] datetime2 NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Budgets] PRIMARY KEY ([Id]),
        CONSTRAINT [CK_Budget_ValidDateRange] CHECK ([StartDate] <= [EndDate]),
        CONSTRAINT [FK_Budgets_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [Transactions] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(200) NOT NULL,
        [Amount] decimal(18,2) NOT NULL,
        [Date] datetime2 NOT NULL,
        [CategoryId] int NOT NULL,
        [AccountId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Type] int NOT NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_Transactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Transactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_Transactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE TABLE [PlannedTransactions] (
        [Id] int NOT NULL IDENTITY,
        [Description] nvarchar(200) NOT NULL,
        [PlannedAmount] decimal(18,2) NOT NULL,
        [PlannedDate] datetime2 NOT NULL,
        [CategoryId] int NOT NULL,
        [AccountId] int NOT NULL,
        [UserId] nvarchar(450) NOT NULL,
        [Type] int NOT NULL,
        [Notes] nvarchar(1000) NOT NULL,
        [Status] int NOT NULL,
        [IsRecurring] bit NOT NULL,
        [RecurrenceType] int NULL,
        [ExecutedTransactionId] int NULL,
        [CreatedDate] datetime2 NOT NULL,
        CONSTRAINT [PK_PlannedTransactions] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PlannedTransactions_Accounts_AccountId] FOREIGN KEY ([AccountId]) REFERENCES [Accounts] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PlannedTransactions_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Categories] ([Id]) ON DELETE NO ACTION,
        CONSTRAINT [FK_PlannedTransactions_Transactions_ExecutedTransactionId] FOREIGN KEY ([ExecutedTransactionId]) REFERENCES [Transactions] ([Id])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Color', N'Description', N'Icon', N'IsActive', N'Name', N'Type') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] ON;
    EXEC(N'INSERT INTO [Categories] ([Id], [Color], [Description], [Icon], [IsActive], [Name], [Type])
    VALUES (1, N''#ff6b6b'', N''Покупка на храна, ресторанти, доставки на храна'', N''utensils'', CAST(1 AS bit), N''Храна и ресторанти'', 2),
    (2, N''#4ecdc4'', N''Гориво, градски транспорт, такси, поддръжка на автомобил'', N''car'', CAST(1 AS bit), N''Транспорт'', 2),
    (3, N''#45b7d1'', N''Дрехи, електроника, подаръци, различни покупки'', N''shopping-bag'', CAST(1 AS bit), N''Пазаруване'', 2),
    (4, N''#f9ca24'', N''Кино, театър, игри, спорт, хобита, излизания'', N''gamepad-2'', CAST(1 AS bit), N''Развлечения'', 2),
    (5, N''#f0932b'', N''Ток, вода, газ, интернет, телефон, наем, ТВ'', N''zap'', CAST(1 AS bit), N''Сметки и комунални услуги'', 2),
    (6, N''#eb4d4b'', N''Лекар, лекарства, болница, дентален преглед, очила'', N''heart'', CAST(1 AS bit), N''Здравеопазване'', 2),
    (7, N''#6c5ce7'', N''Курсове, книги, обучения, университет, сертификати'', N''book'', CAST(1 AS bit), N''Образование'', 2),
    (8, N''#a29bfe'', N''Почистване, поддръжка, мебели, домакински стоки'', N''home'', CAST(1 AS bit), N''Домакинство'', 2),
    (9, N''#fd79a8'', N''Автомобилна, домашна, здравна, живот застраховка'', N''shield'', CAST(1 AS bit), N''Застраховки'', 2),
    (10, N''#fdcb6e'', N''Дрехи, обувки, чанти, бижута, часовници'', N''shirt'', CAST(1 AS bit), N''Облекло и аксесоари'', 2),
    (11, N''#e17055'', N''Фризьор, козметика, SPA, маникюр, педикюр'', N''sparkles'', CAST(1 AS bit), N''Красота и грижи'', 2),
    (12, N''#00cec9'', N''Храна за животни, ветеринар, играчки, грижи'', N''heart-hand'', CAST(1 AS bit), N''Домашни любимци'', 2),
    (20, N''#00b894'', N''Основна месечна заплата, бонуси, надбавки'', N''briefcase'', CAST(1 AS bit), N''Заплата'', 1),
    (21, N''#00cec9'', N''Фрийланс проекти, консултации, временна работа'', N''laptop'', CAST(1 AS bit), N''Свободна практика'', 1),
    (22, N''#81ecec'', N''Дивиденти, печалби от акции, лихви от депозити'', N''trending-up'', CAST(1 AS bit), N''Инвестиции и лихви'', 1),
    (23, N''#74b9ff'', N''Получени подаръци в пари, наследства'', N''gift'', CAST(1 AS bit), N''Подаръци'', 1),
    (24, N''#a29bfe'', N''Връщане на заеми, възстановени разходи, гаранции'', N''refresh-cw'', CAST(1 AS bit), N''Възстановявания'', 1),
    (25, N''#fd79a8'', N''Продажба на стоки, услуги, имущество'', N''tag'', CAST(1 AS bit), N''Продажби'', 1),
    (26, N''#fdcb6e'', N''Спечелени награди, премии, състезания'', N''trophy'', CAST(1 AS bit), N''Награди и премии'', 1),
    (27, N''#e17055'', N''Приходи от наем на имот, рента, авторски права'', N''building'', CAST(1 AS bit), N''Наем и рента'', 1),
    (90, N''#636e72'', N''Други некатегоризирани приходи и разходи'', N''more-horizontal'', CAST(1 AS bit), N''Друго'', 3),
    (91, N''#2d3436'', N''Прехвърляне на средства между собствени сметки'', N''arrow-right-left'', CAST(1 AS bit), N''Трансфер между сметки'', 3)');
    IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'Color', N'Description', N'Icon', N'IsActive', N'Name', N'Type') AND [object_id] = OBJECT_ID(N'[Categories]'))
        SET IDENTITY_INSERT [Categories] OFF;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Accounts_UserId_Name_Unique] ON [Accounts] ([UserId], [Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AccountTransfers_FromAccountId] ON [AccountTransfers] ([FromAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AccountTransfers_ToAccountId] ON [AccountTransfers] ([ToAccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AccountTransfers_UserId_Date] ON [AccountTransfers] ([UserId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Budgets_CategoryId] ON [Budgets] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Budgets_UserId_Active_Dates] ON [Budgets] ([UserId], [IsActive], [StartDate], [EndDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_Name_Unique] ON [Categories] ([Name]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlannedTransactions_AccountId] ON [PlannedTransactions] ([AccountId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlannedTransactions_CategoryId] ON [PlannedTransactions] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlannedTransactions_ExecutedTransactionId] ON [PlannedTransactions] ([ExecutedTransactionId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlannedTransactions_UserId_PlannedDate] ON [PlannedTransactions] ([UserId], [PlannedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_PlannedTransactions_UserId_Status_PlannedDate] ON [PlannedTransactions] ([UserId], [Status], [PlannedDate]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transactions_AccountId_Date] ON [Transactions] ([AccountId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transactions_CategoryId] ON [Transactions] ([CategoryId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    CREATE INDEX [IX_Transactions_UserId_Date] ON [Transactions] ([UserId], [Date]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250615165746_InitialCreate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250615165746_InitialCreate', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250617205527_AccountsUpdate'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Accounts]') AND [c].[name] = N'Description');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [Accounts] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [Accounts] ALTER COLUMN [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250617205527_AccountsUpdate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250617205527_AccountsUpdate', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250619150331_MakeNotesFieldsNullable'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PlannedTransactions]') AND [c].[name] = N'Notes');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PlannedTransactions] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [PlannedTransactions] ALTER COLUMN [Notes] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250619150331_MakeNotesFieldsNullable'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AccountTransfers]') AND [c].[name] = N'Notes');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [AccountTransfers] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [AccountTransfers] ALTER COLUMN [Notes] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250619150331_MakeNotesFieldsNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250619150331_MakeNotesFieldsNullable', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250619150749_MakeNotesTransactionsNullable'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Transactions]') AND [c].[name] = N'Notes');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Transactions] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [Transactions] ALTER COLUMN [Notes] nvarchar(1000) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250619150749_MakeNotesTransactionsNullable'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250619150749_MakeNotesTransactionsNullable', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250619171540_AllowNullDescTransactions'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AccountTransfers]') AND [c].[name] = N'Description');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [AccountTransfers] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [AccountTransfers] ALTER COLUMN [Description] nvarchar(500) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250619171540_AllowNullDescTransactions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250619171540_AllowNullDescTransactions', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20250625094604_ApplicationUserModel'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20250625094604_ApplicationUserModel', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430120811_AddSavingsTargetFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [ExpectedMonthlyIncome] decimal(18,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430120811_AddSavingsTargetFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [MonthlySavingsPercent] decimal(5,2) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430120811_AddSavingsTargetFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [SalaryAccountId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430120811_AddSavingsTargetFields'
)
BEGIN
    ALTER TABLE [AspNetUsers] ADD [SavingsAccountId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260430120811_AddSavingsTargetFields'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260430120811_AddSavingsTargetFields', N'8.0.17');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    UPDATE [AspNetUsers] SET [SalaryAccountId] = NULL, [SavingsAccountId] = NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DELETE FROM [Budgets];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DELETE FROM [PlannedTransactions];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DELETE FROM [AccountTransfers];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DELETE FROM [Transactions];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DELETE FROM [Accounts];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Accounts]') AND [c].[name] = N'Currency');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [Accounts] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [Accounts] DROP COLUMN [Currency];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'Name');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [AspNetUserTokens] ALTER COLUMN [Name] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserTokens]') AND [c].[name] = N'LoginProvider');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserTokens] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [AspNetUserTokens] ALTER COLUMN [LoginProvider] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'ProviderKey');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [AspNetUserLogins] ALTER COLUMN [ProviderKey] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AspNetUserLogins]') AND [c].[name] = N'LoginProvider');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [AspNetUserLogins] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [AspNetUserLogins] ALTER COLUMN [LoginProvider] nvarchar(450) NOT NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20260511203322_EuroOnly'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20260511203322_EuroOnly', N'8.0.17');
END;
GO

COMMIT;
GO


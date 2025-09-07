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
CREATE TABLE [Specializations] (
    [SpecializationId] int NOT NULL IDENTITY,
    [SpecializationName] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_Specializations] PRIMARY KEY ([SpecializationId])
);

CREATE TABLE [Users] (
    [UserId] int NOT NULL IDENTITY,
    [Username] nvarchar(max) NULL,
    [PasswordHash] nvarchar(max) NULL,
    [Role] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
);

CREATE TABLE [Doctors] (
    [DoctorId] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [SpecializationId] int NOT NULL,
    [City] nvarchar(50) NOT NULL,
    [Rating] float NOT NULL,
    CONSTRAINT [PK_Doctors] PRIMARY KEY ([DoctorId]),
    CONSTRAINT [FK_Doctors_Specializations_SpecializationId] FOREIGN KEY ([SpecializationId]) REFERENCES [Specializations] ([SpecializationId]) ON DELETE CASCADE
);

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SpecializationId', N'SpecializationName') AND [object_id] = OBJECT_ID(N'[Specializations]'))
    SET IDENTITY_INSERT [Specializations] ON;
INSERT INTO [Specializations] ([SpecializationId], [SpecializationName])
VALUES (1, N'Cardiology'),
(2, N'Dermatology'),
(3, N'Neurology'),
(4, N'Anesthesiology');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'SpecializationId', N'SpecializationName') AND [object_id] = OBJECT_ID(N'[Specializations]'))
    SET IDENTITY_INSERT [Specializations] OFF;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'PasswordHash', N'Role', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] ON;
INSERT INTO [Users] ([UserId], [PasswordHash], [Role], [Username])
VALUES (1, N'$2a$11$TqN28.OFjtbJmTnDwoMbBuyxMMR5rQWxw1YxwvRLhGpx.9LSE/pC6', N'Admin', N'admin');
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'UserId', N'PasswordHash', N'Role', N'Username') AND [object_id] = OBJECT_ID(N'[Users]'))
    SET IDENTITY_INSERT [Users] OFF;

CREATE INDEX [IX_Doctors_SpecializationId] ON [Doctors] ([SpecializationId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250902113504_InitialCreate', N'9.0.8');

DELETE FROM [Specializations]
WHERE [SpecializationId] = 3;
SELECT @@ROWCOUNT;


DELETE FROM [Specializations]
WHERE [SpecializationId] = 4;
SELECT @@ROWCOUNT;


DECLARE @var sysname;
SELECT @var = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Specializations]') AND [c].[name] = N'SpecializationName');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Specializations] DROP CONSTRAINT [' + @var + '];');
ALTER TABLE [Specializations] ALTER COLUMN [SpecializationName] nvarchar(max) NOT NULL;

DECLARE @var1 sysname;
SELECT @var1 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Doctors]') AND [c].[name] = N'Name');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Doctors] DROP CONSTRAINT [' + @var1 + '];');
ALTER TABLE [Doctors] ALTER COLUMN [Name] nvarchar(max) NOT NULL;

DECLARE @var2 sysname;
SELECT @var2 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Doctors]') AND [c].[name] = N'City');
IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [Doctors] DROP CONSTRAINT [' + @var2 + '];');
ALTER TABLE [Doctors] ALTER COLUMN [City] nvarchar(max) NOT NULL;

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DoctorId', N'City', N'Name', N'Rating', N'SpecializationId') AND [object_id] = OBJECT_ID(N'[Doctors]'))
    SET IDENTITY_INSERT [Doctors] ON;
INSERT INTO [Doctors] ([DoctorId], [City], [Name], [Rating], [SpecializationId])
VALUES (1, N'New York', N'Dr. Smith', 4.5E0, 1),
(2, N'Boston', N'Dr. Johnson', 4.2000000000000002E0, 2);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'DoctorId', N'City', N'Name', N'Rating', N'SpecializationId') AND [object_id] = OBJECT_ID(N'[Doctors]'))
    SET IDENTITY_INSERT [Doctors] OFF;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250902125036_AddDoctorsAndSpecializations', N'9.0.8');

CREATE TABLE [Appointments] (
    [AppointmentId] int NOT NULL IDENTITY,
    [DoctorId] int NOT NULL,
    [UserId] int NOT NULL,
    [AppointmentDate] datetime2 NOT NULL,
    [TimeSlot] nvarchar(max) NOT NULL,
    [Status] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Appointments] PRIMARY KEY ([AppointmentId]),
    CONSTRAINT [FK_Appointments_Doctors_DoctorId] FOREIGN KEY ([DoctorId]) REFERENCES [Doctors] ([DoctorId]) ON DELETE CASCADE,
    CONSTRAINT [FK_Appointments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId]) ON DELETE CASCADE
);

CREATE INDEX [IX_Appointments_DoctorId] ON [Appointments] ([DoctorId]);

CREATE INDEX [IX_Appointments_UserId] ON [Appointments] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250902140831_AddAppointments', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250903051330_AddAppointment1', N'9.0.8');

DECLARE @var3 sysname;
SELECT @var3 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'Username');
IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var3 + '];');
UPDATE [Users] SET [Username] = N'' WHERE [Username] IS NULL;
ALTER TABLE [Users] ALTER COLUMN [Username] nvarchar(max) NOT NULL;
ALTER TABLE [Users] ADD DEFAULT N'' FOR [Username];

DECLARE @var4 sysname;
SELECT @var4 = [d].[name]
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'PasswordHash');
IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT [' + @var4 + '];');
UPDATE [Users] SET [PasswordHash] = N'' WHERE [PasswordHash] IS NULL;
ALTER TABLE [Users] ALTER COLUMN [PasswordHash] nvarchar(max) NOT NULL;
ALTER TABLE [Users] ADD DEFAULT N'' FOR [PasswordHash];

ALTER TABLE [Users] ADD [ProfileImagePath] nvarchar(max) NULL;

UPDATE [Users] SET [ProfileImagePath] = NULL
WHERE [UserId] = 1;
SELECT @@ROWCOUNT;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250904041053_AddUserProfileImage', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250904041209_AddUserProfilePicNew', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250906124714_DoctorAvailability', N'9.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250906154346_AddRatingsTable', N'9.0.8');

COMMIT;
GO

CREATE TABLE Ratings (
    RatingId INT IDENTITY(1,1) PRIMARY KEY,
    DoctorId INT NOT NULL,
    UserId INT NOT NULL,
    RatingValue INT NOT NULL CHECK (RatingValue BETWEEN 1 AND 5),
    Comment NVARCHAR(500) NULL,

    -- ✅ Foreign Keys
    CONSTRAINT FK_Ratings_Doctor FOREIGN KEY (DoctorId)
        REFERENCES Doctors(DoctorId)
        ON DELETE CASCADE,

    CONSTRAINT FK_Ratings_User FOREIGN KEY (UserId)
        REFERENCES Users(UserId)
        ON DELETE CASCADE
);
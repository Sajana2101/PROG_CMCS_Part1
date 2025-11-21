## Contract Monthly Claims System

# About the application
The Contract Monthly Claims System is a web-based platform to streamline submission, verification, and approval of monthly claims for contract lecturers.
The CMCS aims to ensure the complex process of submitting and approving claims is made easy for all parties, including 
lecturers, coordinators, academic managers and HR

- **Role-based access control**
  - HR: Manage users and roles
  - Lecturer: Submit claims with uploaded documents
  - Coordinator: Verify or reject claims
  - Manager: Approve or reject verified claims
 - **HR Superuser**
   -HR staff can create, delete, and update users (Lecuturers, Managers, and Coordinators)
   -HR staff can see all users
- **Claims management**
  - Track hours worked, hourly rate, total amount
  - Submit documents securely
  - View claim details and download encrypted files
  - HR sets lecturer rates and maximum hours worked for each lecturer
- **Security**
  - Password hashing via ASP.NET Core Identity
  - File encryption for uploaded documents
  - Password and usernames are required to enter each view, access to each view is restricted based on role
- **Session management**
  - User sessions expire after 30 minutes of inactivity
- **Database**
  - SQL Server database with cascade deletion for user claims
- **PDF Report Generation**
   -HR staff can generate PDF reports for claims 

# Features from Part 2 
-Demo login page for Lecturers, Managers, and Coordinators 
-Allowed lecturers to submit claims 
-Allowed coordinators to verify or reject claims 
-Allowed managers to accept or reject claims 
-Data was saved to a JSON File 
-Secure upload of files via encryption
-Included a file upload and download interface for lecturers to upload supporting documents and managers and coordinators to download them 

CHANGES AND IMPROVEMENTS TO PART 2 
- Updated the Lecturer, Coordinator, and Manager “View Details” to make the view more visually appealing 
- Updated the file delete feature to ensure the application does not crash when a file is deleted. 

# Features of Part 3
-Instead of storing data in a JSON file, Data is now stored in an SQL database 
- Users now login with usernames and passwords. Additionally, view access has been restricted based on roles
- An HR view has been added as a "superuser" who can see all users, create users, delete users and generate PDF claim reports
- Lecturers no longer need to enter hourly rate, HR sets the rate for the lecturer, and the total is auto-calculated when hours are entered
- Managers can now only see verified claims

## Technologies used
- ASP.NET Core MVC - Application used to develop the application
- Visual Studio 2022 - IDE
- Entity Framework Core - Database ORM
- ASP.NET Core Identity - User management and authentication
- The application was coded using a combination of C#,HTML, JavaScript, CSS ans Bootstrap 5 was used for Frontend Styling
- PdfSharpCore and FileEncryptionService - File handling and encryption

## Using the application 
NB : Please note you will only be able to access the other views once you add that view in the HR view 
eg you will only be able to go to the lecturer page once you create a lecturer when you are logged in as HR and then login to the lecturer page.
It works the same way for the coordinator and manager views.
To login as HR use these details 
Email : hr@example.com
Password : @Admin1234

### Prerequisites 
- .NET 9.0
- Microsoft Visual Studios 2022
- Access to the internet

### Installation 
1. Clone repository to Visual Studios
2. Set up the database : - add migration initial create then update database
3. Run the application

Database Scripts 

### ROLE CLAIMS 
CREATE TABLE [dbo].[AspNetRoleClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [RoleId]     NVARCHAR (450) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AspNetRoleClaims_RoleId]
    ON [dbo].[AspNetRoleClaims]([RoleId] ASC);

### ROLES
CREATE TABLE [dbo].[AspNetRoles] (
    [Id]               NVARCHAR (450) NOT NULL,
    [Name]             NVARCHAR (256) NULL,
    [NormalizedName]   NVARCHAR (256) NULL,
    [ConcurrencyStamp] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex]
    ON [dbo].[AspNetRoles]([NormalizedName] ASC) WHERE ([NormalizedName] IS NOT NULL);

### USER CLAIMS 

CREATE TABLE [dbo].[AspNetUserClaims] (
    [Id]         INT            IDENTITY (1, 1) NOT NULL,
    [UserId]     NVARCHAR (450) NOT NULL,
    [ClaimType]  NVARCHAR (MAX) NULL,
    [ClaimValue] NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserClaims_UserId]
    ON [dbo].[AspNetUserClaims]([UserId] ASC);

### USER LOGINS
CREATE TABLE [dbo].[AspNetUserLogins] (
    [LoginProvider]       NVARCHAR (450) NOT NULL,
    [ProviderKey]         NVARCHAR (450) NOT NULL,
    [ProviderDisplayName] NVARCHAR (MAX) NULL,
    [UserId]              NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY CLUSTERED ([LoginProvider] ASC, [ProviderKey] ASC),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserLogins_UserId]
    ON [dbo].[AspNetUserLogins]([UserId] ASC);

### USER ROLES 
CREATE TABLE [dbo].[AspNetUserRoles] (
    [UserId] NVARCHAR (450) NOT NULL,
    [RoleId] NVARCHAR (450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY CLUSTERED ([UserId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AspNetUserRoles_RoleId]
    ON [dbo].[AspNetUserRoles]([RoleId] ASC);

### USERS
CREATE TABLE [dbo].[AspNetUsers] (
    [Id]                   NVARCHAR (450)     NOT NULL,
    [FirstName]            NVARCHAR (MAX)     NOT NULL,
    [LastName]             NVARCHAR (MAX)     NOT NULL,
    [HourlyRate]           DECIMAL (18, 2)    NOT NULL,
    [MaxHours]             INT                NOT NULL,
    [UserName]             NVARCHAR (256)     NULL,
    [NormalizedUserName]   NVARCHAR (256)     NULL,
    [Email]                NVARCHAR (256)     NULL,
    [NormalizedEmail]      NVARCHAR (256)     NULL,
    [EmailConfirmed]       BIT                NOT NULL,
    [PasswordHash]         NVARCHAR (MAX)     NULL,
    [SecurityStamp]        NVARCHAR (MAX)     NULL,
    [ConcurrencyStamp]     NVARCHAR (MAX)     NULL,
    [PhoneNumber]          NVARCHAR (MAX)     NULL,
    [PhoneNumberConfirmed] BIT                NOT NULL,
    [TwoFactorEnabled]     BIT                NOT NULL,
    [LockoutEnd]           DATETIMEOFFSET (7) NULL,
    [LockoutEnabled]       BIT                NOT NULL,
    [AccessFailedCount]    INT                NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [EmailIndex]
    ON [dbo].[AspNetUsers]([NormalizedEmail] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex]
    ON [dbo].[AspNetUsers]([NormalizedUserName] ASC) WHERE ([NormalizedUserName] IS NOT NULL);

### USER TOKENS
CREATE TABLE [dbo].[AspNetUserTokens] (
    [UserId]        NVARCHAR (450) NOT NULL,
    [LoginProvider] NVARCHAR (450) NOT NULL,
    [Name]          NVARCHAR (450) NOT NULL,
    [Value]         NVARCHAR (MAX) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY CLUSTERED ([UserId] ASC, [LoginProvider] ASC, [Name] ASC),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);

### CLAIMS
CREATE TABLE [dbo].[Claims] (
    [Id]                     INT             IDENTITY (1, 1) NOT NULL,
    [LecturerName]           NVARCHAR (MAX)  NULL,
    [ModuleCode]             NVARCHAR (MAX)  NULL,
    [HoursWorked]            INT             NOT NULL,
    [HourlyRate]             DECIMAL (18, 2) NOT NULL,
    [Comments]               NVARCHAR (MAX)  NULL,
    [Status]                 NVARCHAR (MAX)  NULL,
    [CoordinatorName]        NVARCHAR (MAX)  NOT NULL,
    [ManagerName]            NVARCHAR (MAX)  NOT NULL,
    [Month]                  NVARCHAR (MAX)  NULL,
    [DateSubmitted]          DATETIME2 (7)   NOT NULL,
    [EncryptedDocumentsJson] NVARCHAR (MAX)  NOT NULL,
    [OriginalDocumentsJson]  NVARCHAR (MAX)  NOT NULL,
    [UserId]                 NVARCHAR (450)  NULL,
    [CoordinatorComment]     NVARCHAR (MAX)  NULL,
    [CoordinatorId]          NVARCHAR (MAX)  NULL,
    [DateApproved]           DATETIME2 (7)   NULL,
    [DateVerified]           DATETIME2 (7)   NULL,
    [ManagerComment]         NVARCHAR (MAX)  NULL,
    [ManagerId]              NVARCHAR (MAX)  NULL,
    CONSTRAINT [PK_Claims] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Claims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Claims_UserId]
    ON [dbo].[Claims]([UserId] ASC);


## User Guide 
1. Run the applicaiont
2. Login as HR with the above credentials
3. Create Users (Lectueres, Managers, and Coordinators)
4. Login as Lecturer to create claims
5. Login as Coordinator to verify or reject claims
6. Login as manager to approve or reject claims (can only see claims that have been verified by coordinator)
7. Generate reports as HR
8. Logout when done in each view
NOTE : Session timeout is 30 minutes and deleting a user will automatically delet all related claims





   




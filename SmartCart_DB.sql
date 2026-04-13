/* =====================================================================
   SmartCart Master Database Script
   - Safe to run from scratch on a new SQL Server instance
   - Drops tables (reverse dependency) then creates in required order
   ===================================================================== */

SET NOCOUNT ON;
GO

/* =========================
   Database bootstrap (safe)
   ========================= */
IF DB_ID(N'SmartCart') IS NULL
BEGIN
    CREATE DATABASE [SmartCart];
END
GO

USE [SmartCart];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

/* ==========================================================
   DROP TABLES (reverse dependency order to avoid FK failures)
   ========================================================== */
IF OBJECT_ID(N'[dbo].[ProductVariantAttributes]', N'U') IS NOT NULL
    DROP TABLE [dbo].[ProductVariantAttributes];
GO

IF OBJECT_ID(N'[dbo].[ProductVariants]', N'U') IS NOT NULL
    DROP TABLE [dbo].[ProductVariants];
GO

IF OBJECT_ID(N'[dbo].[Products]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Products];
GO

IF OBJECT_ID(N'[dbo].[VariantValues]', N'U') IS NOT NULL
    DROP TABLE [dbo].[VariantValues];
GO

IF OBJECT_ID(N'[dbo].[Variants]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Variants];
GO

IF OBJECT_ID(N'[dbo].[Brand]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Brand];
GO

IF OBJECT_ID(N'[dbo].[SubCategory]', N'U') IS NOT NULL
    DROP TABLE [dbo].[SubCategory];
GO

IF OBJECT_ID(N'[dbo].[Category]', N'U') IS NOT NULL
    DROP TABLE [dbo].[Category];
GO

/* ==========================================================
   CREATE TABLES (required creation order)
   ========================================================== */

/* ==========================================================
   1) Categories  (physical table name: dbo.Category)
   ========================================================== */
CREATE TABLE [dbo].[Category] (
    [ID_Category]       INT IDENTITY(1,1) NOT NULL,
    [Name]              NVARCHAR(100) NOT NULL,
    [Description]       NVARCHAR(1000) NULL,
    [IsActive]          BIT NOT NULL CONSTRAINT [DF_Category_IsActive] DEFAULT ((1)),
    [Cancelled]         BIT NOT NULL CONSTRAINT [DF_Category_Cancelled] DEFAULT ((0)),
    [CancelledOn]       DATETIME NULL,
    [CancelledReason]   NVARCHAR(255) NULL,

    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([ID_Category] ASC)
);
GO

/* ==========================================================
   2) SubCategories (physical table name: dbo.SubCategory)
   ========================================================== */
CREATE TABLE [dbo].[SubCategory] (
    [ID_SubCategory]    INT IDENTITY(1,1) NOT NULL,
    [Name]              NVARCHAR(100) NOT NULL,
    [Description]       NVARCHAR(1000) NULL,
    [FK_Category]       INT NOT NULL,
    [IsActive]          BIT NOT NULL CONSTRAINT [DF_SubCategory_IsActive] DEFAULT ((1)),
    [Cancelled]         BIT NOT NULL CONSTRAINT [DF_SubCategory_Cancelled] DEFAULT ((0)),
    [CancelledOn]       DATETIME NULL,
    [CancelledReason]   NVARCHAR(255) NULL,

    CONSTRAINT [PK_SubCategory] PRIMARY KEY CLUSTERED ([ID_SubCategory] ASC),
    CONSTRAINT [FK_SubCategory_Category] FOREIGN KEY ([FK_Category])
        REFERENCES [dbo].[Category] ([ID_Category])
);
GO

/* ==========================================================
   3) Brands (physical table name: dbo.Brand)
   ========================================================== */
CREATE TABLE [dbo].[Brand] (
    [ID_Brand]          INT IDENTITY(1,1) NOT NULL,
    [BrandName]         NVARCHAR(100) NOT NULL,
    [Description]       NVARCHAR(1000) NULL,
    [IsActive]          BIT NOT NULL CONSTRAINT [DF_Brand_IsActive] DEFAULT ((1)),
    [Cancelled]         BIT NOT NULL CONSTRAINT [DF_Brand_Cancelled] DEFAULT ((0)),
    [CancelledOn]       DATETIME NULL,
    [CancelledReason]   NVARCHAR(255) NULL,

    CONSTRAINT [PK_Brand] PRIMARY KEY CLUSTERED ([ID_Brand] ASC)
);
GO

/* ==========================================================
   4) Variants (physical table name: dbo.Variants)
   ========================================================== */
CREATE TABLE [dbo].[Variants] (
    [ID_Variant]     INT IDENTITY(1,1) NOT NULL,
    [Name]           NVARCHAR(255) NOT NULL,
    [Description]    NVARCHAR(500) NULL,
    [DisplayOrder]   INT NULL CONSTRAINT [DF_Variants_DisplayOrder] DEFAULT ((0)),
    [IsActive]       BIT NULL CONSTRAINT [DF_Variants_IsActive] DEFAULT ((1)),
    [Cancelled]      BIT NULL CONSTRAINT [DF_Variants_Cancelled] DEFAULT ((0)),
    [CancelledOn]    DATETIME NULL,

    CONSTRAINT [PK_Variants] PRIMARY KEY ([ID_Variant]),
    CONSTRAINT [UQ_Variants_Name] UNIQUE ([Name])
);
GO

/* ==========================================================
   5) VariantValues (physical table name: dbo.VariantValues)
   ========================================================== */
CREATE TABLE [dbo].[VariantValues] (
    [ID_VariantValue]  INT IDENTITY(1,1) NOT NULL,
    [FK_Variant]       INT NOT NULL,
    [Name]             NVARCHAR(255) NOT NULL,
    [Description]      NVARCHAR(500) NULL,
    [DisplayOrder]     INT NULL CONSTRAINT [DF_VariantValues_DisplayOrder] DEFAULT ((0)),
    [Cancelled]        BIT NULL CONSTRAINT [DF_VariantValues_Cancelled] DEFAULT ((0)),
    [CancelledOn]      DATETIME NULL,

    CONSTRAINT [PK_VariantValues] PRIMARY KEY ([ID_VariantValue]),
    CONSTRAINT [FK_VariantValues_Variant] FOREIGN KEY ([FK_Variant])
        REFERENCES [dbo].[Variants] ([ID_Variant]),
    CONSTRAINT [UQ_VariantValues_FK_Variant_Name] UNIQUE ([FK_Variant], [Name])
);
GO

/* ==========================================================
   6) Products (physical table name: dbo.Products)
   ========================================================== */
CREATE TABLE [dbo].[Products] (
    [ID_Product]      INT IDENTITY(1,1) NOT NULL,
    [FK_SubCategory]  INT NOT NULL,
    [FK_Brand]        INT NULL,
    [Name]            NVARCHAR(255) NOT NULL,
    [Slug]            NVARCHAR(255) NOT NULL,
    [Description]     NVARCHAR(MAX) NULL,
    [IsActive]        BIT NOT NULL CONSTRAINT [DF_Products_IsActive] DEFAULT ((1)),
    [CreatedAt]       DATETIME NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (GETDATE()),
    [ModifiedAt]      DATETIME NULL,
    [Cancelled]       BIT NOT NULL CONSTRAINT [DF_Products_Cancelled] DEFAULT ((0)),
    [CancelledOn]     DATETIME NULL,

    CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([ID_Product] ASC),
    CONSTRAINT [UQ_Products_Slug] UNIQUE ([Slug]),
    CONSTRAINT [FK_Products_SubCategory] FOREIGN KEY ([FK_SubCategory])
        REFERENCES [dbo].[SubCategory] ([ID_SubCategory]),
    CONSTRAINT [FK_Products_Brand] FOREIGN KEY ([FK_Brand])
        REFERENCES [dbo].[Brand] ([ID_Brand])
);
GO

/* ==========================================================
   7) ProductVariants (physical table name: dbo.ProductVariants)
   ========================================================== */
CREATE TABLE [dbo].[ProductVariants] (
    [ID_ProductVariant]  INT IDENTITY(1,1) NOT NULL,
    [FK_Product]         INT NOT NULL,
    [SKU]                NVARCHAR(100) NOT NULL,
    [Barcode]            NVARCHAR(100) NULL,
    [VariantLabel]       NVARCHAR(255) NOT NULL,
    [Description]        NVARCHAR(1000) NULL,
    [MRP]                DECIMAL(10,2) NOT NULL,
    [SellingPrice]       DECIMAL(10,2) NOT NULL,
    [UnitOfMeasure]      NVARCHAR(20) NULL,
    [UnitValue]          DECIMAL(10,3) NULL,
    [IsDefault]          BIT NULL CONSTRAINT [DF_ProductVariants_IsDefault] DEFAULT ((0)),
    [MaxOrderQty]        INT NULL CONSTRAINT [DF_ProductVariants_MaxOrderQty] DEFAULT ((10)),
    [IsActive]           BIT NULL CONSTRAINT [DF_ProductVariants_IsActive] DEFAULT ((1)),
    [CreatedAt]          DATETIME NULL CONSTRAINT [DF_ProductVariants_CreatedAt] DEFAULT (GETDATE()),
    [Cancelled]          BIT NULL CONSTRAINT [DF_ProductVariants_Cancelled] DEFAULT ((0)),
    [CancelledOn]        DATETIME NULL,

    CONSTRAINT [PK_ProductVariants] PRIMARY KEY ([ID_ProductVariant]),
    CONSTRAINT [FK_ProductVariants_Product] FOREIGN KEY ([FK_Product])
        REFERENCES [dbo].[Products] ([ID_Product]),
    CONSTRAINT [UQ_ProductVariants_SKU] UNIQUE ([SKU]),
    CONSTRAINT [UQ_ProductVariants_Barcode] UNIQUE ([Barcode])
);
GO

/* ==========================================================
   8) ProductVariantAttributes (physical table name: dbo.ProductVariantAttributes)
   ========================================================== */
CREATE TABLE [dbo].[ProductVariantAttributes] (
    [ID_ProductVariantAttribute]  INT IDENTITY(1,1) NOT NULL,
    [FK_ProductVariant]           INT NOT NULL,
    [FK_Variant]                  INT NOT NULL,
    [FK_VariantValue]             INT NOT NULL,
    [Description]                 NVARCHAR(500) NULL,

    CONSTRAINT [PK_ProductVariantAttributes] PRIMARY KEY ([ID_ProductVariantAttribute]),
    CONSTRAINT [FK_ProductVariantAttributes_ProductVariant] FOREIGN KEY ([FK_ProductVariant])
        REFERENCES [dbo].[ProductVariants] ([ID_ProductVariant]),
    CONSTRAINT [FK_ProductVariantAttributes_Variant] FOREIGN KEY ([FK_Variant])
        REFERENCES [dbo].[Variants] ([ID_Variant]),
    CONSTRAINT [FK_ProductVariantAttributes_VariantValue] FOREIGN KEY ([FK_VariantValue])
        REFERENCES [dbo].[VariantValues] ([ID_VariantValue]),
    CONSTRAINT [UQ_ProductVariantAttributes_FK_ProductVariant_FK_Variant]
        UNIQUE ([FK_ProductVariant], [FK_Variant])
);
GO

/* ==========================================================
   OPTIONAL INDEXES (safe / non-breaking)
   ========================================================== */
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Products_Name'
      AND object_id = OBJECT_ID(N'[dbo].[Products]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Products_Name]
        ON [dbo].[Products] ([Name] ASC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_ProductVariants_SKU'
      AND object_id = OBJECT_ID(N'[dbo].[ProductVariants]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_ProductVariants_SKU]
        ON [dbo].[ProductVariants] ([SKU] ASC);
END
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = N'IX_Variants_Name'
      AND object_id = OBJECT_ID(N'[dbo].[Variants]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Variants_Name]
        ON [dbo].[Variants] ([Name] ASC);
END
GO


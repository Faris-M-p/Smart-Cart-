USE [Ecommerse]
GO

/****** Object:  Table [dbo].[Products]    Script Date: 18-01-2025 22:59:04 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Products](
	[ProductId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Price] [decimal](10, 2) NOT NULL,
	[MRP] [decimal](10, 2) NULL,
	[CategoryId] [int] NULL,
	[SubCategoryId] [int] NULL,
	[BrandId] [int] NULL,
	[Rating] [decimal](2, 1) NULL,
	[Gender] [nvarchar](10) NULL,
	[StatusId] [int] NULL,
	[CreatedAt] [datetime] NULL,
	[UpdatedAt] [datetime] NULL,
	[Cancelled] [bit] NULL,
	[CancelledOn] [datetime] NULL,
	[CancelledReason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Products] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[Products] ADD  DEFAULT ((0)) FOR [Cancelled]
GO

ALTER TABLE [dbo].[Products]  WITH CHECK ADD FOREIGN KEY([StatusId])
REFERENCES [dbo].[ProductStatus] ([StatusId])
GO

ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Brands] FOREIGN KEY([BrandId])
REFERENCES [dbo].[Brands] ([BrandId])
GO

ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Brands]
GO

ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([CategoryId])
GO

ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories]
GO

ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_SubCategories] FOREIGN KEY([SubCategoryId])
REFERENCES [dbo].[SubCategories] ([SubCategoryId])
GO

ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_SubCategories]
GO


USE [Ecommerse]
GO

/****** Object:  Table [dbo].[SubCategories]    Script Date: 18-01-2025 22:56:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SubCategories](
	[SubCategoryId] [int] IDENTITY(1,1) NOT NULL,
	[SubCategoryName] [nvarchar](100) NOT NULL,
	[CategoryId] [int] NOT NULL,
	[Cancelled] [bit] NULL,
	[CancelledOn] [datetime] NULL,
	[CancelledReason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[SubCategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[SubCategories] ADD  DEFAULT ((0)) FOR [Cancelled]
GO

ALTER TABLE [dbo].[SubCategories]  WITH CHECK ADD FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([CategoryId])
GO


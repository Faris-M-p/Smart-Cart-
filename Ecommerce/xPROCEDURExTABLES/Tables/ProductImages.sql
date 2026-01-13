USE [Ecommerse]
GO

/****** Object:  Table [dbo].[ProductImages]    Script Date: 18-01-2025 22:59:28 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProductImages](
	[ProductImageId] [int] IDENTITY(1,1) NOT NULL,
	[ProductId] [int] NOT NULL,
	[ImageUrl] [nvarchar](max) NOT NULL,
	[Cancelled] [bit] NULL,
	[CancelledOn] [datetime] NULL,
	[CancelledReason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ProductImageId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProductImages] ADD  DEFAULT ((0)) FOR [Cancelled]
GO

ALTER TABLE [dbo].[ProductImages]  WITH CHECK ADD FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([ProductId])
GO


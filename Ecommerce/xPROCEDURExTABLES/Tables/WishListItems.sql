USE [SmartCart]
GO

/****** Object:  Table [dbo].[WishlistItems]    Script Date: 18-01-2025 22:53:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[WishlistItems](
	[WishlistItemId] [int] IDENTITY(1,1) NOT NULL,
	[WishlistId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[CreatedAt] [datetime] NULL,
	[Cancelled] [bit] NULL,
	[CancelledOn] [datetime] NULL,
	[CancelledReason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[WishlistItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WishlistItems] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[WishlistItems] ADD  DEFAULT ((0)) FOR [Cancelled]
GO

ALTER TABLE [dbo].[WishlistItems]  WITH CHECK ADD FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([ProductId])
GO

ALTER TABLE [dbo].[WishlistItems]  WITH CHECK ADD FOREIGN KEY([WishlistId])
REFERENCES [dbo].[Wishlist] ([WishlistId])
GO


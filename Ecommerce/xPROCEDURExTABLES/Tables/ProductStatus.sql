USE [Ecommerse]
GO

/****** Object:  Table [dbo].[ProductStatus]    Script Date: 18-01-2025 22:58:42 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[ProductStatus](
	[StatusId] [int] IDENTITY(1,1) NOT NULL,
	[StatusName] [nvarchar](50) NOT NULL,
	[Cancelled] [bit] NULL,
	[CancelledOn] [datetime] NULL,
	[CancelledReason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[StatusId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[ProductStatus] ADD  DEFAULT ((0)) FOR [Cancelled]
GO


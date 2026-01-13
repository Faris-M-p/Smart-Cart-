USE [Ecommerse]
GO

/****** Object:  Table [dbo].[Payments]    Script Date: 18-01-2025 22:59:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Payments](
	[PaymentId] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[PaymentDate] [datetime] NULL,
	[PaymentAmount] [decimal](10, 2) NOT NULL,
	[PaymentStatus] [nvarchar](50) NOT NULL,
	[PaymentMethod] [nvarchar](50) NOT NULL,
	[Cancelled] [bit] NULL,
	[CancelledOn] [datetime] NULL,
	[CancelledReason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[PaymentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Payments] ADD  DEFAULT (getdate()) FOR [PaymentDate]
GO

ALTER TABLE [dbo].[Payments] ADD  DEFAULT ((0)) FOR [Cancelled]
GO

ALTER TABLE [dbo].[Payments]  WITH CHECK ADD FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([OrderId])
GO


USE [SmartCart]
GO

/****** Object:  Table [dbo].[SubCategory]    Script Date: 18-01-2025 22:56:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[SubCategory](
	[ID_SubCategory] INT IDENTITY(1,1) NOT NULL,
	[Name] NVARCHAR(100) NOT NULL,
	[Description] NVARCHAR(1000) NULL,
	[FK_Category] INT NOT NULL,

	[IsActive] BIT NOT NULL DEFAULT 1,
	[Cancelled] BIT NOT NULL DEFAULT 0,
	[CancelledOn] DATETIME NULL,
	[CancelledReason] NVARCHAR(255) NULL,

PRIMARY KEY CLUSTERED 
(
	[ID_SubCategory] ASC
),

CONSTRAINT [FK_SubCategory_Category] FOREIGN KEY ([FK_Category])
REFERENCES [dbo].[Category] ([ID_Category])
)
ON [PRIMARY]
GO
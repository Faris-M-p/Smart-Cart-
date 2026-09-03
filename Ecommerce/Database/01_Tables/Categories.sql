/****** Object:  Table [dbo].[Categories]    Script Date: 18-01-2025 23:00:30 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE  TABLE [dbo].[Category](
	[ID_Category] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](1000)  NULL,
	[IsActive] BIT NOT NULL DEFAULT 1,	-- Active status
	[Cancelled] BIT NOT NULL DEFAULT 0,	-- Soft delete fields
	[CancelledOn] DATETIME NULL,
	[CancelledReason] NVARCHAR(255) NULL,
PRIMARY KEY CLUSTERED 
(
	[ID_Category] ASC
)
) ON [PRIMARY]
GO



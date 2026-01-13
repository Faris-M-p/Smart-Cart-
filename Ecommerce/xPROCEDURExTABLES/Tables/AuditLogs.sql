USE [Ecommerse]
GO

/****** Object:  Table [dbo].[AuditLogs]    Script Date: 18-01-2025 23:01:48 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[AuditLogs](
	[LogId] [int] IDENTITY(1,1) NOT NULL,
	[Action] [nvarchar](100) NOT NULL,
	[UserId] [int] NOT NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[RecordId] [int] NOT NULL,
	[ChangeDetails] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NULL,
	[Cancelled] [bit] NULL,
	[CancelledOn] [datetime] NULL,
	[CancelledReason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[LogId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[AuditLogs] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[AuditLogs] ADD  DEFAULT ((0)) FOR [Cancelled]
GO


SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[AuditLogs]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[AuditLogs](
        [LogId] [int] IDENTITY(1,1) NOT NULL,
        [Action] [nvarchar](100) NOT NULL,
        [UserId] [int] NOT NULL,
        [TableName] [nvarchar](100) NOT NULL,
        [RecordId] [int] NOT NULL,
        [ChangeDetails] [nvarchar](max) NULL,
        [CreatedAt] [datetime] NULL CONSTRAINT [DF_AuditLogs_CreatedAt] DEFAULT (getdate()),
        [Cancelled] [bit] NULL CONSTRAINT [DF_AuditLogs_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([LogId] ASC)
    );
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Payments]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Payments](
        [PaymentId] [int] IDENTITY(1,1) NOT NULL,
        [OrderId] [int] NOT NULL,
        [PaymentDate] [datetime] NULL CONSTRAINT [DF_Payments_PaymentDate] DEFAULT (getdate()),
        [PaymentAmount] [decimal](10, 2) NOT NULL,
        [PaymentStatus] [nvarchar](50) NOT NULL,
        [PaymentMethod] [nvarchar](50) NOT NULL,
        [Cancelled] [bit] NULL CONSTRAINT [DF_Payments_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([PaymentId] ASC),
        CONSTRAINT [FK_Payments_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([OrderId])
    );
END
GO

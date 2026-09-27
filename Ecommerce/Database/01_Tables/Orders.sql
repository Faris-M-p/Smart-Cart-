SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Orders](
        [OrderId] [int] IDENTITY(1,1) NOT NULL,
        [UserId] [int] NOT NULL,
        [OrderDate] [datetime] NULL CONSTRAINT [DF_Orders_OrderDate] DEFAULT (getdate()),
        [TotalAmount] [decimal](10, 2) NOT NULL,
        [OrderStatus] [nvarchar](50) NOT NULL,
        [ShippingAddress] [nvarchar](max) NULL,
        [PaymentMethod] [nvarchar](50) NOT NULL,
        [Cancelled] [bit] NULL CONSTRAINT [DF_Orders_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([OrderId] ASC),
        CONSTRAINT [FK_Orders_Users] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([UserId])
    );
END
GO

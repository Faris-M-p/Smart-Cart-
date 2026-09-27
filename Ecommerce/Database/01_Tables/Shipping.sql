SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID(N'[dbo].[Shipping]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Shipping](
        [ShippingId] [int] IDENTITY(1,1) NOT NULL,
        [OrderId] [int] NOT NULL,
        [ShippingAddress] [nvarchar](max) NOT NULL,
        [ShippingDate] [datetime] NULL,
        [EstimatedDeliveryDate] [datetime] NULL,
        [ShippingStatus] [nvarchar](50) NOT NULL,
        [Cancelled] [bit] NULL CONSTRAINT [DF_Shipping_Cancelled] DEFAULT ((0)),
        [CancelledOn] [datetime] NULL,
        [CancelledReason] [nvarchar](255) NULL,
        PRIMARY KEY CLUSTERED ([ShippingId] ASC),
        CONSTRAINT [FK_Shipping_Orders] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([OrderId])
    );
END
GO

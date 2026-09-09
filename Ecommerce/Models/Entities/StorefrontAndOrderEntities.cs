using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Models.Entities
{
    /// <summary>Maps to Database-Postgres/01_Tables/users.sql (users).</summary>
    [Table("users")]
    public class UserEntity
    {
        [Key]
        [Column("id_user")]
        public int ID_User { get; set; }

        [Column("username")]
        public string UserName { get; set; } = string.Empty;

        [Column("fullname")]
        public string? FullName { get; set; }

        [Column("passwordhash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("phonenumber")]
        public string? PhoneNumber { get; set; }

        [Column("isadmin")]
        public bool IsAdmin { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("updatedat")]
        public DateTime? UpdatedAt { get; set; }

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/orders.sql (orders).</summary>
    [Table("orders")]
    public class OrderEntity
    {
        [Key]
        [Column("id_order")]
        public int ID_Order { get; set; }

        [Column("fk_user")]
        public int FK_User { get; set; }

        [Column("orderdate")]
        public DateTime? OrderDate { get; set; }

        [Column("totalamount")]
        public decimal TotalAmount { get; set; }

        [Column("orderstatus")]
        public string OrderStatus { get; set; } = string.Empty;

        [Column("shippingaddress")]
        public string? ShippingAddress { get; set; }

        [Column("paymentmethod")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }

        [Column("ordernumber")]
        public string? OrderNumber { get; set; }

        [Column("receivername")]
        public string? ReceiverName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("addressline")]
        public string? AddressLine { get; set; }

        [Column("city")]
        public string? City { get; set; }

        [Column("pincode")]
        public string? Pincode { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/order_items.sql (orderitems).</summary>
    [Table("orderitems")]
    public class OrderItemEntity
    {
        [Key]
        [Column("id_orderitem")]
        public int ID_OrderItem { get; set; }

        [Column("fk_order")]
        public int FK_Order { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Column("fk_productvariant")]
        public int? FK_ProductVariant { get; set; }

        [Column("productname")]
        public string ProductName { get; set; } = string.Empty;

        [Column("variantlabel")]
        public string? VariantLabel { get; set; }

        [Column("sku")]
        public string? SKU { get; set; }

        [Column("unitprice")]
        public decimal UnitPrice { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("linetotal")]
        public decimal LineTotal { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/payments.sql (payments).</summary>
    [Table("payments")]
    public class PaymentEntity
    {
        [Key]
        [Column("id_payment")]
        public int ID_Payment { get; set; }

        [Column("fk_order")]
        public int FK_Order { get; set; }

        [Column("paymentdate")]
        public DateTime? PaymentDate { get; set; }

        [Column("paymentamount")]
        public decimal PaymentAmount { get; set; }

        [Column("paymentstatus")]
        public string PaymentStatus { get; set; } = string.Empty;

        [Column("paymentmethod")]
        public string PaymentMethod { get; set; } = string.Empty;

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/shipping.sql (shipping).</summary>
    [Table("shipping")]
    public class ShippingEntity
    {
        [Key]
        [Column("id_shipping")]
        public int ID_Shipping { get; set; }

        [Column("fk_order")]
        public int FK_Order { get; set; }

        [Column("shippingaddress")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Column("shippingdate")]
        public DateTime? ShippingDate { get; set; }

        [Column("estimateddeliverydate")]
        public DateTime? EstimatedDeliveryDate { get; set; }

        [Column("shippingstatus")]
        public string ShippingStatus { get; set; } = string.Empty;

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/cart.sql (cart).</summary>
    [Table("cart")]
    public class CartEntity
    {
        [Key]
        [Column("id_cart")]
        public int ID_Cart { get; set; }

        [Column("fk_user")]
        public int FK_User { get; set; }

        [Column("sessionkey")]
        public Guid? SessionKey { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/cart_items.sql (cartitems).</summary>
    [Table("cartitems")]
    public class CartItemEntity
    {
        [Key]
        [Column("id_cartitem")]
        public int ID_CartItem { get; set; }

        [Column("fk_cart")]
        public int FK_Cart { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Column("fk_productvariant")]
        public int? FK_ProductVariant { get; set; }

        [Column("quantity")]
        public int Quantity { get; set; }

        [Column("price")]
        public decimal Price { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/wishlist.sql (wishlist).</summary>
    [Table("wishlist")]
    public class WishlistEntity
    {
        [Key]
        [Column("id_wishlist")]
        public int ID_Wishlist { get; set; }

        [Column("fk_user")]
        public int FK_User { get; set; }

        [Column("sessionkey")]
        public Guid? SessionKey { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/wishlist_items.sql (wishlistitems).</summary>
    [Table("wishlistitems")]
    public class WishlistItemEntity
    {
        [Key]
        [Column("id_wishlistitem")]
        public int ID_WishlistItem { get; set; }

        [Column("fk_wishlist")]
        public int FK_Wishlist { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/ratings.sql (ratings).</summary>
    [Table("ratings")]
    public class RatingEntity
    {
        [Key]
        [Column("id_rating")]
        public int ID_Rating { get; set; }

        [Column("fk_product")]
        public int FK_Product { get; set; }

        [Column("fk_user")]
        public int FK_User { get; set; }

        [Column("ratingvalue")]
        public decimal RatingValue { get; set; }

        [Column("review")]
        public string? Review { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/audit_logs.sql (auditlogs).</summary>
    [Table("auditlogs")]
    public class AuditLogEntity
    {
        [Key]
        [Column("id_auditlog")]
        public int ID_AuditLog { get; set; }

        [Column("action")]
        public string Action { get; set; } = string.Empty;

        [Column("fk_user")]
        public int FK_User { get; set; }

        [Column("tablename")]
        public string TableName { get; set; } = string.Empty;

        [Column("recordid")]
        public int RecordId { get; set; }

        [Column("changedetails")]
        public string? ChangeDetails { get; set; }

        [Column("createdat")]
        public DateTime? CreatedAt { get; set; }

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }

    /// <summary>Maps to Database-Postgres/01_Tables/product_status.sql (productstatus).</summary>
    [Table("productstatus")]
    public class ProductStatusEntity
    {
        [Key]
        [Column("id_productstatus")]
        public int ID_ProductStatus { get; set; }

        [Column("statusname")]
        public string StatusName { get; set; } = string.Empty;

        [Column("cancelled")]
        public bool? Cancelled { get; set; }

        [Column("cancelledon")]
        public DateTime? CancelledOn { get; set; }

        [Column("cancelledreason")]
        public string? CancelledReason { get; set; }
    }
}

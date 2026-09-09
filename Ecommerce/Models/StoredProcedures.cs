namespace Ecommerce.Models
{
    public static class StoredProcedures
    {
        public static class Order
        {
            public const string GetCheckoutPreview = "get_checkout_preview";
            public const string PlaceOrder = "place_order";
            public const string GetOrder = "get_order";
            public const string GetOrders = "get_orders";
            public const string CancelOrder = "cancel_order";
        }

        public static class Cart
        {
            public const string GetBagCounts = "get_bag_counts";
            public const string GetCart = "get_cart";
            public const string AddCartItem = "add_cart_item";
            public const string UpdateCartItem = "update_cart_item";
            public const string RemoveCartItem = "remove_cart_item";
            public const string ClearCart = "clear_cart";
        }

        public static class Wishlist
        {
            public const string GetWishlist = "get_wishlist";
            public const string GetWishlistStatus = "get_wishlist_status";
            public const string ToggleWishlist = "toggle_wishlist";
            public const string RemoveWishlistItem = "remove_wishlist_item";
        }

        public static class Auth
        {
            public const string RegisterUser = "register_user";
            public const string GetUserByEmail = "get_user_by_email";
            public const string GetUserById = "get_user_by_id";
        }

        public static class Shop
        {
            public const string GetProducts = "get_products";
            public const string GetShopFilters = "get_shop_filters";
            public const string GetProductDetails = "get_product_details";
        }
    }
}

namespace Ecommerce.Models
{
    public class UserAddressModel
    {
        public class UserAddress
        {
            public int AddressId { get; set; }
            public int UserId { get; set; }
            public string AddressType { get; set; } = "Home";
            public string ReceiverName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string AddressLine { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Pincode { get; set; } = string.Empty;
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public bool IsDefault { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class SaveAddressInput
        {
            public int AddressId { get; set; }
            public string AddressType { get; set; } = "Home";
            public string ReceiverName { get; set; } = string.Empty;
            public string Phone { get; set; } = string.Empty;
            public string AddressLine { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
            public string Pincode { get; set; } = string.Empty;
            public decimal? Latitude { get; set; }
            public decimal? Longitude { get; set; }
            public bool IsDefault { get; set; }
        }
    }
}

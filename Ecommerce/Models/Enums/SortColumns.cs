namespace Ecommerce.Models.Enums
{
    public enum CategorySortColumn
    {
        Id = 0,
        Name = 1,
        Description = 2,
        CreatedDate = 3
    }

    public enum BrandSortColumn
    {
        Id = 0,
        Name = 1,
        Description = 2
    }

    public enum ProductSortColumn
    {
        Id = 0,
        Name = 1,
        Price = 2,
        CreatedOn = 3
    }

    public enum SubCategorySortColumn
    {
        Id = 0,
        Name = 1,
        CategoryId = 2,
        CreatedDate = 3
    }

    public enum SupplierSortColumn
    {
        Id = 0,
        Name = 1,
        CompanyName = 2,
        City = 3,
        State = 4
    }

    public enum VariantSortColumn
    {
        Id = 0,
        Name = 1,
        DisplayOrder = 2
    }

    public enum VariantValueSortColumn
    {
        Id = 0,
        Name = 1,
        DisplayOrder = 2
    }

    /// <summary>Sort keys for shop catalog product list (maps from <see cref="Ecommerce.Models.ProductModel.InputProduct.SortColumn"/>).</summary>
    public enum ShopProductSortColumn
    {
        ProductId = 0,
        Name = 1,
        Price = 2,
        CategoryId = 3
    }

    /// <summary>Maps to dynamic sort column names expected by purchase list procedures.</summary>
    public enum PurchaseSortColumn
    {
        Id = 0,
        PurchaseDate = 1,
        TotalAmount = 2,
        InvoiceNumber = 3
    }

    public enum ProductVariantSortColumn
    {
        Id = 0,
        PriceAdjustment = 1,
        CreatedOn = 2,
        StockAvailable = 3
    }
}

namespace BlazorEcommerce.Server.Services.ProductService
{
    public interface IProductService
    {
        Task<ServiceResponse<List<Product>>> GetProductAsync();

        Task<ServiceResponse<Product>> GetProductAsync(int productId);

        Task<ServiceResponse<List<Product>>> GetProductsByCategory(string categoryUrl);

        Task<ServiceResponse<ProductSearchResult>> SearchProducts(string searchText, int page);

        Task<ServiceResponse<List<string>>> GetProductSearchSuggestion(string searchText);

        Task<ServiceResponse<List<Product>>> GetFeaturedProducts();


    }
}

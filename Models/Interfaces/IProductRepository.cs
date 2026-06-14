namespace IMS.Models.Interfaces
{
    public interface IProductRepository
    {
       Task<bool> AddProduct(Product product);
       Task<bool> UpdateProduct(int id ,Product product);
       Task<bool> DeleteProduct(int id);
       Task<Product> GetProduct(int id);
       Task< List<Product>> GetAllProducts();
        Task<int> GetProductCount();
        Task<List<Product>> GetPaginatedProducts(int page, int pageSize);
        Task<List<string>> GetCategories();
        Task<List<Product>> GetFilteredProducts(int page, int pageSize, string searchTerm, string category, decimal? minPrice, decimal? maxPrice);
        Task<int> GetFilteredProductCount(string searchTerm, string category, decimal? minPrice, decimal? maxPrice);
    }
}

namespace IMS.Models.Interfaces
{
    public interface IPurchaseRepository
    {
        Task<bool> AddPurchase(Purchase purchase);
        Task<bool> UpdatePurchase(int id ,Purchase purchase);
        Task<bool> DeletePurchase(int id);
        Task<Purchase> GetPurchase(int id);
        Task< List<Purchase>> GetAllPurchases();
        Task<int> GetPurchaseCount();
        Task<List<Purchase>> GetPaginatedPurchases(int page, int pageSize);
       // Task<List<string>> GetCategories();
        Task<List<Purchase>> GetFilteredPurchases(int page, int pageSize, string? searchTerm, int? supplierId = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<int> GetFilteredPurchasesCount(string? searchTerm, int? supplierId = null, decimal? minPrice = null, decimal? maxPrice = null);
    }
}

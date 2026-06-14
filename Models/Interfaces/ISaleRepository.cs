namespace IMS.Models.Interfaces
{
    public interface ISaleRepository
    {
        Task<bool> AddSale(Sale sale);
        //Task<bool> UpdateSale(int id ,Sale sale);
        //Task<bool> DeleteSale(int id);
        Task<Sale> GetSale(int id);
        Task <List<Sale>> GetAllSales();
        Task<int> GetSaleCount();
        Task<List<Sale>> GetPaginatedSales(int page, int pageSize);
        //Task<List<Sale>> GetFilteredSales(int page, int pageSize, string? searchTerm, int? supplierId = null, decimal? minPrice = null, decimal? maxPrice = null);
        //Task<int> GetFilteredSalesCount(string? searchTerm, int? supplierId = null, decimal? minPrice = null, decimal? maxPrice = null);
    }
}

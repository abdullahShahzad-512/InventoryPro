namespace IMS.Models.Interfaces
{
    public interface IInventoryRepository
    {
       Task<Inventory> GetInventory(int id);
       Task< List<Inventory>> GetAllInventory();
        Task<int> GetInventoryCount();
        Task<List<Inventory>> GetPaginatedInventory(int page, int pageSize);
        //Task<List<string>> GetCategories();
        //Task<List<Inventory>> GetFilteredInventory(int page, int pageSize, string searchTerm, string category, decimal? minPrice, decimal? maxPrice);
        //Task<int> GetFilteredInventoryCount(string searchTerm, string category, decimal? minPrice, decimal? maxPrice);
    }
}

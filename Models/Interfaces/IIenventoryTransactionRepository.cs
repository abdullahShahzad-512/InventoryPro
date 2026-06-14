namespace IMS.Models.Interfaces
{
    public interface IInventoryTransactionRepository
    {
        
        Task< List<InventoryTransaction>> GetAllInventoryTransactions();
        Task<int> GetInventoryTransactionsCount();
        Task<List<InventoryTransaction>> GetPaginatedInventoryTransactions(int page, int pageSize);
        
    }
}

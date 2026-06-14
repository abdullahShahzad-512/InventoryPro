using IMS.Data;
using IMS.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Models.Repositories
{
    public class InventoryTransactionRepositoryDB : IInventoryTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryTransactionRepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<InventoryTransaction>> GetAllInventoryTransactions()
        {
            return await _context.InventoryTransactions
                .AsNoTracking()
                .Include(x => x.Product)
                .ToListAsync();
        }

        public async Task<int> GetInventoryTransactionsCount()
        {
            return await _context.InventoryTransactions.CountAsync();
        }

        public async Task<List<InventoryTransaction>> GetPaginatedInventoryTransactions(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;

            return await _context.InventoryTransactions
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderByDescending(x => x.TransactionDate)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

using IMS.Data;
using IMS.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Models.Repositories
{
    public class InventoryRepositoryDB : IInventoryRepository
    {
        private readonly ApplicationDbContext _context;

        public InventoryRepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Inventory>> GetAllInventory()
        {
            return await _context.Inventory
                .AsNoTracking()
                .Include(x => x.Product)
                .ToListAsync();
        }

        public async Task<Inventory> GetInventory(int id)
        {
            return await _context.Inventory
                .AsNoTracking()
                .Include(x => x.Product)
                .FirstOrDefaultAsync(x => x.ProductId == id);
        }

        public async Task<int> GetInventoryCount()
        {
            return await _context.Inventory.CountAsync();
        }

        public async Task<List<Inventory>> GetPaginatedInventory(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;

            return await _context.Inventory
                .AsNoTracking()
                .Include(x => x.Product)
                .OrderByDescending(x => x.LastUpdated)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

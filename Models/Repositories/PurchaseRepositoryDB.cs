using IMS.Data;
using IMS.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Models.Repositories
{
    public class PurchaseRepositoryDB : IPurchaseRepository
    {
        private readonly ApplicationDbContext _context;

        public PurchaseRepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddPurchase(Purchase purchase)
        {
            purchase.PurchaseProducts ??= new List<PurchaseProduct>();

            foreach (var purchaseProduct in purchase.PurchaseProducts)
            {
                purchaseProduct.Product = null!;
                purchaseProduct.Purchase = purchase;
            }

            _context.Purchases.Add(purchase);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeletePurchase(int id)
        {
            var purchase = await _context.Purchases.FirstOrDefaultAsync(x => x.Id == id);
            if (purchase == null)
            {
                return false;
            }

            _context.Purchases.Remove(purchase);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdatePurchase(int id, Purchase purchase)
        {
            var existingPurchase = await _context.Purchases
                .Include(x => x.PurchaseProducts)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (existingPurchase == null)
            {
                return false;
            }

            existingPurchase.SupplierId = purchase.SupplierId;
            existingPurchase.TotalPaidAmount = purchase.TotalPaidAmount;
            existingPurchase.Notes = purchase.Notes;

            _context.PurchaseProducts.RemoveRange(existingPurchase.PurchaseProducts);

            var updatedPurchaseProducts = purchase.PurchaseProducts ?? new List<PurchaseProduct>();
            foreach (var purchaseProduct in updatedPurchaseProducts)
            {
                purchaseProduct.Product = null!;
                purchaseProduct.PurchaseId = existingPurchase.Id;
            }

            _context.PurchaseProducts.AddRange(updatedPurchaseProducts);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Purchase>> GetAllPurchases()
        {
            return await _context.Purchases
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseProducts)
                    .ThenInclude(x => x.Product)
                .ToListAsync();
        }

        public async Task<Purchase> GetPurchase(int id)
        {
            return await _context.Purchases
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseProducts)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Purchase>> GetPaginatedPurchases(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;

            return await _context.Purchases
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseProducts)
                    .ThenInclude(x => x.Product)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetPurchaseCount()
        {
            return await _context.Purchases.CountAsync();
        }

        public async Task<List<Purchase>> GetFilteredPurchases(int page, int pageSize, string? searchTerm, int? supplierId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            int skip = (page - 1) * pageSize;
            var query = _context.Purchases
                .AsNoTracking()
                .Include(x => x.Supplier)
                .Include(x => x.PurchaseProducts)
                    .ThenInclude(x => x.Product)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                query = query.Where(x => x.Notes != null && EF.Functions.Like(x.Notes, pattern));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(x => x.SupplierId == supplierId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x => x.TotalPaidAmount >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.TotalPaidAmount <= maxPrice.Value);
            }

            return await query
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredPurchasesCount(string? searchTerm, int? supplierId = null, decimal? minPrice = null, decimal? maxPrice = null)
        {
            var query = _context.Purchases.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                query = query.Where(x => x.Notes != null && EF.Functions.Like(x.Notes, pattern));
            }

            if (supplierId.HasValue)
            {
                query = query.Where(x => x.SupplierId == supplierId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x => x.TotalPaidAmount >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.TotalPaidAmount <= maxPrice.Value);
            }

            return await query.CountAsync();
        }
    }
}

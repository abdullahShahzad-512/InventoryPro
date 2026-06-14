using IMS.Data;
using IMS.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Models.Repositories
{
    public class SupplierRepositoryDB : ISupplierRepository
    {
        private readonly ApplicationDbContext _context;

        public SupplierRepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddSupplier(Supplier supplier)
        {
            if (string.IsNullOrWhiteSpace(supplier.SupplierName))
                throw new ArgumentException("Supplier Name is required.");

            if (string.IsNullOrWhiteSpace(supplier.ContactNumber))
                throw new ArgumentException("Contact is required.");

            _context.Suppliers.Add(supplier);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteSupplier(int id)
        {
            var supplier = await _context.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
            if (supplier == null)
            {
                return false;
            }

            _context.Suppliers.Remove(supplier);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateSupplier(int id, Supplier supplier)
        {
            var existingSupplier = await _context.Suppliers.FirstOrDefaultAsync(x => x.Id == id);
            if (existingSupplier == null)
            {
                return false;
            }

            existingSupplier.SupplierName = supplier.SupplierName;
            existingSupplier.ContactNumber = supplier.ContactNumber;
            existingSupplier.Email = supplier.Email;
            existingSupplier.Address = supplier.Address;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Supplier>> GetAllSuppliers()
        {
            return await _context.Suppliers
                .AsNoTracking()
                .OrderBy(x => x.SupplierName)
                .ToListAsync();
        }

        public async Task<Supplier> GetSupplier(int id)
        {
            return await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Supplier>> GetPaginatedSuppliers(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;

            return await _context.Suppliers
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSupplierCount()
        {
            return await _context.Suppliers.CountAsync();
        }

        public async Task<List<Supplier>> GetFilteredSuppliers(int page, int pageSize, string searchTerm)
        {
            int skip = (page - 1) * pageSize;
            var query = _context.Suppliers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                query = query.Where(x => EF.Functions.Like(x.SupplierName, pattern) || EF.Functions.Like(x.ContactNumber, pattern) || EF.Functions.Like(x.Email, pattern) || EF.Functions.Like(x.Address, pattern));
            }

            return await query
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredSupplierCount(string searchTerm)
        {
            var query = _context.Suppliers.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                query = query.Where(x => EF.Functions.Like(x.SupplierName, pattern) || EF.Functions.Like(x.ContactNumber, pattern) || EF.Functions.Like(x.Email, pattern) || EF.Functions.Like(x.Address, pattern));
            }

            return await query.CountAsync();
        }

        public async Task<IEnumerable<Product>> GetProductsBySupplier(int id)
        {
            return await _context.ProductSuppliers
                .AsNoTracking()
                .Where(x => x.SupplierId == id)
                .Include(x => x.Product)
                .Select(x => x.Product)
                .ToListAsync();
        }
    }
}

using IMS.Data;
using IMS.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Models.Repositories
{
    public class ProductRepositoryDB : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductRepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddProduct(Product product)
        {
            if (product.SalePrice < product.CostPrice)
                throw new ArgumentException("Sale Price must be greater than or equal to Cost Price.");

            if (string.IsNullOrWhiteSpace(product.ProductName))
                throw new ArgumentException("Product Name is required.");

            if (string.IsNullOrWhiteSpace(product.Barcode))
                throw new ArgumentException("Barcode is required.");

            _context.Products.Add(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProduct(int id, Product product)
        {
            var existingProduct = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (existingProduct == null)
            {
                return false;
            }

            existingProduct.ProductName = product.ProductName;
            existingProduct.Barcode = product.Barcode;
            existingProduct.Description = product.Description;
            existingProduct.Category = product.Category;
            existingProduct.CostPrice = product.CostPrice;
            existingProduct.SalePrice = product.SalePrice;
            existingProduct.ReorderLevel = product.ReorderLevel;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<Product>> GetAllProducts()
        {
            return await _context.Products
                .AsNoTracking()
                .OrderBy(x => x.ProductName)
                .ToListAsync();
        }

        public async Task<Product> GetProduct(int id)
        {
            return await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Product>> GetPaginatedProducts(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;

            return await _context.Products
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetProductCount()
        {
            return await _context.Products.CountAsync();
        }

        public async Task<List<string>> GetCategories()
        {
            return await _context.Products
                .AsNoTracking()
                .Where(x => x.Category != null && x.Category != string.Empty)
                .Select(x => x.Category)
                .Distinct()
                .OrderBy(x => x)
                .ToListAsync();
        }

        public async Task<List<Product>> GetFilteredProducts(int page, int pageSize, string searchTerm, string category, decimal? minPrice, decimal? maxPrice)
        {
            int skip = (page - 1) * pageSize;
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                query = query.Where(x => EF.Functions.Like(x.ProductName, pattern) || EF.Functions.Like(x.Barcode, pattern) || EF.Functions.Like(x.Description, pattern));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All Categories")
            {
                query = query.Where(x => x.Category == category);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x => x.SalePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.SalePrice <= maxPrice.Value);
            }

            return await query
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetFilteredProductCount(string searchTerm, string category, decimal? minPrice, decimal? maxPrice)
        {
            var query = _context.Products.AsNoTracking().AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string pattern = $"%{searchTerm}%";
                query = query.Where(x => EF.Functions.Like(x.ProductName, pattern) || EF.Functions.Like(x.Barcode, pattern) || EF.Functions.Like(x.Description, pattern));
            }

            if (!string.IsNullOrWhiteSpace(category) && category != "All Categories")
            {
                query = query.Where(x => x.Category == category);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(x => x.SalePrice >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(x => x.SalePrice <= maxPrice.Value);
            }

            return await query.CountAsync();
        }
    }
}

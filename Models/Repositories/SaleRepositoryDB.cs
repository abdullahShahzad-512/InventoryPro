using IMS.Data;
using IMS.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Models.Repositories
{
    public class SaleRepositoryDB : ISaleRepository
    {
        private readonly ApplicationDbContext _context;

        public SaleRepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> AddSale(Sale sale)
        {
            if (sale.Customer == null)
            {
                return false;
            }

            sale.SaleProducts ??= new List<SaleProducts>();
            foreach (var saleProduct in sale.SaleProducts)
            {
                saleProduct.Product = null!;
                saleProduct.Sale = sale;
            }

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var customer = await _context.Customers.FirstOrDefaultAsync(x => x.CustomerPhone == sale.Customer.CustomerPhone);
                if (customer == null)
                {
                    customer = new Customer
                    {
                        CustomerName = sale.Customer.CustomerName,
                        CustomerPhone = sale.Customer.CustomerPhone,
                        CustomerEmail = sale.Customer.CustomerEmail,
                        TotalPurchases = 1,
                        LatestVisited = DateTime.Now
                    };

                    _context.Customers.Add(customer);
                }
                else
                {
                    customer.CustomerName = sale.Customer.CustomerName;
                    customer.CustomerEmail = sale.Customer.CustomerEmail;
                    customer.TotalPurchases += 1;
                    customer.LatestVisited = DateTime.Now;
                }

                await _context.SaveChangesAsync();

                sale.CustomerId = customer.Id;
                sale.Customer = customer;
                _context.Sales.Add(sale);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                return false;
            }
        }

        public async Task<List<Sale>> GetAllSales()
        {
            return await _context.Sales
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.SaleProducts)
                    .ThenInclude(x => x.Product)
                .ToListAsync();
        }

        public async Task<Sale> GetSale(int id)
        {
            return await _context.Sales
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.SaleProducts)
                    .ThenInclude(x => x.Product)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<Sale>> GetPaginatedSales(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;

            return await _context.Sales
                .AsNoTracking()
                .Include(x => x.Customer)
                .Include(x => x.SaleProducts)
                    .ThenInclude(x => x.Product)
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetSaleCount()
        {
            return await _context.Sales.CountAsync();
        }
    }
}

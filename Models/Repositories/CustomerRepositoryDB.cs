using IMS.Data;
using IMS.Models.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace IMS.Models
{
    public class CustomerRepositoryDB : ICustomerRepository
    {
        private readonly ApplicationDbContext _context;

        public CustomerRepositoryDB(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Customer>> GetPaginatedCustomers(int page, int pageSize)
        {
            int skip = (page - 1) * pageSize;

            return await _context.Customers
                .AsNoTracking()
                .OrderByDescending(x => x.Id)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}

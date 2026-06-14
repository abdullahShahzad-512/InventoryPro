namespace IMS.Models.Interfaces
{
    public interface ICustomerRepository
    {
        Task<List<Customer>> GetPaginatedCustomers(int page, int pageSize);
       
    }
}

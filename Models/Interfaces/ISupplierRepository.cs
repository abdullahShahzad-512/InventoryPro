namespace IMS.Models.Interfaces
{
    public interface ISupplierRepository
    {
       Task<bool> AddSupplier(Supplier supplier);
       Task<bool> UpdateSupplier(int id ,Supplier supplier);
       Task<bool> DeleteSupplier(int id);
       Task<Supplier> GetSupplier(int id);
       Task< List<Supplier>> GetAllSuppliers();
        Task<int> GetSupplierCount();
        Task<List<Supplier>> GetPaginatedSuppliers(int page, int pageSize);
        Task<List<Supplier>> GetFilteredSuppliers(int page, int pageSize, string searchTerm);
        Task<int> GetFilteredSupplierCount(string searchTerm);
        Task<IEnumerable<Product>> GetProductsBySupplier(int supplierId);

    }
}

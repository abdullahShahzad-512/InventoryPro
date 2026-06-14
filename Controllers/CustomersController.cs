
using IMS.Models;
using IMS.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    
    public class CustomersController : Controller
    {
        readonly ICustomerRepository _customerRepository;
        public CustomersController(ICustomerRepository customerRepository) 
        {
            _customerRepository = customerRepository;
        }
        [HttpGet]
        public async Task<IActionResult> Customers(int page=1,int pageSize=5) 
        {
            List<Customer> list =  await _customerRepository.GetPaginatedCustomers(page, pageSize);

            return View(list); 
        }
        [HttpGet]
        public IActionResult AddCustomer()
        {
            return View();
        }[HttpGet]
        public IActionResult temple()
        {
            return View();
        }
       
       
    }
}

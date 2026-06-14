using IMS.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NuGet.Common;
using System.Security.Claims;

namespace IMS.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DashboardController> _logger;
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IPurchaseRepository _purchaseRepository;

        public DashboardController(ILogger<DashboardController> logger, IInventoryRepository inventoryRepository, ISaleRepository saleRepository, IPurchaseRepository purchaseRepository, UserManager<IdentityUser> userManager)
        {
            _logger = logger;
            _inventoryRepository = inventoryRepository;
            _saleRepository = saleRepository;
            _purchaseRepository = purchaseRepository;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            var claims = User.Claims;
            if (claims.Any(c => c.Type == ClaimTypes.Role && c.Value == "Admin"))
            
                return RedirectToAction( "AdminDashboard");
             
            
            return RedirectToAction("StaffDashboard");
            
        }
        [Authorize(Policy = "AdminAccess")]

        public async Task<IActionResult> AdminDashboard()
        {
            var inventoryList = await _inventoryRepository.GetAllInventory();
            var saleList=await _saleRepository.GetAllSales();
            var purchaseList=await _purchaseRepository.GetAllPurchases();
            ViewBag.TotalProfit = 0;  ViewBag.TotalSale = 0;  ViewBag.TotalPurchase= 0; 
        
            foreach (var s in saleList)
            {
                if (s.SaleDate.Month == DateTime.Now.Month)
                {
                    ViewBag.TotalSale += s.TotalAmount;
                    ViewBag.TotalProfit += s.Profit;
                }
            }
            foreach (var p in purchaseList)
            {
                if (p.PurchaseDate.Month == DateTime.Now.Month)
                {
                    ViewBag.TotalPurchase += p.TotalPaidAmount;
                }
            }
            return View("AdminDashboard", inventoryList);
        }
        [Authorize(Policy = "StaffAccess")]

        public async Task<IActionResult> StaffDashboard()
        {
            var id=_userManager.GetUserId(User);
            var saleList = await _saleRepository.GetAllSales();
            var filteredSales = saleList.Where(s => s.UserId == id).ToList();

            return View("StaffDashboard",filteredSales);
        }
    }
}

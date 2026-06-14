using IMS.Models;
using IMS.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class ReportsController : Controller
    {
        private readonly IInventoryTransactionRepository _inventoryTransaction;
        public ReportsController(IInventoryTransactionRepository inventoryTransaction)
        {
            _inventoryTransaction = inventoryTransaction;
        }
       

        [HttpGet]
        public async Task<IActionResult> Reports(int page=1, int pageSize = 5)
        {

            var inventoryTransactions = await _inventoryTransaction.GetPaginatedInventoryTransactions(page, pageSize);
            int totalRecords = await _inventoryTransaction.GetInventoryTransactionsCount();

            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalEntries = totalRecords;
            ViewBag.PageSize = pageSize;
            return View("Reports",inventoryTransactions);
        }

    }
}

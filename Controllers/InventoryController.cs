using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IMS.Models;
using IMS.Models.Interfaces;
namespace IMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class InventoryController : Controller
    {
        private readonly IInventoryRepository _inventory;

        public InventoryController(IInventoryRepository inventory)
        {
            _inventory = inventory;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllInventory()
        {
            return Json(await _inventory.GetAllInventory());
        }
        [HttpGet]
        public async Task<IActionResult> Inventory(int page = 1)
        {
            int pageSize = 5;

            var inventory = await _inventory.GetPaginatedInventory(page, pageSize);
            int totalRecords = await _inventory.GetInventoryCount();

            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalEntries = totalRecords;
            ViewBag.PageSize = pageSize;
            //ViewBag.Categories = await _inventory.GetCategories();

            return View(inventory);
        }


    }
}

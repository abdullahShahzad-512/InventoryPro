using IMS.Models;
using IMS.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class PurchasesController : Controller
    {
        private readonly IPurchaseRepository _purchase;

        public PurchasesController(IPurchaseRepository purchase)
        {
            _purchase = purchase;
        }
        [HttpGet]
        public async Task<IActionResult> Purchases(int page = 1)
        {
            int pageSize = 3;

            var purchases = await _purchase.GetPaginatedPurchases(page, pageSize);
            int totalRecords = await _purchase.GetPurchaseCount();

            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalEntries = totalRecords;
            ViewBag.PageSize = pageSize;

            return View(purchases);
        }
        [HttpGet]
        public IActionResult AddPurchase()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPurchase(Purchase purchase)
        {
            bool isAdded = await _purchase.AddPurchase(purchase);
            if (isAdded)
                TempData["successMsg"] = "Purchase has been Added successfully";
            else
                TempData["failMsg"] = "Unable to Add purchase";

            return RedirectToAction("Purchases");
        }
        public async Task<IActionResult> DeletePurchase(int id)
        {
            bool isDeleted = await _purchase.DeletePurchase(id);
            if (isDeleted)
                TempData["successMsg"] = "Purchase has been deleted successfully";
            else
                TempData["failMsg"] = "Unable to delete purchase";

            return RedirectToAction("Purchases");
        }
        public async Task<IActionResult> ViewPurchase(int id)
        {
            var purchase = await _purchase.GetPurchase(id);

            return View(purchase);
        } 
        public async Task<IActionResult> EditPurchase(int id)
        {
            var purchase = await _purchase.GetPurchase(id);

            return View(purchase);
        }
        [HttpPost]
        public async Task<IActionResult> UpdatePurchase(int id,Purchase purchase)
        {
            bool updated = await _purchase.UpdatePurchase(id, purchase);
            if (updated)
                TempData["successMsg"] = "Purchase has been Updated successfully";
            else
                TempData["failMsg"] = "Unable to Update purchase";

            return RedirectToAction("Purchases");

        }
    }
}

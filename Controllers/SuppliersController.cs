using IMS.Models;
using IMS.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]

    public class SuppliersController : Controller
    {
        private readonly ISupplierRepository _supplier;

        public SuppliersController(ISupplierRepository supplier)
        {
            _supplier = supplier;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllSuppliers()
        {
            return Json(await _supplier.GetAllSuppliers());
        }

        [HttpGet]
        public async Task<IActionResult> Suppliers(int page = 1)
        {
            int pageSize = 5;

            var suppliers = await _supplier.GetPaginatedSuppliers(page, pageSize);
            int totalRecords = await _supplier.GetSupplierCount();

            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalEntries = totalRecords;
            ViewBag.PageSize = pageSize;

            return View(suppliers);
        }

        [HttpGet]
        public async Task<IActionResult> FilterSuppliers(int page = 1, string searchTerm = "")
        {
            int pageSize = 5;

            var suppliers = await _supplier.GetFilteredSuppliers(page, pageSize, searchTerm);
            int totalRecords = await _supplier.GetFilteredSupplierCount(searchTerm);

            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return Json(new
            {
                suppliers = suppliers,
                totalPages = totalPages,
                currentPage = page,
                totalEntries = totalRecords,
                pageSize = pageSize
            });
        }

        [HttpGet]
        public IActionResult AddSupplier()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSupplier(Supplier supplier)
        {
            bool isAdded = await _supplier.AddSupplier(supplier);
            if (isAdded)
                TempData["successMsg"] = "Supplier has been Added successfully";
            else
                TempData["failMsg"] = "Unable to Add supplier";

            return RedirectToAction("Suppliers");
        }

        [HttpGet]
        public async Task<IActionResult> EditSupplier(int id)
        {
            var supplier = await _supplier.GetSupplier(id);

            return View(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateSupplier(int id, Supplier supplier)
        {
            bool updated = await _supplier.UpdateSupplier(id, supplier);
            if (updated)
                TempData["successMsg"] = "Supplier has been Updated successfully";
            else
                TempData["failMsg"] = "Unable to Update supplier";

            return RedirectToAction("Suppliers");

        }

        public async Task<IActionResult> DeleteSupplier(int id)
        {
            bool isDeleted = await _supplier.DeleteSupplier(id);
            if (isDeleted)
                TempData["successMsg"] = "Supplier has been deleted successfully";
            else
                TempData["failMsg"] = "Unable to delete supplier";

            return RedirectToAction("Suppliers");
        }
        [HttpGet]
        public async Task<IActionResult> ViewSupplier(int id)
        {
            var supplier = await _supplier.GetSupplier(id);

            return View(supplier);
        }
        [HttpGet]
        public async Task<IActionResult> ViewProductsBySupplier(int id,string name)
        {
            var listOfProducts = await _supplier.GetProductsBySupplier(id);
            TempData["sn"] = name;
            return View(listOfProducts);
        }
    }
}

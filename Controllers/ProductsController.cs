using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IMS.Models;
using IMS.Models.Interfaces;

namespace IMS.Controllers
{
    [Authorize(Policy = "AdminAccess")]
    public class ProductsController : Controller
    {
        private readonly IProductRepository _product;

        public ProductsController(IProductRepository product)
        {
            _product = product;
        }

        [HttpGet]
        public async Task<JsonResult> GetAllProducts()
        {
            return Json(await _product.GetAllProducts());
        }
        [HttpGet]
        public async Task<IActionResult> Products(int page = 1)
        {
            int pageSize = 5;

            var products = await _product.GetPaginatedProducts(page, pageSize);
            int totalRecords = await _product.GetProductCount();

            ViewBag.TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);
            ViewBag.CurrentPage = page;
            ViewBag.TotalEntries = totalRecords;
            ViewBag.PageSize = pageSize;
            ViewBag.Categories = await _product.GetCategories();

            return View(products);
        }
        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _product.GetCategories();
            return Json(categories);
        }

        [HttpGet]
        public async Task<IActionResult> FilterProducts(int page = 1, string searchTerm = "", string category = "", decimal? minPrice = null, decimal? maxPrice = null)
        {
            int pageSize = 5;

            var products = await _product.GetFilteredProducts(page, pageSize, searchTerm, category, minPrice, maxPrice);
            int totalRecords = await _product.GetFilteredProductCount(searchTerm, category, minPrice, maxPrice);

            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            return Json(new
            {
                products = products,
                totalPages = totalPages,
                currentPage = page,
                totalEntries = totalRecords,
                pageSize = pageSize
            });
        }
        [HttpGet]
        public IActionResult AddProduct()
        {
            return View();
        }
        
        [HttpPost]
        public async Task<IActionResult> AddProduct(Product product)
        {
            bool isAdded = await _product.AddProduct(product);
            if (isAdded)
                TempData["successMsg"] = "Product has been Added successfully";
            else
                TempData["failMsg"] = "Unable to Add product";

            return RedirectToAction("Products");
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id) 
        {
            var product = await _product.GetProduct(id);            

            return View(product);
        }
        [HttpGet]
        public async Task<IActionResult> ViewProduct(int id) 
        {
            var product = await _product.GetProduct(id);            

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProduct(int id,Product product)
        {
            bool updated = await _product.UpdateProduct(id, product);
            if (updated)
                TempData["successMsg"] = "Product has been Updated successfully";
            else
                TempData["failMsg"] = "Unable to Update product";

            return RedirectToAction("Products");

        }
        
        public async Task<IActionResult> DeleteProduct(int id)
        {
            bool isDeleted = await _product.DeleteProduct(id);
            if (isDeleted)
                TempData["successMsg"] = "Product has been deleted successfully";
            else
                TempData["failMsg"] = "Unable to delete product";

            return RedirectToAction("Products");
        }
       

    }
}

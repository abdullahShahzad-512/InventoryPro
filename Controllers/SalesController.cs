using IMS.Models;
using IMS.Models.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;

namespace IMS.Controllers
{
    public class SalesController : Controller
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly UserManager<IdentityUser> _UserManager;
        private const string CookieName = "ProductInCart";

        public SalesController(IInventoryRepository ir,ISaleRepository sr, UserManager<IdentityUser> userManager,IProductRepository pr)
        {
            _saleRepository = sr;
            _inventoryRepository = ir;
            _UserManager = userManager;
            _productRepository = pr;
        }

        [Authorize(Policy = "AdminAccess")]
        public async Task<IActionResult> Sales()
        {
            List<Sale> list =await _saleRepository.GetPaginatedSales(1,100);
            return View(list);
        }

        [Authorize(Policy = "AdminOrStaffAccess")]
        public async Task<IActionResult> POS()
        {
            var inventory = await _inventoryRepository.GetAllInventory();
            return View(inventory);
        }

        [HttpGet]
        public JsonResult GetCart()
        {
            try
            {
                var cartList = GetCartFromCookie();
                return Json(cartList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting cart: {ex.Message}");
                return Json(new List<CartItem>());
            }
        }

        [HttpPost]
        public async Task<JsonResult> AddToCart(int productId, string productName, decimal price)
        {
            var cartList = GetCartFromCookie();

            var existingItem = cartList.FirstOrDefault(item => item.ProductId == productId);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                Inventory inventory= await _inventoryRepository.GetInventory(productId);
                if (existingItem.Quantity> inventory.CurrentQuantity)
                {
                    throw new Exception("Not enough stock available");
                }
            }
            else
            {
                cartList.Add(new CartItem
                {
                    ProductId = productId,
                    ProductName = productName,
                    ProductPrice = price,
                    Quantity = 1
                });
            }

            SaveCartToCookie(cartList);
            return Json(cartList);
        }

        [HttpPost]
        public async Task<JsonResult> UpdateQuantity(int productId, string action)
        {
            try
            {
                var cartList = GetCartFromCookie();

                var item = cartList.FirstOrDefault(x => x.ProductId == productId);

                if (item != null)
                {
                    if (action == "increase")
                    {
                        item.Quantity++;
                        Inventory inventory = await _inventoryRepository.GetInventory(productId);
                        if (item.Quantity > inventory.CurrentQuantity)
                        {
                            throw new Exception("Not enough stock available");
                        }
                    }
                    else if (action == "decrease" && item.Quantity > 1)
                    {
                        item.Quantity--;
                    }

                    SaveCartToCookie(cartList);
                }

                return Json(cartList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating quantity: {ex.Message}");
                return Json(new List<CartItem>());
            }
        }

        [HttpPost]
        public JsonResult RemoveFromCart(int productId)
        {
            try
            {
                var cartList = GetCartFromCookie();

                var item = cartList.FirstOrDefault(x => x.ProductId == productId);

                if (item != null)
                {
                    cartList.Remove(item);
                    SaveCartToCookie(cartList);
                }

                return Json(cartList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing from cart: {ex.Message}");
                return Json(new List<CartItem>());
            }
        }

        public JsonResult ViewCart()
        {
            var cartList = GetCartFromCookie();
            return Json(cartList);
        }

        [HttpPost]
        public bool ClearCart()
        {
            if (Request.Cookies.ContainsKey(CookieName))
            {
                Response.Cookies.Delete(CookieName);
                return true;
            }
            return false;
        }

        private List<CartItem> GetCartFromCookie()
        {
            if (HttpContext.Request.Cookies.ContainsKey(CookieName))
            {
                string? cartJson = HttpContext.Request.Cookies[CookieName];
                return JsonSerializer.Deserialize<List<CartItem>>(cartJson!) ?? new List<CartItem>();
            }
            return new List<CartItem>();
        }

        private void SaveCartToCookie(List<CartItem> cartList)
        {
            CookieOptions option = new CookieOptions
            {
                Expires = DateTime.Now.AddHours(2),
                HttpOnly = true, // Security: prevents client-side script access
                SameSite = SameSiteMode.Strict // Security: prevents CSRF attacks
            };

            string jsonString = JsonSerializer.Serialize(cartList);
            HttpContext.Response.Cookies.Append(CookieName, jsonString, option);
        }

        public IActionResult Checkout()
        {
            var cartList = GetCartFromCookie();
            return View(cartList);
        }

        [HttpPost]
        public async Task<JsonResult> ConfirmSale(string customerName, string customerPhone, string customerEmail, string paymentMethod, string invoiceNumber)
        {
            try
            {
                var cartList = GetCartFromCookie();

                if (cartList == null || cartList.Count == 0)
                {
                    return Json(new { success = false, message = "Cart is empty" });
                }

                decimal totalAmount = cartList.Sum(item => item.Quantity * item.ProductPrice);
                var currentUser = await _UserManager.GetUserAsync(User);

                if (currentUser == null)
                {
                    return Json(new { success = false, message = "User not authenticated" });
                }
                var sale = new Sale{
                    InvoiceNumber = invoiceNumber,
                    UserName = User.Identity.Name,
                    UserId = currentUser.Id,
                    Customer = new Customer
                    {
                        CustomerName = customerName,
                        CustomerPhone = customerPhone,
                        CustomerEmail = customerEmail
                    },
                    PaymentMethod = paymentMethod,
                    TotalAmount = totalAmount,
                    SaleDate = DateTime.Now,
                    SaleProducts= cartList.Select( item => new SaleProducts
                    {

                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.ProductPrice,
                        TotalPrice = item.Quantity * item.ProductPrice,
                        
                    }).ToList()

                };
                foreach (var product in sale.SaleProducts)
                {
                    Product p = await _productRepository.GetProduct(product.ProductId);
                    product.Product = p;
                    product.Profit = (product.Product.SalePrice - product.Product.CostPrice) * product.Quantity;
                 
                    

                }
                if (await _saleRepository.AddSale(sale))
                {



                    // Clear the cart after successful sale
                    Response.Cookies.Delete(CookieName);

                    return Json(new { success = true, message = "Sale completed successfully", invoiceNumber = invoiceNumber });
                }
                else
                {
                    throw  new Exception("unable to process sale");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error confirming sale: {ex.Message}");
                return Json(new { success = false, message = "Error processing sale" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoice(int id)
        {
            Sale sale= await _saleRepository.GetSale(id);
            return View(sale);
        }
    }

}
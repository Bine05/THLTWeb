using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Repositories;
using WebBanHang.Session; // 🔥 ĐÃ THÊM: Dòng này giúp Controller nhận diện được GetObjectFromJson và SetObjectAsJson

namespace WebBanHang.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductRepository _productRepository;

        public CartController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        // 1. Trang hiển thị danh sách giỏ hàng
        public async Task<IActionResult> Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            
            // Lấy 4 sản phẩm ngẫu nhiên cho phần "Có thể bạn cũng thích"
            var allProducts = await _productRepository.GetAllAsync();
            var suggestedProducts = allProducts.OrderBy(x => Guid.NewGuid()).Take(4).ToList();
            ViewBag.SuggestedProducts = suggestedProducts;

            // Lấy mã giảm giá từ session (nếu có)
            var discount = HttpContext.Session.GetInt32("DiscountAmount") ?? 0;
            ViewBag.DiscountAmount = discount;

            return View(cart);
        }

        // 2. Xử lý logic Thêm vào giỏ hàng
        public async Task<IActionResult> AddToCart(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Lấy giỏ hàng hiện tại từ Session, nếu chưa có thì tạo mới list trống
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Kiểm tra xem sản phẩm này đã có trong giỏ hàng chưa
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

            if (cartItem == null)
            {
                // Nếu chưa có, tạo mới mục hàng và add vào giỏ
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = (decimal)product.Price, // Ép kiểu nếu Price ở model đang là double/float
                    ImageUrl = product.ImageUrl,
                    Quantity = 1
                });
            }
            else
            {
                // Nếu sản phẩm đã tồn tại, tăng số lượng lên 1
                cartItem.Quantity++;
            }

            // Lưu lại giỏ hàng đã cập nhật vào Session
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            // Bắn thông báo ngọt ngào lên màn hình thông qua TempData
            TempData["SuccessMessage"] = $"Đã thêm thành công sản phẩm '{product.Name}' vào giỏ hàng!";

            // Quay trở lại trang trước đó người dùng đang đứng (Trang chủ danh sách sản phẩm)
            return Redirect(Request.Headers["Referer"].ToString() ?? "/");
        }

        // 3. Xóa sản phẩm khỏi giỏ hàng
        public IActionResult RemoveFromCart(int id)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

            if (cartItem != null)
            {
                cart.Remove(cartItem);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                TempData["SuccessMessage"] = $"Đã xóa sản phẩm khỏi giỏ hàng!";
            }

            return RedirectToAction("Index");
        }

        // --- CÁC HÀM AJAX MỚI ---

        [HttpPost]
        public IActionResult UpdateQuantity(int id, int quantity)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            var cartItem = cart.FirstOrDefault(c => c.ProductId == id);

            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cart.Remove(cartItem);
                }
                HttpContext.Session.SetObjectAsJson("Cart", cart);
            }

            var discount = HttpContext.Session.GetInt32("DiscountAmount") ?? 0;
            var subtotal = cart.Sum(c => c.Price * c.Quantity);
            var total = subtotal - discount;
            if (total < 0) total = 0;

            return Json(new { 
                success = true, 
                itemTotal = cartItem != null ? cartItem.Price * cartItem.Quantity : 0, 
                subtotal = subtotal,
                total = total 
            });
        }

        [HttpPost]
        public IActionResult ApplyCoupon(string code)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any()) return Json(new { success = false, message = "Giỏ hàng trống!" });

            // Logic mã giảm giá cơ bản
            int discountAmount = 0;
            string message = "";
            bool success = true;

            if (code == "GIAM10K")
            {
                discountAmount = 10000;
                message = "Đã áp dụng mã giảm giá 10,000đ!";
            }
            else if (code == "GIAM20K")
            {
                discountAmount = 20000;
                message = "Đã áp dụng mã giảm giá 20,000đ!";
            }
            else
            {
                success = false;
                message = "Mã giảm giá không hợp lệ!";
            }

            if (success)
            {
                HttpContext.Session.SetInt32("DiscountAmount", discountAmount);
            }

            var subtotal = cart.Sum(c => c.Price * c.Quantity);
            var total = subtotal - discountAmount;
            if (total < 0) total = 0;

            return Json(new { success = success, message = message, discountAmount = discountAmount, total = total });
        }
        // 4. Trang Checkout (GET)
        public IActionResult Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }
            ViewBag.DiscountAmount = HttpContext.Session.GetInt32("DiscountAmount") ?? 0;
            ViewBag.Cart = cart;
            return View(new Order());
        }

        // 5. Xử lý Checkout (POST)
        [HttpPost]
        public async Task<IActionResult> Checkout(Order order, [FromServices] ApplicationDbContext context, [FromServices] Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager)
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            if (!cart.Any())
            {
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                var discount = HttpContext.Session.GetInt32("DiscountAmount") ?? 0;
                var subtotal = cart.Sum(c => c.Price * c.Quantity);
                var total = subtotal - discount;
                if (total < 0) total = 0;

                order.OrderDate = DateTime.Now;
                order.TotalAmount = total;
                order.DiscountAmount = discount;
                
                // Module 2: Lưu UserId và cộng điểm thưởng
                if (User.Identity != null && User.Identity.IsAuthenticated)
                {
                    var user = await userManager.GetUserAsync(User);
                    if (user != null)
                    {
                        order.UserId = user.Id;
                        // Cộng điểm (VD: 10,000đ = 1 điểm)
                        user.RewardPoints += (int)(total / 10000);
                        await userManager.UpdateAsync(user);
                    }
                }
                
                order.OrderDetails = new List<OrderDetail>();
                foreach (var item in cart)
                {
                    order.OrderDetails.Add(new OrderDetail
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price
                    });
                }

                context.Orders.Add(order);
                await context.SaveChangesAsync();

                // Xóa giỏ hàng và mã giảm giá
                HttpContext.Session.Remove("Cart");
                HttpContext.Session.Remove("DiscountAmount");

                return RedirectToAction("Success", new { id = order.Id });
            }

            ViewBag.DiscountAmount = HttpContext.Session.GetInt32("DiscountAmount") ?? 0;
            ViewBag.Cart = cart;
            return View(order);
        }

        // 6. Trang Success
        public async Task<IActionResult> Success(int id, [FromServices] ApplicationDbContext context)
        {
            var order = await context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            ViewBag.OrderId = id;
            ViewBag.TotalAmount = order.TotalAmount;
            return View();
        }
    }
}
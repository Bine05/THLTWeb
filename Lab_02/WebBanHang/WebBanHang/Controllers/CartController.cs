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

        // 1. Trang hiển thị danh sách giỏ hàng (localhost:xxxx/Cart hoặc localhost:xxxx/Cart/Index)
        public IActionResult Index()
        {
            // Lấy danh sách giỏ hàng từ Session ra
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();

            // Trả về View đúng thư mục Views/Cart/Index.cshtml để hiển thị bảng kê tính tiền
            return View(cart);
        }

        // 2. Xử lý logic Thêm vào giỏ hàng
        public IActionResult AddToCart(int id)
        {
            var product = _productRepository.GetById(id);
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
    }
}
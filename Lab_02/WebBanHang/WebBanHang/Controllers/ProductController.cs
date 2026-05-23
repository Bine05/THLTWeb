using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using WebBanHang.Models;
using WebBanHang.Repositories;
using System.Text.Json; // Khai báo thêm để xử lý ép chuỗi JSON cho Session

namespace WebBanHang.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // 1. Hiển thị danh sách sản phẩm
        public IActionResult Index()
        {
            var products = _productRepository.GetAll();
            return View(products);
        }

        // 2. Xem chi tiết một sản phẩm
        public IActionResult Display(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // 3. Giao diện Thêm sản phẩm (GET)
        public IActionResult Add()
        {
            var categories = _categoryRepository.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // 4. Xử lý Thêm sản phẩm có Upload ảnh (POST)
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile Image, List<IFormFile> imageUrls)
        {
            if (ModelState.IsValid)
            {
                // Lưu hình ảnh đại diện chính
                if (Image != null)
                {
                    product.ImageUrl = await SaveImage(Image);
                }

                // Lưu các hình ảnh chi tiết bổ sung (nếu có)
                if (imageUrls != null && imageUrls.Count > 0)
                {
                    product.ImageUrls = new List<string>();
                    foreach (var file in imageUrls)
                    {
                        product.ImageUrls.Add(await SaveImage(file));
                    }
                }

                _productRepository.Add(product);

                // Thêm thông báo lưu thành công
                TempData["SuccessMessage"] = "Sản phẩm đã được tạo và lưu thành công!";

                return RedirectToAction("Index", "Product");
            }

            var categories = _categoryRepository.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 5. Giao diện Cập nhật sản phẩm (GET)
        public IActionResult Update(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = _categoryRepository.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 6. Xử lý Cập nhật sản phẩm có Upload ảnh (POST)
        [HttpPost]
        public async Task<IActionResult> Update(Product product, IFormFile Image, List<IFormFile> imageUrls)
        {
            if (ModelState.IsValid)
            {
                // Nếu người dùng chọn ảnh đại diện mới -> thay thế ảnh cũ
                if (Image != null)
                {
                    product.ImageUrl = await SaveImage(Image);
                }

                // Nếu người dùng chọn loạt ảnh chi tiết mới -> cập nhật lại danh sách ảnh
                if (imageUrls != null && imageUrls.Count > 0)
                {
                    product.ImageUrls = new List<string>();
                    foreach (var file in imageUrls)
                    {
                        product.ImageUrls.Add(await SaveImage(file));
                    }
                }

                _productRepository.Update(product);

                // Thêm thông báo sửa thành công
                TempData["SuccessMessage"] = "Cập nhật thông tin sản phẩm thành công!";

                return RedirectToAction("Index", "Product");
            }

            var categories = _categoryRepository.GetAllCategories();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // 7. Giao diện Xác nhận xóa (GET)
        public IActionResult Delete(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // 8. Xử lý Xóa sản phẩm (POST)
        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            _productRepository.Delete(id);

            // Thêm thông báo xóa thành công
            TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công ra khỏi hệ thống!";

            return RedirectToAction("Index", "Product");
        }

        // 🔥 CHỨC NĂNG MỚI CẬP NHẬT: Xử lý logic Thêm vào giỏ hàng (AddToCart)
        public IActionResult AddToCart(int id)
        {
            var product = _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            // 1. Đọc danh sách giỏ hàng hiện tại từ Session
            var sessionCart = HttpContext.Session.GetString("Cart");
            var cart = new List<CartItem>();

            if (!string.IsNullOrEmpty(sessionCart))
            {
                cart = JsonSerializer.Deserialize<List<CartItem>>(sessionCart) ?? new List<CartItem>();
            }

            // 2. Tìm xem sản phẩm này đã được click thêm trước đó chưa
            var existingItem = cart.FirstOrDefault(c => c.ProductId == id);
            if (existingItem == null)
            {
                // Nếu chưa có, tiến hành thêm mới vào list giỏ hàng
                cart.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = (decimal)product.Price, // Ép kiểu dữ liệu về decimal tính toán cho chuẩn
                    ImageUrl = product.ImageUrl,
                    Quantity = 1
                });
            }
            else
            {
                // Nếu sản phẩm đã tồn tại trong giỏ thì tăng số lượng lên 1 đơn vị
                existingItem.Quantity++;
            }

            // 3. Tiến hành đóng gói và ghi đè danh sách giỏ hàng mới vào lại Session
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(cart));

            // 4. Tạo thông báo SweetAlert2 nổ màn hình cực đẹp
            TempData["SuccessMessage"] = $"Đã thêm thành công sản phẩm '{product.Name}' vào giỏ hàng!";

            // 5. Điều hướng giữ nguyên người dùng ở lại trang danh sách sản phẩm hiện tại
            return RedirectToAction("Index");
        }
        // 🔥 CHỨC NĂNG MỚI: Giao diện xem danh sách giỏ hàng
        public IActionResult Cart()
        {
            // 1. Đọc danh sách giỏ hàng từ Session ra
            var sessionCart = HttpContext.Session.GetString("Cart");
            var cart = new List<CartItem>();

            if (!string.IsNullOrEmpty(sessionCart))
            {
                cart = JsonSerializer.Deserialize<List<CartItem>>(sessionCart) ?? new List<CartItem>();
            }

            // 2. Trả về View cùng với danh sách món hàng trong giỏ
            return View(cart);
        }

        // Hàm helper dùng chung để lưu ảnh vào thư mục wwwroot/images
        private async Task<string> SaveImage(IFormFile image)
        {
            var savePath = Path.Combine("wwwroot/images", image.FileName);
            using (var fileStream = new FileStream(savePath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return "/images/" + image.FileName;
        }
    }
}
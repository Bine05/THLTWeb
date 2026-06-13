using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Controllers
{
    [Authorize] // Yêu cầu đăng nhập mới được xem lịch sử mua hàng
    public class CustomerOrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CustomerOrderController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 1. Màn hình Lịch sử mua hàng
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            // Tính toán Hạng thành viên dựa trên RewardPoints
            string membershipTier = "Đồng";
            string tierColor = "text-secondary";
            if (user.RewardPoints >= 1000)
            {
                membershipTier = "Vàng";
                tierColor = "text-warning";
            }
            else if (user.RewardPoints >= 200)
            {
                membershipTier = "Bạc";
                tierColor = "text-info";
            }

            ViewBag.RewardPoints = user.RewardPoints;
            ViewBag.MembershipTier = membershipTier;
            ViewBag.TierColor = tierColor;

            // Lấy danh sách đơn hàng của User này, bao gồm cả chi tiết và thông tin sản phẩm
            var orders = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // 2. Màn hình Chi tiết đơn hàng
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == user.Id); // Chỉ cho phép xem đơn của chính mình

            if (order == null) return NotFound();

            return View(order);
        }

        // 3. Tính năng Đặt lại (Re-order)
        [HttpPost]
        public IActionResult ReOrder(int productId)
        {
            // Chuyển hướng sang action AddToCart của CartController
            return RedirectToAction("AddToCart", "Cart", new { id = productId });
        }
    }
}

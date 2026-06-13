using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .ToListAsync();

            // 1. Tính toán các KPI cơ bản
            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var aov = totalOrders > 0 ? totalRevenue / totalOrders : 0; // Giá trị trung bình 1 đơn (AOV)

            // Giả lập tỷ lệ chuyển đổi (Do không có track số lượt truy cập thực tế)
            var conversionRate = totalOrders > 0 ? 3.5 : 0; // Ví dụ 3.5%

            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;
            ViewBag.AOV = aov;
            ViewBag.ConversionRate = conversionRate;

            // 2. Dữ liệu cho Biểu đồ doanh thu (7 ngày gần nhất)
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-i))
                .Reverse()
                .ToList();

            var revenueData = new List<decimal>();
            var labels = new List<string>();

            foreach (var day in last7Days)
            {
                var dailyRevenue = orders
                    .Where(o => o.OrderDate.Date == day)
                    .Sum(o => o.TotalAmount);
                    
                revenueData.Add(dailyRevenue);
                labels.Add(day.ToString("dd/MM"));
            }

            ViewBag.ChartLabels = labels;
            ViewBag.ChartData = revenueData;

            // 3. Xếp hạng Top 5 Sản phẩm bán chạy nhất
            var topProducts = orders
                .SelectMany(o => o.OrderDetails!)
                .GroupBy(od => new { od.ProductId, od.Product!.Name, od.Product.ImageUrl, od.Product.Price })
                .Select(g => new
                {
                    ProductId = g.Key.ProductId,
                    ProductName = g.Key.Name,
                    ImageUrl = g.Key.ImageUrl,
                    Price = g.Key.Price,
                    TotalQuantitySold = g.Sum(od => od.Quantity),
                    TotalRevenue = g.Sum(od => od.Quantity * od.Price)
                })
                .OrderByDescending(x => x.TotalQuantitySold)
                .Take(5)
                .ToList();

            ViewBag.TopProducts = topProducts;

            return View();
        }
    }
}

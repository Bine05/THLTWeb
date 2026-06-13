using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebBanHang.Models;

namespace WebBanHang.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status = "all")
        {
            var query = _context.Orders.AsQueryable();

            if (status != "all")
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.CurrentStatus = status;
            ViewBag.TotalRevenue = orders.Sum(o => o.TotalAmount);

            return View(orders);
        }

        // --- MỚI: Tính năng cập nhật trạng thái đơn hàng ---
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order != null)
            {
                order.OrderStatus = newStatus;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật trạng thái thành công!";
            }
            return RedirectToAction("Index");
        }

        // --- MỚI: Tính năng xuất CSV ---
        public async Task<IActionResult> ExportToCSV()
        {
            var orders = await _context.Orders.OrderByDescending(o => o.OrderDate).ToListAsync();
            
            var builder = new System.Text.StringBuilder();
            builder.AppendLine("Ma Don Hang,Khach Hang,So Dien Thoai,Email,Ngay Dat,Phuong Thuc,Trang Thai,Tong Tien");

            foreach (var o in orders)
            {
                // Xử lý chuỗi để tránh lỗi dấu phẩy trong CSV
                var name = o.CustomerName?.Replace(",", " ") ?? "";
                var phone = o.PhoneNumber?.Replace(",", " ") ?? "";
                var email = o.Email?.Replace(",", " ") ?? "";
                var method = o.PaymentMethod?.Replace(",", " ") ?? "";
                var status = o.OrderStatus?.Replace(",", " ") ?? "";
                
                builder.AppendLine($"{o.Id},{name},{phone},{email},{o.OrderDate:dd/MM/yyyy HH:mm},{method},{status},{o.TotalAmount}");
            }

            return File(System.Text.Encoding.UTF8.GetBytes(builder.ToString()), "text/csv", $"Orders_{DateTime.Now:yyyyMMdd_HHmm}.csv");
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var order = await _context.Orders
                .Include(o => o.OrderDetails!)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}

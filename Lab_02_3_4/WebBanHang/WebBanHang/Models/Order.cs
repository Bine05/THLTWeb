using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Order
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên người nhận")]
        [StringLength(100)]
        public string CustomerName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(15)]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email để nhận tài khoản")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        public string? Notes { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public decimal TotalAmount { get; set; }

        // Mới thêm cho Module 1
        public string PaymentMethod { get; set; } = "Chuyển khoản"; // Mặc định
        public decimal DiscountAmount { get; set; } = 0;

        // Mới thêm cho Module 2
        public string? UserId { get; set; }
        public string OrderStatus { get; set; } = "Đã xác nhận"; // Đã xác nhận -> Đã gửi hàng -> Đang vận chuyển -> Đã giao

        public List<OrderDetail>? OrderDetails { get; set; }
    }
}

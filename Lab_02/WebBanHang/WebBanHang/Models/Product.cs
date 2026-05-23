using System.ComponentModel.DataAnnotations;

namespace WebBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(100, 10000000000)]
        public decimal Price { get; set; }
        public string Description { get; set; }
        public int CategoryId { get; set; }
		// Các thuộc tính hiện có
		public string? ImageUrl { get; set; } // Đường dẫn đến hình ảnh đại diện
		public List<string>? ImageUrls { get; set; } // Danh sách các hình ảnh khác
	}
}

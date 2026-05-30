using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebBanHang.Models
{
    public class Product
    {
        public int Id { get; set; }
        [Required, StringLength(100)]
        public string Name { get; set; }
        [Range(0.01, 10000000000.00, ErrorMessage = "Giá sản phẩm phải lớn hơn 0 và hợp lệ.")]
        public decimal Price { get; set; }

        [Range(0, 100, ErrorMessage = "Phần trăm giảm giá phải từ 0 đến 100")]
        public int? DiscountPercentage { get; set; }

        [Range(0, 10000000000.00, ErrorMessage = "Số tiền giảm phải lớn hơn hoặc bằng 0")]
        public decimal? DiscountAmount { get; set; }

        [NotMapped]
        public decimal FinalPrice 
        {
            get 
            {
                if (DiscountPercentage.HasValue && DiscountPercentage.Value > 0)
                {
                    return Price - (Price * DiscountPercentage.Value / 100);
                }
                if (DiscountAmount.HasValue && DiscountAmount.Value > 0)
                {
                    return Math.Max(0, Price - DiscountAmount.Value);
                }
                return Price;
            }
        }

        public string Description { get; set; }
        public string? ImageUrl { get; set; }
        public List<ProductImage>? Images { get; set; }
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }
}

using WebBanHang.Models;

namespace WebBanHang.Repositories
{
    public class MockCategoryRepository : ICategoryRepository
    {
        private List<Category> _categoryList;
        public MockCategoryRepository()
        {
            _categoryList = new List<Category>
            {
                new Category { Id = 1, Name = "Laptop" },
                new Category { Id = 2, Name = "Desktop" },
                new Category { Id = 3, Name = "Điện thoại thông minh (Smartphone)" },
                new Category { Id = 4, Name = "Máy tính bảng (Tablet)" },
                new Category { Id = 5, Name = "Phụ kiện điện tử" },
                new Category { Id = 6, Name = "Đồng hồ thông minh (Smartwatch)" }
            };
        }
        public IEnumerable<Category> GetAllCategories()
        {
            return _categoryList;
        }

    }
}

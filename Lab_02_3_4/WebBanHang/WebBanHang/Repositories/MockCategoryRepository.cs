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

        public async Task<IEnumerable<Category>> GetAllAsync()
        {
            return await Task.FromResult(_categoryList);
        }

        public async Task<Category> GetByIdAsync(int id)
        {
            var category = _categoryList.FirstOrDefault(c => c.Id == id);
            // Ignore nullable warning for mock since this is just to satisfy the interface
            return await Task.FromResult(category!);
        }

        public async Task AddAsync(Category category)
        {
            category.Id = _categoryList.Max(c => c.Id) + 1;
            _categoryList.Add(category);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Category category)
        {
            var index = _categoryList.FindIndex(c => c.Id == category.Id);
            if (index != -1)
            {
                _categoryList[index] = category;
            }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(int id)
        {
            var category = _categoryList.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _categoryList.Remove(category);
            }
            await Task.CompletedTask;
        }
    }
}

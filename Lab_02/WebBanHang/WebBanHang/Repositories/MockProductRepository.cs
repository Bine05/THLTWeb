using System.IO;
using System.Text.Json;
using WebBanHang.Models;

namespace WebBanHang.Repositories
{
    public class MockProductRepository : IProductRepository
    {
        private readonly List<Product> _products;
        private readonly string _filePath = "products.json";

        public MockProductRepository()
        {
            if (File.Exists(_filePath))
            {
                var json = File.ReadAllText(_filePath);
                _products = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
            }
            else
            {
                // Tạo một số dữ liệu mẫu ban đầu nếu file chưa tồn tại
                _products = new List<Product>
                {
                    new Product { Id = 1, Name = "Laptop", Price = 10000000, Description = "A high-end laptop"}
                };
                SaveToFile();
            }
        }

        private void SaveToFile()
        {
            var json = JsonSerializer.Serialize(_products, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_filePath, json);
        }

        public IEnumerable<Product> GetAll()
        {
            return _products;
        }

        public Product GetById(int id)
        {
            return _products.FirstOrDefault(p => p.Id == id);
        }

        public void Add(Product product)
        {
            product.Id = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
            _products.Add(product);
            SaveToFile();
        }

        public void Update(Product product)
        {
            var index = _products.FindIndex(p => p.Id == product.Id);
            if (index != -1)
            {
                _products[index] = product;
                SaveToFile();
            }
        }

        public void Delete(int id)
        {
            var product = _products.FirstOrDefault(p => p.Id == id);
            if (product != null)
            {
                _products.Remove(product);
                SaveToFile();
            }
        }
    }
}

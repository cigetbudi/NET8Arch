using Core.Entities;
using Core.Interfaces;
using Dapper;
using Npgsql;

namespace Infrastructure.Repositories
{
    public class ProductRepository(string connectionString) : IProductRepository
    {
        private readonly string _connectionString = connectionString;

        private NpgsqlConnection DBProductConn()
        {
            return new NpgsqlConnection(_connectionString);
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            using var db = DBProductConn();
            var sql = "INSERT INTO Products (Name, Price) VALUES (@Name, @Price) RETURNING Id";
            product.Id = await db.ExecuteScalarAsync<int>(sql, product);
            return product;
        }

        public async Task DeleteProductAsync(int id)
        {
            using var db = DBProductConn();
            var sql = "DELETE FROM Products WHERE Id = @Id";
            await db.ExecuteAsync(sql, new { Id = id });
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync()
        {
            using var db = DBProductConn();
            return await db.QueryAsync<Product>("SELECT * FROM Products");
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            using var db = DBProductConn();
            return await db.QueryFirstOrDefaultAsync<Product>("SELECT * FROM Products WHERE Id = @Id", new { Id = id });
        }

        public async Task<Product> UpdateProductAsync(Product product)
        {
            using var db = DBProductConn();
            var sql = "UPDATE Products SET Name = @Name, Price = @Price WHERE Id = @Id";
            await db.ExecuteAsync(sql, product);
            return product;
        }
    }
}
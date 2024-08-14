using Core.Entities;
using Core.Interfaces;

namespace API.Handlers
{
    public static class ProductHandlers
    {
        public static async Task<IResult> GetAllProductAsync(IProductRepository repository)
        {
            var products = await repository.GetAllProductsAsync();
            return Results.Ok(products);
        }

        public static async Task<IResult> GetProductByIdAsync(int id, IProductRepository repository)
        {
            var product = await repository.GetProductByIdAsync(id);
            return product is not null ? Results.Ok(product) : Results.NotFound();
        }

        public static async Task<IResult> AddProductAsync(Product item, IProductRepository repository)
        {
            var createdItem = await repository.AddProductAsync(item);
            return Results.Created($"/products/{createdItem.Id}", createdItem);
        }

        public static async Task<IResult> UpdateProductAsync(int id, Product updatedItem, IProductRepository repository)
        {
            var product = await repository.GetProductByIdAsync(id);
            if (product is null) return Results.NotFound();

            product.Name = updatedItem.Name;
            product.Price = updatedItem.Price;
            try
            {
                await repository.UpdateProductAsync(product);
                return Results.NoContent();
            }
            catch (Exception ex)
            {

                return Results.BadRequest(ex.Message);
            }
        }

        public static async Task<IResult> DeleteProductAsync(int id, IProductRepository repository)
        {
            var product = await repository.GetProductByIdAsync(id);
            if (product is null) return Results.NotFound();

            try
            {
                await repository.DeleteProductAsync(id);
                return Results.NoContent();
            }
            catch (Exception ex)
            {

                return Results.BadRequest(ex.Message);
            }
        }
    }
}
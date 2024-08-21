using Core.Entities;
using Core.Interfaces;
using FluentValidation;
using API.Helpers;
namespace API.Handlers
{
    public static class ProductHandlers
    {
        public static async Task<IResult> GetAllProductAsync(IProductRepository repository)
        {
            var products = await repository.GetAllProductsAsync();
            return Results.Ok(Helper.CreateResponse(products, null));
        }

        public static async Task<IResult> GetProductByIdAsync(int id, IProductRepository repository)
        {
            try
            {
                var product = await repository.GetProductByIdAsync(id);
                return Results.Ok(Helper.CreateResponse(product, null));
            }
            catch (Exception ex)
            {

                return Results.NotFound(Helper.CreateResponse<string[]>(null, ex.Message));
            }
        }

        public static async Task<IResult> AddProductAsync(Product item, IProductRepository repository, IValidator<Product> validator)
        {
            var validationResult = await validator.ValidateAsync(item);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(Helper.CreateResponse<TodoItem>(null, Helper.ExtractValidationErrors(validationResult)));
            }

            var createdItem = await repository.AddProductAsync(item);
            return Results.Created($"/products/{createdItem.Id}", Helper.CreateResponse(createdItem, null));

        }

        public static async Task<IResult> UpdateProductAsync(int id, Product updatedItem, IProductRepository repository, IValidator<Product> validator)
        {
            var validationResult = await validator.ValidateAsync(updatedItem);
            if (!validationResult.IsValid)
            {
                return Results.BadRequest(Helper.CreateResponse<Product>(null, Helper.ExtractValidationErrors(validationResult)));
            }

            var product = await repository.GetProductByIdAsync(id);
            if (product is null) return Results.NotFound(Helper.CreateResponse<string[]>(null, "Produk tidak ditemukan"));


            product.Name = updatedItem.Name;
            product.Price = updatedItem.Price;
            try
            {
                await repository.UpdateProductAsync(product);
                return Results.Ok(Helper.CreateResponse(product, null));
            }
            catch (Exception ex)
            {

                return Results.NotFound(Helper.CreateResponse<string[]>(null, ex.Message));
            }
        }

        public static async Task<IResult> DeleteProductAsync(int id, IProductRepository repository)
        {
            var product = await repository.GetProductByIdAsync(id);
            if (product is null) return Results.NotFound(Helper.CreateResponse<string[]>(null, "Produk tidak ditemukan"));

            try
            {
                await repository.DeleteProductAsync(id);
                return Results.Ok(Helper.CreateResponse<string[]>(null, ""));
            }
            catch (Exception ex)
            {
                return Results.BadRequest(Helper.CreateResponse<string[]>(null, ex.Message));
            }
        }
    }
}
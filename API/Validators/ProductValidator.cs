using Core.Entities;
using FluentValidation;

namespace API.Validators
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Nama produk tidak boleh kosong").MaximumLength(150).WithMessage("Nama produk tidak boleh lebih dari 150 karakter");
            RuleFor(x => x.Price).GreaterThan(0).WithMessage("Harga harus lebih dari 0");
        }
    }
}
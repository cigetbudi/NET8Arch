using Core.Entities;
using FluentValidation;

namespace API.Validators
{
    public class TodoItemValidator : AbstractValidator<TodoItem>
    {
        public TodoItemValidator()
        {
            RuleFor(x => x.Title).NotEmpty().WithMessage("Judul tidak boleh kosong").MaximumLength(50).WithMessage("Judul tidak boleh lebih dari 50 karakter");
            RuleFor(x => x.IsCompleted).NotNull().WithMessage("Harus dipilh");
        }
    }
}
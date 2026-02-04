using FluentValidation;
using Perfect.Application.Contracts;

namespace Perfect.Application.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class InvoiceCreateRequestValidator : AbstractValidator<InvoiceCreateRequest>
{
    public InvoiceCreateRequestValidator()
    {
        RuleFor(x => x.PaymentType).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new InvoiceItemRequestValidator());
        RuleFor(x => x.CreditDaysApplied)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(365)
            .When(x => x.CreditDaysApplied.HasValue);
    }
}

public class InvoiceItemRequestValidator : AbstractValidator<InvoiceItemRequest>
{
    public InvoiceItemRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).When(x => x.UnitPrice.HasValue);
    }
}

public class PaymentRequestValidator : AbstractValidator<PaymentRequest>
{
    public PaymentRequestValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.Method).NotEmpty();
    }
}

public class ProductRequestValidator : AbstractValidator<ProductRequest>
{
    public ProductRequestValidator()
    {
        RuleFor(x => x.Sku).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(180);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Cost).GreaterThanOrEqualTo(0).When(x => x.Cost.HasValue);
        RuleFor(x => x.MinStock).GreaterThanOrEqualTo(0);
    }
}

public class CustomerRequestValidator : AbstractValidator<CustomerRequest>
{
    public CustomerRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(160);
        RuleFor(x => x.Identification).NotEmpty().MaximumLength(60);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(40);
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.DefaultCreditDays).InclusiveBetween(0, 365);
        RuleFor(x => x.CreditLimit).GreaterThanOrEqualTo(0).When(x => x.CreditLimit.HasValue);
    }
}

public class InventoryMovementRequestValidator : AbstractValidator<InventoryMovementRequest>
{
    public InventoryMovementRequestValidator()
    {
        RuleFor(x => x.ProductId).NotEmpty();
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).MaximumLength(240);
    }
}

public class TenantSettingsRequestValidator : AbstractValidator<TenantSettingsRequest>
{
    public TenantSettingsRequestValidator()
    {
        RuleFor(x => x.Currency).NotEmpty().MaximumLength(12);
        RuleFor(x => x.Timezone).NotEmpty().MaximumLength(100);
        RuleFor(x => x.DefaultCreditDays).InclusiveBetween(0, 365);
        RuleFor(x => x.DueSoonThresholdDays).InclusiveBetween(1, 30);
        RuleFor(x => x.LowStockThreshold).InclusiveBetween(0, 5000);
        RuleFor(x => x.AdministrativeFeePercent).InclusiveBetween(0, 100);
        RuleFor(x => x.InvoiceNumberingFormat).NotEmpty().MaximumLength(32);
    }
}

public class UserRequestValidator : AbstractValidator<UserRequest>
{
    public UserRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).MinimumLength(8);
    }
}

public class UserUpdateRequestValidator : AbstractValidator<UserUpdateRequest>
{
    public UserUpdateRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}

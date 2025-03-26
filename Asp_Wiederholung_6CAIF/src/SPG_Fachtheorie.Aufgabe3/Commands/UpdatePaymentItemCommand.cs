using System.ComponentModel.DataAnnotations;

public class UpdatePaymentItemCommand
{
    public int Id { get; set; }
    public string ArticleName { get; set; }
    public int Amount { get; set; }
    public decimal Price { get; set; }
    public int PaymentId { get; set; }
    public DateTime? LastUpdated { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Id <= 0) yield return new ValidationResult("Id must be greater than 0");
        if (string.IsNullOrWhiteSpace(ArticleName)) yield return new ValidationResult("ArticleName is required");
        if (Amount <= 0) yield return new ValidationResult("Amount must be greater than 0");
        if (Price <= 0) yield return new ValidationResult("Price must be greater than 0");
        if (PaymentId <= 0) yield return new ValidationResult("PaymentId must be greater than 0");
    }
}

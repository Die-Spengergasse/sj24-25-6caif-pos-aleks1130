using System.ComponentModel.DataAnnotations;

public class UpdateConfirmedCommand : IValidatableObject
{
    public DateTime? Confirmed { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Confirmed.HasValue && Confirmed.Value > DateTime.UtcNow.AddMinutes(1))
        {
            yield return new ValidationResult("Confirmed cannot be more than 1 minute in the future.");
        }
    }
}

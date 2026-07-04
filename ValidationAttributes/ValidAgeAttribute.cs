using System.ComponentModel.DataAnnotations;

namespace Midix.ValidationAttributes
{
    /// <summary>
    /// Validates that a DateTime represents a realistic date of birth:
    /// not in the future, not older than <see cref="MaxAge"/>, and
    /// at least <see cref="MinAge"/> years old.
    /// </summary>
    public class ValidAgeAttribute : ValidationAttribute
    {
        public int MinAge { get; set; } = 0;
        public int MaxAge { get; set; } = 120;

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value is not DateTime dob)
                return ValidationResult.Success; // [Required] handles empty/missing values.

            var today = DateTime.Today;

            if (dob.Date > today)
                return new ValidationResult("Date of birth cannot be in the future.");

            var age = today.Year - dob.Year;
            if (dob.Date > today.AddYears(-age)) age--; // hasn't had this year's birthday yet

            if (age < MinAge)
                return new ValidationResult($"You must be at least {MinAge} years old.");

            if (age > MaxAge)
                return new ValidationResult("Please enter a valid date of birth.");

            return ValidationResult.Success;
        }
    }
}

using System.ComponentModel.DataAnnotations;

namespace Midix.ValidationAttributes
{
    public class MustBeTrueAttribute: ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value is bool b)
            {
                if (b == true)
                {
                    return true;
                }
            }
            return false;
        }
    }
}

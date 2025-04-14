using System;
using System.ComponentModel.DataAnnotations;

namespace Models.Common
{
    public class DateGreaterThanAttribute : ValidationAttribute
    {
        public string OtherPropertyName { get; }

        public DateGreaterThanAttribute(string otherPropertyName)
        {
            OtherPropertyName = otherPropertyName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var currentDate = value as DateTime?;
            if (!currentDate.HasValue)
                return ValidationResult.Success;

            var containerType = validationContext.ObjectInstance.GetType();
            var referenceProperty = containerType.GetProperty(OtherPropertyName);

            if (referenceProperty == null)
            {
                return new ValidationResult($"Không tìm thấy thuộc tính: {OtherPropertyName}.");
            }

            var referenceDate = referenceProperty.GetValue(validationContext.ObjectInstance) as DateTime?;
            if (!referenceDate.HasValue)
            {
                return ValidationResult.Success;
            }

            if (currentDate <= referenceDate)
            {
                return new ValidationResult(ErrorMessage ?? "Ngày kết thúc phải muộn hơn ngày bắt đầu.");
            }

            return ValidationResult.Success;
        }
    }
}
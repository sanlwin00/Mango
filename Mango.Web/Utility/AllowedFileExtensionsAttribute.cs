using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility
{
    public class AllowedFileExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _allowedFileExtensions;
        public AllowedFileExtensionsAttribute(string[] allowedExtensions)
        {
            _allowedFileExtensions = allowedExtensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file != null)
            {
                string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!_allowedFileExtensions.Select(e => e.ToLowerInvariant()).Contains(fileExtension))
                {
                    string allowedExtensions = string.Join(", ", _allowedFileExtensions);
                    return new ValidationResult($"This file extension '{fileExtension}' is not allowed. Allowed extensions are: {allowedExtensions}.");
                }

            }
            return ValidationResult.Success;
        }
    }
}

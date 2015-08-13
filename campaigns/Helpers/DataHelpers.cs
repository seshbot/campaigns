using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using System.Web.Mvc;

namespace campaigns.Helpers
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class AllowSafeHtmlAttribute : ValidationAttribute, IMetadataAware
    {
        public void OnMetadataCreated(ModelMetadata metadata)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException("metadata");
            }

            metadata.RequestValidationEnabled = false;
        }

        protected override ValidationResult IsValid(object value, ValidationContext context)
        {
            if (null == value)
                return ValidationResult.Success;

            var htmlString = value.ToString();
            if (Regex.Match(htmlString, "<script.*?</script>", RegexOptions.IgnoreCase).Success)
                return new ValidationResult(context.DisplayName + " contains potentially unsafe HTML (javascript)");
            if (Regex.Match(htmlString, "onclick[ ]*=", RegexOptions.IgnoreCase).Success)
                return new ValidationResult(context.DisplayName + " contains potentially unsafe HTML (onclick event)");

            return ValidationResult.Success;
        }
    }
}

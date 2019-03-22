using GroceryStore.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryStore.Models.HomeViewModels
{
    public class CategoryViewModel : IValidatableObject
    {
        public Category Category { get; set; }
        public IFormFile Image { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            bool isImageAltEmpty = string.IsNullOrWhiteSpace(Category.ImageAlt);

            if (Image == null && !isImageAltEmpty || isImageAltEmpty && Image != null)
            {
                yield return new ValidationResult("Either both the image and image description fields must be empty or both image and image description fields must have something entered.");
            }

            if (Image != null)
            {
                if (!FileUtility.IsImageSupported(Image))
                {
                    yield return new ValidationResult("The specified image is not in a valid format.", new List<string> { nameof(Image) });
                }
                else if (Image.Length == 0)
                {
                    yield return new ValidationResult("The specified file is empty. Only files with content inside can be uploaded.", new List<string> { nameof(Image) });
                }
                else if (Image.Length > FileUtility.MaxImageSizeInBinaryBytes)
                {
                    yield return new ValidationResult("The specified file is too large.", new List<string> { nameof(Image) });
                }
            }

            yield return ValidationResult.Success;
        }
    }
}

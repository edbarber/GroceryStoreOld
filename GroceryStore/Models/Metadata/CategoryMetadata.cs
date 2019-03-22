using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Validations;

namespace GroceryStore.Models
{
    public class CategoryMetadata
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [MaxLength(10)]
        [NoSpaces(ErrorMessage = "The Code field cannot contain spaces.")]
        public string Code { get; set; }

        [MaxLength(50)]
        [Display(Name = "Image description")]
        public string ImageAlt { get; set; }
    }

    [ModelMetadataType(typeof(CategoryMetadata))]
    public partial class Category : IValidatableObject
    {
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            yield return ValidationResult.Success;
        }
    }
}

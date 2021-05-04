using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Antiques_Auction_WebApp.Models
{
    public class AntiqueItemModel : IValidatableObject
    {
        public string Id { get; set; }
        [StringLength(50), Required(ErrorMessage = "Please enter the name of the antique item")]
        public string Name { get; set; }
        [StringLength(256), Required]
        public string Description { get; set; }
        [Required(ErrorMessage = "Please enter the price")]
        public int Price { get; set; }
        [Display(Name = "Choose the image for the antique item"), Required]
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
        [Display(Name = "Choose auction start time and date"), DataType(DataType.DateTime), Required]
        [BindProperty, DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AuctionOpenDateTime { get; set; }
        [Display(Name = "Choose auction end time and date"), DataType(DataType.DateTime), Required]
        [BindProperty, DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AuctionCloseDateTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();
            if (AuctionCloseDateTime < AuctionOpenDateTime)
            {
                errors.Add(new ValidationResult($"{nameof(AuctionCloseDateTime)} field needs to be greater than {nameof(AuctionOpenDateTime)} field", new List<string> { nameof(AuctionCloseDateTime) }));
            }
            if(AuctionOpenDateTime < DateTime.Now)
            {
                errors.Add(new ValidationResult($"{nameof(AuctionOpenDateTime)} field can not be in the past", new List<string> { nameof(AuctionOpenDateTime) }));
            }
            return errors;
        }
    }
}

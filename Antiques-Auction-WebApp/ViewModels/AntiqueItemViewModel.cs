using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class AntiqueItemViewModel : IValidatableObject
    {
        public string Id { get; set; }
        [StringLength(50), Required(ErrorMessage = "Please enter the name of the antique item")]
        public string Name { get; set; }
        [StringLength(256), Required]
        public string Description { get; set; }
        [Required(ErrorMessage = "Please enter the price")]
        public int Price { get; set; }
        [Required]
        public IFormFile Image { get; set; }
        public string ImageUrl { get; set; }
        [Display(Name = "Start Date and Time"), DataType(DataType.DateTime), Required]
        [BindProperty, DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AuctionOpenDateTime { get; set; }
        [Display(Name = "End Date and Time"), DataType(DataType.DateTime), Required]
        [BindProperty, DisplayFormat(DataFormatString = "{0:yyyy-MM-ddTHH:mm}", ApplyFormatInEditMode = true)]
        public DateTime AuctionCloseDateTime { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> errors = new List<ValidationResult>();
            if (AuctionCloseDateTime < AuctionOpenDateTime)
            {
                errors.Add(new ValidationResult($"Auction Close date has to be greater than auction open date.",  new List<string> { nameof(AuctionCloseDateTime) }));
            }
            if (AuctionOpenDateTime < DateTime.Now)
            {
                errors.Add(new ValidationResult($"Auction open date can not be in the past.", new List<string> { nameof(AuctionOpenDateTime) }));
            }
            return errors;
        }
    }
}
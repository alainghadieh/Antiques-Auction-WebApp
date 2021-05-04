using System.ComponentModel.DataAnnotations;

namespace Antiques_Auction_WebApp.ViewModels
{
    public class UserLoginModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

    }
}
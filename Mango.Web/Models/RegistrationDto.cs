using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models.Dto
{
    public class RegistrationDto
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Name { get; set; }
        public string PhoneNumber { get; set; }      
        public string Role { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;

namespace MyCodeCamp.DTOs
{
    public class CredentialDto
    {
        [Required]
        public string UserName { get; set; }
        [Required]
        public string Password { get; set; }
    }
}

using PSKVideoProjectBackend.Properties;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class UserToRegister
    {
        [Required]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public UserToRegister()
        {
            Username = Resources.FillerVideoUsername;
            EmailAddress = Resources.FillerEmail;
            FirstName = Resources.FillerFirstName;
            LastName = Resources.FillerLastName;
        }
    }
}

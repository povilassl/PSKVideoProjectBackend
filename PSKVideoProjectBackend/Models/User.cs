using PSKVideoProjectBackend.Properties;
using System.ComponentModel.DataAnnotations;

namespace PSKVideoProjectBackend.Models
{
    public class User
    {
        [Key]
        public uint Id { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public byte[] PasswordHashed { get; set; }
        public DateTime AccountCreationDateTime { get; set; }
        public DateTime LastLoginDateTime { get; set; }
        public string ProfilePictureURL { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public User()
        {
            Username = Resources.FillerVideoUsername;
            EmailAddress = Resources.FillerEmail;
            PasswordHashed = new byte[0];
            AccountCreationDateTime = DateTime.Now;
            LastLoginDateTime = DateTime.MinValue; //MinValue means that the user has never logged in
            ProfilePictureURL = String.Empty; //TODO: change to some filler (or is it better on frontend?)
            FirstName = Resources.FillerFirstName;
            LastName = Resources.FillerLastName;
        }
    }
}

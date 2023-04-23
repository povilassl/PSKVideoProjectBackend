using PSKVideoProjectBackend.Properties;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using Konscious.Security.Cryptography;
using System.Text;
using PSKVideoProjectBackend.Helpers;

namespace PSKVideoProjectBackend.Models
{
    public class RegisteredUser
    {
        [Key]
        public uint Id { get; set; }
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public byte[] PasswordHashed { get; set; }
        public byte[] Salt { get; set; }
        public DateTime AccountCreationDateTime { get; set; }
        public DateTime LastLoginDateTime { get; set; }
        public string ProfilePictureURL { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public RegisteredUser()
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

        public RegisteredUser(UserToRegister userToRegister)
        {
            Username = userToRegister.Username;
            EmailAddress = userToRegister.EmailAddress;
            FirstName = userToRegister.FirstName;
            LastName = userToRegister.LastName;

            AccountCreationDateTime = DateTime.Now;
            LastLoginDateTime = DateTime.MinValue; //MinValue means that the user has never logged in
            ProfilePictureURL = String.Empty; //TODO: change to some filler (or is it better on frontend?)            

            //hashing the password
            Salt = HashHelpers.GenerateSalt();
            PasswordHashed = userToRegister.Password.HashPassword(Salt);
        }
    }
}

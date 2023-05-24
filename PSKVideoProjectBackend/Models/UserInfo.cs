namespace PSKVideoProjectBackend.Models
{
    public class UserInfo
    {
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public DateTime AccountCreationDateTime { get; set; }
        public string ProfilePictureURL { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        public UserInfo()
        {
            Username = "";
            EmailAddress = "";
            AccountCreationDateTime = DateTime.MinValue;
            ProfilePictureURL = "";
            FirstName = "";
            LastName = "";
        }

        public UserInfo(RegisteredUser user)
        {
            Username = user.Username;
            EmailAddress = user.EmailAddress;
            AccountCreationDateTime = user.AccountCreationDateTime;
            ProfilePictureURL = user.ProfilePictureURL;
            FirstName = user.FirstName;
            LastName = user.LastName;
        }
    }
}

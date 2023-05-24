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

        }
    }
}

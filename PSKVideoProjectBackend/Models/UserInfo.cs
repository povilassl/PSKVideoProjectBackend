﻿namespace PSKVideoProjectBackend.Models
{
    public class UserInfo
    {
        public string Username { get; set; }
        public string EmailAddress { get; set; }
        public DateTime AccountCreationDateTime { get; set; }
        public DateTime LastInfoUpdateDateTime { get; set; }
        public string ProfilePictureURL { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }


        public UserInfo()
        {
            Username = "";
            EmailAddress = "";
            AccountCreationDateTime = DateTime.MinValue;
            LastInfoUpdateDateTime = DateTime.MinValue;
            ProfilePictureURL = "";
            FirstName = "";
            LastName = "";
        }

        public UserInfo(RegisteredUser user)
        {
            Username = user.Username;
            EmailAddress = user.EmailAddress;
            AccountCreationDateTime = user.AccountCreationDateTime;
            LastInfoUpdateDateTime = user.LastInfoUpdateDateTime;
            ProfilePictureURL = user.ProfilePictureURL;
            FirstName = user.FirstName;
            LastName = user.LastName;
        }
    }
}

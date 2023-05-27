using PSKVideoProjectBackend.Interfaces;
using System.Text.RegularExpressions;

namespace PSKVideoProjectBackend.Validators
{
    public class UsernameValidatorBundle
    {
        public class UsernameValidator : IInputValidator
        {
            public bool IsValid(string username)
            {
                if (String.IsNullOrEmpty(username)) return false;

                if (username.Length <= 4 || username.Length > 20) return false;

                // Username should contain characters a-z A-Z 0-9 
                string allowedCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
                return username.All(c => allowedCharacters.Contains(c));
            }
        }

        public class UsernameValidatorRegex : IInputValidator
        {
            public bool IsValid(string username)
            {
                // Username should contain characters a-z A-Z 0-9 
                return Regex.IsMatch(username, "^[a-zA-Z0-9]+$");
            }
        }

        public class UsernameValidatorLettersOnly : IInputValidator
        {
            public bool IsValid(string username)
            {
                // Username should contain characters a-z A-Z 0-9 
                return Regex.IsMatch(username, "^[a-zA-Z]+$");
            }
        }
    }
}

using PSKVideoProjectBackend.Interfaces;
using System.Text.RegularExpressions;

namespace PSKVideoProjectBackend.Validators
{
    public class PasswordValidatorBundle
    {
        public class PasswordValidator : IInputValidator
        {
            public bool IsValid(string password)
            {
                //length is 8 - 20
                if (password.Length < 8 || password.Length > 20) return false;

                //At least 1 uppercase character
                if (!Regex.IsMatch(password, "[A-Z]")) return false;

                //At least 1 lowercase character
                if (!Regex.IsMatch(password, "[a-z]")) return false;

                //At least 1 special character
                if (!Regex.IsMatch(password, "[!@#$%^&*(),.?\":{}|<>]")) return false;

                return true;
            }
        }
    }
}

using PSKVideoProjectBackend.Interfaces;
using System.Text.RegularExpressions;

namespace PSKVideoProjectBackend.Validators
{
    public class EmailValidatorBundle
    {
        public class EmailValidatorRegex : IInputValidator
        {
            public bool IsValid(string email)
            {
                if (String.IsNullOrEmpty(email)) return false;

                // Email should be of email format
                var emailRegex = new Regex(@"^[a-zA-Z0-9_.+-]+@[a-zA-Z0-9-]+\.[a-zA-Z0-9-.]+$");
                return emailRegex.IsMatch(email);
            }
        }
    }

}

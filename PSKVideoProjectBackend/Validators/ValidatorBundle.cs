using PSKVideoProjectBackend.Interfaces;

namespace PSKVideoProjectBackend.Validators
{
    public class ValidatorBundle
    {
        public class UsernameValidator : IValidator
        {
            public bool IsValid(string username)
            {
                return true;
            }
        }

        public class PasswordValidator : IValidator
        {
            public bool IsValid(string password)
            {
                return true;
            }
        }

        public class EmailValidator : IValidator
        {
            public bool IsValid(string email)
            {
                return true;
            }
        }
    }

}

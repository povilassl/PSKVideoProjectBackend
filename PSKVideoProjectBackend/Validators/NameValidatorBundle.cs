using PSKVideoProjectBackend.Interfaces;
using System.Text.RegularExpressions;

namespace PSKVideoProjectBackend.Validators
{
    public class NameValidatorBundle
    {
        public class NameValidatorRegex : IInputValidator
        {
            public bool IsValid(string name)
            {
                if (String.IsNullOrEmpty(name)) return false;

                if (name.Length > 50) return false;

                // Name and lastname should only contain characters a-z A-Z
                Regex nameRegex = new Regex("^[a-zA-Z]+$");
                return nameRegex.IsMatch(name);
            }
        }
    }

}

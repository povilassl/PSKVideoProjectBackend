using PSKVideoProjectBackend.Interfaces;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace PSKVideoProjectBackend.Validators
{
    public class CommentValidatorBundle
    {
        public class CommentValidatorCharOnly : IInputValidator
        {
            public bool IsValid(string comment)
            {
                return Regex.IsMatch(comment, "^[a-zA-Z]+$");
            }
        }

        public class CommentValidatorNumbers : IInputValidator
        {
            public bool IsValid(string comment)
            {
                return Regex.IsMatch(comment, "^[a-zA-Z0-9]+$");
            }
        }

        public class CommentValidatorSpecialChars : IInputValidator
        {
            public bool IsValid(string comment)
            {
                return Regex.IsMatch(comment, "^[a-zA-Z0-9!@#$%^&*()]+$");
            }
        }
    }

}

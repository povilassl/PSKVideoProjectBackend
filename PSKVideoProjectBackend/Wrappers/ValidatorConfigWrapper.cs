using PSKVideoProjectBackend.Interfaces;
using PSKVideoProjectBackend.Models.Enums;

namespace PSKVideoProjectBackend.Wrappers
{
    public class ValidatorConfigWrapper
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Comment { get; set; }

        public Dictionary<InputType, IInputValidator> Validators { get; set; }
    }
}

using PSKVideoProjectBackend.Models.Enums;

namespace PSKVideoProjectBackend.Interfaces
{
    public interface IObjectValidator
    {
        InfoValidation IsValid(object obj);
    }
}

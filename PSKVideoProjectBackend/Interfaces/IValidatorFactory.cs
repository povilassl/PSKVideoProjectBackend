using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Models.Enums;

namespace PSKVideoProjectBackend.Interfaces
{
    public interface IDataValidator
    {
        bool ValidateUsername(string username);
        bool ValidatePassword(string username);
        bool ValidateEmail(string username);
        bool ValidateNameAndLastName(string username);
        bool ValidateRegularTextInput(string username);

        InfoValidation ValidateUserInfo(UserInfo userInfo);
    }
}

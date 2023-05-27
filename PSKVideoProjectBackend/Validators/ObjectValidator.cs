using Microsoft.OpenApi.Writers;
using Microsoft.Rest.Serialization;
using PSKVideoProjectBackend.Factories;
using PSKVideoProjectBackend.Interfaces;
using PSKVideoProjectBackend.Models;
using PSKVideoProjectBackend.Models.Enums;

namespace PSKVideoProjectBackend.Validators
{
    public class ObjectValidator : IObjectValidator
    {
        private readonly ValidatorFactory _validatorFactory;

        public ObjectValidator(ValidatorFactory validatorFactory)
        {
            _validatorFactory = validatorFactory;
        }

        public InfoValidation IsValid(object obj)
        {
            if (obj is null) return InfoValidation.NotSupplied;

            if (obj is UserInfo userInfo) return Validate(userInfo);

            if (obj is UserToRegister userToRegister) return Validate(userToRegister);

            if (obj is VideoComment videoComment) return Validate(videoComment);

            ///...

            return InfoValidation.NotSupportedInputType;
        }

        public InfoValidation Validate(UserInfo userInfo)
        {
            bool usernameValid = _validatorFactory.ValidateInput(userInfo.Username, InputType.Username);
            bool emailValid = _validatorFactory.ValidateInput(userInfo.EmailAddress, InputType.Email);
            bool firstNameValid = _validatorFactory.ValidateInput(userInfo.FirstName, InputType.Name);
            bool lastNameValid = _validatorFactory.ValidateInput(userInfo.LastName, InputType.Name);

            if (!usernameValid) return InfoValidation.BadUsername;
            if (!emailValid) return InfoValidation.BadEmail;
            if (!firstNameValid) return InfoValidation.BadFirstName;
            if (!lastNameValid) return InfoValidation.BadLastName;

            return InfoValidation.Validated;
        }

        public InfoValidation Validate(UserToRegister userToRegister)
        {
            bool usernameValid = _validatorFactory.ValidateInput(userToRegister.Username, InputType.Username);
            bool emailValid = _validatorFactory.ValidateInput(userToRegister.EmailAddress, InputType.Email);
            bool passwordValid = _validatorFactory.ValidateInput(userToRegister.Password, InputType.Password);
            bool firstNameValid = _validatorFactory.ValidateInput(userToRegister.FirstName, InputType.Name);
            bool lastNameValid = _validatorFactory.ValidateInput(userToRegister.LastName, InputType.Name);

            if (!usernameValid) return InfoValidation.BadUsername;
            if (!emailValid) return InfoValidation.BadEmail;
            if (!passwordValid) return InfoValidation.BadPassword;
            if (!firstNameValid) return InfoValidation.BadFirstName;
            if (!lastNameValid) return InfoValidation.BadLastName;

            return InfoValidation.Validated;
        }

        public InfoValidation Validate(VideoComment videoComment)
        {
            bool commentValid = _validatorFactory.ValidateInput(videoComment.Comment, InputType.Comment);

            if (!commentValid) return InfoValidation.BadComment;

            return InfoValidation.Validated;
        }
    }
}

using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.Extensions.Primitives;
using PSKVideoProjectBackend.Interfaces;
using PSKVideoProjectBackend.Models.Enums;
using Microsoft.Extensions.Configuration;
using System.Configuration;
using System.Diagnostics;
using PSKVideoProjectBackend.Wrappers;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace PSKVideoProjectBackend.Factories
{
    public class ValidatorFactory
    {
        //private Dictionary<InputType, IInputValidator> _validators;
        private readonly ValidatorConfigWrapper _validatorConfig;
        private readonly IOptionsMonitor<ValidatorConfigWrapper> _optionsMonitor;

        public ValidatorFactory(IOptionsMonitor<ValidatorConfigWrapper> options)
        {
            _optionsMonitor = options;
            _validatorConfig = options.CurrentValue;
            //ConfigUpdated();
        }

        public bool ValidateInput(string input, InputType inputType)
        {
            if (_validatorConfig.Validators is null || ConfigUpdated()) ReloadConfig();

            if (_validatorConfig.Validators.TryGetValue(inputType, out var validator))
            {
                return validator.IsValid(input);
            }

            return false;
        }

        private void ReloadConfig()
        {
            _validatorConfig.Validators = new();

            // Get the type of the ValidatorConfigWrapper class
            Type wrapperType = typeof(ValidatorConfigWrapper);

            // Get all properties of the ValidatorConfigWrapper class
            PropertyInfo[] properties = wrapperType.GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                object propertyValue = property.GetValue(_optionsMonitor.CurrentValue)!;

                if (propertyName != "Validators")
                {
                    if (Enum.TryParse<InputType>(propertyName, ignoreCase: true, out var inputType))
                    {

                        var validatorClassName = propertyValue as string;

                        var validatorType = Type.GetType(validatorClassName);
                        if (validatorType != null)
                        {
                            var validator = Activator.CreateInstance(validatorType) as IInputValidator;
                            _validatorConfig.Validators[inputType] = validator!;
                        }
                        else
                        {
                            throw new InvalidOperationException("Invalid validator class name: " + validatorClassName);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException("Invalid input type:" + propertyName);
                    }
                }
            }
        }

        private bool ConfigUpdated()
        {
            var currentVal = _validatorConfig.Validators;
            var newV = _optionsMonitor.CurrentValue;

            return !(currentVal[InputType.Username].ToString() == newV.Username.ToString() &&
                currentVal[InputType.Password].ToString() == newV.Password.ToString() &&
                currentVal[InputType.Email].ToString() == newV.Email.ToString() &&
                currentVal[InputType.Comment].ToString() == newV.Comment.ToString() &&
                currentVal[InputType.Name].ToString() == newV.Name.ToString());
        }
    }
}

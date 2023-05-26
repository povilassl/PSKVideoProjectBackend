using Microsoft.AspNetCore.Mvc.TagHelpers;
using PSKVideoProjectBackend.Interfaces;
using PSKVideoProjectBackend.Models.Enums;

namespace PSKVideoProjectBackend.Factories
{
    public class ValidatorFactory
    {
        private readonly Dictionary<InputType, IValidator> validators;
        private readonly IHostEnvironment _hostEnvironment;
        private readonly ILogger<ValidatorFactory> _logger;

        public ValidatorFactory(string configFilename, IHostEnvironment hostEnvironment, ILogger<ValidatorFactory> logger)
        {
            validators = new();
            LoadValidatorsFromConfig(configFilename);
            _hostEnvironment = hostEnvironment;
            _logger = logger;

        }

        private void LoadValidatorsFromConfig(string configFilename)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(_hostEnvironment.ContentRootPath)
                .AddJsonFile(configFilename, optional: false, reloadOnChange: true);

            var config = builder.Build();

            foreach (var validatorConfig in config.GetSection("Validators").GetChildren())
            {
                var inputTypeString = validatorConfig.GetValue<string>("InputType");

                if (Enum.TryParse<InputType>(inputTypeString, ignoreCase: true, out var inputType))
                {

                    var validatorClassName = validatorConfig.GetValue<string>("ValidatorClass");

                    var validatorType = Type.GetType(validatorClassName);
                    if (validatorType != null)
                    {
                        var validator = Activator.CreateInstance(validatorType) as IValidator;
                        validators[inputType] = validator!;
                    }
                    else
                    {
                        _logger.LogError("Invalid validator class name: " + validatorClassName);
                        throw new InvalidOperationException("Invalid validator class name: " + validatorClassName);
                    }
                }
                else
                {
                    _logger.LogError("Invalid input type: " + inputTypeString);
                    throw new InvalidOperationException("Invalid input type: " + inputTypeString);
                }
            }
        }

        public bool ValidateInput(string input, InputType inputType)
        {
            if (validators.TryGetValue(inputType, out var validator))
            {
                return validator.IsValid(input);
            }

            return false;
        }
    }
}

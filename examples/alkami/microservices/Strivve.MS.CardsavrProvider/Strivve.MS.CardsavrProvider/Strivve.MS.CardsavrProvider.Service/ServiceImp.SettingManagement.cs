using Alkami.Data.Validations;
using Alkami.MicroServices.Settings.ProviderBased.Contracts;
using Alkami.TrackableObjects.Plugins;
using Strivve.MS.CardsavrProvider.Data.ProviderSettings;
using System.Collections.Generic;

namespace Strivve.MS.CardsavrProvider.Service
{
    /// <inheritdoc />
    public partial class ServiceImp
    {
        /// <summary>
        /// Create default settings that the service expects when first being deployed, these can be anything you'd like them to be
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, string> DefaultSettings()
        {
            var settings = base.DefaultSettings() ?? new Dictionary<string, string>();

            if (!settings.ContainsKey(SettingNames.CardsavrURL))
            {
                settings.Add(SettingNames.CardsavrURL, "Default CardSavr URL");
            }

            if (!settings.ContainsKey(SettingNames.IntegratorName))
            {
                settings.Add(SettingNames.IntegratorName, "Default Integrator Name");
            }

            if (!settings.ContainsKey(SettingNames.IntegratorKey))
            {
                settings.Add(SettingNames.IntegratorKey, "Default Integrator Key");
            }

            if (!settings.ContainsKey(SettingNames.CustomerAgentUsername))
            {
                settings.Add(SettingNames.CustomerAgentUsername, "Default Customer Agent Username");
            }

            if (!settings.ContainsKey(SettingNames.CustomerAgentPassword))
            {
                settings.Add(SettingNames.CustomerAgentPassword, "");
            }

            if (!settings.ContainsKey(SettingNames.CardupdatrAppURL))
            {
                settings.Add(SettingNames.CardupdatrAppURL, "");
            }

            return settings;
        }

        /// <summary>
        /// These descriptors need to be added in order to properly display them in the Admin Portal
        /// Use the SettingDescriptor constructor to create a new descriptor for each setting name    
        /// </summary>
        /// <returns></returns>
        public override List<SettingDescriptor> SettingDescriptors()
        {
            var descriptors = new List<SettingDescriptor>();

            // We'll add the range of default setting descriptors, this is most likely empty
            descriptors.AddRange(base.SettingDescriptors());

            // Next add a descriptor for each of the settings
            descriptors.Add(new SettingDescriptor(SettingNames.CardsavrURL, "https URL of cardsavr instance.", typeof(string), true, "A meaningful display name", false));
            descriptors.Add(new SettingDescriptor(SettingNames.IntegratorName, "Integrator name configured in the portal.", typeof(string), true, "A meaningful display name", false));
            descriptors.Add(new SettingDescriptor(SettingNames.IntegratorKey, "Integrator Key obtained from the portal.", typeof(string), true, "A meaningful display name", false));
            descriptors.Add(new SettingDescriptor(SettingNames.CustomerAgentUsername, "Customer Agent Username.", typeof(string), true, "A meaningful display name", false));
            descriptors.Add(new SettingDescriptor(SettingNames.CustomerAgentPassword, "Customer Agent Password.", typeof(string), true, "A meaningful display name", false));
            descriptors.Add(new SettingDescriptor(SettingNames.CardupdatrAppURL, "Cardupdatr App URL.", typeof(string), true, "A meaningful display name", false));

            return descriptors;
        }

        /// <summary>
        /// Each setting should be validated as well as possible. This will safeguard the implementation from incorrect configuration.
        /// </summary>
        /// <param name="settingDescriptor">The setting Descriptor to validate</param>
        /// <param name="settingValue">The value to validate</param>
        /// <param name="errors">Pre-initialized collection of errors for you to add any additional errors to</param>
        /// <param name="isValidated">A flag that determines if the SettingDescriptor type has been validated; it will be false only if the default type validation fails.</param>
        /// <param name="performedValidation">Set this to true if validation checks have been performed</param>
        protected override void ValidateChangedSetting(SettingDescriptor settingDescriptor, string settingValue, List<ValidationResult> errors, bool isValidated, ref bool performedValidation)
        {
            if (!isValidated) return;

            switch (settingDescriptor.Name)
            {
                case SettingNames.CardsavrURL:
                    {
                        if (!settingValue.Contains("https://"))
                        {
                            Logger.Error(
                                $"{settingDescriptor.Name} value requires that the `https://` exist as part of the string value. [{settingValue}]");
                            errors.AddValidationError(SettingNames.CardsavrURL,
                                "URL must be https", SubCode.ValueUnsupported);
                        }

                        performedValidation = true;
                        break;
                    }
                case SettingNames.IntegratorName:
                case SettingNames.IntegratorKey:
                case SettingNames.CustomerAgentUsername:
                case SettingNames.CustomerAgentPassword:
                    {
                        performedValidation = true;
                        break;
                    }
                case SettingNames.CardupdatrAppURL:
                    {
                        if (!settingValue.Contains("https://"))
                        {
                            Logger.Error(
                                $"{settingDescriptor.Name} value requires that `https://` exist as part of the string value. [{settingValue}]");
                            errors.AddValidationError(SettingNames.CardupdatrAppURL,
                                "URL must be https", SubCode.ValueUnsupported);
                        }

                        performedValidation = true;
                        break;
                    }
                default:
                    {
                        Logger.Info(
                            $"{settingDescriptor.Name} value was not validated! Has it been defined within the DefaultSettings class and has it been added to the SettingsDescriptors collections?");
                        performedValidation = false;
                        break;
                    }
            }
        }
    }
}
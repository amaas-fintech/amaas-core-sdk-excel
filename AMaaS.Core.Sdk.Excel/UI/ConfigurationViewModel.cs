using AMaaS.Core.Sdk.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AMaaS.Core.Sdk.Enums;
using IContainer = Autofac.IContainer;

namespace AMaaS.Core.Sdk.Excel.UI
{
    public class ConfigurationViewModel : ViewModelBase, IAMaaSConfiguration
    {
        #region Fields

        private IAMaaSConfiguration _selectedConfiguration;
        private string _username;
        private string _password;

        #endregion

        #region Properties

        public string CognitoPoolId => SelectedConfiguration?.CognitoPoolId;
        public string CognitoClientId => SelectedConfiguration?.CognitoClientId;
        public string AwsRegion => SelectedConfiguration?.AwsRegion;
        public Uri Endpoint => SelectedConfiguration?.Endpoint;
        public bool IsInitialized => SelectedConfiguration?.IsInitialized ?? false;
        public string ApiVersion => SelectedConfiguration?.ApiVersion;
        public string Username
        {
            get => _username;
            set
            {
                _username = value;
                RaisePropertyChange(nameof(Username));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertyChange(nameof(Password));
            }
        }

        public AMaaSEnvironment Environment
        {
            get => SelectedConfiguration?.Environment ?? AMaaSEnvironment.Default;
            set { }
        }

        public IAMaaSConfiguration SelectedConfiguration
        {
            get => _selectedConfiguration;
            set
            {
                _selectedConfiguration = value;
                RaisePropertyChange(nameof(SelectedConfiguration));
            }
        }

        public List<IAMaaSConfiguration> Configurations => new List<IAMaaSConfiguration>
        {
            new AMaaSConfigDefault(Username, Password, ApiVersion),
            new AMaaSConfigLive(Username, Password, ApiVersion)
        };

        #endregion

        #region Constructor

        public ConfigurationViewModel()
        {
            SelectedConfiguration = Configurations.OfType<AMaaSConfigDefault>().FirstOrDefault();
        }

        #endregion
    }
}

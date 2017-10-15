using Autofac;
using System;
using System.Linq;
using System.Threading.Tasks;
using AMaaS.Core.Sdk.Configuration;
using AMaaS.Core.Sdk.AssetManagers;
using AMaaS.Core.Sdk.Constants;
using System.Collections.Generic;
using AMaaS.Core.Sdk.Parties.Models;
using AMaaS.Core.Sdk.Parties;

namespace AMaaS.Core.Sdk.Excel.UI
{
    public class UserViewModel : ViewModelBase, IUserViewModel
    {
        #region Fields

        private string _username;
        private string _password;
        private bool _isLoggedIn;
        private IAMaaSConfiguration _configuration;
        private IEnumerable<Party> _assetManagerParties;
        private Party _selectedAssetManagerParty;

        #endregion

        #region Properties

        public string Username
        {
            get => _configuration.Username;
            set
            {
                _configuration.Username = value;
                RaisePropertyChange(nameof(Username));
            }
        }

        public string Password
        {
            get => _configuration.Password;
            set
            {
                _configuration.Password = value;
                RaisePropertyChange(nameof(Password));
            }
        }

        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                _isLoggedIn = value;
                RaisePropertyChange(nameof(IsLoggedIn));
            }
        }

        public IEnumerable<Party> AssetManagerParties
        {
            get => _assetManagerParties;
            set
            {
                _assetManagerParties = value;
                RaisePropertyChange(nameof(AssetManagerParties));
            }
        }

        public Party SelectedAssetManagerParty
        {
            get => _selectedAssetManagerParty;
            set
            {
                _selectedAssetManagerParty = value;
                AddinContext.AssumedAmid = _selectedAssetManagerParty.AssetManagerId;
                RaisePropertyChange(nameof(SelectedAssetManagerParty));
            }
        }

        #endregion

        #region Constructor

        public UserViewModel(IAMaaSConfiguration configuration)
        {
            _configuration = configuration;
        }

        #endregion

        #region Methods

        public async Task LoginAsync(Action onSuccess = null, Action<string> onError = null)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                onError("Username and Password are required.");
                return;
            }

            IsBusy      = true;
            BusyMessage = "Logging in...";

            try
            {
                var assetManagerInterface = Container.Resolve<IAssetManagersInterface>();
                var partiesInterface      = Container.Resolve<IPartiesInterface>();
                AddinContext.UserAmid     = await assetManagerInterface.Session.GetTokenAttribute(CognitoAttributes.AssetManagerId);
                AddinContext.Username     = await assetManagerInterface.Session.GetTokenAttribute(CognitoAttributes.UserName);
                var relationships         = await assetManagerInterface.GetUserRelationships(int.Parse(AddinContext.UserAmid));
                var assetManagerParties   = new List<Party>();

                foreach(var relationship in relationships)
                {
                    var parties = await partiesInterface.SearchParties(relationship.AssetManagerId, partyTypes: new List<string> { "AssetManager" });
                    assetManagerParties.AddRange(parties);
                }
                AssetManagerParties       = assetManagerParties;
                SelectedAssetManagerParty = AssetManagerParties.FirstOrDefault();

                AddinContext.UserContext = this;
                AddinContext.Excel.Ribbon?.Invalidate();
                IsLoggedIn               = true;
                onSuccess?.Invoke();
            }
            catch(Exception ex)
            {
                onError($"Login failed for user: {Username}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}

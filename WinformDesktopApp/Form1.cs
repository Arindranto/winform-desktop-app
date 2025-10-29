using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using Newtonsoft.Json.Linq;
using System;
using System.Data;
using System.Security.Claims;
using WinformDesktopApp.Configuration;
using WinformDesktopApp.Configuration.MyApp.Configuration;
using WinformDesktopApp.Configuration.POCO;

namespace WinformDesktopApp
{
    public partial class Form1 : Form
    {
        private OidcClient _oidcClient;
        private KeycloakSettings _keycloakSettings
        {
            get
            {
                return ConfigurationService.Instance.KeycloakSettings;
            }
        }
        public Form1()
        {
            InitializeComponent();
            Authenticate();
        }

        private void InitializeOidcClient()
        {
            _oidcClient = null;
            // Replace these values with your Keycloak and Client details
            var authority = $"{_keycloakSettings.Server}/realms/{_keycloakSettings.Realm}";
            var clientId = _keycloakSettings.Client; // Your Keycloak Client ID
            var redirectUri = $"http://127.0.0.1:{_keycloakSettings.RedirectPort}/"; // Must match the one registered in Keycloak and Windows


            var options = new OidcClientOptions
            {
                Authority = authority,
                ClientId = clientId,
                Scope = "openid profile offline_access", // Requested scopes
                RedirectUri = redirectUri,
                Browser = SystemBrowser.Singleton(port: _keycloakSettings.RedirectPort), // Use the System Browser for secure flow
                // This is the crucial part to allow HTTP discovery
                Policy = new Policy
                {
                    Discovery = new DiscoveryPolicy
                    {
                        // Allow the discovery document to be loaded over unencrypted HTTP
                        RequireHttps = false
                    }
                }
            };

            _oidcClient = new OidcClient(options);
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await Authenticate();
        }

        private async Task Authenticate()
        {
            // Code for authentication
            // 1. Initiate the Login Flow (generates PKCE, launches external browser)
            InitializeOidcClient();
            var loginRequest = new LoginRequest
            {

                // This is the key part to force the login prompt
                FrontChannelExtraParameters = new Parameters(new Dictionary<string, string>
                {
                    // Example to force a new login prompt
                    { "prompt", "login" },
        
                    // Example to pre-fill a username
                    // { "login_hint", "user@example.com" }
                })
            };
            var loginResult = await _oidcClient.LoginAsync(loginRequest);

            if (loginResult.IsError)
            {
                MessageBox.Show($"Login Error: {loginResult.Error}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Show();
                return;
            }

            // 2. Login Successful!
            var accessToken = loginResult.AccessToken;
            var idToken = loginResult.IdentityToken;
            var userName = loginResult.User.FindFirst("preferred_username")?.Value;

            // Display results and enable the main application UI
            // MessageBox.Show($"Welcome, {userName}! Access Token Received.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            var resourceAccessClaim = processAccessToken(accessToken);
            if (_keycloakSettings.NeedRole)
            {
                if (resourceAccessClaim == null)
                {
                    MessageBox.Show("Autorization failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return;
                }
                // Parse the JSON inside the claim
                var resourceAccess = JObject.Parse(resourceAccessClaim.Value);

                if (resourceAccess[_keycloakSettings.Client]?["roles"] is JArray rolesArray)
                {
                    ConfigurationService.Instance.SetRoles(rolesArray.Select(r => r.Value<string>()).ToList());
                }
                else
                {
                    MessageBox.Show("Autorization failed", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    Show();
                    return;
                }   
            }
            Hide();
            AppForm application = new AppForm();
            application.Show();

            // Store tokens securely (e.g., in memory for the session)
            // You would typically use the accessToken to call your protected APIs.
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            // Hide();
        }

        private Claim? processAccessToken(string accessToken)
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(accessToken);

            List<string> roles = new List<string>();

            // Find the 'realm_access' claim in the token's payload
            // Replace "my-winforms-app" with your actual client ID

            // Find the 'resource_access' claim in the token's payload
            var resourceAccessClaim = token.Claims.FirstOrDefault(c => c.Type == "resource_access");
            return resourceAccessClaim;
        }
    }
}

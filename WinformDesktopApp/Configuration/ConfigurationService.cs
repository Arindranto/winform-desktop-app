using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinformDesktopApp.Configuration.POCO;

namespace WinformDesktopApp.Configuration
{
    using Microsoft.Extensions.Configuration;
    using System.Diagnostics;
    using System.IO;

    // You will need to install the Microsoft.Extensions.Configuration.Json NuGet package.

    namespace MyApp.Configuration
    {
        public sealed class ConfigurationService
        {
            // 1. Static instance for the Singleton pattern
            private static readonly ConfigurationService instance = new ConfigurationService();

            // 2. Private IConfigurationRoot object
            private readonly IConfigurationRoot _configuration;

            // 3. Static property to expose the instance
            public static ConfigurationService Instance => instance;

            // --- Expose Settings Directly ---
            // Expose a section (e.g., DatabaseSettings) as a property
            public KeycloakSettings KeycloakSettings { get; }

            public List<string> Roles { get; private set; }


            // 4. Private Constructor for Singleton initialization
            private ConfigurationService()
            {
                // Build the configuration directly from the JSON file
                string baseDirectory = Path.GetDirectoryName(Application.StartupPath)!;
                _configuration = new ConfigurationBuilder()
                    .SetBasePath(baseDirectory)
                    .AddJsonFile("config.json", optional: false, reloadOnChange: true)
                    .Build();

                // Bind the strongly-typed objects
                KeycloakSettings = new KeycloakSettings();
                _configuration.GetSection("Keycloak").Bind(KeycloakSettings);
            }

            // 5. Optional: Method to get any value
            public string GetValue(string key)
            {
                return _configuration[key];
            }

            public void SetRoles(List<string> roles)
            {
                Roles = roles;
            }
        }
    }
}

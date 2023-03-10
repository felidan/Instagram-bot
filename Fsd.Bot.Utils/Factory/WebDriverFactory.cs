using Fsd.Bot.Utils.Configuration;
using Fsd.Bot.Utils.Enum;
using Fsd.Bot.Utils.Logger;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;

namespace Fsd.Bot.Utils.Factory
{
    public static class WebDriverFactory
    {
        public static IWebDriver GetDriver(BrowserEnum browser, string path, bool headless)
        {
            IWebDriver webDriver = null;

            var version = GetVersionDriverChrome();

            if (string.IsNullOrEmpty(version))
                version = "83";

            switch (browser)
            {
                case BrowserEnum.Chrome:
                    ChromeOptions chromeOptions = new ChromeOptions();

                    if (headless)
                        chromeOptions.AddArgument("--headless");
                    
                    chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.notifications", 2);
                    
                    webDriver = new ChromeDriver($"{path}{version}", chromeOptions);
                    break;
            }

            return webDriver;
        }

        private static string GetVersionDriverChrome()
        {
            string version = String.Empty;

            if (Directory.Exists(ConfigApp.Driver.pathChromeMachine))
            {
                var dirs = Directory.GetDirectories(ConfigApp.Driver.pathChromeMachine);

                foreach (var x in dirs)
                {
                    var folder = x.Replace(ConfigApp.Driver.pathChromeMachine, "");
                    if ((int)folder[0] >= 49 && (int)folder[0] <= 57)
                    {
                        version = folder.Split('.')[0];
                        break;
                    }
                }
            }

            if (String.IsNullOrEmpty(version))
                LogBot.Add(new Entity.Log($"Utilizando Driver padrão", LevelLogEnum.Info));
            else
                LogBot.Add(new Entity.Log($"Versão {version} do Driver localizada", LevelLogEnum.Info));

            return version;
        }
    }
}

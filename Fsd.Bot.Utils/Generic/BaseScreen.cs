using Fsd.Bot.Utils.Configuration;
using Fsd.Bot.Utils.Enum;
using Fsd.Bot.Utils.Factory;
using Fsd.Bot.Utils.Logger;
using OpenQA.Selenium;
using System;
using System.IO;

namespace Fsd.Bot.Utils.Generic
{
    public class BaseScreen
    {
        protected IWebDriver _webDriver;
        private readonly int _sleepLow = ConfigApp.Sleep.Low;
        private readonly int _sleepMiddle = ConfigApp.Sleep.Middle;
        private readonly double _coef = ConfigApp.Sleep.Coef;

        public BaseScreen(BrowserEnum browser, string driverPath, bool headless)
        {
            _webDriver = WebDriverFactory.GetDriver(browser, driverPath, headless);
        }

        public (bool, string) Init(TypeExecEnum typeExec)
        {
            var result = (false, "");

            try
            {
                result = Exec(typeExec);
            }
            catch (Exception ex)
            {
                LogBot.Add(new Entity.Log(ex.Message, LevelLogEnum.Erro, ex));
            }
            finally
            {
                Finally();
            }

            return result;
        }

        public virtual (bool, string) Exec(TypeExecEnum typeExec)
        {
            return (false, "Not Implemented");
        }
        
        public void Finally()
        {
            _webDriver.Close();
            _webDriver.Quit();
            _webDriver.Dispose();
        }

        public void PrintScreen(string path, string name)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            ITakesScreenshot takesScreenshot = _webDriver as ITakesScreenshot;
            Screenshot screenshot = takesScreenshot.GetScreenshot();
            screenshot.SaveAsFile($"{path}{name}", ScreenshotImageFormat.Png);
        }

        public int GetSleepMiddle()
        {
            var variation = (int)((double)_sleepMiddle * (_coef / 100));
            Random random = new Random();
            var rm = random.Next((Math.Abs(variation) * -1), Math.Abs(variation));
            return _sleepMiddle + rm;
        }

        public int GetSleepLow()
        {
            var variation = (int)((double)_sleepLow * (_coef / 100));
            Random random = new Random();
            var rm = random.Next((Math.Abs(variation) * -1), Math.Abs(variation));
            return _sleepLow + rm;
        }
    }
}

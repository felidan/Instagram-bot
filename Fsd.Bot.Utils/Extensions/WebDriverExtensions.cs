using OpenQA.Selenium;
using System;
using System.Collections.ObjectModel;

namespace Fsd.Bot.Utils.Extensions
{
    public static class WebDriverExtensions
    {
        public static void CarregarPagina(this IWebDriver driver, TimeSpan timeOut, string url)
        {
            driver.Manage().Timeouts().PageLoad = timeOut;
            driver.Navigate().GoToUrl(url);
        }

        public static string BuscarHtml(this IWebDriver driver, By by, string atributo)
        {
            IWebElement webElement = driver.FindElement(by);
            return webElement.GetAttribute(atributo);
        }

        public static void IncluirValor(this IWebDriver webDriver, By by, string value, bool limparTudo = false)
        {
            IWebElement webElement = webDriver.FindElement(by);

            if (limparTudo)
                webElement.Clear();

            webElement.SendKeys(value);
        }

        public static bool AguardarMudancaHtml(this IWebDriver driver, TimeSpan timeOut, By by, string value, bool contem = false)
        {
            var tempoLimite = new TimeSpan(DateTime.Now.Ticks);
            tempoLimite += timeOut;

            while (new TimeSpan(DateTime.Now.Ticks) <= tempoLimite)
            {
                try
                {
                    IWebElement webElement = driver.FindElement(by);
                    var newValue = webElement.GetAttribute("innerText");

                    if (contem && newValue.Contains(value))
                    {
                        return true;
                    }
                    else
                    {
                        if (newValue != value)
                            return true;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            return false;
        }

        public static void SubmeterBotao(this IWebDriver driver, By by)
        {
            IWebElement webElement = driver.FindElement(by);
            webElement.Submit();
        }

        public static void PressionarTecla(this IWebDriver driver, By by, string key)
        {
            IWebElement webElement = driver.FindElement(by);
            webElement.SendKeys(key);
        }

        public static void ClicarBotao(this IWebDriver driver, By by)
        {
            IWebElement webElement = driver.FindElement(by);
            webElement.Click();
        }

        public static ReadOnlyCollection<IWebElement> BuscarElementos(this IWebDriver driver, By by)
        {
            ReadOnlyCollection<IWebElement> webElements = driver.FindElements(by);
            return webElements;
        }
    }
}

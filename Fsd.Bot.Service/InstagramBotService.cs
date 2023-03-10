using Fsd.Bot.Domain.Interfaces.Service;
using Fsd.Bot.Domain.Model;
using Fsd.Bot.Utils.Configuration;
using Fsd.Bot.Utils.Enum;
using Fsd.Bot.Utils.Generic;
using Fsd.Bot.Utils.Logger;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace Fsd.Bot.Service
{
    public class InstagramBotService : BaseScreen, IInstagramBotService
    {
        private readonly string _initUrl;
        private readonly string _login;
        private readonly string _senha;
        private readonly TimeSpan _timeout;

        public InstagramBotService() : base(BrowserEnum.Chrome, ConfigApp.Driver.Path, ConfigApp.Driver.Headless)
        {
            _initUrl = @"https://www.instagram.com";
            _timeout = TimeSpan.FromSeconds(ConfigApp.BotConfig.Timeout);
            _login = ConfigApp.BotConfig.Login;
            _senha = ConfigApp.BotConfig.Pass;
        }

        public override (bool, string) Exec(TypeExecEnum typeExec)
        {
            LoadPage();

            var tryLogin = Login();

            if (!tryLogin.Item1)
                tryLogin = Login();

            if (tryLogin.Item1)
            {
                FileServerService fileService = new FileServerService();
                var config = fileService.GetConfig();

                LogBot.Add(tryLogin.Item2);

                if(typeExec == TypeExecEnum.FollowProfileAndLike)
                {
                    var listUsers = fileService.GetUsers();

                    int countExec = 0;

                    listUsers.ForEach(user =>
                    {
                        if (!user.Follow && countExec < ConfigApp.BotConfig.QtFollowHour)
                        {
                            var likeFirstPostResult = LikeFirstPost(user.NameUser);
                            LogBot.Add(likeFirstPostResult.Item2);
                            var followResult = Follow(user.NameUser, true);
                            LogBot.Add(followResult.Item2);
                            if(followResult.Item1)
                                user.Follow = true;

                            countExec++;
                        }

                        //if (countExec >= ConfigApp.BotConfig.QtFollowHour)
                        //{
                            //countExec = 0;
                            //Thread.Sleep(TimeSpan.FromMinutes(60));
                        //}

                    });

                    fileService.SaveUsers(listUsers);
                    
                    return (true, "Finalizando..");
                }
                else if(typeExec == TypeExecEnum.GetUsersByProfile)
                {
                    config.ProfilesToGetUsers.ForEach(item =>
                    {
                        var users = GetFollowersByProfile(item, ConfigApp.BotConfig.QtScroll);
                        var listUsers = fileService.GetUsers();
                        listUsers.AddRange(users);
                        fileService.SaveUsers(listUsers);
                    });
                    
                    return (true, "Finalizando..");
                }
                else if(typeExec == TypeExecEnum.GetUsersByTag)
                {
                    config.TagsToGetUsers.ForEach(item =>
                    {
                        var users = GetProfileByTag(item, ConfigApp.BotConfig.QtScroll);
                        var listUsers = fileService.GetUsers();
                        listUsers.AddRange(users);
                        fileService.SaveUsers(listUsers);
                    });

                    return (true, "Finalizando..");
                }
                else if(typeExec == TypeExecEnum.LikePostsTimeLine)
                {
                    LikePostsTimeLine();
                    return (true, "Finalizando..");
                }
                else if(typeExec == TypeExecEnum.UnfollowUnfollowers)
                {
                    var unFollowing = GetUnFollowers(ConfigApp.BotConfig.Login, ConfigApp.BotConfig.QtScroll);
                    
                    fileService.SaveUnFollowers(unFollowing);

                    unFollowing.ForEach(item =>
                    {
                        Unfollow(item.NameUser);
                        item.Follow = false;
                        Thread.Sleep(GetSleepMiddle());
                    });

                    fileService.SaveUnFollowers(unFollowing);

                    return (true, "Finalizando..");
                }
                else
                {
                    throw new NotImplementedException("Tipo de execução não reconhecido");
                }
            }
            else
            {
                return tryLogin;
            }
        }
        public void LoadPage()
        {
            LogBot.Add("Iniciando carregamento da página.");
            _webDriver.Manage().Timeouts().PageLoad = _timeout;
            _webDriver.Navigate().GoToUrl($"{_initUrl}/accounts/login/");
            Thread.Sleep(GetSleepMiddle());
            LogBot.Add("Página carregada.");
        }

        public (bool, string) Login()
        {
            try
            {
                LogBot.Add("Iniciando tentativa de Login.");
                WebDriverWait wait = new WebDriverWait(_webDriver, _timeout);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Name("username")));

                IWebElement inputUsername = _webDriver.FindElement(By.Name("username"));
                inputUsername.Clear();
                inputUsername.SendKeys(_login);

                Thread.Sleep(GetSleepLow());

                IWebElement inputPassword = _webDriver.FindElement(By.Name("password"));
                inputPassword.Clear();
                inputPassword.SendKeys(_senha);

                Thread.Sleep(GetSleepLow());

                IWebElement buttonEnter = _webDriver.FindElement(By.XPath("//*[@id=\"loginForm\"]/div/div[3]/button"));
                buttonEnter.Click();

                Thread.Sleep(GetSleepMiddle());

                return (true, $"Login efetuado com o usuário {_login}");
            }
            catch(Exception ex)
            {
                return (false, $"erro ao efetuar Login com o usuário {_login}. Message: {ex.Message}");
            }
        }
        
        public List<User> GetProfileByTag(string tag, int qtScroll = 20)
        {
            LogBot.Add($"Iniciando coleta de usuários da tag {tag}. Scroll: {qtScroll}");
            _webDriver.Manage().Timeouts().PageLoad = _timeout;
            _webDriver.Navigate().GoToUrl($"{_initUrl}/explore/tags/{tag}/");

            Thread.Sleep(GetSleepMiddle());

            IJavaScriptExecutor jse = (IJavaScriptExecutor)_webDriver;
            for (int i = 0; i < qtScroll; i++)
            {
                jse.ExecuteScript("window.scrollBy(0, 10000)");
                Thread.Sleep(GetSleepMiddle());
            }

            ReadOnlyCollection<IWebElement> photoPosts = _webDriver.FindElements(By.ClassName("v1Nh3"));

            List<User> users = new List<User>();

            photoPosts.ToList().ForEach(post =>
            {
                post.Click();

                Thread.Sleep(GetSleepMiddle());

                var nameElement = _webDriver.FindElement(By.ClassName("Ppjfr"));

                var data = nameElement.GetAttribute("innerText");

                if(data != null)
                {
                    var values = data.Split("Seguir");
                    
                    users.Add(new User(values[0]
                        .Replace("•", "")
                        .Replace("\r", "")
                        .Replace("\n", "")));
                }

                IWebElement btnClose = _webDriver.FindElement(By.XPath("/html/body/div[6]/div[1]/button"));
                btnClose.Click();

                Thread.Sleep(GetSleepLow());
            });

            LogBot.Add($"Usuários coletados com sucesso. {users.Count} usuários");
            return users;
        }

        public List<User> GetFollowersByProfile(string profile, int qtScroll = 10)
        {
            LogBot.Add($"Iniciando coleta de seguidores do perfil {profile}. scroll: {qtScroll}");
            _webDriver.Manage().Timeouts().PageLoad = _timeout;
            _webDriver.Navigate().GoToUrl($"{_initUrl}/{profile}/");

            Thread.Sleep(GetSleepMiddle());

            IWebElement btnFollowers = _webDriver.FindElement(By.XPath("//*[@id=\"react-root\"]/section/main/div/header/section/ul/li[2]/a"));
            btnFollowers.Click();

            Thread.Sleep(GetSleepMiddle());

            int qtIntervalScroll = 0;
            int lastSize = -1;
            IJavaScriptExecutor jse = (IJavaScriptExecutor)_webDriver;
            for (int i = 0; i < qtScroll; i++)
            {
                jse.ExecuteScript("document.getElementsByClassName(\"isgrP\")[0].scrollBy(0, 5000)");
                Thread.Sleep(GetSleepMiddle());
                qtIntervalScroll++;

                if (qtIntervalScroll == 3)
                {
                    IWebElement we = _webDriver.FindElement(By.XPath("/html/body/div[6]/div"));
                    var p = we.GetAttribute("innerText");

                    qtIntervalScroll = 0;

                    if (p.Length == lastSize)
                        break;

                    lastSize = p.Length;
                }
            }

            IWebElement webElement = _webDriver.FindElement(By.XPath("/html/body/div[6]/div"));
            var page = webElement.GetAttribute("innerText");
            
            page = page
                .Replace("Seguidores", "")
                .Replace("studiokimdantas", "")
                .Replace("Studio Kim Dantas", "")
                .Replace("\n", "");

            var users = page.Split("\r");

            var indexFirstFollowred = users.ToList().FindIndex(x => x == "Seguir");

            if (indexFirstFollowred > 0)
                    users.ToList().RemoveRange(0, indexFirstFollowred - 2);

            users = string.Join("#####", users).Split("Seguir");
            
            List<User> listUsers = new List<User>();

            users.ToList().ForEach(item => 
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item[0] == '#')
                        item = item.Substring(5);

                    if (item[item.Length - 1] == '#')
                        item = item.Substring(0, item.Length - 5);

                    var line = item.Split("#####");

                    if (line.Length == 2)
                        listUsers.Add(new User(line[1], line[0]));
                    else
                        listUsers.Add(new User(line[0]));
                }
            });

            LogBot.Add($"Usuários coletados com sucesso. {listUsers.Count} usuários");

            return listUsers;
        }

        public (bool, string) Follow(string profile, bool isInPage = false)
        {
            try
            {
                LogBot.Add($"Iniciando tentariva de seguir o perfil {profile}");

                //if (!isInPage)
                //{
                _webDriver.Manage().Timeouts().PageLoad = _timeout;
                _webDriver.Navigate().GoToUrl($"{_initUrl}/{profile}/");

                Thread.Sleep(GetSleepMiddle());
                //}
                
                IWebElement validatePrivateAccount;
                string msg = "";
                try
                {
                    validatePrivateAccount = _webDriver.FindElement(By.ClassName("rkEop"));
                    msg = validatePrivateAccount.GetAttribute("innerText");
                }
                catch (Exception ex){}
                
                if (msg.Contains("conta é privada"))
                {
                    ReadOnlyCollection<IWebElement> btnFollow = _webDriver.FindElements(By.ClassName("sqdOP"));
                    var element = btnFollow.FirstOrDefault();
                    if (element != null)
                        element.Click();
                }
                else
                {
                    IWebElement btnFollow = _webDriver.FindElement(By.XPath("//*[@id=\"react-root\"]/section/main/div/header/section/div[1]/div[1]/div/div/div/span/span[1]/button"));
                    btnFollow.Click();
                }

                

                return (true, $"Usuário {profile} seguido com sucesso.");
            }
            catch(Exception ex)
            {
                return (false, $"Erro ao seguir o usuário {profile}. Message: {ex.Message}");
            }
        }

        public (bool, string) LikeFirstPost(string profile, bool isInPage = false)
        {
            try
            {
                LogBot.Add($"Iniciando tentativa de LIKE último post do perfil {profile}.");
                if (!isInPage)
                {
                    _webDriver.Manage().Timeouts().PageLoad = _timeout;
                    _webDriver.Navigate().GoToUrl($"{_initUrl}/{profile}/");

                    Thread.Sleep(GetSleepMiddle());
                }


                IWebElement validatePrivateAccount;
                string msg = "";
                try
                {
                    validatePrivateAccount = _webDriver.FindElement(By.ClassName("rkEop"));
                    msg = validatePrivateAccount.GetAttribute("innerText");
                }
                catch (Exception ex) { }

                if (!msg.Contains("conta é privada"))
                {
                    IWebElement firsPost = _webDriver.FindElement(By.ClassName("v1Nh3"));
                    firsPost.Click();

                    Thread.Sleep(GetSleepMiddle());

                    IWebElement btnLike = _webDriver.FindElement(By.XPath("/html/body/div[6]/div[3]/div/article/div/div[2]/div/div/div[2]/section[1]/span[1]/button"));
                    btnLike.Click();

                    return (true, "Último post do perfil {profile} curtido com sucesso.");
                }

                return (true, "A conta é privada, não é possível curtir o post.");
            }
            catch(Exception ex)
            {
                return (false, $"Erro ao curtir primeiro post do perfil {profile}. Message: {ex.Message}");
            }
        }

        public void LikePostsTimeLine()
        {
            LogBot.Add($"Iniciando tentativa de LIKE na time line.");
            _webDriver.Manage().Timeouts().PageLoad = _timeout;


            _webDriver.Navigate().GoToUrl(_initUrl);

            Thread.Sleep(GetSleepMiddle());

            for (int i = 1; i <= ConfigApp.BotConfig.QtPostsLike; i++)
            {
                IWebElement post = _webDriver.FindElement(By.XPath($"//*[@id=\"react-root\"]/section/main/section/div[1]/div[2]/div/article[{i}]/div/div[3]/div/div/section[1]/span[1]/button/div[1]/svg"));
                post.Click();

                IJavaScriptExecutor jse = (IJavaScriptExecutor)_webDriver;
                jse.ExecuteScript("window.scrollBy(0, 1100)");
                
                Thread.Sleep(GetSleepMiddle());
            }
            
            LogBot.Add($"Finalizando tentativa de LIKE na time line.");
        }

        public List<User> GetUnFollowers(string profile, int qtScroll = 50)
        {
            LogBot.Add($"Iniciando coleta de não seguidores do perfil {profile}. scroll: {qtScroll}");
            _webDriver.Manage().Timeouts().PageLoad = _timeout;
            _webDriver.Navigate().GoToUrl($"{_initUrl}/{profile}/");

            Thread.Sleep(GetSleepMiddle());

            ReadOnlyCollection<IWebElement> btns = _webDriver.FindElements(By.ClassName("g47SY"));

            btns[2].Click();
            
            Thread.Sleep(GetSleepMiddle());

            int qtIntervalScroll = 0;
            int lastSize = -1;
            IJavaScriptExecutor jse = (IJavaScriptExecutor)_webDriver;
            for (int i = 0; i < qtScroll; i++)
            {
                jse.ExecuteScript("document.getElementsByClassName(\"isgrP\")[0].scrollBy(0, 5000)");
                Thread.Sleep(GetSleepMiddle());
                qtIntervalScroll++;

                if(qtIntervalScroll == 3)
                {
                    IWebElement we = _webDriver.FindElement(By.XPath("/html/body/div[6]/div"));
                    var p = we.GetAttribute("innerText");

                    qtIntervalScroll = 0;

                    if (p.Length == lastSize)
                        break;

                    lastSize = p.Length;
                }
            }

            IWebElement webElement = _webDriver.FindElement(By.XPath("/html/body/div[6]/div"));
            var page = webElement.GetAttribute("innerText");

            page = page
                .Replace("Seguidores", "")
                .Replace("studiokimdantas", "")
                .Replace("Pessoas", "")
                .Replace("Hashtags", "")
                .Replace("\n", "");

            var users = page.Split("\r");

            var indexFirstFollowred = users.ToList().FindIndex(x => x == "Seguindo");

            if (indexFirstFollowred > 0)
                users.ToList().RemoveRange(0, indexFirstFollowred - 2);

            users = string.Join("#####", users).Split("Seguindo");

            List<User> listUsers = new List<User>();

            users.ToList().ForEach(item =>
            {
                if (!string.IsNullOrEmpty(item))
                {
                    if (item[0] == '#')
                        item = item.Substring(5);

                    if (item[item.Length - 1] == '#')
                        item = item.Substring(0, item.Length - 5);

                    var line = item.Split("#####");

                    if (line.Length == 2)
                        listUsers.Add(new User(line[1], line[0]));
                    else if(line.Length == 4 && string.IsNullOrEmpty(line[0]))
                        listUsers.Add(new User(line[3], line[2]));
                    else
                        listUsers.Add(new User(line[0]));
                }
            });

            var followers = GetFollowers(profile, qtScroll);

            List<User> unFollowings = new List<User>();

            listUsers.ForEach(item =>
            {
                if (!followers.Contains(item.NameUser))
                {
                    item.Follow = true;
                    unFollowings.Add(item);
                }
            });

            LogBot.Add($"Usuários coletados com sucesso. {listUsers.Count} usuários");

            return unFollowings;
        }

        public (bool, string) Unfollow(string profile, bool isInPage = false)
        {
            try
            {
                LogBot.Add($"Iniciando tentariva de seguir o perfil {profile}");

                _webDriver.Manage().Timeouts().PageLoad = _timeout;
                _webDriver.Navigate().GoToUrl($"{_initUrl}/{profile}/");

                Thread.Sleep(GetSleepMiddle());
                
                IWebElement validatePrivateAccount;
                string msg = "";
                try
                {
                    validatePrivateAccount = _webDriver.FindElement(By.ClassName("rkEop"));
                    msg = validatePrivateAccount.GetAttribute("innerText");
                }
                catch (Exception ex) { }

                if (msg.Contains("conta é privada"))
                {
                    ReadOnlyCollection<IWebElement> btnFollow = _webDriver.FindElements(By.ClassName("sqdOP"));
                    var element = btnFollow.FirstOrDefault();
                    if (element != null)
                        element.Click();
                }
                else
                {
                    IWebElement btnFollow = _webDriver.FindElement(By.XPath("//*[@id=\"react-root\"]/section/main/div/header/section/div[1]/div[1]/div/div/div/span/span[1]/button"));
                    btnFollow.Click();
                }

                Thread.Sleep(GetSleepLow());
                
                ReadOnlyCollection<IWebElement> btnActions = _webDriver.FindElements(By.ClassName("aOOlW"));

                btnActions[0].Click();

                return (true, $"Usuário {profile} seguido com sucesso.");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao seguir o usuário {profile}. Message: {ex.Message}");
            }
        }

        private string GetFollowers(string profile, int qtScroll = 50)
        {
            LogBot.Add($"Iniciando coleta de não seguidores do perfil {profile}. scroll: {qtScroll}");
            _webDriver.Manage().Timeouts().PageLoad = _timeout;
            _webDriver.Navigate().GoToUrl($"{_initUrl}/{profile}/");

            Thread.Sleep(GetSleepMiddle());

            ReadOnlyCollection<IWebElement> btns = _webDriver.FindElements(By.ClassName("g47SY"));

            btns[1].Click();

            Thread.Sleep(GetSleepMiddle());

            int qtIntervalScroll = 0;
            int lastSize = -1;
            IJavaScriptExecutor jse = (IJavaScriptExecutor)_webDriver;
            for (int i = 0; i < qtScroll; i++)
            {
                jse.ExecuteScript("document.getElementsByClassName(\"isgrP\")[0].scrollBy(0, 5000)");
                Thread.Sleep(GetSleepMiddle());
                qtIntervalScroll++;

                if (qtIntervalScroll == 3)
                {
                    IWebElement we = _webDriver.FindElement(By.XPath("/html/body/div[6]/div"));
                    var p = we.GetAttribute("innerText");

                    qtIntervalScroll = 0;

                    if (p.Length == lastSize)
                        break;

                    lastSize = p.Length;
                }
            }

            IWebElement webElement = _webDriver.FindElement(By.XPath("/html/body/div[6]/div"));
            var page = webElement.GetAttribute("innerText");

            LogBot.Add($"Usuários coletados com sucesso.");

            return page;
        }
    }
}

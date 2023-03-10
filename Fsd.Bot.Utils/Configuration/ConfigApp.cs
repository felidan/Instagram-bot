using Microsoft.Extensions.Configuration;
using System.IO;

namespace Fsd.Bot.Utils.Configuration
{
    public static class ConfigApp
    {
        public static IConfiguration Configuration { get
            {
                return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();
            }
        }

        public static Driver Driver => Configuration.GetSection(nameof(Driver)).Get<Driver>();
        public static Logger Log => Configuration.GetSection(nameof(Log)).Get<Logger>();
        public static BotConfig BotConfig => Configuration.GetSection(nameof(BotConfig)).Get<BotConfig>();
        public static Sleep Sleep => Configuration.GetSection(nameof(Sleep)).Get<Sleep>();
        public static FilesPath FilesPath => Configuration.GetSection(nameof(FilesPath)).Get<FilesPath>();
        
    }

    public class Driver
    {
        public string Path { get; set; }
        public string pathChromeMachine { get; set; }
        public bool Headless { get; set; }
    }

    public class Logger
    {
        public string Path { get; set; }
    }

    public class BotConfig
    {
        public int Timeout { get; set; }
        public int QtFollowHour { get; set; }
        public int QtPostsLike { get; set; }
        public string Login { get; set; }
        public string Pass { get; set; }
        public int QtScroll { get; set; }
    }

    public class Sleep
    {
        public int Low { get; set; }
        public int Middle { get; set; }
        public int Coef { get; set; }
    }

    public class FilesPath
    {
        public string UsersToFollow { get; set; }
        public string UsersToFollowHist { get; set; }
        public string DataConfigBot { get; set; }
        public string UsersUnFollowers { get; set; }
    }
}

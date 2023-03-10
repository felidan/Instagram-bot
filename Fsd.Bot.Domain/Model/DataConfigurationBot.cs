using System.Collections.Generic;

namespace Fsd.Bot.Domain.Model
{
    public class DataConfigurationBot
    {
        public List<string> ProfilesToGetUsers { get; set; } = new List<string>();
        public List<string> TagsToGetUsers { get; set; } = new List<string>();
    }
}

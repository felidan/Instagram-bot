using Fsd.Bot.Domain.Model;
using System.Collections.Generic;

namespace Fsd.Bot.Domain.Interfaces.Service
{
    public interface IInstagramBotService
    {
        void LoadPage();
        (bool, string) Login();
        List<User> GetProfileByTag(string tag, int qtScroll = 20);
        List<User> GetFollowersByProfile(string profile, int qtScroll = 10);
        (bool, string) Follow(string profile, bool isInPage = false);
        (bool, string) LikeFirstPost(string profile, bool isInPage = false);
        void LikePostsTimeLine();
        List<User> GetUnFollowers(string profile, int qtScroll = 50);
        (bool, string) Unfollow(string profile, bool isInPage = false);
    }
}

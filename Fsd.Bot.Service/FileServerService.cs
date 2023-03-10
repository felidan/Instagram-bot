using Fsd.Bot.Domain.Model;
using Fsd.Bot.Utils.Configuration;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace Fsd.Bot.Service
{
    public class FileServerService
    {
        public List<User> GetUsers()
        {
            using (var stream = new StreamReader(ConfigApp.FilesPath.UsersToFollow))
            {
                var result = stream.ReadToEnd();

                if (string.IsNullOrEmpty(result))
                    return new List<User>();

                return JsonSerializer.Deserialize<List<User>>(result);
            }
        }

        public List<User> GetUnFollowers()
        {
            using (var stream = new StreamReader(ConfigApp.FilesPath.UsersUnFollowers))
            {
                var result = stream.ReadToEnd();

                if (string.IsNullOrEmpty(result))
                    return new List<User>();

                return JsonSerializer.Deserialize<List<User>>(result);
            }
        }

        public List<User> GetUsersHist()
        {
            using (var stream = new StreamReader(ConfigApp.FilesPath.UsersToFollowHist))
            {
                var result = stream.ReadToEnd();

                if (string.IsNullOrEmpty(result))
                    return new List<User>();

                return JsonSerializer.Deserialize<List<User>>(result);
            }
        }

        public void SaveUnFollowers(List<User> users)
        {
            using (var stream = new StreamWriter(ConfigApp.FilesPath.UsersUnFollowers))
            {
                var json = JsonSerializer.Serialize(users);
                stream.Write(json);
            }
        }

        public void SaveUsers(List<User> users)
        {
            List<User> userHist = GetUsersHist();
            List<User> usersProcess = new List<User>();

            users.ForEach(u =>
            {
                if (u.Follow)
                    userHist.Add(u);
                else
                    usersProcess.Add(u);
            });

            using (var stream = new StreamWriter(ConfigApp.FilesPath.UsersToFollow))
            {
                var json = JsonSerializer.Serialize(usersProcess);
                stream.Write(json);
            }

            using (var stream = new StreamWriter(ConfigApp.FilesPath.UsersToFollowHist))
            {
                var json = JsonSerializer.Serialize(userHist);
                stream.Write(json);
            }
        }

        public DataConfigurationBot GetConfig()
        {
            using (var stream = new StreamReader(ConfigApp.FilesPath.DataConfigBot))
            {
                var result = stream.ReadToEnd();

                if (string.IsNullOrEmpty(result))
                    return new DataConfigurationBot();

                return JsonSerializer.Deserialize<DataConfigurationBot>(result);
            }
        }

        public void SaveConfig(DataConfigurationBot data)
        {
            using (var stream = new StreamWriter(ConfigApp.FilesPath.DataConfigBot))
            {
                var json = JsonSerializer.Serialize(data);
                stream.Write(json);
            }
        }
    }
}

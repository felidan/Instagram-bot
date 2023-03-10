namespace Fsd.Bot.Domain.Model
{
    public class User
    {
        public User(string name, string user)
        {
            NameUser = user;
            Name = name;
        }

        public User(string user)
        {
            NameUser = user;
        }

        public User()
        {

        }

        public string Name { get; set; }
        public string NameUser { get; set; }
        public bool Follow { get; set; }
    }
}

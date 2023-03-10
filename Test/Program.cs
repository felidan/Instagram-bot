using Fsd.Bot.Service;
using Fsd.Bot.Utils.Logger;
using System;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            LogBot.GenerateLogFile(true);

            LogBot.Add("---------------------- INICIO ----------------------");

            PrintMenu();

            string typeExec = "";
            var option = Console.ReadLine();
            
            switch (option)
            {
                case "0":
                    PrintInfo();
                    break;
                case "1":
                    typeExec = "GetUsersByProfile";
                    break;
                case "2":
                    typeExec = "GetUsersByTag";
                    break;
                case "3":
                    typeExec = "FollowProfileAndLike";
                    break;
                case "4":
                    typeExec = "LikePostsTimeLine";
                    break;
                case "5":
                    typeExec = "UnfollowUnfollowers";
                    break;
                default:
                    Console.WriteLine("Opção incorreta.");
                    typeExec = "XXX";
                    break;
            }

            //if (args.Length == 0)
            //    typeExec = "GetUsersByProfile";
            //else
            //    typeExec = args[0];

            (bool, string) result = (false, "Not Init");
            
            if (typeExec == "GetUsersByProfile")
            {
                InstagramBotService instagramBotService = new InstagramBotService();
                result = instagramBotService.Init(Fsd.Bot.Utils.Enum.TypeExecEnum.GetUsersByProfile);
            }
            else if (typeExec == "GetUsersByTag")
            {
                InstagramBotService instagramBotService = new InstagramBotService();
                result = instagramBotService.Init(Fsd.Bot.Utils.Enum.TypeExecEnum.GetUsersByTag);
            }
            else if (typeExec == "FollowProfileAndLike")
            {
                InstagramBotService instagramBotService = new InstagramBotService();
                result = instagramBotService.Init(Fsd.Bot.Utils.Enum.TypeExecEnum.FollowProfileAndLike);
            }
            else if (typeExec == "LikePostsTimeLine")
            {
                InstagramBotService instagramBotService = new InstagramBotService();
                result = instagramBotService.Init(Fsd.Bot.Utils.Enum.TypeExecEnum.LikePostsTimeLine);
            }
            else if (typeExec == "UnfollowUnfollowers")
            {
                InstagramBotService instagramBotService = new InstagramBotService();
                result = instagramBotService.Init(Fsd.Bot.Utils.Enum.TypeExecEnum.UnfollowUnfollowers);
            }
            else if (typeExec == "test")
                LogBot.Add("LOG TESTE EXEC");

            LogBot.Add(result.Item2);

            LogBot.Add("---------------------- FIM ----------------------");
        }

        private static void PrintMenu()
        {
            Console.WriteLine("OPÇÕES:");
            Console.WriteLine("\t 0 => Consultar informações");
            Console.WriteLine("\t 1 => Buscar usuários por perfil");
            Console.WriteLine("\t 2 => Buscar usuários por Tag");
            Console.WriteLine("\t 3 => Seguir lista de usuários e curtir primeira foto");
            Console.WriteLine("\t X => Curtir Posts na Time Line (NÃO IMPLEMENTADO)");
            Console.WriteLine("\t 5 => Deixar de seguir não seguidores");
            Console.WriteLine("");
            Console.Write("Digite a opção desejada: ");
        }

        private static void PrintInfo()
        {
            FileServerService fileService = new FileServerService();

            var users = fileService.GetUsers();
            var usersHist = fileService.GetUsersHist();
            var config = fileService.GetConfig();

            Console.WriteLine("");
            Console.WriteLine($"Quantidade de usuários para Seguir: {users.Count}");
            Console.WriteLine($"Quantidade de usuários seguidos: {usersHist.Count}");

            Console.WriteLine($"Perfis de coleta de usuários: ");

            config.ProfilesToGetUsers.ForEach(x =>
            {
                Console.WriteLine($"\t {x}");
            });

            Console.WriteLine($"Tags de coleta de usuários: ");

            config.TagsToGetUsers.ForEach(x =>
            {
                Console.WriteLine($"\t {x}");
            });

            Console.WriteLine("");
            Console.WriteLine("Precione qualquer tecla para continuar...");
            Console.WriteLine("");
            Console.ReadKey();
        }
    }
}

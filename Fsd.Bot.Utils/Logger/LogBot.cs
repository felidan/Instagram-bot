using Fsd.Bot.Utils.Configuration;
using Fsd.Bot.Utils.Entity;
using Fsd.Bot.Utils.Enum;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Fsd.Bot.Utils.Logger
{
    public static class LogBot
    {
        private static List<Log> Logs = new List<Log>();
        private static readonly string Url = ConfigApp.Log.Path;
        private static bool GenarateLogFile = false;

        public static void GenerateLogFile(bool flag)
        {
            GenarateLogFile = flag;
        }

        public static List<Log> GetLogs()
        {
            return Logs;
        }

        public static List<Log> GetLogs(LevelLogEnum level)
        {
            return Logs.Where(x => x.Level == level).ToList();
        }

        public static void ClearLogs()
        {
            Logs.Clear();
        }

        public static void Add(Log res)
        {
            Logs.Add(res);
            Console.WriteLine(GetMessage(res));

            if (GenarateLogFile)
            {
                GenarateFileLog(res, true);
            }
        }

        public static void Add(string msg)
        {
            var log = new Log(msg);
            Logs.Add(log);
            Console.WriteLine(GetMessage(log));

            if (GenarateLogFile)
            {
                GenarateFileLog(log, true);
            }
        }

        public static bool HasErros()
        {
            return Logs.Any(x => x.Level == LevelLogEnum.Erro);
        }

        private static void GenarateFileLog(Log response, bool append)
        {
            using (StreamWriter file = new StreamWriter(Url, append))
            {
                file.WriteLine(GetMessage(response));
            }
        }

        private static string GetMessage(Log response)
        {
            var dataHora = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();
            var error = response.Error == null ? "" : $" | Error: {response.Error.StackTrace.ToString()}";
            return $"[{dataHora}] {response.Level.ToString()} => Message: {response.Message}{error}";
        
        }
    }
}

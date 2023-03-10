using Fsd.Bot.Utils.Enum;
using System;

namespace Fsd.Bot.Utils.Entity
{
    public class Log
    {
        public Log(string msg, LevelLogEnum level, Exception error = null)
        {
            Ok = level == LevelLogEnum.Info;
            Message = msg;
            Error = error;
            Level = level;
        }

        public Log(string msg)
        {
            Ok = true;
            Message = msg;
            Error = null;
            Level = LevelLogEnum.Info;
        }

        public bool Ok { get; set; }
        public string Message { get; set; }
        public Exception Error { get; set; }
        public LevelLogEnum Level { get; set; }
    }
}

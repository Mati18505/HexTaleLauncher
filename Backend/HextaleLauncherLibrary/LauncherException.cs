using System;

namespace HexTaleLauncherLibrary
{
    public class LauncherException : Exception
    {
        public enum Type { General, InvalidOperation, BadConfig };
        public Type type { get; }

        public LauncherException(Type type) : base($"Type: {type}")
        { 
            this.type = type;
        }
        public LauncherException(Type type, string message) : base($"Type: {type}\n{message}")
        { 
            this.type = type;
        }
        public LauncherException(Type type, string message, Exception inner) : base($"Type: {type}\n{message}", inner)
        {
            this.type = type;
        }
    }

    //General - application error - the user cannot do anything about it
    //InvalidOperation - operation not allowed at the time e.g. CheckForUpdates while game is downloading
    //BadConfig - user could repair this
}
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HexTaleLauncherLibrary
{
    public class Logger
    {

        public Logger()
        {}

        public void WriteLine(string message)
        {
            API.Info(message);
        }
    }
}

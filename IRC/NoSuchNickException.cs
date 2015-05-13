using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AnonymousFerretTwitchLogger.IRC
{
    class NoSuchNickException : Exception
    {
        private string p;

        public NoSuchNickException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }
    }
}

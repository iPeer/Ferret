using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnonymousFerretTwitchLogger.IRC
{
    class NoSuchChannelException : Exception
    {
        private string p;

        public NoSuchChannelException(string p)
        {
            // TODO: Complete member initialization
            this.p = p;
        }

        public NoSuchChannelException()
        {
            // TODO: Complete member initialization
        }
    }
}

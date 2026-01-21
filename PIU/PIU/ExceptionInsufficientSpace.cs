using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PIU
{
    internal class ExceptionInsufficientSpace :Exception
    {
        public ExceptionInsufficientSpace()
        {
        }

        public ExceptionInsufficientSpace(string message)
            : base(message)
        {
        }


    }
}

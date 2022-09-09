using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wb_star_bot.Wb_handler
{
    internal class WbException : Exception
    {
        public WbException(ExceptionType exception) : base(exception.ToString()) { exceptionType = exception; }

        public ExceptionType exceptionType;

        public enum ExceptionType : uint
        {
            data_bad_request = 0,
            data_too_many_request = 1,
        }
    }
}

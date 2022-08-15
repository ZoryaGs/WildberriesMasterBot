namespace Wildberries_master_bot
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

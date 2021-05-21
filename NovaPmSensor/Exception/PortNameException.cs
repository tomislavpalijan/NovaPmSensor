

namespace NovaPmSensor.Exception
{
    public class PortNameException : System.Exception
    {
        public override string Message { get; }

        public PortNameException(string message)
        {
            Message = message;
        }
    }
}
using Xunit.Sdk;

namespace BtcTransmuter.Tests.Base
{
    public class Assert: Xunit.Assert
    {
        public static void NullOrEmpty(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            
            throw new NotNullException();
            
        }
        
        public static void NotNullOrEmpty(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return;
            }
            
            throw new NullException(value);
            
        }
    }
}
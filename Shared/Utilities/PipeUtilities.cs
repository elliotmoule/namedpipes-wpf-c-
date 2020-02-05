using System;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Utilities
{
    public static class PipeUtilities
    {
        public static void SendPipedMessage(StreamString streamString, string message)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    if (streamString != null)
                    {
                        streamString.WriteString(message);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

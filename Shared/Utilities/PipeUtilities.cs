using System;
using System.IO;
using System.Threading.Tasks;

namespace Shared.Utilities
{
    public static class PipeUtilities
    {
        public static void SendPipedMessage(StreamWriter writer, string message)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(message))
                {
                    Task.Factory.StartNew(async () =>
                    {
                        if (writer != null)
                        {
                            await writer.WriteLineAsync(message);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

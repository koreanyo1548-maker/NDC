using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OPS.Editor.IO
{
    /// <summary>
    /// Internal path helper for the obfuscator.
    /// </summary>
    public static class FileHelper
    {
        /// <summary>
        /// Move a File _From _To with _Retries if not successfully at first.
        /// </summary>
        /// <param name="_From"></param>
        /// <param name="_To"></param>
        /// <param name="_Retries"></param>
        /// <returns></returns>
        public static bool TryToMove(String _From, String _To, int _Retries = 40)
        {
            if (!System.IO.File.Exists(_From) && System.IO.File.Exists(_To))
            {
                return true;
            }

            int var_RetryCount = _Retries;
            while (var_RetryCount > 0)
            {
                try
                {
                    System.IO.File.Move(_From, _To);
                    break;
                }
#pragma warning disable
                catch (Exception e)
                {
                    System.Threading.Thread.Sleep(300);

                    var_RetryCount -= 1;

                    if (var_RetryCount <= 0)
                    {
                        return false;
                    }
                }
#pragma warning restore
            }

            return true;
        }
    }
}

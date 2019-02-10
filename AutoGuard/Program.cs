using GuerrillaNtp;
using System;
using System.Net;
using System.Runtime.InteropServices;

namespace AutoGuard
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SystemTime
    {
        public short Year;
        public short Month;
        public short DayOfWeek;
        public short Day;
        public short Hour;
        public short Minute;
        public short Second;
        public short Milliseconds;
    }

    internal class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetLocalTime([In] ref SystemTime st);
        
        private static void Main()
        {
            Console.Title = "Auto Guard - by Logxn";

            if(!SyncWindowsTime())
            {
                throw new Exception($"Unhandled Error When Setting SystemTime: 0x{Marshal.GetLastWin32Error():X}");
            }

#if DEBUG
            Console.WriteLine("[SyncWindowsTime] -> Succeeded");
#endif

            while (true) {}
        }

        //private static string GenerateConfirmationKey(string secret, long time, string tag)
        //{
        //    var identitySecret = Utilities.BytesFromSecret(secret);

        //    int dataLength = 8;
        //    if(!string.IsNullOrWhiteSpace(tag))
        //    {
        //        if (tag.Length > 32)
        //            dataLength += 32;
        //        else
        //            dataLength += tag.Length;
        //    }

        //    byte[] buff = new byte[dataLength];
        //    int len = 8;
        //    while(true)
        //    {
        //        int newLen = len - 1;
        //        if (len <= 0) break;

        //        buff[newLen] = (byte)time;
        //        time >>= 8;
        //        len = newLen;
        //    }

        //    if(!string.IsNullOrWhiteSpace(tag))
        //        Array.Copy(Encoding.UTF8.GetBytes(tag), 0, buff, 8, dataLength - 8);

        //    try
        //    {
        //        HMACSHA1 hmacGenerator = new HMACSHA1
        //        {
        //            Key = identitySecret
        //        };

        //        byte[] hashedData = hmacGenerator.ComputeHash(buff);
        //        string encodedData = Convert.ToBase64String(hashedData, Base64FormattingOptions.None);
        //        string hash = WebUtility.UrlEncode(encodedData);

        //        return hash;
        //    }
        //    catch(Exception e)
        //    {
        //        throw new Exception($"Unhandled Exception While Generating Confirmation Key => {e}");
        //    }
        //}

        //private static string GenerateConfirmationUrl(string tag = "conf")
        //{
        //    var endpoint = "https://steamcommunity.com" + "/mobileconf/conf?";
        //    var queryStr = GenerateConfirmationKey("cg4LJoNrW2GWOLYIHrgahCWUd3E=", DateTimeOffset.Now.ToUnixTimeMilliseconds(), tag);
        //    return endpoint + queryStr;
        //}

        /// <summary>
        /// Fetches the time from time.windows.com via NTP
        /// </summary>
        /// <returns>
        /// The current time in DateTime format
        /// </returns>
        private static DateTime GetNetworkTime()
        {
            TimeSpan offsetSpan;

            try
            {
                // Initiates the NTP client with the given NTP server and receives correction offsets
                NtpClient ntpClient = new NtpClient(Dns.GetHostAddresses("time.windows.com")[0]);
                offsetSpan = ntpClient.GetCorrectionOffset();
            }
            catch (Exception)
            {
                // The request timed out or the response was not what we expected
                offsetSpan = TimeSpan.Zero;
            }

            return DateTime.Now + offsetSpan;
        }

        /// <summary>
        /// Tries to modify the local user's time
        /// </summary>
        /// <returns>
        /// true if succeeded
        /// false if windows error code is thrown
        /// </returns>
        private static bool SyncWindowsTime()
        {
            while (true)
            {
                var cDate = GetNetworkTime();
                var sDate = DateTime.Now.ToShortTimeString();

                if (cDate.ToShortTimeString() == sDate) return true;

#if DEBUG
                Console.WriteLine($"[SyncWindowsTime] -> Correct Date: {cDate} | Sys Date: {sDate}");
#endif

                var sysTime = new SystemTime
                {
                    Year = (short) cDate.Year,
                    Day = (short) cDate.Day,
                    DayOfWeek = (short) cDate.DayOfWeek,
                    Hour = (short) cDate.Hour,
                    Milliseconds = (short) cDate.Millisecond,
                    Minute = (short) cDate.Minute,
                    Month = (short) cDate.Month,
                    Second = (short) cDate.Second
                };

                // We return false if windows throws an error code
                if (!SetLocalTime(ref sysTime)) return false;
            }
        }
    }
}

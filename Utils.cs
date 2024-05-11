using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TonSdk.Core;

namespace TONWallet
{
    public class Utils
    {
        public static int API_RATELIMIT = 1100; // 1.1 second
        public static Random random = new Random();

        public static byte[] hexBA(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        public static string BAhex(byte[] ba) => BitConverter.ToString(ba).ToLower().Replace("-", "");

        public static string Input(string v)
        {
            Console.Write($">{v}: "); return Console.ReadLine();
        }

        public static void YieldReturn()
        {
            Console.WriteLine("Press ENTER to continue..."); Console.ReadLine();
        }

        public static string sha256_s(string v)
        {
            using (var s = SHA256.Create())
            {
                return BAhex(s.ComputeHash(Encoding.UTF8.GetBytes(v)));
            }
        }

        public static byte[] sha256_v(string v)
        {
            using (var s = SHA256.Create())
            {
                return s.ComputeHash(Encoding.UTF8.GetBytes(v));
            }
        }

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static string MiniAddress(Address addr)
        {
            var str = addr.ToString();
            return str.Substring(0, 6) + "..." + str.Substring(str.Length - 9, 8);
        }

        public static string Time(long time)
        {
            var dto = DateTimeOffset.FromUnixTimeSeconds(time);
            return dto.ToString("G");
        }
    }
}

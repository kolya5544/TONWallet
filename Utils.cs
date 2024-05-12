using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using TonSdk.Client;
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

        public static async Task PrintTXs(List<TransactionsInformationResult> txs, int count = 5)
        {
            for (int i = 0; i < Math.Min(count, txs.Count); i++)
            {
                var tx = txs[i];

                bool isIngoing = !tx.OutMsgs.Any();

                // classify the transaction
                var classify = await TransactionClassifier.ClassifyTXAsync(tx);
                if (classify.Item1 == TransactionType.FAKE)
                {
                    txs.RemoveAt(i); i--; continue;
                }
                Console.Write($"[{i + 1}] ");

                switch (classify.Item1)
                {
                    case TransactionType.SIMPLE_TX:
                        Console.Write($"Got {classify.Item2} TON from {MiniAddress(tx.InMsg.Source)}"); break;
                    case TransactionType.COMMENTED_TX:
                        Console.Write($"Got {classify.Item2} TON from {MiniAddress(tx.InMsg.Source)} with comment: '{classify.Item3}'"); break;
                    case TransactionType.SIMPLE_JETTON_TRANSFER:
                        Console.Write($"Got {classify.Item2} Jetton from {MiniAddress(tx.InMsg.Source)}"); break;
                    case TransactionType.COMMENTED_JETTON_TRANSFER:
                        Console.Write($"Got {classify.Item2} Jetton from {MiniAddress(tx.InMsg.Source)} with comment: '{classify.Item3}'"); break;
                    case TransactionType.SIMPLE_NFT_TRANSFER:
                        Console.Write($"Got an NFT #{classify.Item2} Item {MiniAddress(tx.InMsg.Source)}"); break;
                    case TransactionType.COMMENTED_NFT_TRANSFER:
                        Console.Write($"Got an NFT #{classify.Item2} Item {MiniAddress(tx.InMsg.Source)} with comment: '{classify.Item3}'"); break;
                    case TransactionType.UNKNOWN:
                        Console.Write($"Unknown {(isIngoing ? "INGOING" : "OUTGOING")} transaction"); break;
                }

                Console.Write($" @ {Time(tx.Utime)}\r\n");

                Thread.Sleep(API_RATELIMIT);
            }
        }
    }
}

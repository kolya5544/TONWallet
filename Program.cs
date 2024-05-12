using System.Drawing;
using System.Net.Sockets;
using System.Net;
using TonSdk.Client;
using TonSdk.Core.Crypto;
using TonSdk.Contracts.Wallet;
using TonSdk.Core;
using TonSdk.Core.Boc;
using TonSdk.Core.Block;
using System.Globalization;
using TONWallet.ConfigManager;
using static TONWallet.Utils;
using TONWallet.ActionSequence;

namespace TONWallet
{
    internal class Program
    {
        public static Config cfg = Config.Load("config.json");
        public static TonClient client;
        public static byte[] realMK = new byte[0];

        static async Task Main(string[] args)
        {
            if (cfg.MasterPwdSHA256 is null)
            {
                await RunSequence(typeof(InitWalletSequence)); 
            } else
            {
                await RunSequence(typeof(RequestMasterKeySequence));
            }

            // create http parameters for ton client 
            HttpParameters tonClientParams = new HttpParameters
            {
                Endpoint = "https://toncenter.com/api/v2/jsonRPC",
                ApiKey = cfg.TONAPIKey
            };

            // create ton client to fetch data
            client = new TonClient(TonClientType.HTTP_TONCENTERAPIV2, tonClientParams);

            await RunSequence(typeof(MainMenuSequence));
        }

        public static async Task RunSequence(Type sequenceType, object[] args = null)
        {
            object instance = ReflectionHelper.CreateInstance(sequenceType, args);
            await ReflectionHelper.InvokeRunMethod(instance);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonSdk.Contracts.Wallet;
using TonSdk.Core.Crypto;

namespace TONWallet.Wallet
{
    public class WalletGenerator
    {
        public static WalletV4 CreateWallet(string[] mnemo)
        {
            var seed = Mnemonic.GenerateSeed(mnemo);
            return CreateWallet(seed);
        }

        public static WalletV4 CreateWallet(byte[] seed)
        {
            var pair = Mnemonic.GenerateKeyPair(seed);

            WalletV4Options options = new WalletV4Options()
            {
                PublicKey = pair.PublicKey
            };
            WalletV4 wallet = new WalletV4(options, 2);
            wallet.Address.IsBounceable(false);

            return wallet;
        }
    }
}

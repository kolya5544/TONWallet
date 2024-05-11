using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonSdk.Contracts.Wallet;
using TonSdk.Core;
using TonSdk.Core.Crypto;
using TONWallet.Cryptography;
using TONWallet.JSONAPI;
using TONWallet.Wallet;

namespace TONWallet.ConfigManager
{
    public enum WalletSource
    {
        MNEMONIC_PHRASE, // 24 words
        SEED,            // 32 bytes
        HARDWARE,        // special file stored on an USB with either mnemonic phrase or seed
        NONE             // wallet doesn't exist
    }
    public class ConfigWallet
    {
        public WalletSource Source { get; set; }
        [JsonIgnore]
        public WalletSource TransformedSource { get; set; }
        public byte[] EncryptedContents { get; set; }

        public byte[] GetDecrypted() => AESManager.Decrypt(EncryptedContents, Utils.sha256_v(Program.cfg.MasterPwdSHA512));

        public void EncryptContent(byte[] input) => EncryptedContents = AESManager.Encrypt(input, Utils.sha256_v(Program.cfg.MasterPwdSHA512));

        [JsonIgnore]
        public byte[]? Seed
        {
            get
            {
                if (Program.cfg.MasterPwdSHA512 is null) return null;

                var c = GetDecrypted();

                if (Source == WalletSource.MNEMONIC_PHRASE)
                {
                    var ph = GetMnemonicPhrase();
                    return Mnemonic.GenerateSeed(ph);
                } else
                {
                    return GetSeed();
                }
            }
        }

        [JsonIgnore]
        private WalletV4 _Wallet { get; set; }

        [JsonIgnore]
        public WalletV4? Wallet
        {
            get
            {
                if (_Wallet is not null) return _Wallet;
                if (Program.cfg.MasterPwdSHA512 is null) return null;

                _Wallet = WalletGenerator.CreateWallet(Seed);
                return _Wallet;
            }
        }

        public async Task<decimal> GetWalletBalance()
        {
            var coin = await Program.client.GetBalance(Wallet.Address);
            Thread.Sleep(Utils.API_RATELIMIT);
            return coin.ToDecimal();
            /*var dict = new Dictionary<string, string>()
            {
                ["address"] = Wallet.Address.ToString(),
            };
            var balresp = await RequestManager.GETRequestAsync<GetAddressBalanceResponse>("/getAddressBalance", dict);
            if (balresp.Ok) return long.Parse(balresp.Result) / 1e9d;
            return 0;*/
        }

        public void Init()
        {
            if (Source == WalletSource.HARDWARE)
            {
                string wallet = HardwareManager.GetWalletPath();
                if (wallet is null) return;

                ConfigWallet cwl = JsonConvert.DeserializeObject<ConfigWallet>(File.ReadAllText(wallet));
                this.TransformedSource = cwl.Source;
                this.EncryptedContents = cwl.EncryptedContents;
            }
        }

        public string[] GetMnemonicPhrase()
        {
            if (TransformedSource != WalletSource.MNEMONIC_PHRASE) throw new Exception("Incorrect source.");

            var decrypted = GetDecrypted();
            var commaSeparatedString = Encoding.UTF8.GetString(decrypted);
            var phrase = commaSeparatedString.Split(',');

            return phrase;
        }

        public byte[] GetSeed()
        {
            if (TransformedSource != WalletSource.SEED) throw new Exception("Incorrect source.");

            return GetDecrypted();
        }
    }
}

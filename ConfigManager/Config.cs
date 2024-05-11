using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TONWallet.ConfigManager
{
    public class Config : ConfigBase<Config>
    {
        public string MasterPwdSHA512 { get; set; } = null;
        public string TONAPIKey { get; set; } = null;

        public List<ConfigWallet> wallets { get; set; } = new List<ConfigWallet>();

        public void AddWallet(string[] mnemo)
        {
            string mnemoFull = string.Join(',', mnemo);
            var walletCfg = new ConfigWallet()
            {
                Source = WalletSource.MNEMONIC_PHRASE
            };
            walletCfg.EncryptContent(Encoding.UTF8.GetBytes(mnemoFull));

            this.wallets.Add(walletCfg);
            this.Save();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TONWallet.Utils;

namespace TONWallet.ActionSequence
{
    public class RequestMasterKeySequence : IActionSequence
    {
        public async Task Run()
        {
            while (true)
            {
                Console.WriteLine("Please, enter your masterkey to access the wallet");
                string mk = Input("Masterkey");
                var hash = sha256_s(mk);
                Program.realMK = sha256_v(mk);

                if (Program.cfg.MasterPwdSHA256 == hash)
                {
                    // init all wallets
                    Program.cfg.wallets.ForEach((z) => z.Init());
                    break;
                }
                Console.WriteLine($"Incorrect masterkey!");
            }
        }
    }
}

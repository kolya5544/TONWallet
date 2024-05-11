using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TONWallet.Utils;

namespace TONWallet.ActionSequence
{
    public class MainMenuSequence : IActionSequence
    {
        public async Task Run()
        {
            //throw new NotImplementedException();
            while (true) {
                Console.Clear();
                Console.WriteLine("===================");
                Console.WriteLine("==> Wallet List <==");
                Console.WriteLine("===================");

                Console.WriteLine();

                if (Program.cfg.wallets.Count == 0)
                {
                    Console.WriteLine("You don't have a single wallet! Do you want to add one?");
                    Console.WriteLine();
                    string inp = Input("Add a new wallet? (Y/n)");
                    if (inp.Equals("n", StringComparison.OrdinalIgnoreCase)) continue;
                    await Program.RunSequence(typeof(AddNewWalletSequence));
                } else
                {
                    Console.WriteLine($"You have a total of {Program.cfg.wallets.Count} wallets!");
                    Console.WriteLine();
                    for (int i = 0; i < Program.cfg.wallets.Count; i++)
                    {
                        var w = Program.cfg.wallets[i];
                        
                        Console.WriteLine($"[{i + 1}] A: {w.Wallet.Address} | Balance: {await w.GetWalletBalance()} TON");
                    }
                    Console.WriteLine();
                    Console.WriteLine($"Choose a wallet (from 1 to {Program.cfg.wallets.Count}) or enter 'new' to add a new one.");
                    string inp = Input($"Wallet ID (from 1 to {Program.cfg.wallets.Count}) or 'new'");
                    
                    if (inp.Equals("new", StringComparison.OrdinalIgnoreCase)) {
                        await Program.RunSequence(typeof(AddNewWalletSequence));
                    } else
                    {
                        // check if corresponds to a proper wallet ID
                        int walletID = -1;
                        bool success = int.TryParse(inp, out walletID);

                        if (!success) continue;

                        var wallet = Program.cfg.wallets[walletID - 1];
                        await Program.RunSequence(typeof(ManageWalletSequence), [wallet]);
                    }
                }
            }
        }
    }
}

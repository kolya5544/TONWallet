
using System.Diagnostics;
using System.Runtime.InteropServices;
using TonSdk.Client;
using TonSdk.Core.Boc;
using TONWallet.ConfigManager;
using static TONWallet.Utils;

namespace TONWallet.ActionSequence
{
    public class ManageWalletSequence : IActionSequence
    {
        public ConfigWallet wallet;
        public ManageWalletSequence(ConfigWallet wallet)
        {
            this.wallet = wallet;
        }

        public async Task Run()
        {
            while (true)
            {
                Console.Clear();

                var balance = await wallet.GetWalletBalance();
                var txs = await Program.client.GetTransactions(wallet.Wallet.Address, 20);
                var txList = txs.ToList();

                Console.WriteLine("=============================");
                Console.WriteLine("=> Wallet Management Panel <=");
                Console.WriteLine("=============================");
                Console.WriteLine();
                Console.WriteLine($"Address: {wallet.Wallet.Address}");
                Console.WriteLine($"Balance: {balance} TON");
                Console.WriteLine();
                Console.WriteLine("Last 5 transactions:");
                await PrintTXs(txList, 5);

                Console.WriteLine();

                Console.WriteLine("Select an action for the wallet");
                Console.WriteLine("[1] Open your wallet in browser on tonscan.org");
                Console.WriteLine("[2] List more (20) wallet transactions");
                Console.WriteLine("[3*] List all wallet NFTs");
                Console.WriteLine("[4] Send TON");
                Console.WriteLine("[5*] Send NFT");
                Console.WriteLine("[q] Quit to main menu");
                Console.WriteLine("[delete] Delete the wallet");
                Console.WriteLine("[reveal] Reveal the mnemonic phrase/seed");

                Console.WriteLine();

                string userchoice = Input("My action");
                switch (userchoice)
                {
                    case "1":
                        var url = $"https://tonscan.org/address/{wallet.Wallet.Address}";
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
                        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                        {
                            Process.Start("xdg-open", url);
                        } else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                        {
                            Process.Start("open", url);
                        }
                        continue;
                    case "2":
                        await PrintTXs(txList, 20); break;
                    case "3":
                        // todo
                        break;
                    case "4":
                        await Program.RunSequence(typeof(SendTONSequence), [wallet]);
                        break;
                    case "5":
                        // ehm todo
                        break;
                    case "q":
                        return;
                    case "delete":
                        await Program.RunSequence(typeof(RequestMasterKeySequence));

                        Console.WriteLine("Are you sure you want to remove your wallet?");
                        Console.WriteLine("Losing your mnemonic phrase or wallet seed will lead to permanent fund loss");
                        Console.WriteLine("<!> Please, don't forget to save your phrase/seed <!>");
                        Console.WriteLine();
                        string confirm = Input("Yes, I truly want to remove my wallet (enter 'confirm' to confirm)");

                        if (confirm.Equals("confirm", StringComparison.OrdinalIgnoreCase))
                        {
                            Program.cfg.wallets.Remove(wallet); // goodbye!
                            return;
                        }
                        break;
                    case "reveal":
                        await Program.RunSequence(typeof(RequestMasterKeySequence));

                        if (wallet.Source == WalletSource.SEED)
                        {
                            Console.WriteLine($"Here's your wallet seed. Keep it safe and NEVER share it with anyone!: {BAhex(wallet.GetSeed())}");
                        } else if (wallet.Source == WalletSource.MNEMONIC_PHRASE)
                        {
                            var mnemo = wallet.GetMnemonicPhrase();
                            Console.WriteLine($"Here's your mnemonic phrase. Keep it safe and NEVER share it with anyone!:");

                            for (int i = 0; i < mnemo.Length; i += 2)
                            {
                                Console.Write($"{(i + 1),-2}. {mnemo[i],-16}| {i + 2}.{mnemo[i + 1],-16}\r\n");
                            }
                        }
                        break;
                    default:
                        continue;
                }
                YieldReturn();
            }
        }
    }
}
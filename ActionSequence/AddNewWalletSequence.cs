using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TonSdk.Contracts.Wallet;
using TonSdk.Core.Crypto;
using TONWallet.ConfigManager;
using TONWallet.Wallet;
using static TONWallet.Utils;

namespace TONWallet.ActionSequence
{
    public class AddNewWalletSequence : IActionSequence
    {
        public async Task Run()
        {
            Console.Clear();
            Console.WriteLine("Do you want to CREATE a new wallet (c) or IMPORT an existing one (i)?");
            while (true)
            {
                string choice = Input("My choice (i or c)");

                if (choice.Equals("c", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("You will now get a new V4R2 Wallet created for you...");

                    string[] mnemo;
                    WalletV4 wallet;
                    while (true)
                    {
                        mnemo = Mnemonic.GenerateWords();
                        wallet = WalletGenerator.CreateWallet(mnemo);

                        Console.WriteLine("Successfully created your wallet!");
                        Console.WriteLine($"Wallet address: {wallet.Address}");
                        Console.WriteLine("Generate another address? Reply with y if yes, N if no.");
                        string reply = Input("Generate another address (y/N)");

                        if (reply.Equals("y", StringComparison.OrdinalIgnoreCase)) continue;
                        break;
                    }

                    while (true)
                    {
                        Console.Clear();
                        Console.WriteLine("Great. Now, you *HAVE TO* write down these next 24 words.");
                        Console.WriteLine("<!> LOSING THIS MNEMONIC PHRASE WILL LOSE ACCESS TO YOUR FUNDS <!>");

                        for (int i = 0; i < mnemo.Length; i += 2)
                        {
                            Console.Write($"{(i + 1),-2}. {mnemo[i],-16}| {i + 2}.{mnemo[i + 1],-16}\r\n");
                        }

                        Console.WriteLine("Security standards require me to test whether or not you've actually SAVED the words.");
                        Console.WriteLine("You will have to take a short test. Press ENTER when you're ready");
                        YieldReturn();

                        Console.Clear();
                        HashSet<int> indices = new HashSet<int>();
                        do
                        {
                            indices.Add(random.Next(0, mnemo.Length));
                        } while (indices.Count < 5);

                        bool failed = false;
                        foreach (var index in indices)
                        {
                            string word = Input($"Please, input word #{index + 1}");
                            string realWord = mnemo[index];

                            if (word != realWord)
                            {
                                Console.WriteLine("You're wrong. Please try again."); failed = true; break;
                            }
                        }
                        if (failed)
                        {
                            YieldReturn(); continue;
                        }
                        break;
                    }

                    Console.WriteLine();
                    Console.WriteLine($"=> WALLET SUCCESSFULLY CREATED! <=");
                    Console.WriteLine($"Address: {wallet.Address}");
                    Console.WriteLine($"Mnemonic phrase: <HIDDEN>");
                    Console.WriteLine($"Balance: 0.00 TON");
                    Console.WriteLine($"Blockchain URL: https://tonscan.org/address/{wallet.Address}");

                    Program.cfg.AddWallet(mnemo);

                    Console.WriteLine();
                    YieldReturn();
                    break;
                }
                else if (choice.Equals("i", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Please choose your wallet type. Currently, these options are supported:");
                    Console.WriteLine("1) Mnemonic phrase (24 words)");
                    Console.WriteLine("2) HEX seed value (64 HEX characters / 32 bytes)");
                    string type = Input("My wallet type");

                    if (type == "1")
                    {
                        List<string> mnemo = new List<string>();

                        Console.WriteLine("Please input your mnemonic phrase, consisting of 24 English words.");
                        Console.WriteLine("You can paste it as a 'word1, word2, word3, ...' or 'word1 word2 word3' list.");
                        Console.WriteLine("Alternatively, you can enter the phrase word-by-word.");

                        string phrase = Input("My mnemonic phrase");

                        // try to parse it now ig
                        string[] split = phrase.Split([' ', ',']);
                        if (split.Length != 24 && split.Length < 2)
                        {
                            // assume we're dealing with individual words. Well then!
                            mnemo.Add(phrase);

                            for (int i = 0; i < 23; i++)
                            {
                                string newWord = Input($"Input word #{i + 2}");
                                mnemo.Add(newWord);
                            }
                        } else if (split.Length == 24)
                        {
                            // success!
                            mnemo = split.ToList();
                        } else
                        {
                            // critical failure
                            Console.WriteLine("The mnemonic phrase is entered in an unknown format! Please, try again.");
                            YieldReturn();
                            continue;
                        }

                        Console.WriteLine("Attempting to resolve the wallet...");
                        var wlt = WalletGenerator.CreateWallet(mnemo.ToArray());
                        Console.WriteLine($"Address of wallet imported: {wlt.Address}. Is that right?");
                        string opt = Input("Is that right? (Y/n)");
                        if (opt.Equals("n", StringComparison.OrdinalIgnoreCase)) continue;
                        Console.WriteLine();
                        Console.WriteLine($"=> WALLET SUCCESSFULLY ADDED! <=");
                        Console.WriteLine($"Address: {wlt.Address}");

                        Program.cfg.AddWallet(mnemo.ToArray());

                        Console.WriteLine();
                        YieldReturn();
                        break;
                    } else if (type == "2")
                    {
                        // todo
                    }
                }
            }
        }
    }
}

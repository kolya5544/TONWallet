using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TONWallet.Utils;

namespace TONWallet.ActionSequence
{
    public class InitWalletSequence : IActionSequence
    {
        public async Task Run()
        {
            Console.WriteLine("First, let's initialize your wallet.");
            Console.WriteLine("Make sure nobody is watching your screen, since some questions asked are confidential.");

            Console.WriteLine();
            Console.WriteLine("Step 1 [REQUIRED]. Choose your master password that will be used for encryption.");
            Console.WriteLine("Alternatively, press ENTER to generate an empty password");

            string masterPwd;
            while (true)
            {
                masterPwd = Input("My master password");

                if (masterPwd is null)
                {
                    masterPwd = RandomString(16);
                    Console.WriteLine($"Your master password: {masterPwd}. Make sure you save it!");
                    break;
                }

                if (masterPwd.Length <= 3)
                {
                    Console.WriteLine("Your master password has to be at least 4 characters long.");
                    continue;
                }

                string confirmPwd = Input("Confirm your master password");

                if (confirmPwd != masterPwd)
                {
                    Console.WriteLine($"The passwords don't match! Try again.");
                    continue;
                }
                break;
            }
            Console.WriteLine("<!> YOU WILL NOT BE ABLE TO RECOVER YOUR ACCESS IF MASTER PASSWORD IS LOST! <!>");
            Program.cfg.MasterPwdSHA512 = sha256_s(masterPwd);

            Console.WriteLine();

            Console.WriteLine("Step 3 [REQUIRED]. Enter your tonaccess.com API key.");
            Console.WriteLine("Your API key will be used for generic API operations. Get one here, it's free -> https://t.me/tonapibot");
            string apiKey = Input("My API key");
            Program.cfg.TONAPIKey = apiKey;

            Console.WriteLine();

            Program.cfg.Save();

            Console.WriteLine("Step 2 [OPTIONAL]. Import your first wallet");
            Console.WriteLine("Do you want to set up your first wallet now (Y) or later (n)?");
            string choice = Input("Set up a wallet (Y/n)?");

            if (choice.Equals("n", StringComparison.OrdinalIgnoreCase)) {
                Console.WriteLine("Alright! You're all done with initialization process.");
                Console.WriteLine("Enjoy using TONWallet!");
                YieldReturn();
            } else
            {
                await Program.RunSequence(typeof(AddNewWalletSequence));
            }
        }
    }
}

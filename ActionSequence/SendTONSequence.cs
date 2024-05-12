using TonSdk.Client;
using TonSdk.Connect;
using TonSdk.Contracts.Wallet;
using TonSdk.Core;
using TonSdk.Core.Block;
using TonSdk.Core.Boc;
using TonSdk.Core.Crypto;
using TONWallet.ConfigManager;
using static TONWallet.Utils;

namespace TONWallet.ActionSequence
{
    public class SendTONSequence : IActionSequence
    {
        public ConfigWallet wallet;
        public SendTONSequence(ConfigWallet wallet)
        {
            this.wallet = wallet;
        }

        public async Task Run()
        {
            var isActive = await Program.client.IsContractDeployed(wallet.Wallet.Address);
            if (!isActive)
            {
                var bal = await wallet.GetWalletBalance();
                if (bal <= (decimal)0.1)
                {
                    Console.WriteLine($"<!> Your wallet is not deployed yet! You should first fund it with at least 0.1 TON. <!>");
                    return;
                } else
                {
                    Console.WriteLine($"<!> Your wallet is not deployed yet! Attempting to deploy the wallet... <!>");
                    var deploy = wallet.Wallet.CreateDeployMessage();
                    deploy.Sign(wallet.KeyPair.PrivateKey);
                    var cell = deploy.Cell;
                    // send the msg
                    var res = await Program.client.SendBoc(cell);

                    // hope it succeeded
                    Console.WriteLine($"Wallet successfully deployed! Proceeding to the transfer...");
                }
            }

            Console.WriteLine($"====================================");
            Console.WriteLine($"=> Transfer TON to another wallet <=");
            Console.WriteLine($"====================================");

            Address addr = null;
            while (true) {
                Console.WriteLine($"Input the destination wallet (like UQD...G7u) or destination domain (like nerd.ton)");
                string destination = Input("Destination");

                try
                {                    
                    if (destination.EndsWith(".ton"))
                    {
                        // is a domain
                        addr = await Program.client.Dns.GetWalletAddress(destination);
                    }
                    else
                    {
                        addr = new Address(destination);
                    }

                    if (addr is null) throw new Exception("Address couldn't be parsed");
                }
                catch
                {
                    Console.WriteLine($"Couldn't parse address! Make sure it is correct and try again."); continue;
                }
                break;
            }

            Coins? coin;
            while (true)
            {
                Console.WriteLine($"Enter the amount of TON you want to send (like 0.05 to send 0.05 TON)");
                string amount = Input("Amount");

                try
                {
                    coin = new Coins(decimal.Parse(amount));
                    if (coin.IsZero() || coin.IsNegative()) throw new Exception("Amount cannot be zero or negative");
                } catch
                {
                    Console.WriteLine($"Couldn't parse TON amount entered! Make sure it is correct and try again."); continue;
                }
                break;
            }

            string? memo = null;
            Console.WriteLine($"Input memo/comment/message to include with your transaction. Leave empty to include no memo");
            memo = Input("Memo");

            bool bounceable = true;
            Console.WriteLine($"Should transaction be 'bounceable'? Reply with n if it's being sent to a new contract/wallet, and Y in most of other cases.");
            var bounceInput = Input("Bounceable? (Y/n)");
            if (bounceInput.Equals("n", StringComparison.OrdinalIgnoreCase)) bounceable = false;

            Console.WriteLine($"Calculating... please wait a second");

            // getting seqno using tonClient
            uint? seqno = await Program.client.Wallet.GetSeqno(wallet.Wallet.Address);

            Cell body = null;
            if (!string.IsNullOrEmpty(memo))
            {
                // send COMMENTED TON TX (with memo)
                // create transaction body query + memo
                body = new CellBuilder().StoreUInt(0, 32).StoreString(memo).Build();
            } else memo = null;

            // create transfer message
            ExternalInMessage message = wallet.Wallet.CreateTransferMessage(
            [
                new WalletTransfer
                {
                    Message = new InternalMessage(new InternalMessageOptions
                    {
                        Info = new IntMsgInfo(new IntMsgInfoOptions
                        {
                            Dest = addr,
                            Value = coin,
                            Bounce = bounceable // make bounceable message
                        }),
                        Body = body
                    }),
                    Mode = 1 // message mode
                }
            ], seqno ?? 0); // if seqno is null we pass 0, wallet will auto deploy on message send

            var feeEstimate = await Program.client.EstimateFee(message);
            var sf = feeEstimate?.SourceFees;
            var totalFee = (sf?.GasFee ?? 0) + (sf?.FwdFee ?? 0) + (sf?.InFwdFee ?? 0) + (sf?.StorageFee ?? 0);
            var totalCoinFee = Coins.FromNano(totalFee);

            Console.WriteLine("=> TRANSFER PREVIEW <=");
            Console.WriteLine($"Destination: {addr}");
            Console.WriteLine($"Bounceable: {(bounceable ? "YES" : "NO")}");
            Console.WriteLine($"Memo/comment: {(memo is null ? "<EMPTY>" : memo)}");
            Console.WriteLine($"Amount: {coin} TON");
            Console.WriteLine($"ESTIMATED FEE: {totalCoinFee} TON");
            Console.WriteLine($"TOTAL TRANSACTION PRICE: {new Coins(coin.ToDecimal() + totalCoinFee.ToDecimal())} TON");

            Console.WriteLine();

            Console.WriteLine("Is that right? Reply with 'y' to confirm transaction, or 'N' to cancel it.");
            string choice = Input("Confirm transaction? (y/N)");

            if (choice.Equals("y", StringComparison.OrdinalIgnoreCase))
            {
                // sign transfer message
                message.Sign(wallet.KeyPair.PrivateKey);

                // get message cell
                Cell cell = message.Cell;

                // send this message via TonClient
                var tx = await Program.client.SendBoc(cell);

                Console.WriteLine("=> Transaction sent successfully! <=");
            }
            return;
        }
    }
}
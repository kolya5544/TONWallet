
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
                var txs = await Program.client.GetTransactions(wallet.Wallet.Address, 10);
                var txList = txs.ToList();

                Console.WriteLine("=============================");
                Console.WriteLine("=> Wallet Management Panel <=");
                Console.WriteLine("=============================");
                Console.WriteLine();
                Console.WriteLine($"Address: {wallet.Wallet.Address}");
                Console.WriteLine($"Balance: {balance}");
                Console.WriteLine();
                Console.WriteLine("Last 5 transactions:");
                for (int i = 0; i < Math.Min(5, txList.Count); i++)
                {
                    var tx = txList[i];

                    // classify the transaction
                    var classify = await TransactionClassifier.ClassifyTXAsync(tx);
                    if (classify.Item1 == TransactionType.FAKE)
                    {
                        txList.RemoveAt(i); i--; continue;
                    }
                    Console.Write($"[{i + 1}] ");

                    switch (classify.Item1)
                    {
                        case TransactionType.SIMPLE_TX:
                            Console.Write($"Got {classify.Item2} TON from {MiniAddress(tx.InMsg.Source)}"); break;
                        case TransactionType.COMMENTED_TX:
                            Console.Write($"Got {classify.Item2} TON from {MiniAddress(tx.InMsg.Source)} with comment: '{classify.Item3}'"); break;
                        case TransactionType.SIMPLE_JETTON_TRANSFER:
                            Console.Write($"Got {classify.Item2} Jetton from {MiniAddress(tx.InMsg.Source)}"); break;
                        case TransactionType.COMMENTED_JETTON_TRANSFER:
                            Console.Write($"Got {classify.Item2} Jetton from {MiniAddress(tx.InMsg.Source)} with comment: '{classify.Item3}'"); break;
                        case TransactionType.SIMPLE_NFT_TRANSFER:
                            Console.Write($"Got an NFT #{classify.Item2} from {MiniAddress(tx.InMsg.Source)}"); break;
                        case TransactionType.COMMENTED_NFT_TRANSFER:
                            Console.Write($"Got an NFT #{classify.Item2} from {MiniAddress(tx.InMsg.Source)} with comment: '{classify.Item3}'"); break;
                        case TransactionType.UNKNOWN:
                            Console.Write($"Unknown transaction"); break;
                    }

                    Console.Write($" @ {Time(tx.Utime)}\r\n");

                    Thread.Sleep(API_RATELIMIT);
                }

                YieldReturn();
            }
        }
    }
}
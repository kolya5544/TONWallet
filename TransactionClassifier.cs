using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TonSdk.Client;
using TonSdk.Client.Stack;
using TonSdk.Core;
using TonSdk.Core.Boc;

namespace TONWallet
{
    public enum TransactionType
    {
        SIMPLE_TX,
        COMMENTED_TX,
        SIMPLE_JETTON_TRANSFER,
        COMMENTED_JETTON_TRANSFER,
        SIMPLE_NFT_TRANSFER,
        COMMENTED_NFT_TRANSFER,
        FAKE,
        UNKNOWN
    }

    public class TransactionClassifier
    {
        public static async Task<(TransactionType, decimal, string?)> ClassifyTXAsync(TransactionsInformationResult tx)
        {
            var myAddr = tx.InMsg.Destination;
            var body = tx.InMsg.MsgData.Body;
            if (body is null) return (TransactionType.SIMPLE_TX, tx.InMsg.Value.ToDecimal(), null);
            var parse = new CellSlice(body);
            var sender = tx.InMsg.Source;

            if (parse.RemainderBits < 32)
            {
                // if body doesn't have opcode: it's a simple message without comment
                return (TransactionType.SIMPLE_TX, tx.InMsg.Value.ToDecimal(), null);
            }
            else
            {
                var opcode = parse.LoadUInt(32);

                if (opcode == 0)
                {
                    // if opcode is 0: it's a simple message with comment
                    var comment = parse.LoadString();
                    return (TransactionType.COMMENTED_TX, tx.InMsg.Value.ToDecimal(), comment);
                }
                else if (opcode == 0x7362d09c)
                {
                    // if opcode is 0x7362d09c: it's a Jetton transfer notification

                    parse.SkipBits(32); // skip query_id
                    var jettonAmount = parse.LoadCoins();
                    var jettonSender = parse.LoadAddress();
                    var forwardPayload = parse.LoadBit() ? new CellSlice(parse.LoadRef()) : parse;

                    // IMPORTANT: we have to verify the source of this message because it can be faked
                    var s = new string[0][];
                    var walletData = await Program.client.RunGetMethod(sender, "get_wallet_data", s);
                    var stack = walletData?.Stack;
                    var jettonMaster = new CellSlice((Cell)stack[2]).ReadAddress();
                    var newStack = new List<IStackItem>();
                    var cb = new CellBuilder();
                    cb.StoreAddress(myAddr);
                    newStack.Add(new VmStackCell(cb.Build()));
                    var walletAddr = await Program.client.RunGetMethod(jettonMaster, "get_wallet_address", newStack.ToArray());
                    var jettonWallet = new CellSlice((Cell)walletAddr?.Stack.FirstOrDefault()).ReadAddress();

                    if (!jettonWallet.Equals(sender))
                    {
                        // fake and gay
                        return (TransactionType.FAKE, 0, null);
                    }

                    // process forward payload
                    if (forwardPayload.RemainderBits < 32)
                    {
                        // if forward payload doesn't have opcode: it's a simple Jetton transfer
                        return (TransactionType.SIMPLE_JETTON_TRANSFER, jettonAmount.ToDecimal(), null);
                    }
                    else
                    {
                        var forwardOp = forwardPayload.LoadUInt(32);
                        if (forwardOp == 0)
                        {
                            // if forward payload opcode is 0: it's a simple Jetton transfer with comment
                            var comment = forwardPayload.LoadString();
                            return (TransactionType.COMMENTED_JETTON_TRANSFER, jettonAmount.ToDecimal(), comment);
                        }
                        else
                        {
                            return (TransactionType.COMMENTED_JETTON_TRANSFER, jettonAmount.ToDecimal(), null);
                        }
                    }
                }
                else if (opcode == 0x05138d91)
                {
                    // if opcode is 0x05138d91: it's a NFT transfer notification

                    parse.SkipBits(64); // skip query_id
                    var prevOwner = parse.LoadAddress();
                    var forwardPayload = parse.LoadBit() ? new CellSlice(parse.LoadRef()) : parse;

                    // IMPORTANT: we have to verify the source of this message because it can be faked
                    var s = new string[0][];
                    var walletData = await Program.client.RunGetMethod(sender, "get_nft_data", s);
                    var index = (BigInteger)walletData?.Stack[1];
                    var collection = new CellSlice((Cell)walletData?.Stack[2]).ReadAddress();
                    var newStack = new List<IStackItem>
                    {
                        new VmStackTinyInt(index)
                    };
                    var walletAddr = await Program.client.RunGetMethod(collection, "get_nft_address_by_index", newStack.ToArray());
                    var itemAddress = new CellSlice((Cell)walletAddr?.Stack.FirstOrDefault()).ReadAddress();

                    if (!itemAddress.Equals(sender))
                    {
                        // fake and gay
                        return (TransactionType.FAKE, 0, null);
                    }

                    // process forward payload
                    if (forwardPayload.RemainderBits < 32)
                    {
                        // if forward payload doesn't have opcode: it's a simple NFT transfer
                        return (TransactionType.SIMPLE_NFT_TRANSFER, (int)index, null);
                    }
                    else
                    {
                        var forwardOp = forwardPayload.LoadUInt(32);
                        if (forwardOp == 0)
                        {
                            // if forward payload opcode is 0: it's a simple NFT transfer with comment
                            var comment = forwardPayload.LoadString();
                            return (TransactionType.COMMENTED_NFT_TRANSFER, (int)index, comment);
                        }
                        else
                        {
                            return (TransactionType.COMMENTED_NFT_TRANSFER, (int)index, null);
                        }
                    }
                } else
                {
                    return (TransactionType.UNKNOWN, 0, null);
                }
            }
        }
    }
}

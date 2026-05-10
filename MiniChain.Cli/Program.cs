using MiniChain.Core.Services;
using System.Diagnostics;
using MiniChain.Core.Interface;

namespace MiniChain.Cli;

public class Program
{
    private static Transaction SignedTx(IWallet? sender, string to, decimal amount)
    {
        var tx = new Transaction(sender!.PublicKeyHex, to, amount, false);
        tx.Sign(sender);
        return tx;
    }

    private static IBlock MineFromMempoolAndPrint(IBlockchain blockchain, Mempool mempool, string minerAddress)
    {
        Console.WriteLine($"Mining block {blockchain.Height + 1}...");
        var sw = Stopwatch.StartNew();
        var block = blockchain.MineFromMempool(mempool, mempool.Pending.Count, minerAddress);
        sw.Stop();
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
        
        return block;
    }

    private static void Menu()
    {
        Console.WriteLine("Enter the commands:");
        Console.WriteLine("wallet new");
        Console.WriteLine("wallet load");
        Console.WriteLine("balance");
        Console.WriteLine("send <to> <amount>");
        Console.WriteLine("mine");
        Console.WriteLine("chain");
    }

    public static void Main(string[] args)
    {
        var blockchain = new Blockchain();
        var mempool = new Mempool();
        var node = new Node(blockchain);
        IWallet? wallet = null;

        Menu();
        while (true)
        {
            Console.Write("> ");
            var input = Console.ReadLine()!;
            if (input is null) break;
            var parts =  input.Split(' ');
            switch (parts[0])
            {
                case "wallet":
                    switch (parts[1])
                    {
                        case "new":
                            var newWallet = new Wallet();
                            wallet = newWallet;
                            newWallet.SaveWallet("wallet.json");
                            Console.WriteLine($"Saved new wallet with public key: {newWallet.PublicKeyHex[..12]}");
                            break;
                        case "load":
                            wallet = Wallet.LoadWallet("wallet.json");
                            Console.WriteLine($"Loaded wallet with public key: {wallet.PublicKeyHex[..12]}");
                            break;
                    }

                    break;
                case "balance":
                    if (wallet is null)
                    {
                        Console.WriteLine("No wallet loaded");
                    }
                    else
                    {
                        var confirmed = blockchain.GetBalance(wallet.PublicKeyHex);
                        var pendingSpend = mempool.Pending
                            .Where(t => t.From == wallet.PublicKeyHex)
                            .Sum(t => t.Amount);
                        Console.WriteLine($"Balance: {confirmed - pendingSpend} ({confirmed} confirmed {pendingSpend} pending)");
                    }

                    break;
                case "send":
                    var to = parts[1];
                    var amount = decimal.Parse(parts[2]);
                    if (string.IsNullOrEmpty(to) || amount <= 0)
                    {
                        Console.WriteLine("Invalid send");
                    }
                    else
                    {
                        var transaction = SignedTx(wallet, to, amount);
                        Console.WriteLine(
                            mempool.Submit(transaction, blockchain)
                                ? $"Transaction Accepted: Sent {amount} to {to}"
                                : $"Transaction failed: {amount} to {to}");
                    }
                    break;
                case "mine":
                    if (wallet is null)
                    {
                        Console.WriteLine("No wallet loaded");
                    }
                    else
                    {
                        MineFromMempoolAndPrint(blockchain, mempool, wallet.PublicKeyHex);
                        Console.WriteLine($"Blockchain hash: {blockchain.Tip.ComputeHash()[..12]}");
                        Console.WriteLine($"Wallet balance: {blockchain.GetBalance(wallet.PublicKeyHex)}");
                    }
                    break;
                case "chain":
                    foreach (var block in blockchain.Blocks)
                    {
                        Console.WriteLine(block.ToString());
                    }
                    break;
                case "quit":
                    break;
                default: Console.WriteLine("Unknown command");
                    continue;
            }

            if (parts[0] == "quit")
            {
                break;
            }
        }
    }
}
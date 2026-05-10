using MiniChain.Core.Services;
using System.Diagnostics;
using System.Reflection;
using MiniChain.Core.Interface;

namespace MiniChain.Cli;

public class Program
{
    private static void ReplaceBlockAt(Blockchain chain, int index, IBlock replacement)
    {
        var field = typeof(Blockchain)
            .GetField("_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<IBlock>)field!.GetValue(chain)!;
        list[index] = replacement;
    }

    private static Transaction SignedTx(Wallet sender, Wallet to, decimal amount)
    {
        var tx = new Transaction(sender.PublicKeyHex, to.PublicKeyHex, amount);
        tx.Sign(sender);
        return tx;
    }

    private static IBlock MineFromMempoolAndPrint(IBlockchain blockchain, Mempool mempool)
    {
        Console.WriteLine($"Mining block {blockchain.Height + 1}...");
        var sw = Stopwatch.StartNew();
        var block = blockchain.MineFromMempool(mempool, 2);
        sw.Stop();
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
        
        return block;
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Mini Chain Demo");
        
        var node1Chain = new Blockchain(4);
        var node2Chain = new Blockchain(4);
        var mempool = new Mempool();
        var alice = new Wallet();
        var bob = new Wallet();
        var node1 = new Node(node1Chain);
        var node2 = new Node(node2Chain);
        
        Console.WriteLine($"Alice: {alice.PublicKeyHex[..12]}...");
        Console.WriteLine($"Bob: {bob.PublicKeyHex[..12]}...");
        node1.Connect(node2);
        
        mempool.Submit(SignedTx(alice, bob, 10));
        mempool.Submit(SignedTx(bob, alice, 6));
        mempool.Submit(SignedTx(bob, alice, 100));
        Console.WriteLine($"Mempool has {mempool.Pending.Count} transactions");
        var minedBlock = MineFromMempoolAndPrint(node1.Blockchain, mempool);
        node1.Broadcast(minedBlock);
        Console.WriteLine($"Mempool has {mempool.Pending.Count} transactions");
        Console.WriteLine($"Chain from Node 1: {node1.Blockchain.Height} {node1.Blockchain.Tip.ComputeHash()[..12]}");
        Console.WriteLine($"Chain from Node 2: {node2.Blockchain.Height} {node2.Blockchain.Tip.ComputeHash()[..12]}");
        
        foreach (var block in node1Chain.Blocks)
        {
            Console.WriteLine($"{block}");
        }
        Console.WriteLine($"The chain is valid:{node1Chain.IsValid()}");

        Console.WriteLine("Simulating Tampering....");

        mempool.Submit(SignedTx(alice, bob, 102));
        mempool.Submit(SignedTx(bob, alice, 200));
        mempool.Submit(SignedTx(bob, alice, 200));
        minedBlock = MineFromMempoolAndPrint(node2Chain, mempool);
        node2.Broadcast(minedBlock);
        minedBlock = MineFromMempoolAndPrint(node2Chain, mempool);
        node2.Broadcast(minedBlock);
        ReplaceBlockAt(node1Chain, 1, node2Chain.Blocks[2]);

        Console.WriteLine($"The chain is valid:{node1Chain.IsValid()}");
    }
}
using MiniChain.Core;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json.Nodes;

public class Program
{
    private static void ReplaceBlockAt(Blockchain chain, int index, Block replacement)
    {
        var field = typeof(Blockchain)
            .GetField("_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<Block>) field!.GetValue(chain)!;
        list[index] = replacement;
    }
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Mini Chain Demo");

        var chain = new Blockchain(4);
        
        var sw = Stopwatch.StartNew();
        Console.WriteLine($"Mining block {chain.Height + 1}...");
        sw.Restart();
        var block = chain.AddBlock(["Bob->Alice:10"]);
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
        Console.WriteLine($"Mining block {chain.Height + 1}...");
        sw.Restart();
        block = chain.AddBlock(["Bob->Alice:11"]);
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
        Console.WriteLine($"Mining block {chain.Height + 1}...");
        sw.Restart();
        block = chain.AddBlock(["Bob->Alice:12"]);
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
        Console.WriteLine($"Mining block {chain.Height + 1}...");
        sw.Restart();
        block = chain.AddBlock(["Alice->Bob:13"]);
        Console.WriteLine($"Nonce found: {block.Nonce} Elapsed ms: {sw.Elapsed}");
        sw.Stop();

        foreach (var block1 in chain.Blocks)
        {
            Console.WriteLine($"{block1}");
        }
        Console.WriteLine($"The chain is valid:{chain.IsValid()}");

        Console.WriteLine("Simulating Tampering....");

        var chain1 = new Blockchain(4);
        chain1.AddBlock(["Alice->Alarico:10"]);
        chain1.AddBlock(["Alice->Alarico:11"]);
        
        ReplaceBlockAt(chain, 2, chain1.Blocks[0]);
        ReplaceBlockAt(chain, 3, chain1.Blocks[1]);
        
        Console.WriteLine($"The chain is valid:{chain.IsValid()}");
    }
}
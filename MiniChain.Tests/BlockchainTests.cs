using MiniChain.Core.Interface;
using MiniChain.Core.Services;
using System.Reflection;

namespace MiniChain.Tests;

public class BlockchainTests
{
    [Fact]
    public void NewChain_HasOneBlock()
    {
        var chain = new Blockchain();
        
        Assert.Single(chain.Blocks);
        Assert.Equal(0, chain.Height);
        Assert.Equal(Block.CreateGenesis().ComputeHash(), chain.Tip.ComputeHash());
    }

    [Fact]
    public void AddBlock_LinksToPreviousTip()
    {
        var chain = new Blockchain();
        var tipHash = chain.Tip.ComputeHash();
        var alice = new Wallet();
        var bob = new Wallet();
        var block = chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 10)]);
        
        Assert.Equal(1, block.Index);
        Assert.Equal(tipHash, block.PreviousHash);
        Assert.Equal(1, chain.Height);
    }
    
    [Fact]
    public void IsValid_ReturnsTrue_ForValidChain()
    {
        var chain = new Blockchain();
        var alice = new Wallet();
        var bob = new Wallet();
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 10)]);
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 11)]);
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 12)]);
        
        Assert.True(chain.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenMiddleTransactionsAreTamperedWith()
    {
        var chain = new Blockchain();
        var alice = new Wallet();
        var bob = new Wallet();
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 10)]);
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 11)]);
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 12)]);
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 13)]);

        
        var block = new Block(chain.Blocks[2].Index, chain.Blocks[2].Timestamp, chain.Blocks[2].PreviousHash, []);
        ReplaceBlockAt(chain, 2, block);
        
        Assert.False(chain.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenPreviousHashIsDifferent()
    {
        var chain = new Blockchain();
        var alice = new Wallet();
        var bob = new Wallet();
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 10)]);
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 11)]);
        
        var block = new Block(chain.Blocks[2].Index, chain.Blocks[2].Timestamp, new string('f', 64) , []);
        ReplaceBlockAt(chain, 2, block);
        
        Assert.False(chain.IsValid());
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenGenesisIsTamperedWith()
    {
        var chain = new Blockchain();

        var block = new Block(chain.Blocks[0].Index, Block.CreateGenesis().Timestamp, Block.CreateGenesis().PreviousHash, []);
        ReplaceBlockAt(chain, 0, block);
        
        Assert.False(chain.IsValid());
    }
    
    private static void ReplaceBlockAt(Blockchain chain, int index, IBlock replacement)
    {
        var field = typeof(Blockchain)
            .GetField("_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<IBlock>) field!.GetValue(chain)!;
        list[index] = replacement;
    }
    
    private static Transaction SignedTx(Wallet sender, string to, decimal amount)
    {
        var tx = new Transaction(sender.PublicKeyHex, to, amount, false);
        tx.Sign(sender);
        return tx;
    }
}
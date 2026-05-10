using MiniChain.Core.Interface;
using MiniChain.Core.Services;
using System.Reflection;

namespace MiniChain.Tests;

public class MinerTests
{
    [Fact]
    public void Mine_ProducesHashWithRequiredZeros()
    {
        var block = Block.CreateGenesis();
        var miner = new Miner();
        var hash = miner.Mine(block, 2);
        
        Assert.StartsWith(new string('0', 2), hash!);
    }
    
    [Fact]
    public void Mine_SetsNonceAboveZero_ForNonTrivialDifficulty()
    {
        var block = Block.CreateGenesis();
        var miner = new Miner();
        miner.Mine(block, 2);
        
        Assert.True(block.Nonce > 0);
    }
    
    [Fact]
    public void Mine_IsIdempotent()
    {
        var block = Block.CreateGenesis();
        var miner = new Miner();
        
        var hash1 = miner.Mine(block, 2);
        var hash2 = miner.Mine(block, 2);
        
        Assert.Equal(hash1, hash2);
    }
    
    [Fact]
    public void IsValid_ReturnsFalse_WhenBlockIsNotMined()
    {
        var chain = new Blockchain(2);
        var alice = new Wallet();
        var bob = new Wallet();
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 10)]);
        chain.AddBlock([SignedTx(alice, bob.PublicKeyHex, 11)]);
        var replaceBlock = new Block(chain.Blocks[1].Index, chain.Blocks[1].Timestamp, chain.Blocks[1].PreviousHash, chain.Blocks[1].Transactions);
        
        ReplaceBlockAt(chain, 1, replaceBlock);
        
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
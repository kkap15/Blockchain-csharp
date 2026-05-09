using MiniChain.Core;
using System.Reflection;

namespace MiniChain.Tests;

public class MinerTests
{
    [Fact]
    public void Mine_ProducesHashWithRequiredZeros()
    {
        var block = Block.CreateGenesis();
        
        var hash = Miner.Mine(block, 2);
        
        Assert.StartsWith(new string('0', 2), hash!);
    }
    
    [Fact]
    public void Mine_SetsNonceAboveZero_ForNonTrivialDifficulty()
    {
        var block = Block.CreateGenesis();
        
        Miner.Mine(block, 2);
        
        Assert.True(block.Nonce > 0);
    }
    
    [Fact]
    public void Mine_IsIdempotent()
    {
        var block = Block.CreateGenesis();
        
        var hash1 = Miner.Mine(block, 2);
        var hash2 = Miner.Mine(block, 2);
        
        Assert.Equal(hash1, hash2);
    }
    
    [Fact]
    public void IsValid_ReturnsFalse_WhenBlockIsNotMined()
    {
        var chain = new Blockchain(2);
        chain.AddBlock(["Bob->Alice:10"]);
        var replaceBlock = new Block(chain.Blocks[1].Index, chain.Blocks[1].Timestamp, chain.Blocks[1].PreviousHash, chain.Blocks[1].Transactions);
        
        ReplaceBlockAt(chain, 1, replaceBlock);
        
        Assert.False(chain.IsValid());
    }
    
    private static void ReplaceBlockAt(Blockchain chain, int index, Block replacement)
    {
        var field = typeof(Blockchain)
            .GetField("_blocks", BindingFlags.NonPublic | BindingFlags.Instance);
        var list = (List<Block>) field!.GetValue(chain)!;
        list[index] = replacement;
    }
}
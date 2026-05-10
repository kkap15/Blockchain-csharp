using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Blockchain(int difficulty = 3, IMiner? miner = null) : IBlockchain
{
    private readonly List<IBlock> _blocks = [Block.CreateGenesis()];
    private readonly IMiner _miner = miner ?? new Miner();
    public IReadOnlyList<IBlock> Blocks => _blocks;
    public IBlock Tip => _blocks.Last();
    public int Height => _blocks.Count - 1;
    public int Difficulty { get; } = difficulty;

    public IBlock AddBlock(IReadOnlyList<ITransaction> transactions)
    {
        
        var tip = Tip;
        var newBlock = new Block(
            tip.Index + 1,
            DateTimeOffset.UtcNow,
            tip.ComputeHash(),
            transactions
            );
        _miner.Mine(newBlock, Difficulty);
        _blocks.Add(newBlock);
        return _blocks.Last();
    }
    
    public void ReplaceChain(IReadOnlyList<IBlock> chain)
    {
        _blocks.Clear();
        foreach(var block in chain)
        {
            _blocks.AddRange(block);
        }
    }

    public bool IsValid()
    {
        if (_blocks.Count == 0)
        {
            return false;
        }

        var expectedGenesisHash = Block.CreateGenesis().ComputeHash();
        if (_blocks[0].ComputeHash() != expectedGenesisHash)
        {
             Console.WriteLine("FAIL: genesis hash");
            return false;
        }

        for (var i = 1; i < _blocks.Count; ++i)
        {
            var current =  _blocks[i];
            var previous =  _blocks[i - 1];

            if (current.Index != i)
            {
                return false;
            }
            if (current.PreviousHash != previous.ComputeHash()) return false;
            if (!_miner.MeetsTarget(current.ComputeHash(), Difficulty)) return false;
            if (current.Transactions.Any(t => !t.IsValid())) return false;
        }
        
        return true;
    }
    
    public bool IsValidChain(IReadOnlyList<IBlock> chain)
    {
        if (chain.Count == 0)
        {
            return false;
        }
        
        var expectedGenesisHash = Block.CreateGenesis().ComputeHash();
        if (chain[0].ComputeHash() != expectedGenesisHash)
        {
            return false;
        }
        
        for (var i = 1; i < chain.Count; ++i)
        {
            var current = chain[i];
            var previous = chain[i - 1];
            
            if (current.Index != i)
            {
                return false;
            }
            if (current.PreviousHash != previous.ComputeHash()) return false;
            if (!_miner.MeetsTarget(current.ComputeHash(), Difficulty)) return false;
            if (current.Transactions.Any(t => !t.IsValid())) return false;
        }
        return true;
    }
    
    public IBlock MineFromMempool(IMempool mempool, int count)
    {
        var transactions = mempool.Take(count);
        var block = AddBlock(transactions);
        mempool.Remove(block.Transactions);
        
        return block;
    }
}
using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Mempool : IMempool
{
    private List<ITransaction> _pending = [];
    public IReadOnlyList<ITransaction> Pending => _pending;

    public void Remove(IEnumerable<ITransaction> confirmed)
    {
        _pending.RemoveAll(confirmed.Contains);
    }

    public bool Submit(ITransaction transaction,  IBlockchain? blockchain = null)
    {
        if (!transaction.IsValid()) return false;
        if (blockchain is not null)
        {
            if (!transaction.IsCoinbase)
            {
                var balance = blockchain.GetBalance(transaction.From);
                var pendingSpend = _pending
                    .Where(x => x.From == transaction.From)
                    .Sum(t => t.Amount);
                if (balance - pendingSpend < transaction.Amount) return false;
            }
        }
        _pending.Add(transaction);
        
        return true;
    }

    public IReadOnlyList<ITransaction> Take(int count)
    {
        var list = _pending.Take(count).ToList();
        
        return list;
    }
}
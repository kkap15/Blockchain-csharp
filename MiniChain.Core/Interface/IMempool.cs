namespace MiniChain.Core.Interface;

public interface IMempool
{
    public IReadOnlyList<ITransaction> Pending { get; }
    public IReadOnlyList<ITransaction> Take(int count);
    public bool Submit(ITransaction transaction, IBlockchain? blockchain = null);
    public void Remove(IEnumerable<ITransaction> confirmed);
}
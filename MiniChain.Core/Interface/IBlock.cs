namespace MiniChain.Core.Interface;

public interface IBlock
{
    public int Index { get; }
    public DateTimeOffset Timestamp { get; }
    public string PreviousHash { get; }
    public IReadOnlyList<ITransaction> Transactions { get; } 
    public long Nonce { get; set; }
    public string ComputeHash();
    public string ToString();
    public string MerkleRoot { get; }
}
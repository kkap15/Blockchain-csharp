namespace MiniChain.Core.Interface;

public interface IBlockchain
{
    public IReadOnlyList<IBlock> Blocks { get; }
    public int Difficulty { get; }
    public int Height { get; }
    public IBlock Tip { get; }
    public IBlock AddBlock(IReadOnlyList<ITransaction> transactions);
    public bool IsValid();
    public IBlock MineFromMempool(IMempool mempool, int count, string minerAddress);
    public void ReplaceChain(IReadOnlyList<IBlock> chain);
    public bool IsValidChain(IReadOnlyList<IBlock> chain);
    public decimal GetBalance(string walletPublicKeyHex);
}
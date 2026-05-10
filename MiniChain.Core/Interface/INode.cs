namespace MiniChain.Core.Interface;

public interface INode
{
    public IBlockchain Blockchain { get; }
    public void Connect(INode peer);
    public void Broadcast(IBlock block);
    public bool AcceptChain(IReadOnlyList<IBlock> chain);
}
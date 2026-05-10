using MiniChain.Core.Interface;

namespace MiniChain.Core.Services;

public sealed class Node(IBlockchain blockchain) : INode
{
    private readonly List<INode> _nodes = [];
    private readonly IBlockchain _blockchain = blockchain;
    public IBlockchain Blockchain => _blockchain;

    public bool AcceptChain(IReadOnlyList<IBlock> chain)
    {
        if (chain.Count > _blockchain.Blocks.Count && _blockchain.IsValidChain(chain))
        {
            _blockchain.ReplaceChain(chain);
            return true;
        } 
        return false;
    }

    public void Broadcast(IBlock block)
    {
        foreach (var peer in _nodes)
        {
            peer.AcceptChain(_blockchain.Blocks);
        }
    }

    public void Connect(INode peer)
    {
        _nodes.Add(peer);
    }
}
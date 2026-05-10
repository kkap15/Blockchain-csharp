using MiniChain.Core.Services;

namespace MiniChain.Tests;

public class NodeTests
{
    [Fact]
    public void Broadcast_PropagatesChainToPeers()
    {
        var alice = new Wallet();
        var bob = new Wallet();
        var mempool = new Mempool();
        mempool.Submit(SignedTx(alice, bob.PublicKeyHex, 10));
        mempool.Submit(SignedTx(bob, alice.PublicKeyHex, 5));

        var node1 = new Node(new Blockchain());
        var node2 = new Node(new Blockchain());
        node1.Connect(node2);

        node1.Blockchain.MineFromMempool(mempool, 2);
        node1.Broadcast(node1.Blockchain.Tip);

        Assert.Equal(node1.Blockchain.Height, node2.Blockchain.Height);
        Assert.Equal(node1.Blockchain.Tip.ComputeHash(), node2.Blockchain.Tip.ComputeHash());
    }

    [Fact]
    public void AcceptChain_ReturnsFalse_WhenIncomingChainIsShorter()
    {
        var alice = new Wallet();
        var bob = new Wallet();
        var mempool = new Mempool();
        mempool.Submit(SignedTx(alice, bob.PublicKeyHex, 10));
        mempool.Submit(SignedTx(bob, alice.PublicKeyHex, 5));

        var node1 = new Node(new Blockchain());
        var node2 = new Node(new Blockchain());
        node1.Blockchain.MineFromMempool(mempool, 2);

        var accepted = node1.AcceptChain(node2.Blockchain.Blocks);

        Assert.False(accepted);
        Assert.Equal(1, node1.Blockchain.Height);
    }

    [Fact]
    public void AcceptChain_ReturnsFalse_WhenIncomingChainIsInvalid()
    {
        var alice = new Wallet();
        var bob = new Wallet();
        var mempool = new Mempool();
        mempool.Submit(SignedTx(alice, bob.PublicKeyHex, 10));
        mempool.Submit(SignedTx(bob, alice.PublicKeyHex, 5));

        var node1 = new Node(new Blockchain());
        var node2 = new Node(new Blockchain());
        node2.Blockchain.MineFromMempool(mempool, 2);

        var tamperedChain = node2.Blockchain.Blocks.ToList();
        tamperedChain[1] = new Block(
            tamperedChain[1].Index,
            tamperedChain[1].Timestamp,
            tamperedChain[1].PreviousHash,
            []);

        var accepted = node1.AcceptChain(tamperedChain);

        Assert.False(accepted);
        Assert.Equal(0, node1.Blockchain.Height);
    }

    private static Transaction SignedTx(Wallet sender, string to, decimal amount)
    {
        var tx = new Transaction(sender.PublicKeyHex, to, amount);
        tx.Sign(sender);
        return tx;
    }
}

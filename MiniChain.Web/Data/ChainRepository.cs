using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MiniChain.Core.Interface;
using MiniChain.Core.Services;

namespace MiniChain.Web.Data;

public class ChainRepository(ChainDbContext context)
{
    public async Task SaveBlockAsync(IBlock block)
    {
        var entity = new BlockEntity
        {
            Id = block.Index,
            TimestampUnixMilliseconds = block.Timestamp.ToUnixTimeMilliseconds(),
            Index = block.Index,
            Nonce = block.Nonce,
            PreviousBlockHash = block.PreviousHash,
            Transactions = block.Transactions.Select(t => new TransactionEntity
            {
                From = t.From,
                To = t.To,
                Amount = t.Amount,
                Signature = t.Signature,
                IsCoinbase = t.IsCoinbase
            }).ToList()
        };
        context.Blocks.Add(entity);
        await context.SaveChangesAsync();
    }

    public async Task<List<IBlock>> LoadAllBlocksAsync()
    {
        var entities = await context.Blocks
            .Include(b => b.Transactions)
            .OrderBy(b => b.Index)
            .ToListAsync();
        
        return entities.Select(IBlock (e) => new Block(
                e.Index,
                DateTimeOffset.FromUnixTimeMilliseconds(e.TimestampUnixMilliseconds),
                e.PreviousBlockHash,
                e.Transactions.Select(ITransaction (t) => new Transaction(
                    t.From, t.To, t.Amount, t.IsCoinbase, t.Signature
                )).ToList(),
                e.Nonce)).ToList();
    }
}
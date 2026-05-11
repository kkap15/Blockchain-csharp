using System.Collections.Generic;

namespace MiniChain.Web.Data;

public class BlockEntity
{
    public int Id {get; set;}
    public int Index { get; set; }
    public long TimestampUnixMilliseconds { get; set; }
    public string PreviousBlockHash { get; set; } = string.Empty;
    public long Nonce { get; set; }
    public List<TransactionEntity> Transactions { get; set; } = [];
}
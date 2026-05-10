# MiniChain

A blockchain built from scratch in C# / .NET 10 — for learning, not production.

Most "build a blockchain" tutorials are in Python or JavaScript. This one's in C#, partly because it's a language I work in daily and partly because there's surprisingly little material out there for the .NET crowd. The goal is to understand the primitives — hashing, linked blocks, proof-of-work, signed transactions, P2P consensus — by building each one by hand rather than reading about them.

This is a 6-weekend project. Each weekend ships a working, demoable milestone. The current state of the chain is reflected in the table below.

| Weekend | Milestone | Status |
|---:|---|---|
| 1 | Blocks, hashing, tamper detection | ✅ |
| 2 | Proof-of-Work mining | ✅ |
| 3 | Wallets & signed transactions (ECDSA) | ✅ |
| 4 | Mempool & block assembly | ✅ |
| 5 | P2P networking & longest-chain rule | ✅ |
| 6 | CLI wallet & polish | ⬜ |

## Quickstart

```bash
git clone https://github.com/<your-username>/minichain.git
cd minichain
dotnet restore
dotnet test
dotnet run --project MiniChain.Cli
```

You should see a chain printed, the message `Chain valid? True`, then a tampering simulation that flips it to `Chain valid? False`.

## What's in each weekend

### Weekend 1 — Blocks, hashing, tamper detection
- **`Block`** — index, timestamp, previous hash, transactions, nonce. Immutable except for the nonce (which the miner mutates during mining).
- **Deterministic hashing** — every block produces the same hash on every machine, every runtime, every locale. This sounds trivial but is the source of most "why don't my nodes agree" bugs in real chains.
- **`Blockchain`** — an ordered list of blocks with `IsValid()` that detects tampered transactions, broken hash links, and tampered genesis blocks.

### Weekend 2 — Proof-of-Work mining
- **`Miner`** — brute-forces a `Nonce` until the block's SHA-256 hash starts with `difficulty` leading hex zeros. Each hex zero = 4 bits of work, so difficulty 4 ≈ 1-in-65536 chance per attempt.
- **`Blockchain(difficulty)`** — mines every block on `AddBlock` and rejects unmined blocks in `IsValid()`.
- **`IMiner`** — injected into `Blockchain` so tests can swap in a zero-difficulty miner without waiting for real PoW.

### Weekend 3 — Wallets & signed transactions (ECDSA)
- **`Wallet`** — generates a P-256 ECDSA key pair. `Sign(data)` produces a hex signature; `PublicKeyHex` is the DER-encoded public key as hex.
- **`Transaction`** — `From`, `To`, `Amount`, `Signature`. `Sign(wallet)` attaches the sender's signature; `IsValid()` verifies it using the public key in `From` without needing the wallet instance.
- **`Blockchain.IsValid()`** — extended to reject blocks containing any transaction with an invalid or missing signature.
- `CultureInfo.InvariantCulture` used throughout the signable payload so amounts hash identically on every locale.

### Weekend 4 — Mempool & block assembly
- **`Mempool`** — a waiting room for broadcast transactions. `Submit` validates the signature before accepting; `Take(n)` returns the next N pending txs; `Remove` evicts confirmed ones after a block is mined.
- **`Blockchain.MineFromMempool(mempool, count)`** — pulls from the mempool, mines a block, and cleans up confirmed transactions in one call.

### Weekend 5 — P2P networking & longest-chain rule
- **`Node`** — wraps a `Blockchain` and holds a list of peer nodes. `Connect(peer)` links nodes; `Broadcast(block)` pushes your chain to all peers after mining; `AcceptChain(chain)` adopts an incoming chain if it is longer and passes `IsValidChain`.
- **`Blockchain.IsValidChain(chain)`** — validates an external list of blocks using this blockchain's own difficulty and miner, without mutating state.
- **`Blockchain.ReplaceChain(chain)`** — atomically replaces the internal block list (clear + refill).
- Longest-chain rule: a node only accepts a peer's chain if it is strictly longer and valid — the same consensus rule Bitcoin uses.

## Project layout

```
MiniChain.Core/
  Interface/   # IBlock, IBlockchain, ITransaction, IWallet, IMiner, IMempool, INode
  Models/      # Transaction
  Services/    # Block, Blockchain, Miner, Wallet, Mempool, Node
MiniChain.Cli/     # Demo console app
MiniChain.Tests/   # Unit tests
```

## Design notes

**Canonical pipe-delimited serialization for hashing, not JSON.** JSON property ordering isn't guaranteed across runtimes, and a single whitespace difference changes the hash. Two nodes hashing the same logical block must produce byte-identical strings. Bitcoin's serialization is binary and equally strict for the same reason.

**No `Hash` field on `Block`.** The hash is computed on demand from the data, so it can't drift out of sync with the data. If `Hash` were stored as a field, tampering with the data while leaving the field alone would defeat the entire chain.

**Genesis is hard-coded with a fixed Unix timestamp.** Every node must produce the same genesis block. `DateTimeOffset.UtcNow` would give a different value on every node, and they could never agree on the chain's starting point. Bitcoin hard-codes its genesis too — that's why the embedded "Chancellor on brink…" headline is famous.

**`Transaction.IsValid()` verifies without the wallet.** The public key is embedded in `From` as a DER-encoded hex string. Any node can verify any transaction without storing key material — the same property that makes Bitcoin's UTXO model work.

**`IMiner` is injected into `Blockchain`.** This lets tests pass a zero-difficulty miner to skip PoW entirely, keeping the test suite fast without compromising the real mining logic.

**Numeric formatting uses `CultureInfo.InvariantCulture` everywhere.** Default `ToString()` on numbers respects the system locale — `1234.5` becomes `"1234,5"` on a German machine. That's a hash-breaker waiting to happen. Invariant culture sidesteps the entire problem.

## What's deliberately not in scope

- Smart contracts and a VM (large enough to be its own project)
- Merkle trees (a nice-to-have optimization; can be added in a later PR)
- Proof-of-Stake (interesting but doubles the project)
- Persistent peer discovery and NAT traversal (localhost suffices)
- Persistence (in-memory chain only; comes later if at all)

## Tests

```bash
dotnet test
```

The test suite covers:
- Block-level: hash determinism, tamper detection, genesis reproducibility
- Chain-level: linking, validity, PoW enforcement
- Miner: correct leading zeros, idempotency
- Wallet & transactions: signing, verification, rejection of unsigned txs
- Mempool: submit validation, take, remove, integration with `MineFromMempool`
- Node: broadcast propagation, chain acceptance, rejection of shorter/invalid chains

## License

MIT

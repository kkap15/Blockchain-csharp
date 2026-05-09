# MiniChain

A blockchain built from scratch in C# / .NET 10 — for learning, not production.

Most "build a blockchain" tutorials are in Python or JavaScript. This one's in C#, partly because it's a language I work in daily and partly because there's surprisingly little material out there for the .NET crowd. The goal is to understand the primitives — hashing, linked blocks, proof-of-work, signed transactions, P2P consensus — by building each one by hand rather than reading about them.

This is a 6-weekend project. Each weekend ships a working, demoable milestone. The current state of the chain is reflected in the table below.

| Weekend | Milestone | Status |
|---:|---|---|
| 1 | Blocks, hashing, tamper detection | ✅ |
| 2 | Proof-of-Work mining | ✅ |
| 3 | Wallets & signed transactions (ECDSA) | ⬜ |
| 4 | Mempool & block assembly | ⬜ |
| 5 | P2P networking & longest-chain rule | ⬜ |
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

## What's in Weekend 1

- **`Block`** — index, timestamp, previous hash, transactions, nonce. Immutable except for the nonce (which Weekend 2's miner will need to mutate).
- **Deterministic hashing** — every block produces the same hash on every machine, every runtime, every locale. This sounds trivial but is the source of most "why don't my nodes agree" bugs in real chains.
- **`Blockchain`** — an ordered list of blocks with `IsValid()` that detects:
  - tampered transactions in any block,
  - tampered indexes,
  - broken `PreviousHash` links,
  - tampered genesis blocks.

11 unit tests cover the happy path and three distinct tampering scenarios.

## Project layout

```
MiniChain.Core/    # Block, Blockchain, hashing utilities
MiniChain.Cli/     # Demo console app
MiniChain.Tests/   # Unit tests
```

The split between `Core` and `Cli` exists so that Weekend 5's networking project can also reference `Core` without dragging in console-app concerns. Keeping the domain logic free of I/O from day one avoids a painful refactor later.

## Design notes

A few decisions that came up during Weekend 1 and the reasoning behind each:

**Canonical pipe-delimited serialization for hashing, not JSON.** JSON property ordering isn't guaranteed across runtimes, and a single whitespace difference changes the hash. Two nodes hashing the same logical block must produce byte-identical strings. Bitcoin's serialization is binary and equally strict for the same reason.

**No `Hash` field on `Block`.** The hash is computed on demand from the data, so it can't drift out of sync with the data. If `Hash` were stored as a field, tampering with the data while leaving the field alone would defeat the entire chain.

**Genesis is hard-coded with a fixed Unix timestamp.** Every node must produce the same genesis block. `DateTimeOffset.UtcNow` would give a different value on every node, and they could never agree on the chain's starting point. Bitcoin hard-codes its genesis too — that's why the embedded "Chancellor on brink…" headline is famous.

**Account-based model (planned for Weekend 3), not UTXO.** Simpler to reason about, easier to debug. Ethereum took the same path for similar reasons.

**Numeric formatting uses `CultureInfo.InvariantCulture` everywhere.** Default `ToString()` on numbers respects the system locale — `1234.5` becomes `"1234,5"` on a German machine. That's a hash-breaker waiting to happen. Invariant culture sidesteps the entire problem.

## What's deliberately not in scope

To keep each weekend finishable, the following are explicitly out of scope for v1:

- Smart contracts and a VM (large enough to be its own project)
- Merkle trees (a nice-to-have optimization; can be added in a later PR)
- Proof-of-Stake (interesting but doubles the project)
- Persistent peer discovery and NAT traversal (localhost suffices)
- Persistence (in-memory chain only; comes later if at all)

Each of these is a great follow-up. None of them are necessary to learn the core ideas.

## Tests

```bash
dotnet test
```

The test suite covers:
- Block-level: hash determinism, sensitivity to every field, hex format, genesis reproducibility
- Chain-level: initial state, correct linking on append, valid-chain happy path
- Tampering: middle-block transaction tamper, broken `PreviousHash` link, tampered genesis

The tampering tests use reflection to mutate the private `_blocks` list, simulating what an attacker who'd modified the on-disk chain would produce. The public API doesn't allow this kind of mutation — that's the point.

## License

MIT

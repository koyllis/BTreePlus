# BTreePlus

A lightweight, high-performance B+-Tree engine for .NET.
Designed for embedded indexing, fast point lookups, and deterministic on-disk persistence — with zero dependencies.

Supports in-memory and file-backed modes with predictable performance.

---

## Performance (Community Edition)

3,000,000 inserts in 0.95s  
vs SQLite (WAL): 3.10s  
≈ 3.26× faster on this workload

---

## Features — Community Edition

* Sorted B+ Tree with fast point lookups
* Fixed length keys and values for maximum speed
* In-memory or file-backed operation
* Insert, Find, Commit, Close
* Deterministic page layout
* Zero dependencies — pure C#
* Safe single-writer operation

The Community Edition is ideal for embedded systems, local indexing, POS engines, research, file formats, and custom storage projects.

For journaling, concurrency, iterators, range scans, variable-length keys, and compression, see Commercial Edition.

---

## Quick Start Example

```csharp
using BTreePlus;

// Create or open an on-disk index
using var tree = new BTree(
    path: "data.idx",
    keyLength: 8,
    valueLength: 16,
    pageSize: 8
);

// Insert Int64 → 16-byte record
var key = BitConverter.GetBytes(12345L);
var value = new byte[16];
value[0] = 42;

tree.Insert(key, value);

// Lookup
if (tree.Find(key, out var result))
{
    Console.WriteLine("Found value: " + result[0]);
}

// Flush pages to disk
tree.Commit();
```

---

## Advanced Low-Level API

```csharp
bool Insert(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value, bool bLock = true);
bool Find(ReadOnlySpan<byte> key, Span<byte> value, bool bLock = false);

void Bof();
bool Next(out ReadOnlySpan<byte> key, Span<byte> value, bool acquireLock = true);

void Commit();
void Close();
```

---

## Disk Format (Simplified)

* Fixed-size pages (default 4096 bytes)
* Fixed-length keys and values
* Balanced B+-Tree with node splits
* Commit ensures persisted page writes
* Crash tolerance depends on environment (single-writer, no WAL)

For full durability (WAL/MVCC), see Commercial Edition.

---

## Performance Notes

* File-backed with cache enabled: 3,000,000 inserts in 0.95 seconds on a modern NVMe system
* Minimal allocations — avoids GC churn
* Paging strategy optimized for sequential inserts
* Performance scales with page size and key ordering

---

## Commercial Edition

Adds:

* Variable-length keys and values
* Journaling / crash-safe commits
* MVCC or multi-writer concurrency
* Cursors and forward/backward iteration
* Prefix and range scans
* Bulk load and compaction


Recommended for:

* Full database engines
* Document stores
* Ledgers
* POS / ERP systems
* High-throughput indexing on NVMe

Contact: btplus@mmhsys.com

---

## License

Community Edition is licensed under MIT.  
Commercial features require a license.

---

## Roadmap

* Expanded tests and property-based correctness suite
* Optional iterators for Community Edition
* Metadata pages / schema metadata

---

## Status

Active development.
API may see minor refinements before v1.2.5.

Pull requests and discussions welcome.

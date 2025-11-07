# BTreePlus – Public API (Community)

> All buffers are **fixed length**. `key.Length == keyBytes`, `value.Length == dataBytes`.

```csharp
namespace mmh
{
    public sealed class BTree
    {
        // Construction
        public static BTree CreateMemory(
            int keyBytes, int dataBytes, int pageSize,
            bool enableCache, bool large_pat, bool balance);

        public static BTree CreateOrOpen(
            string path,
            int keyBytes, int dataBytes, int pageSize,
            bool enableCache, bool large_pat, bool balance);

        public static BTree Open(
            string path, bool enableCache, bool balance);

        // Operations (Community)
        public bool Insert(ReadOnlySpan<byte> key, ReadOnlySpan<byte> value);
        public bool Find(ReadOnlySpan<byte> key, Span<byte> value);
        public void Commit();
        public void Close();
    }
}
```

## Notes
- **Numeric keys**: encode **big‑endian** and right‑align (last 8 bytes for `Int64`).
- **Text**: UTF‑8, zero‑padded or truncated to fixed size.
- **Durability**: file‑backed + cache → `Commit()` flushes cache/PAT/header.

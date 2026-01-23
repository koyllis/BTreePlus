# BTreePlus File Format (v1 – current implementation)

This document describes the **on-disk layout** for BTreePlus so that a third party can build a **read-only** reader/inspector and so users are never locked out of their data.

---

## 1) Encoding rules

### Byte order
All integer fields are **Little-Endian**.

### Struct packing
Header structs are written/read as packed (`Pack = 1`) blocks (no padding).

---

## 2) File layout

### Disk mode
```
[Header                : Header.Size bytes]
[PAT bitset region     : pat_size_bytes bytes]
[Page region           : N pages, each pagesize bytes]
```

**Page offset**
- Page IDs are **1-based**
- `pageId = 1` is the root page

```
basePagesOffset = Header.Size + pat_size_bytes
PageOffset(pageId) = basePagesOffset + (pageId - 1) * pagesize
```

### Mem-only mode
- `pat_size_bytes = 0`
- Pages begin immediately after the Header

---

## 3) Header (file offset 0)

| Field | Type | Description |
|------|------|-------------|
| pagesize | ushort | Page size in bytes |
| keylen | ushort | Key length (fixed) |
| datalen | ushort | Value length (fixed) |
| lastrec | int | Last allocated page id |
| pat_size_bytes | uint | Size of PAT region |

Notes:
- Header size = `Marshal.SizeOf<Header>()`
- No magic/version field exists in v1

---

## 4) PAT (Page Allocation Table)

- Bitset following the header
- One bit per page
- Bit 0 → pageId 1
- Used to track allocated pages
- Can be ignored by read-only tools

---

## 5) Pages

Each page is exactly `pagesize` bytes.

### PageHeader

| Field | Type | Meaning |
|------|------|--------|
| pagerec | ushort | Number of records in page |
| container | byte | 0 = leaf, non-zero = internal |
| next_ptr | uint | Next leaf page (leaf only) |

---

## 6) Slot layout

Let:
- `K = keylen`
- `D = datalen`

### Internal page
```
[uint32 childPageId][byte[K] key]
```

### Leaf page
```
[byte[K] key][byte[D] value]
```

### Slot count
```
MaxSlots = floor((pagesize - PageHeader.Size) / slotSize)
```

---

## 7) Key normalization

Keys are stored as fixed-width byte arrays:

- If key length > K → truncated
- If key length < K → zero-padded
- Comparison is lexicographic byte order

---

## 8) Leaf traversal

Leaf pages are linked via `next_ptr`.

- `next_ptr = 0` → end of chain
- Enables efficient range scans

---

## 9) Reader guarantees

A third-party reader can:
- Read header
- Locate pages
- Traverse leaves
- Extract all key/value pairs

No write or balancing logic is required.

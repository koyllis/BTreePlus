# BTreePlus – Quickstart (Community Edition)

**BTreePlus** is a high-performance embedded **B+Tree** index engine for .NET.
This repository provides a **minimal usage example** of the **Community Edition** using
**fixed-length `byte[]` keys and values**.

The Community Edition supports:
- `Insert`
- `Find`
- `Commit`
- `Close`

The index can operate **in-memory** or be **persisted to disk** for durability.

---

## Installation

```bash
dotnet add package BTreePlus

using System;
using System.Buffers.Binary;
using System.Text;
using mmh;

class Program
{
    // --- Configure fixed sizes (must match tree creation) ---
    const int KeyLen  = 16;   // bytes
    const int DataLen = 32;   // bytes

    static void Main()
    {
        Console.WriteLine("=== BTreePlus (byte[] sample) ===");

        InMemoryDemo();
        FileBackedDemo("data.btp");

        Console.WriteLine("Done.");
    }

    // --------------------------------------------------------------------
    // 1) In-memory demo: fixed-size numeric keys and ASCII values
    // --------------------------------------------------------------------
    static void InMemoryDemo()
    {
        var bt = BTree.CreateMemory(
            keyBytes:  KeyLen,
            dataBytes: DataLen,
            pageSize:  8,          // logical page=8 → 8*512B physical (per manager.cs)
            enableCache: false,    // cache off in memory mode
            large_pat:  false,
            balance:    true
        );

        // Insert few numeric IDs → values as ASCII
        InsertNum(bt, 1001, "John Doe");
        InsertNum(bt, 1002, "Maria");
        InsertNum(bt, 1003, "Alice");

        // Lookup
        if (FindNum(bt, 1002, out var name))
            Console.WriteLine($"[mem] 1002 → {name}");

        bt.Commit();  // no-op for mem mode when cache=false, safe to call
        bt.Close();
    }

    // --------------------------------------------------------------------
    // 2) File-backed demo: same layout, persistent on disk
    // --------------------------------------------------------------------
    static void FileBackedDemo(string path)
    {
        // Create if missing, else open existing — enable cache to exercise Commit()
        var bt = BTree.CreateOrOpen(
            path,
            keyBytes:   KeyLen,
            dataBytes:  DataLen,
            pageSize:   16,        // 16 * 512B physical
            enableCache: true,     // PageCache on; Commit() will flush pages/PAT/header
            large_pat:   false,
            balance:     true
        );

        InsertAscii(bt, "customer:1001", "John Doe");
        InsertAscii(bt, "customer:1002", "Maria");
        bt.Commit();   // fully durable on disk (per manager.cs Commit/Close)
        bt.Close();

        // Reopen and read back
        var reopened = BTree.Open(path, enableCache: true, balance: true);

        if (FindAscii(reopened, "customer:1001", out var v1))
            Console.WriteLine($"[disk] customer:1001 → {v1}");

        if (FindAscii(reopened, "customer:1002", out var v2))
            Console.WriteLine($"[disk] customer:1002 → {v2}");

        reopened.Close();
    }

    // ========================== Helpers ===============================

    // Create a fixed-length KEY from long: big-endian int64 right-aligned into KeyLen.
    // Remaining leading bytes are zero (keeps numeric sort correct).
    static byte[] MakeNumericKey(long id)
    {
        var key = new byte[KeyLen];
        Span<byte> be = stackalloc byte[8];
        BinaryPrimitives.WriteInt64BigEndian(be, id);
        be.CopyTo(key.AsSpan(KeyLen - 8)); // right-align last 8 bytes
        return key;
    }

    // Create a fixed-length KEY from ASCII/UTF8 text (zero-padded / truncated to KeyLen).
    // Lexicographic compare matches byte-wise order.
    static byte[] MakeAsciiKey(string text)
    {
        var key = new byte[KeyLen];
        int n = Encoding.UTF8.GetBytes(text, 0, text.Length, key, 0);
        if (n > KeyLen) n = KeyLen; // truncated by GetBytes if longer
        // rest already zero
        return key;
    }

    // Create a fixed-length DATA buffer from string (zero-padded / truncated to DataLen).
    static byte[] MakeValue(string text)
    {
        var buf = new byte[DataLen];
        Encoding.UTF8.GetBytes(text, 0, text.Length, buf, 0);
        return buf;
    }

    // ---------------- Insert / Find for numeric key -------------------
    static void InsertNum(BTree bt, long id, string value)
    {
        byte[] k = MakeNumericKey(id);
        byte[] d = MakeValue(value);

        if (!bt.Insert(k, d))               // ReadOnlySpan<byte> from byte[]
            Console.WriteLine($"Insert skipped (duplicate): {id}");
    }

    static bool FindNum(BTree bt, long id, out string value)
    {
        byte[] k = MakeNumericKey(id);
        byte[] d = new byte[DataLen];       // Span<byte> from byte[]
        bool ok = bt.Find(k, d);            // writes into d
        value = ok ? BytesToString(d) : "";
        return ok;
    }

    // --------------- Insert / Find for ASCII key ---------------------
    static void InsertAscii(BTree bt, string keyText, string value)
    {
        byte[] k = MakeAsciiKey(keyText);
        byte[] d = MakeValue(value);

        if (!bt.Insert(k, d))
            Console.WriteLine($"Insert skipped (duplicate): {keyText}");
    }

    static bool FindAscii(BTree bt, string keyText, out string value)
    {
        byte[] k = MakeAsciiKey(keyText);
        byte[] d = new byte[DataLen];
        bool ok = bt.Find(k, d);
        value = ok ? BytesToString(d) : "";
        return ok;
    }

    static string BytesToString(byte[] data)
    {
        // Trim trailing zeros (our padding) for display
        int len = data.Length;
        while (len > 0 && data[len - 1] == 0) len--;
        return Encoding.UTF8.GetString(data, 0, len);
    }
}

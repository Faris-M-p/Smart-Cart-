using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Repositories.Interfaces;

namespace SmartCartPro.Repositories;

public class DocumentNumberRepository : IDocumentNumberRepository
{
    private readonly SmartCartDbContext _db;

    public DocumentNumberRepository(SmartCartDbContext db) => _db = db;

    public async Task<string> NextAsync(int storeId, string sequenceType, CancellationToken ct = default)
    {
        // Join the caller's transaction when there is one (sale / return / GRN flows already run inside one and
        // EF Core refuses a second BeginTransaction on the same context). The row lock from FOR UPDATE is then
        // held until the caller commits, so a rolled-back document also rolls back its number (no gaps).
        var ownsTransaction = _db.Database.CurrentTransaction is null;
        await using var tx = ownsTransaction ? await _db.Database.BeginTransactionAsync(ct) : null;

        var seq = await _db.InvoiceSequences
            .FromSqlRaw(
                @"SELECT * FROM invoice_sequences
                  WHERE fk_store = {0} AND sequencetype = {1}
                  FOR UPDATE",
                storeId, sequenceType)
            .FirstOrDefaultAsync(ct);

        if (seq is null)
        {
            seq = new InvoiceSequence
            {
                FkStore = storeId,
                SequenceType = sequenceType,
                Prefix = string.Empty,
                NextNumber = 1,
                Padding = 6,
                CreatedAt = DateTime.UtcNow
            };
            _db.InvoiceSequences.Add(seq);
            await _db.SaveChangesAsync(ct);
        }

        var current = seq.NextNumber;
        seq.NextNumber++;
        seq.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        if (ownsTransaction)
            await tx!.CommitAsync(ct);

        var numericPart = current.ToString().PadLeft(seq.Padding, '0');
        return string.IsNullOrEmpty(seq.Prefix)
            ? numericPart
            : $"{seq.Prefix}{numericPart}";
    }
}

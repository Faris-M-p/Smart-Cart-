using Microsoft.EntityFrameworkCore;
using SmartCartPro.Data;
using SmartCartPro.Entities;
using SmartCartPro.Services.Interfaces;

namespace SmartCartPro.Services;

/// <summary>
/// Generates formatted document numbers (e.g. "INV-000042") with a row-level lock
/// on the <c>invoice_sequences</c> table to prevent duplicates under concurrency.
/// </summary>
public class DocumentNumberService : IDocumentNumberService
{
    private readonly SmartCartDbContext _db;

    public DocumentNumberService(SmartCartDbContext db) => _db = db;

    public async Task<string> NextAsync(int storeId, string sequenceType, CancellationToken ct = default)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

        // SELECT … FOR UPDATE to lock the row while we increment.
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

        await tx.CommitAsync(ct);

        var numericPart = current.ToString().PadLeft(seq.Padding, '0');
        return string.IsNullOrEmpty(seq.Prefix)
            ? numericPart
            : $"{seq.Prefix}{numericPart}";
    }
}

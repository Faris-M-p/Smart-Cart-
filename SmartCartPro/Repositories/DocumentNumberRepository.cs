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
        await using var tx = await _db.Database.BeginTransactionAsync(ct);

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

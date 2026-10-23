using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZTP_Lab01.Domains.Entities;

namespace ZTP_Lab01.Infarstructure.Repositories;

public interface IForbiddenPhraseRepository
{
    Task<List<ForbiddenPhrase>> GetAllAsync();
    Task<ForbiddenPhrase?> GetByIdAsync(int id);
    Task<ForbiddenPhrase> AddAsync(ForbiddenPhrase phrase);
    Task DeleteAsync(ForbiddenPhrase phrase);
}

public class ForbiddenPhraseRepository : IForbiddenPhraseRepository
{
    private readonly AppDbContext _db;

    public ForbiddenPhraseRepository(AppDbContext db) => _db = db;

    public async Task<List<ForbiddenPhrase>> GetAllAsync() =>
        await _db.ForbiddenPhrases.AsNoTracking().ToListAsync();

    public async Task<ForbiddenPhrase?> GetByIdAsync(int id) =>
        await _db.ForbiddenPhrases.FindAsync(id);

    public async Task<ForbiddenPhrase> AddAsync(ForbiddenPhrase phrase)
    {
        _db.ForbiddenPhrases.Add(phrase);
        await _db.SaveChangesAsync();
        return phrase;
    }

    public async Task DeleteAsync(ForbiddenPhrase phrase)
    {
        _db.ForbiddenPhrases.Remove(phrase);
        await _db.SaveChangesAsync();
    }
}
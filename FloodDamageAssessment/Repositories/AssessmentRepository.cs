using FloodDamageAssessment.Data;
using FloodDamageAssessment.Models;
using Microsoft.EntityFrameworkCore;

namespace FloodDamageAssessment.Repositories;

public class AssessmentRepository : IAssessmentRepository
{
    private readonly AppDbContext _db;
    public AssessmentRepository(AppDbContext db) { _db = db; }

    public async Task<IEnumerable<Assessment>> GetAllAsync()
    {
        return await _db.Assessments.OrderByDescending(a => a.CreatedAt).ToListAsync();
    }

    public async Task<Assessment?> GetByIdAsync(string id)
    {
        return await _db.Assessments.FindAsync(id);
    }

    public async Task UpsertAsync(Assessment a)
    {
        var existing = await _db.Assessments.FindAsync(a.Id);
        if (existing == null)
        {
            _db.Assessments.Add(a);
        }
        else
        {
            _db.Entry(existing).CurrentValues.SetValues(a);
        }
        await _db.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(string id)
    {
        var existing = await _db.Assessments.FindAsync(id);
        if (existing == null) return false;
        _db.Assessments.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> DeleteAllAsync()
    {
        var records = await _db.Assessments.ToListAsync();
        var count = records.Count;
        _db.Assessments.RemoveRange(records);
        await _db.SaveChangesAsync();
        return count;
    }
}

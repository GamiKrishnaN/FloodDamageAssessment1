using FloodDamageAssessment.Models;

namespace FloodDamageAssessment.Repositories;

public interface IAssessmentRepository
{
    Task<IEnumerable<Assessment>> GetAllAsync();
    Task<Assessment?> GetByIdAsync(string id);
    Task UpsertAsync(Assessment a);
    Task<bool> DeleteAsync(string id);
    Task<int> DeleteAllAsync();
}

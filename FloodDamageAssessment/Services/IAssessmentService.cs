using FloodDamageAssessment.Models;

namespace FloodDamageAssessment.Services;

public interface IAssessmentService
{
    Task<IEnumerable<Assessment>> GetAllAsync();
    Task<Assessment?> GetByIdAsync(string id);
    Task UpsertAsync(Assessment a);
    Task<bool> DeleteAsync(string id);
    Task<int> DeleteAllAsync();
}

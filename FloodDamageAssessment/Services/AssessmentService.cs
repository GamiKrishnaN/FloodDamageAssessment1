using FloodDamageAssessment.Models;
using FloodDamageAssessment.Repositories;

namespace FloodDamageAssessment.Services;

public class AssessmentService : IAssessmentService
{
    private readonly IAssessmentRepository _repo;
    public AssessmentService(IAssessmentRepository repo) { _repo = repo; }

    public Task<IEnumerable<Assessment>> GetAllAsync() => _repo.GetAllAsync();
    public Task<Assessment?> GetByIdAsync(string id) => _repo.GetByIdAsync(id);
    public Task UpsertAsync(Assessment a) => _repo.UpsertAsync(a);
    public Task<bool> DeleteAsync(string id) => _repo.DeleteAsync(id);
    public Task<int> DeleteAllAsync() => _repo.DeleteAllAsync();
}

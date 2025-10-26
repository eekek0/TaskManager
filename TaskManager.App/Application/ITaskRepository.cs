using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.App.Domain;

namespace TaskManager.App.Application;

public interface ITaskRepository
{
    Task<int> AddAsync(TaskItem task);
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task<bool> SetCompletedAsync(int id, bool isCompleted);
    Task<bool> DeleteAsync(int id);
}

using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using TaskManager.App.Application;
using TaskManager.App.Domain;

namespace TaskManager.App.Infrastructure;

public class TaskRepository : ITaskRepository
{
    private readonly IDbConnection _conn;

    public TaskRepository(IDbConnection conn)
    {
        _conn = conn;
    }

    public async Task<int> AddAsync(TaskItem task)
    {
        const string sql = @"
INSERT INTO Tasks (Title, Description, IsCompleted, CreatedAt)
VALUES (@Title, @Description, @IsCompleted, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() as int);";

        var id = await _conn.ExecuteScalarAsync<int>(sql, new
        {
            task.Title,
            task.Description,
            task.IsCompleted,
            task.CreatedAt
        });

        return id;
    }

    public Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        const string sql = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Tasks ORDER BY Id;";
        return _conn.QueryAsync<TaskItem>(sql);
    }

    public async Task<TaskItem?> GetByIdAsync(int id)
    {
        const string sql = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Tasks WHERE Id = @Id;";
        return await _conn.QueryFirstOrDefaultAsync<TaskItem>(sql, new { Id = id });
    }

    public async Task<bool> SetCompletedAsync(int id, bool isCompleted)
    {
        const string sql = "UPDATE Tasks SET IsCompleted = @IsCompleted WHERE Id = @Id;";
        var rows = await _conn.ExecuteAsync(sql, new { Id = id, IsCompleted = isCompleted });
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Tasks WHERE Id = @Id;";
        var rows = await _conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }
}

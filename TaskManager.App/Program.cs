using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using TaskManager.App.Domain;
using TaskManager.App.Application;
using TaskManager.App.Infrastructure;

Console.OutputEncoding = System.Text.Encoding.UTF8; 
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var connectionString = config.GetConnectionString("Default");

using var conn = new SqlConnection(connectionString);
await conn.OpenAsync();

ITaskRepository repo = new TaskRepository(conn);

Console.WriteLine("=== Task Manager ===");
Console.WriteLine("Команды: add | list | complete <id> | uncomplete <id> | delete <id> | exit");

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine();
    if (string.IsNullOrWhiteSpace(input)) continue;

    var parts = input.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
    var cmd = parts[0].ToLowerInvariant();

    switch (cmd)
    {
        case "exit":
            return;

        case "add":
            Console.Write("Заголовок: ");
            var title = Console.ReadLine() ?? "";
            Console.Write("Описание: ");
            var desc = Console.ReadLine();
            var task = new TaskItem
            {
                Title = title,
                Description = desc,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            };
            var id = await repo.AddAsync(task);
            Console.WriteLine($"Задача создана с Id={id}");
            break;

        case "list":
            var all = await repo.GetAllAsync();
            foreach (var t in all)
            {
                var status = t.IsCompleted ? "[Выполнено]" : "[Не выполнено]";
                Console.WriteLine($"[{t.Id}] {status} {t.Title} | {t.CreatedAt:u}");
                if (!string.IsNullOrEmpty(t.Description))
                    Console.WriteLine($"    {t.Description}");
            }
            break;

        case "complete":
        case "uncomplete":
            if (parts.Length < 2 || !int.TryParse(parts[1], out var targetId))
            {
                Console.WriteLine("Использование: complete <id> | uncomplete <id>");
                break;
            }
            var state = cmd == "complete";
            var ok = await repo.SetCompletedAsync(targetId, state);
            Console.WriteLine(ok ? "Обновлено" : "Не найдено");
            break;

        case "delete":
            if (parts.Length < 2 || !int.TryParse(parts[1], out var delId))
            {
                Console.WriteLine("Использование: delete <id>");
                break;
            }
            var deleted = await repo.DeleteAsync(delId);
            Console.WriteLine(deleted ? "Удалено" : "Не найдено");
            break;

        default:
            Console.WriteLine("Неизвестная команда");
            break;
    }
}

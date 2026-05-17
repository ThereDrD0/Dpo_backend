using System.Text.Json;
using TaskManager.Api.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var jsonOptions = new JsonSerializerOptions
{
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = true
};

var dataPath = Path.Combine(app.Environment.ContentRootPath, "Data", "tasks.json");
Directory.CreateDirectory(Path.GetDirectoryName(dataPath)!);

if (!File.Exists(dataPath))
{
    File.WriteAllText(dataPath, "[]");
}

List<TaskModel> ReadTasks()
{
    var json = File.ReadAllText(dataPath);
    return JsonSerializer.Deserialize<List<TaskModel>>(json, jsonOptions) ?? [];
}

void SaveTasks(List<TaskModel> tasks)
{
    var json = JsonSerializer.Serialize(tasks, jsonOptions);
    File.WriteAllText(dataPath, json);
}

var tasksApi = app.MapGroup("/api/tasks").WithTags("Tasks");

tasksApi.MapGet("", () => ReadTasks());

tasksApi.MapGet("/completed", () =>
{
    var tasks = ReadTasks();
    return tasks.Where(task => task.IsCompleted);
});

tasksApi.MapGet("/{id:int}", (int id) =>
{
    var task = ReadTasks().FirstOrDefault(task => task.Id == id);
    return task is null ? Results.NotFound() : Results.Ok(task);
});

tasksApi.MapPost("", (TaskModel newTask) =>
{
    var tasks = ReadTasks();

    newTask.Id = tasks.Count == 0 ? 1 : tasks.Max(task => task.Id) + 1;
    newTask.IsCompleted = false;

    tasks.Add(newTask);
    SaveTasks(tasks);

    return Results.Created($"/api/tasks/{newTask.Id}", newTask);
});

tasksApi.MapPut("/{id:int}", (int id, TaskModel updatedTask) =>
{
    var tasks = ReadTasks();
    var task = tasks.FirstOrDefault(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    task.Title = updatedTask.Title;
    task.Description = updatedTask.Description;
    task.IsCompleted = updatedTask.IsCompleted;

    SaveTasks(tasks);

    return Results.Ok(task);
});

tasksApi.MapDelete("/{id:int}", (int id) =>
{
    var tasks = ReadTasks();
    var task = tasks.FirstOrDefault(task => task.Id == id);

    if (task is null)
    {
        return Results.NotFound();
    }

    tasks.Remove(task);
    SaveTasks(tasks);

    return Results.NoContent();
});

app.Run();

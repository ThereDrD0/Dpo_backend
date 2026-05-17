using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public class StudentCatalogService : IStudentCatalogService
{
    private readonly List<Student> _students =
    [
        new Student { Id = 1, FullName = "Ivan Petrov", Group = "ip-21", Direction = "Backend" },
        new Student { Id = 2, FullName = "Anna Smirnova", Group = "ip-21", Direction = "Frontend" },
        new Student { Id = 3, FullName = "Pavel Sokolov", Group = "is-22", Direction = "Testing" },
        new Student { Id = 4, FullName = "Maria Volkova", Group = "is-22", Direction = "DevOps" }
    ];

    public IReadOnlyList<Student> GetAll()
    {
        return _students;
    }

    public IReadOnlyList<Student> GetByGroup(string group)
    {
        return _students
            .Where(x => string.Equals(x.Group, group, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public Student? GetById(string group, int id)
    {
        return _students.FirstOrDefault(x =>
            x.Id == id && string.Equals(x.Group, group, StringComparison.OrdinalIgnoreCase));
    }
}

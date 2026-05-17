using TaskManager.Api.Models;

namespace TaskManager.Api.Services;

public interface IStudentCatalogService
{
    IReadOnlyList<Student> GetAll();

    IReadOnlyList<Student> GetByGroup(string group);

    Student? GetById(string group, int id);
}

using System.Text;
using TaskManager.Api.Middleware;
using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCampusServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestAudit();

app.MapGet("/", () => Results.Json(new
{
    app = "CampusRouteLab",
    routes = new[]
    {
        "/students",
        "/students/{group}",
        "/students/{group}/{id}",
        "/reports/{section?}",
        "/portal/{module=home}/{page=index}/{id?}",
        "/files/{**path}",
        "/routes",
        "/diag/lifetimes",
        "/diag/lifetimes/check",
        "/diag/request-services",
        "/diag/app-services"
    }
}));

app.MapGet("/students", (IStudentCatalogService catalog) =>
{
    return Results.Json(catalog.GetAll());
});

app.MapGet("/students/{group}", (string group, IStudentCatalogService catalog) =>
{
    var students = catalog.GetByGroup(group);
    return Results.Json(new { group, count = students.Count, students });
});

app.MapGet("/students/{group}/{id:int}", (string group, int id, IStudentCatalogService catalog) =>
{
    var student = catalog.GetById(group, id);

    if (student is null)
    {
        return Results.NotFound(new { message = "Student not found", group, id });
    }

    return Results.Json(student);
});

app.MapGet("/reports/{section?}", (string? section, DiagnosticsReportService report) =>
{
    section ??= "overview";

    return Results.Json(new
    {
        section,
        generatedAt = DateTime.Now,
        text = report.BuildReport(section)
    });
});

app.MapGet("/portal/{module=home}/{page=index}/{id?}", (string module, string page, string? id) =>
{
    return Results.Json(new
    {
        module,
        page,
        id = id ?? "not set"
    });
});

app.MapGet("/files/{**path}", (string? path) =>
{
    return Results.Json(new
    {
        path = path ?? "",
        parts = (path ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries)
    });
});

app.MapGet("/routes", (EndpointDataSource endpoints) =>
{
    var routes = endpoints.Endpoints
        .OfType<RouteEndpoint>()
        .Select(x => new
        {
            pattern = x.RoutePattern.RawText,
            displayName = x.DisplayName
        })
        .OrderBy(x => x.pattern)
        .ToList();

    return Results.Json(routes);
});

app.MapGet("/diag/lifetimes", (
    IAppInfoService appInfo,
    IRequestContextService requestContext,
    ITransientMarkerService transientMarker,
    DiagnosticsReportService report) =>
{
    return Results.Json(new
    {
        singleton = new
        {
            appInfo.AppInstanceId,
            appInfo.StartedAt
        },
        scoped = new
        {
            requestContext.RequestId,
            requestContext.CreatedAt
        },
        transient = new
        {
            transientMarker.MarkerId,
            transientMarker.CreatedAt
        },
        report = report.BuildReport("lifetimes")
    });
});

app.MapGet("/diag/lifetimes/check", (
    IRequestContextService requestContext,
    ITransientMarkerService first,
    HttpContext context) =>
{
    var second = context.RequestServices.GetRequiredService<ITransientMarkerService>();

    return Results.Json(new
    {
        requestContext.RequestId,
        firstTransient = first.MarkerId,
        secondTransient = second.MarkerId,
        transientIsDifferent = first.MarkerId != second.MarkerId
    });
});

app.MapGet("/diag/request-services", (HttpContext context) =>
{
    var requestContext = context.RequestServices.GetRequiredService<IRequestContextService>();
    var transientMarker = context.RequestServices.GetRequiredService<ITransientMarkerService>();

    return Results.Json(new
    {
        from = "HttpContext.RequestServices",
        requestContext.RequestId,
        transientMarker.MarkerId
    });
});

app.MapGet("/diag/app-services", () =>
{
    var appInfo = app.Services.GetRequiredService<IAppInfoService>();

    return Results.Json(new
    {
        from = "app.Services",
        appInfo.AppInstanceId,
        appInfo.StartedAt
    });
});

app.Run();

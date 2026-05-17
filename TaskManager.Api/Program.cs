using System.Text;
using TaskManager.Api.Configuration;
using TaskManager.Api.Middleware;
using TaskManager.Api.Models;
using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddXmlFile("portal.xml", optional: false, reloadOnChange: true)
    .AddIniFile("notifications.ini", optional: false, reloadOnChange: true)
    .AddTextFile("customsettings.txt")
    .AddInMemoryCollection(new Dictionary<string, string?>
    {
        ["Notifications:Sender"] = "memory-notify@college.local"
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCampusServices();
builder.Services.Configure<PortalOptions>(builder.Configuration.GetSection("Portal"));
builder.Services.Configure<NotificationOptions>(builder.Configuration.GetSection("Notifications"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRequestAudit();
app.UsePortalHeaders();

app.MapGet("/", () => Results.Json(new
{
    app = "CampusHub.ConfigCenter",
    text = "Practice 6: configuration diagnostics",
    routes = new[]
    {
        "/config/raw",
        "/config/section/portal",
        "/config/tree?section=Portal",
        "/config/connection",
        "/config/providers",
        "/config/custom",
        "/config/bind",
        "/config/options",
        "/config/effective",
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

app.MapGet("/config/raw", (IConfiguration config) =>
{
    return Results.Json(new
    {
        title = config["Portal:Title"],
        supportEmail = config["Portal:SupportEmail"],
        notificationsSender = config["Notifications:Sender"],
        customOwner = config["Custom:Owner"]
    });
});

app.MapGet("/config/section/portal", (IConfiguration config) =>
{
    var section = config.GetSection("Portal");

    return Results.Json(new
    {
        key = section.Key,
        title = section["Title"],
        semester = section["Semester"],
        supportEmail = section["SupportEmail"],
        campusName = section["CampusName"],
        admin = new
        {
            name = section["Admin:Name"],
            email = section["Admin:Email"]
        },
        modules = section.GetSection("Modules").GetChildren().Select(x => x.Value).ToList()
    });
});

app.MapGet("/config/tree", (IConfiguration config, string? section) =>
{
    section ??= "Portal";

    object BuildTree(IConfigurationSection item)
    {
        var children = item.GetChildren().ToList();

        if (children.Count == 0)
        {
            return new { item.Key, item.Value };
        }

        return new
        {
            item.Key,
            Children = children.Select(BuildTree).ToList()
        };
    }

    return Results.Json(BuildTree(config.GetSection(section)));
});

app.MapGet("/config/connection", (IConfiguration config) =>
{
    return Results.Json(new
    {
        defaultConnection = config.GetConnectionString("DefaultConnection")
    });
});

app.MapGet("/config/providers", (IConfiguration config) =>
{
    if (config is not IConfigurationRoot root)
    {
        return Results.Json(Array.Empty<object>());
    }

    var providers = root.Providers
        .Select((provider, index) => new
        {
            order = index + 1,
            providerType = provider.GetType().FullName
        })
        .ToList();

    return Results.Json(providers);
});

app.MapGet("/config/custom", (IConfiguration config) =>
{
    return Results.Json(new
    {
        owner = config["Custom:Owner"],
        version = config["Custom:Version"],
        note = config["Custom:Note"]
    });
});

app.MapGet("/config/bind", (IConfiguration config) =>
{
    var portal = new PortalOptions();
    config.GetSection("Portal").Bind(portal);

    return Results.Json(portal);
});

app.MapGet("/config/options", (
    Microsoft.Extensions.Options.IOptions<PortalOptions> portal,
    Microsoft.Extensions.Options.IOptions<NotificationOptions> notifications) =>
{
    return Results.Json(new
    {
        portal = portal.Value,
        notifications = notifications.Value
    });
});

app.MapGet("/config/effective", (IConfiguration config, IWebHostEnvironment env) =>
{
    return Results.Json(new
    {
        environment = env.EnvironmentName,
        values = new
        {
            portalTitle = new
            {
                value = config["Portal:Title"],
                winner = "commandLineArgs from launchSettings.json"
            },
            portalSupportEmail = new
            {
                value = config["Portal:SupportEmail"],
                winner = "environmentVariables from launchSettings.json"
            },
            notificationsSender = new
            {
                value = config["Notifications:Sender"],
                winner = "AddInMemoryCollection after notifications.ini"
            }
        }
    });
});

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

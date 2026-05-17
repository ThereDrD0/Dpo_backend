using System.Text;
using TaskManager.Api.Middleware;
using TaskManager.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddStudentPortalServices();

var servicesList = builder.Services
    .Take(15)
    .Select(s => new ServiceInfo(
        s.ServiceType.FullName ?? s.ServiceType.Name,
        s.Lifetime.ToString(),
        s.ImplementationType?.FullName ?? s.ImplementationInstance?.GetType().FullName ?? "factory"))
    .ToList();

var servicesCount = builder.Services.Count;

var app = builder.Build();

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Use(async (context, next) =>
{
    var started = DateTime.Now;
    Console.WriteLine($"Start: {context.Request.Method} {context.Request.Path}");

    context.Response.OnStarting(() =>
    {
        var ms = (DateTime.Now - started).TotalMilliseconds;
        context.Response.Headers["X-Request-Time-Ms"] = ms.ToString("0");
        return Task.CompletedTask;
    });

    await next();

    Console.WriteLine($"Finish: {context.Request.Path} {context.Response.StatusCode}");
});

app.UseWhen(
    context => context.Request.Query["trace"] == "true",
    traceApp =>
    {
        traceApp.Use(async (context, next) =>
        {
            Console.WriteLine($"Trace branch: {context.Request.Path}");
            context.Response.Headers["X-Debug-Trace"] = "true";
            await next();
        });
    });

app.MapWhen(
    context => context.Request.Query.ContainsKey("format") && context.Request.Query["format"] == "plain",
    plainApp =>
    {
        plainApp.Run(async context =>
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Plain text branch handled this request.");
        });
    });

app.Map("/tools", tools =>
{
    tools.Map("/time", timeApp =>
    {
        timeApp.Run(async context =>
        {
            var dateTime = context.RequestServices.GetRequiredService<IDateTimeService>();
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync($"Current time: {dateTime.GetTime()}");
        });
    });

    tools.Map("/date", dateApp =>
    {
        dateApp.Run(async context =>
        {
            var dateTime = context.RequestServices.GetRequiredService<IDateTimeService>();
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync($"Current date: {dateTime.GetDate()}");
        });
    });

    tools.Map("/info", infoApp =>
    {
        infoApp.Run(async context =>
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("StudentPortal.Diagnostics: tools branch is working.");
        });
    });
});

app.Map("/secure", secure =>
{
    secure.UseToken("study2026");

    secure.Map("/report", reportApp =>
    {
        reportApp.Run(async context =>
        {
            context.Response.ContentType = "text/plain; charset=utf-8";
            await context.Response.WriteAsync("Secure report: access granted.");
        });
    });
});

app.MapGet("/", () =>
{
    return """
StudentPortal.Diagnostics

Routes:
/tools/time
/tools/date
/tools/info
/tools/time?trace=true
/anything?format=plain
/secure/report
/secure/report?token=study2026
/env
/di/services
""";
});

app.MapGet("/env", (IEnvironmentReportService report) => report.GetReport());

app.MapGet("/di/services", () =>
{
    var text = new StringBuilder();
    text.AppendLine($"Services count: {servicesCount}");
    text.AppendLine();

    foreach (var item in servicesList)
    {
        text.AppendLine($"{item.ServiceType} | {item.Lifetime} | {item.ImplementationType}");
    }

    return text.ToString();
});

app.Run();

record ServiceInfo(string ServiceType, string Lifetime, string ImplementationType);

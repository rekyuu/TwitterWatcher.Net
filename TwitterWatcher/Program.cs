using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using TwitterWatcher;
using TwitterWatcher.Models;
using TwitterWatcher.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Add services to the container.
    builder.Services.AddControllers();

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add health check
    builder.Services.AddHealthChecks();
    
    // Add PostgreSQL
    builder.Services.AddDbContext<DatabaseContext>(options => { options.UseNpgsql(Config.PsqlConnectionString); });
    
    // Add Watcher service
    builder.Services.AddHostedService<WatcherService>();

    // Add Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        //.MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:yy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"));

    var app = builder.Build();

    // app.UseSerilogRequestLogging();

    // Run database migrations
    using (IServiceScope scope = app.Services.CreateScope())
    {
        IServiceProvider services = scope.ServiceProvider;
        DatabaseContext context = services.GetRequiredService<DatabaseContext>();

        context.Database.Migrate();
    }

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.MapControllers();
    app.MapHealthChecks("/health");

    app.Run();
}
catch (Exception ex)
{
    string type = ex.GetType().Name;
    if (type.Equals("HostAbortedException", StringComparison.Ordinal)) throw;

    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
using System.CommandLine;
using System.Reflection;

using Tada.Cli.Extensions;
using Tada.Cli.Types;
using Tada.Cli.Models;

namespace Tada.Cli.Commands;

public class NewSolutionCommand : Command
{

    public NewSolutionCommand() : base("new", "creates a new tada solution")
    {
        var name = new Argument<string>
            ("name", "solution name");

        var dbType = new Option<DbTypes>("--dbtype", () => DbTypes.SqlServer, "Select the db type");

        this.Add(name);
        this.Add(dbType);

        this.SetHandler((nameValue, dbTypeValue) => Execute(nameValue, dbTypeValue), name, dbType);
    }

    public void Execute(string solutionName, DbTypes dbType)
    {
        ConsoleWriter.Start("Creating Project");
        if (string.IsNullOrEmpty(solutionName))
        {
            ConsoleWriter.Error("name is required");
        }

        var shell = new ProcessShell();
        shell.Execute(Assembly.GetExecutingAssembly().GetResourceText("Scripts.NewSolution.ps1")
            .Replace("$SOLUTION_NAME", solutionName));

        var config = ConfigurationLoader.LoadTadaFile()!;

        if (dbType == DbTypes.Mysql)
        {
            config.Database = new DatabaseConfiguration
            {
                Type = DbTypes.Mysql,
                Name = $"{solutionName.ToLower()}db",
                Username = "root",
                Password = "P@ssw0rd",
                ContainerId = $"{solutionName.ToLower()}-db"
            };

            shell.Execute(Assembly.GetExecutingAssembly().GetResourceText("Scripts.AddMySql.ps1")
                .Replace("$SOLUTION_NAME", solutionName));

            shell.Execute("dotnet", $"user-secrets set \"ConnectionStrings:Database\" \"server=localhost;port=3308;database={config.Database.Name}db;uid={config.Database.Username};pwd={config.Database.Password};ConvertZeroDateTime=True\" -p \"./src/2.Infrastructure/Database/{solutionName}.Infrastructure.Database/{solutionName}.Infrastructure.Database.csproj\"");
        }
        else if (dbType == DbTypes.Postgres)
        {
            config.Database = new DatabaseConfiguration
            {
                Type = DbTypes.Postgres,
                Name = $"{solutionName.ToLower()}db",
                Username = "root",
                Password = "P@ssw0rd",
                ContainerId = $"{solutionName.ToLower()}-db"
            };

            shell.Execute(Assembly.GetExecutingAssembly().GetResourceText("Scripts.AddPostgres.ps1")
                .Replace("$SOLUTION_NAME", solutionName));

            shell.Execute("dotnet", $"user-secrets set \"ConnectionStrings:Database\" \"Server=localhost;Port=5432;Database={config.Database.Name}db;Username={config.Database.Username};Password={config.Database.Password};\" -p \"./src/2.Infrastructure/Database/{solutionName}.Infrastructure.Database/{solutionName}.Infrastructure.Database.csproj\"");
        }
        else if (dbType == DbTypes.SqlServer)
        {
            config.Database = new DatabaseConfiguration
            {
                Type = DbTypes.SqlServer,
                Name = solutionName,
                Username = "sa",
                Password = "P@ssw0rd",
                ContainerId = $"{solutionName.ToLower()}-db"
            };

            shell.Execute(Assembly.GetExecutingAssembly().GetResourceText("Scripts.AddSqlServer.ps1")
                .Replace("$SOLUTION_NAME", solutionName));

            shell.Execute("dotnet", $"user-secrets set \"ConnectionStrings:Database\" \"Server=localhost,1433;Database={config.Database.Name};User Id={config.Database.Username};Password={config.Database.Password};TrustServerCertificate=True;\" -p \"./src/2.Infrastructure/Database/{solutionName}.Infrastructure.Database/{solutionName}.Infrastructure.Database.csproj\"");
        }

        shell.DeleteFileInSubDirectories("Class1.cs");
        shell.DeleteFileInSubDirectories("UnitTest1.cs");
        shell.DeleteFileInSubDirectories("WeatherForecastController.cs");
        shell.DeleteFileInSubDirectories("WeatherForecast.cs");
        shell.DeleteFileInSubDirectories("Worker.cs");

        var apiProgramFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"src/4.Presentation/{solutionName}.Presentation.Api/Program.cs");
        FileUpdater.UpdateContent(apiProgramFilePath,
            new Dictionary<string, string>()
        {
            {
                "var builder = WebApplication.CreateBuilder(args);",
                @$"
using {solutionName}.Domain.Core;
using {solutionName}.Presentation.Api.Attributes;
using {solutionName}.Presentation.Api.Filters;
using {solutionName}.Presentation.Api.Startup;

var builder = WebApplication.CreateBuilder(args);"
            },
            {
                "builder.Services.AddControllers();",
                @"
builder.Services.AddControllers(options =>
{
    options.Filters.Add<ExceptionHandlerFilter>();
    options.Filters.Add<ModelStateFilter>();

    options.ModelBinderProviders.Insert(0, new RequestPatchBinderProvider());
}).AddJsonOptions(options => Json.SetOptions(options.JsonSerializerOptions));

builder.Services.RegisterAuthentication(builder.Configuration);
builder.Services.Register(builder.Configuration);
        "
            },
            {
                "builder.Services.AddEndpointsApiExplorer();",
                "builder.Services.RegisterSwagger(builder.Configuration);"
            },
            {
                "builder.Services.AddSwaggerGen();",
                ""
            },
            {
                "app.Run();",
                @"
app.RegisterMiddleware();

app.Run();
        "
            }
        });

        var apiProjFilePath = Path.Combine(Directory.GetCurrentDirectory(), $"src/4.Presentation/{solutionName}.Presentation.Api/{solutionName}.Presentation.Api.csproj");

        FileUpdater.UpdateContent(apiProjFilePath, "</Project>", @"
	<PropertyGroup>
		<GenerateDocumentationFile>true</GenerateDocumentationFile>
		<NoWarn>$(NoWarn);1591</NoWarn>
	</PropertyGroup>
</Project>
        ");

        var editorConfigPath = Path.Combine(Directory.GetCurrentDirectory(), $".editorconfig");

        FileUpdater.UpdateContent(editorConfigPath, "[*.cs]", @"
[*.cs]
csharp_style_namespace_declarations = file_scoped:error
dotnet_diagnostic.IDE0005.severity = warning

# private fields naming rules
dotnet_naming_rule.private_members_with_underscore.symbols  = private_fields
dotnet_naming_rule.private_members_with_underscore.style    = prefix_underscore
dotnet_naming_rule.private_members_with_underscore.severity = warning

dotnet_naming_symbols.private_fields.applicable_kinds           = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.prefix_underscore.capitalization = camel_case
dotnet_naming_style.prefix_underscore.required_prefix = _
        ");

        FileUpdater.UpdateContent(editorConfigPath, "end_of_line = crlf", @"end_of_line = lf");

        ConfigurationLoader.Update(config);

        shell.DotnetFormat();

        ConsoleWriter.Success("Project Created");
    }
}
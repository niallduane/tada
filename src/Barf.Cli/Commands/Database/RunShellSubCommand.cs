using System.CommandLine;
using System.Diagnostics;

using Barf.Cli.Types;

namespace Barf.Cli.Commands;

public class RunShellSubCommand : Command
{
    public RunShellSubCommand() : base("shell", "creates a shell for executing sql commands against the database")
    {
        this.SetHandler(() => Execute());
    }
    public void Execute()
    {
        var config = ConfigurationLoader.LoadBarfFile();
        string ns = (config?.Namespace ?? "barf")!;

        ConsoleWriter.Start("Starting database shell");

        var shell = new ProcessShell();
        if (config.Database.Type == DbType.Mysql)
        {
            shell.Execute("docker", $"compose exec {config.Database.ContainerId} mysql -u {config.Database.Username} -p{config.Database.Password} {config.Database.Name}");
            ConsoleWriter.Success("Database shell closed");
        }
        else if (config.Database.Type == DbType.Postgres)
        {
            shell.Execute("docker", $"compose exec {config.Database.ContainerId} psql -U {config.Database.Username} -d {config.Database.Name}");
            ConsoleWriter.Success("Database shell closed");
        }
        else if (config.Database.Type == DbType.SqlServer)
        {
            shell.Execute("docker", $"compose exec {config.Database.ContainerId} /opt/mssql-tools/bin/sqlcmd -S localhost -U {config.Database.Username} -P {config.Database.Password}  -d {config.Database.Name}");
            ConsoleWriter.Success("Database shell closed");
        }
    }
}
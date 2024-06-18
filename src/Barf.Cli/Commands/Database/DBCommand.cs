using System.CommandLine;

namespace Barf.Cli.Commands.Database
{
    public class DBCommand : Command
    {
        public DBCommand() : base("db")
        {
            this.AddCommand(new AddMigrationSubCommand());
            this.AddCommand(new UpdateDatabaseSubCommand());
        }
    }
}
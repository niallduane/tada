using System.CommandLine;

namespace Tada.Cli.Commands.Add;



public class AddServiceSubCommand : Command
{
    public enum ServiceTemplateTypes
    {
        Basic = 1,
        ExcludeEntity = 2,
        Full = 3
    }
    public AddServiceSubCommand() : base("service", "add service code")
    {
        var name = new Argument<string>
            ("name", "Service name");

        var includeEntity = new Option<bool>("--entity", () => false, "include a database entity");
        var excludeValidations = new Option<bool>("--no-validations", () => false, "include a database entity");
        var excludeRepository = new Option<bool>("--no-repository", () => false, "include a database repository");
        var excludeController = new Option<bool>("--no-controller", () => false, "exclude a service presentation controller");

        var templateType = new Option<ServiceTemplateTypes>("--type", () => ServiceTemplateTypes.Basic, "Select the service type");

        this.Add(name);
        this.Add(name);
        this.Add(includeEntity);
        this.Add(excludeValidations);
        this.Add(excludeRepository);
        this.Add(excludeController);

        this.SetHandler(Execute, name, includeEntity, excludeValidations, excludeRepository, excludeController);
    }

    public void Execute(string name, bool includeEntity, bool excludeValidations, bool excludeRepository, bool excludeController)
    {
        var config = ConfigurationLoader.LoadTadaFile();
        var ns = config?.Namespace ?? "tada";

        ConsoleWriter.Start($"Adding {name} service");

        var shell = new ProcessShell();
        shell.Execute("dotnet", "new install Tada.TemplatePack");
        shell.Execute("dotnet", $"new tada-domain-service -n {name} --nameSpace {ns} --idType \"{config?.Templates.Entity.IdType}\" --idTypeNameSpace \"{config?.Templates.Entity.IdTypeNameSpace}\"");

        shell.Execute("dotnet", $"new tada-service-basic -n {name} --nameSpace {ns} --use_validators {(!excludeValidations).ToString().ToLower()} --use_repository {(!excludeRepository).ToString().ToLower()} --idType \"{config?.Templates.Entity.IdType}\" --idTypeNameSpace \"{config?.Templates.Entity.IdTypeNameSpace}\"");
        if (includeEntity) 
        {
            var entityExists = FileUpdater.FileExists($"src/2.Infrastructure/Database/{ns}.Infrastructure.Database/Entities/{name}.cs");
            if (!entityExists) {
                shell.Execute("dotnet", $"new tada-database-entity -n {name} --nameSpace {ns} -o \"./src/2.Infrastructure/Database/\" --idType \"{config?.Templates.Entity.IdType}\" --idTypeNameSpace \"{config?.Templates.Entity.IdTypeNameSpace}\"");

                AddEntitySubCommand.UpdateContent(name, ns);
            }
        }
        
        if (!excludeRepository)
        {
            var repositoryExists = FileUpdater.FileExists($"src/2.Infrastructure/Database/{ns}.Infrastructure.Database.Repositories/{name}s/{name}Repository.cs");
            if (!repositoryExists) 
            {
                shell.Execute("dotnet", $"new tada-database-repository -n {name} --nameSpace {ns} -o \"./src/2.Infrastructure/Database/\" --idType \"{config?.Templates.Entity.IdType}\" --idTypeNameSpace \"{config?.Templates.Entity.IdTypeNameSpace}\"");
                AddRepositorySubCommand.UpdateContent(name, ns);
            }
        }
        if (!excludeController)
        {
            shell.Execute("dotnet", $"new tada-service-controller -n {name} --nameSpace {ns} --idType \"{config?.Templates.Entity.IdType}\" --idTypeNameSpace \"{config?.Templates.Entity.IdTypeNameSpace}\"");
        }

        UpdateContent(name, ns, excludeValidations);

        shell.DotnetFormat();

        ConsoleWriter.Success($"{name} service added");
    }

    public static void UpdateContent(string name, string nameSpace, bool excludeValidations = false)
    {
        var serviceRegistration = Path.Combine(Directory.GetCurrentDirectory(), $"src/3.Services/{nameSpace}.Services/DependencyRegistration.cs");
        var injectValidators = (excludeValidations) ? "":
        $@"
        services.AddTransient<{name}s.Validators.Create{name}Validator>();
        services.AddTransient<{name}s.Validators.Update{name}Validator>();
        services.AddTransient<{name}s.Validators.Upsert{name}Validator>();
        ";

        FileUpdater.UpdateContent(serviceRegistration,
        @"// <!-- tada injection token -->",
        $@"// <!-- tada injection token -->
        services.AddTransient<Domain.Services.{name}s.I{name}Service, {name}s.{name}Service>();
        {injectValidators}
        ");
    }
}
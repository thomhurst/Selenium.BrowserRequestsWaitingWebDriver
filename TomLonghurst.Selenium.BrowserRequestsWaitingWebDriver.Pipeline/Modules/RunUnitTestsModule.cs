using ModularPipelines;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Options;

namespace TomLonghurst.Selenium.BrowserRequestsWaitingWebDriver.Pipeline.Modules;

public class RunUnitTestsModule : Module<List<CommandResult>>
{
    protected override void Configure(ModuleConfigurationBuilder module) => module
        .WithTimeout(TimeSpan.FromMinutes(5));

    protected override async Task<List<CommandResult>> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var results = new List<CommandResult>();
        var git = await context.Tools.Git.Information.GetRequiredInfoAsync(cancellationToken);

        foreach (var unitTestProjectFile in git.Root
                     .GetFiles(file => file.Path.EndsWith("Tests.csproj", StringComparison.OrdinalIgnoreCase)))
        {
            results.Add(await context.Tools.DotNet.TestVstestAsync(new DotNetTestVstestOptions
            {
                ProjectSolution = unitTestProjectFile.Path,
            }, new CommandExecutionOptions
            {
                Logging = new CommandLoggingOptions
                {
                    Verbosity = CommandLogVerbosity.InputOnly,
                    ShowStandardError = true,
                },
            }, cancellationToken));
        }

        return results;
    }
}

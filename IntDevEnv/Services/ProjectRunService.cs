namespace IntDevEnv.Services;

internal sealed class ProjectRunService
{
	private const string ManagedTargetFramework = "net11.0";

	public string CreateRunCommand(WorkspaceItem project, bool debug)
	{
		string configuration = ProjectBuildService.GetBuildConfigurationName(debug);
		string csProjectPath = Path.Combine(project.Path, "bin", configuration, ManagedTargetFramework, $"{project.Name}.exe");

		if (project.ProjectType == "cs" && !File.Exists(csProjectPath))
			return $"dotnet run --framework {ManagedTargetFramework} --configuration {configuration}";

		string executablePath = project.ProjectType switch
		{
			"cs" => csProjectPath,
			"cpp" or "c" => Path.Combine(project.Path, "bin", configuration, $"{project.Name}.exe"),
			_ => string.Empty,
		};

		if (string.IsNullOrWhiteSpace(executablePath) || !File.Exists(executablePath))
			return string.Empty;

		return QuoteArgument(executablePath);
	}

	private static string QuoteArgument(string value)
	{
		return $"\"{value}\"";
	}
}


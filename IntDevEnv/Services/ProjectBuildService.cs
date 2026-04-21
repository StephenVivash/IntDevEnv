using System.Text;
using System.Text.Json;

namespace IntDevEnv.Services;

internal sealed class ProjectBuildService(ProjectBuildServiceOptions options, Action<string> appendOutput)
{
	private static readonly HashSet<string> CleanArtifactFolderNames =
		new(StringComparer.OrdinalIgnoreCase) { "bin", "obj", ".vs", "TestResults", "AppPackages", "BundleArtifacts" };

	private readonly HashSet<string> _projectFileTypeSet =
		options.ProjectFileTypes.Select(extension => $".{extension}".ToLowerInvariant()).ToHashSet();
	private readonly Queue<WorkspaceItem> _buildQueue = [];
	private ActiveBuildContext? _activeBuildContext;
	private bool _isBuildAllRunning;
	private bool _isSingleBuildRunning;
	private bool _queuedBuildRebuild;
	private bool _queuedDebug;
	private int _buildAllSucceededCount;
	private int _buildAllFailedCount;

	public bool IsBuildRunning => _isBuildAllRunning || _isSingleBuildRunning;

	public void Build(
		WorkspaceItem currentProject,
		IEnumerable<WorkspaceItem> workspaceProjects,
		bool debug,
		bool rebuild,
		Func<BuildProcessStart, bool> tryStartProcess)
	{
		_buildQueue.Clear();
		_isBuildAllRunning = false;
		_isSingleBuildRunning = true;
		_queuedBuildRebuild = rebuild;
		_queuedDebug = debug;
		_activeBuildContext = null;

		foreach (WorkspaceItem project in GetProjectsForBuild(currentProject, workspaceProjects))
			_buildQueue.Enqueue(project);

		BuildNextQueuedProject(tryStartProcess);
	}

	public void BuildAll(
		IEnumerable<WorkspaceItem> projects,
		bool debug,
		bool rebuild,
		Func<BuildProcessStart, bool> tryStartProcess)
	{
		_buildQueue.Clear();
		_buildAllSucceededCount = 0;
		_buildAllFailedCount = 0;
		_queuedBuildRebuild = rebuild;
		_queuedDebug = debug;
		_activeBuildContext = null;

		foreach (WorkspaceItem project in OrderProjectsForBuild(projects))
			_buildQueue.Enqueue(project);

		_isBuildAllRunning = true;
		_isSingleBuildRunning = false;
		BuildNextQueuedProject(tryStartProcess);
	}

	public void OnBuildProcessExited(
		uint exitCode,
		Action<Action> dispatch,
		Func<BuildProcessStart, bool> tryStartProcess)
	{
		bool succeeded = exitCode == 0;
		ActiveBuildContext? buildContext = _activeBuildContext;
		_activeBuildContext = null;
		string projectName = buildContext?.ProjectName ?? "Build";

		if (succeeded && buildContext is not null)
			succeeded = CopyRuntimeDependencies(buildContext);

		if (_isBuildAllRunning)
		{
			if (succeeded)
				_buildAllSucceededCount++;
			else
				_buildAllFailedCount++;

			appendOutput(Environment.NewLine);
			dispatch(() => BuildNextQueuedProject(tryStartProcess));
			return;
		}

		if (_isSingleBuildRunning)
		{
			if (succeeded && _buildQueue.Count > 0)
			{
				appendOutput(Environment.NewLine);
				dispatch(() => BuildNextQueuedProject(tryStartProcess));
				return;
			}

			_buildQueue.Clear();
			_isSingleBuildRunning = false;
			ReportSingleBuildOutcome(projectName, succeeded);
		}
	}

	public CleanWorkspaceResult CleanWorkspaceBuildArtifacts(IReadOnlyList<WorkspaceItem> projects)
	{
		List<string> deletedDirectories = [];
		List<string> failedDirectories = [];

		foreach (WorkspaceItem project in projects)
		{
			foreach (string artifactDirectory in EnumerateBuildArtifactDirectories(project.Path))
			{
				try
				{
					DeleteDirectoryTree(artifactDirectory);
					deletedDirectories.Add(Path.GetRelativePath(options.WorkspacePath, artifactDirectory));
				}
				catch (Exception ex)
				{
					failedDirectories.Add($"{Path.GetRelativePath(options.WorkspacePath, artifactDirectory)}: {ex.Message}");
				}
			}
		}

		StringBuilder message = new();
		if (deletedDirectories.Count == 0 && failedDirectories.Count == 0)
		{
			message.AppendLine("Clean completed. No bin or obj folders were found.");
			return new CleanWorkspaceResult(message.ToString());
		}

		message.AppendLine($"Clean completed. Removed {deletedDirectories.Count} folder(s).");
		foreach (string deletedDirectory in deletedDirectories.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
			message.AppendLine($"[deleted] {deletedDirectory}");

		if (failedDirectories.Count > 0)
		{
			message.AppendLine();
			message.AppendLine($"Failed to remove {failedDirectories.Count} folder(s).");
			foreach (string failedDirectory in failedDirectories.OrderBy(path => path, StringComparer.OrdinalIgnoreCase))
				message.AppendLine($"[error] {failedDirectory}");
		}

		return new CleanWorkspaceResult(message.ToString());
	}

	public static string GetBuildConfigurationName(bool debug)
	{
		return debug ? "Debug" : "Release";
	}

	private List<WorkspaceItem> OrderProjectsForBuild(IEnumerable<WorkspaceItem> projects)
	{
		List<WorkspaceItem> projectList = projects.ToList();
		Dictionary<string, WorkspaceItem> projectsByPath = projectList.ToDictionary(
			project => NormalizeProjectPath(project.Path),
			StringComparer.OrdinalIgnoreCase);
		List<WorkspaceItem> orderedProjects = [];
		HashSet<string> visiting = new(StringComparer.OrdinalIgnoreCase);
		HashSet<string> visited = new(StringComparer.OrdinalIgnoreCase);

		foreach (WorkspaceItem project in projectList)
			Visit(project);

		return orderedProjects;

		void Visit(WorkspaceItem project)
		{
			string normalizedProjectPath = NormalizeProjectPath(project.Path);
			if (visited.Contains(normalizedProjectPath))
				return;

			if (!visiting.Add(normalizedProjectPath))
			{
				appendOutput($"[warning] Cyclic project reference detected for {project.Name}.{Environment.NewLine}");
				return;
			}

			ProjectSettings projectSettings = LoadProjectSettings(project.Path);
			foreach (string referencePath in ResolveProjectReferencePaths(project.Path, projectSettings.GetReferences()))
				if (projectsByPath.TryGetValue(NormalizeProjectPath(referencePath), out WorkspaceItem? referencedProject))
					Visit(referencedProject);

			visiting.Remove(normalizedProjectPath);
			visited.Add(normalizedProjectPath);
			orderedProjects.Add(project);
		}
	}

	private List<WorkspaceItem> GetProjectsForBuild(WorkspaceItem project, IEnumerable<WorkspaceItem> workspaceProjects)
	{
		Dictionary<string, WorkspaceItem> workspaceProjectsByPath = workspaceProjects
			.Where(item => item.IsProject)
			.ToDictionary(item => NormalizeProjectPath(item.Path), StringComparer.OrdinalIgnoreCase);
		List<WorkspaceItem> projects = [];
		HashSet<string> added = new(StringComparer.OrdinalIgnoreCase);

		Collect(project);
		return OrderProjectsForBuild(projects);

		void Collect(WorkspaceItem currentProject)
		{
			string normalizedProjectPath = NormalizeProjectPath(currentProject.Path);
			if (!added.Add(normalizedProjectPath))
				return;

			projects.Add(currentProject);

			ProjectSettings projectSettings = LoadProjectSettings(currentProject.Path);
			foreach (string referencePath in ResolveProjectReferencePaths(currentProject.Path, projectSettings.GetReferences()))
				if (workspaceProjectsByPath.TryGetValue(NormalizeProjectPath(referencePath), out WorkspaceItem? referencedProject))
					Collect(referencedProject);
		}
	}

	private void BuildNextQueuedProject(Func<BuildProcessStart, bool> tryStartProcess)
	{
		while (_buildQueue.Count > 0)
		{
			WorkspaceItem project = _buildQueue.Dequeue();
			appendOutput($"Building project {project.Name}{Environment.NewLine}{Environment.NewLine}");

			if (BuildProject(project, _queuedDebug, _queuedBuildRebuild, tryStartProcess))
				return;

			if (!_isBuildAllRunning)
			{
				_buildQueue.Clear();
				_isSingleBuildRunning = false;
				ReportSingleBuildOutcome(project.Name, false);
				return;
			}

			_buildAllFailedCount++;
			appendOutput($"{project.Name} failed.{Environment.NewLine}");
			appendOutput(Environment.NewLine);
		}

		_isBuildAllRunning = false;
		appendOutput($"Build all completed. {_buildAllSucceededCount} succeeded, {_buildAllFailedCount} failed.{Environment.NewLine}");
	}

	private bool BuildProject(
		WorkspaceItem project,
		bool debug,
		bool rebuild,
		Func<BuildProcessStart, bool> tryStartProcess)
	{
		ProjectSettings projectSettings = LoadProjectSettings(project.Path);
		EnsureBuildDirectories(project.Path, debug, project.ProjectType);
		string command = CreateBuildCommand(project.Name, project.Path, project.ProjectType, debug, rebuild, projectSettings);

		if (string.IsNullOrWhiteSpace(command))
		{
			appendOutput("No source files to build." + Environment.NewLine);
			return false;
		}

		_activeBuildContext = CreateActiveBuildContext(project.Name, project.Path, project.ProjectType, debug, projectSettings);
		if (tryStartProcess(new BuildProcessStart(command, project.Path)))
			return true;

		_activeBuildContext = null;
		return false;
	}

	private string GetSourceFiles(string projectPath, string projectType)
	{
		string sourceExtension = $".{projectType}";
		IEnumerable<string> sourceFiles = EnumerateProjectFiles(projectPath)
			.Where(filePath => string.Equals(Path.GetExtension(filePath), sourceExtension, StringComparison.OrdinalIgnoreCase))
			.OrderBy(filePath => Path.GetRelativePath(projectPath, filePath), StringComparer.OrdinalIgnoreCase)
			.Select(filePath => QuoteArgument(Path.GetRelativePath(projectPath, filePath)));
		return string.Join(" ", sourceFiles);
	}

	private string CreateBuildCommand(
		string projectName,
		string projectPath,
		string projectType,
		bool debug,
		bool rebuild,
		ProjectSettings? projectSettings = null)
	{
		projectSettings ??= new ProjectSettings();
		bool clr = false;
		string sourceFiles = GetSourceFiles(projectPath, projectType);
		if (string.IsNullOrWhiteSpace(sourceFiles))
			return string.Empty;

		if (projectType == "cs")
			return "dotnet build" + (debug ? "" : " -c Release");

		if (projectType == "cpp" || projectType == "c")
		{
			string configuration = GetBuildConfigurationName(debug);
			string compilerFlags = projectSettings.GetCompilerFlags(debug);
			string defineFlags = projectSettings.GetDefineFlags(debug);
			string outputType = projectSettings.GetOutputType();
			bool buildDll = string.Equals(outputType, "dll", StringComparison.OrdinalIgnoreCase);
			string outputExtension = buildDll ? "dll" : "exe";
			string outputPath = QuoteOptionPath(Path.Combine("bin", configuration, $"{projectName}.{outputExtension}"));
			string importLibraryPath = QuoteOptionPath(Path.Combine("bin", configuration, $"{projectName}.lib"));
			string objFolder = EnsureTrailingDirectorySeparator(Path.Combine("obj", configuration));
			string pdbPath = QuoteOptionPath(Path.Combine("obj", configuration, "vc145.pdb"));
			string runtimeFlag = debug ? "/MDd" : "/MD";
			string cppCompilerFlags = projectType == "cpp" ? "/std:c++20 " : string.Empty;
			string[] referenceProjectPaths = ResolveProjectReferencePaths(projectPath, projectSettings.GetReferences());
			string referenceIncludeFlags = string.Join(" ",
				referenceProjectPaths.Select(referencePath => $"/I{QuoteOptionPath(referencePath)}"));
			string referenceLibraryFlags = string.Join(" ",
				GetReferenceLibraryPaths(referenceProjectPaths, configuration)
					.Select(QuoteArgument));

			if (!string.IsNullOrWhiteSpace(compilerFlags) &&
				compilerFlags.Split(' ', StringSplitOptions.RemoveEmptyEntries)
				.Any(flag => string.Equals(flag, "/clr", StringComparison.OrdinalIgnoreCase)))
				clr = true;

			if ((projectType == "cpp") && !clr)
				cppCompilerFlags += "/EHsc ";

			string defaultCompilerFlags =
				$"{cppCompilerFlags}{runtimeFlag}";
			if (buildDll)
				defaultCompilerFlags += " /LD";

			string effectiveCompilerFlags = JoinCommandParts(defaultCompilerFlags, compilerFlags, defineFlags);
			effectiveCompilerFlags = JoinCommandParts(
				effectiveCompilerFlags,
				$"/Fo{objFolder}",
				$"/Fd{pdbPath}");

			string clrInclude = clr
				? $"/I{QuoteOptionPath($"{options.ClrPath}include")} /I{QuoteOptionPath($"{options.ClrPath}include\\um")} "
				: string.Empty;
			string clrLibs = clr
				? $"/LIBPATH:\"{options.ClrPath}lib\\um\\x64\" "
				: string.Empty;

			string outputFlags = buildDll
				? $"/OUT:{outputPath} /IMPLIB:{importLibraryPath}"
				: $"/OUT:{outputPath}";

			return JoinCommandParts(
				$"\"{options.VsPath}bin\\Hostx64\\x64\\cl\"",
				effectiveCompilerFlags,
				$"/I{QuoteOptionPath($"{options.VsPath}include")}",
				$"/I{QuoteOptionPath($"{options.SdkPath}include\\{options.SdkVersion}\\ucrt")}",
				clrInclude,
				referenceIncludeFlags,
				sourceFiles,
				"/link",
				$"/LIBPATH:\"{options.VsPath}lib\\x64\"",
				$"/LIBPATH:\"{options.SdkPath}lib\\{options.SdkVersion}\\ucrt\\x64\"",
				$"/LIBPATH:\"{options.SdkPath}lib\\{options.SdkVersion}\\um\\x64\"",
				clrLibs,
				outputFlags,
				referenceLibraryFlags);
		}

		return string.Empty;
	}

	private string[] ResolveProjectReferencePaths(string projectPath, IReadOnlyList<string> references)
	{
		if (references.Count == 0)
			return [];

		List<string> resolvedPaths = [];
		HashSet<string> uniquePaths = new(StringComparer.OrdinalIgnoreCase);

		foreach (string reference in references)
		{
			string absoluteReferencePath = Path.GetFullPath(Path.Combine(projectPath, reference));
			if (!Directory.Exists(absoluteReferencePath))
			{
				appendOutput($"[warning] Referenced project path not found: {reference}{Environment.NewLine}");
				continue;
			}

			if (uniquePaths.Add(absoluteReferencePath))
				resolvedPaths.Add(absoluteReferencePath);
		}

		return [.. resolvedPaths];
	}

	private IEnumerable<string> GetReferenceLibraryPaths(IEnumerable<string> referenceProjectPaths, string configuration)
	{
		foreach (string referenceProjectPath in referenceProjectPaths)
		{
			string projectName = Path.GetFileName(referenceProjectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
			ProjectSettings referenceSettings = LoadProjectSettings(referenceProjectPath);

			if (!string.Equals(referenceSettings.GetOutputType(), "dll", StringComparison.OrdinalIgnoreCase))
				continue;

			yield return Path.Combine(referenceProjectPath, "bin", configuration, $"{projectName}.lib");
		}
	}

	private ActiveBuildContext CreateActiveBuildContext(
		string projectName,
		string projectPath,
		string projectType,
		bool debug,
		ProjectSettings projectSettings)
	{
		if (projectType != "cpp" && projectType != "c")
			return new ActiveBuildContext(projectName, projectPath, debug, []);

		if (string.Equals(projectSettings.GetOutputType(), "dll", StringComparison.OrdinalIgnoreCase))
			return new ActiveBuildContext(projectName, projectPath, debug, []);

		string configuration = GetBuildConfigurationName(debug);
		string[] referenceProjectPaths = ResolveProjectReferencePaths(projectPath, projectSettings.GetReferences());
		string[] runtimeDependencySourcePaths = referenceProjectPaths
			.Select(referenceProjectPath =>
			{
				string referenceProjectName = Path.GetFileName(referenceProjectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
				ProjectSettings referenceSettings = LoadProjectSettings(referenceProjectPath);
				if (!string.Equals(referenceSettings.GetOutputType(), "dll", StringComparison.OrdinalIgnoreCase))
					return null;

				return Path.Combine(referenceProjectPath, "bin", configuration, $"{referenceProjectName}.dll");
			})
			.Where(path => !string.IsNullOrWhiteSpace(path))
			.Select(path => path!)
			.Distinct(StringComparer.OrdinalIgnoreCase)
			.ToArray();

		return new ActiveBuildContext(projectName, projectPath, debug, runtimeDependencySourcePaths);
	}

	private bool CopyRuntimeDependencies(ActiveBuildContext buildContext)
	{
		if (buildContext.RuntimeDependencySourcePaths.Count == 0)
			return true;

		string configuration = GetBuildConfigurationName(buildContext.Debug);
		string outputDirectory = Path.Combine(buildContext.ProjectPath, "bin", configuration);

		try
		{
			Directory.CreateDirectory(outputDirectory);

			foreach (string sourcePath in buildContext.RuntimeDependencySourcePaths)
			{
				if (!File.Exists(sourcePath))
				{
					appendOutput($"[error] Missing referenced DLL: {sourcePath}{Environment.NewLine}");
					return false;
				}

				string destinationPath = Path.Combine(outputDirectory, Path.GetFileName(sourcePath));
				File.Copy(sourcePath, destinationPath, true);
				appendOutput($"Copied {Path.GetFileName(sourcePath)} to {outputDirectory}{Environment.NewLine}");
			}
		}
		catch (Exception ex)
		{
			appendOutput($"[error] Failed to copy referenced DLLs: {ex.Message}{Environment.NewLine}");
			return false;
		}

		return true;
	}

	private ProjectSettings LoadProjectSettings(string projectPath)
	{
		string projectSettingsPath = Path.Combine(projectPath, "project.json");
		if (!File.Exists(projectSettingsPath))
			return new ProjectSettings();

		try
		{
			return JsonSerializer.Deserialize<ProjectSettings>(
				File.ReadAllText(projectSettingsPath),
				new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					AllowTrailingCommas = true,
				}) ?? new ProjectSettings();
		}
		catch (JsonException ex)
		{
			appendOutput($"[error] Invalid project.json: {ex.Message}{Environment.NewLine}");
			return new ProjectSettings();
		}
	}

	private IEnumerable<string> EnumerateProjectFiles(string rootPath)
	{
		foreach (string filePath in Directory.EnumerateFiles(rootPath))
			if (_projectFileTypeSet.Contains(Path.GetExtension(filePath).ToLowerInvariant()))
				yield return filePath;

		foreach (string directoryPath in Directory.EnumerateDirectories(rootPath))
		{
			if (options.ExcludedFolderNames.Contains(Path.GetFileName(directoryPath)))
				continue;

			foreach (string filePath in EnumerateProjectFiles(directoryPath))
				yield return filePath;
		}
	}

	private static IEnumerable<string> EnumerateBuildArtifactDirectories(string rootPath)
	{
		Stack<string> pendingDirectories = new();
		pendingDirectories.Push(rootPath);

		while (pendingDirectories.Count > 0)
		{
			string currentDirectory = pendingDirectories.Pop();

			IEnumerable<string> childDirectories;
			try
			{
				childDirectories = Directory.EnumerateDirectories(currentDirectory);
			}
			catch
			{
				continue;
			}

			foreach (string childDirectory in childDirectories)
			{
				string directoryName = Path.GetFileName(childDirectory);
				if (CleanArtifactFolderNames.Contains(directoryName))
				{
					yield return childDirectory;
					continue;
				}

				pendingDirectories.Push(childDirectory);
			}
		}
	}

	private static void DeleteDirectoryTree(string directoryPath)
	{
		foreach (string filePath in Directory.EnumerateFiles(directoryPath, "*", SearchOption.AllDirectories))
			File.SetAttributes(filePath, FileAttributes.Normal);

		foreach (string nestedDirectoryPath in Directory.EnumerateDirectories(directoryPath, "*", SearchOption.AllDirectories))
			File.SetAttributes(nestedDirectoryPath, FileAttributes.Normal);

		File.SetAttributes(directoryPath, FileAttributes.Normal);
		Directory.Delete(directoryPath, true);
	}

	private static void EnsureBuildDirectories(string projectPath, bool debug, string projectType)
	{
		if (projectType != "cpp" && projectType != "c")
			return;

		string configuration = GetBuildConfigurationName(debug);
		Directory.CreateDirectory(Path.Combine(projectPath, "bin", configuration));
		Directory.CreateDirectory(Path.Combine(projectPath, "obj", configuration));
	}

	private void ReportSingleBuildOutcome(string projectName, bool succeeded)
	{
		if (succeeded)
			appendOutput($"{Environment.NewLine}Build completed successfully.{Environment.NewLine}");
		else
			appendOutput($"{Environment.NewLine}Build failed.{Environment.NewLine}");
	}

	private static string JoinCommandParts(params string?[] parts)
	{
		return string.Join(" ", parts.Where(part => !string.IsNullOrWhiteSpace(part)).Select(part => part!.Trim()));
	}

	private static string QuoteArgument(string value)
	{
		return $"\"{value}\"";
	}

	private static string QuoteOptionPath(string value)
	{
		return $"\"{value}\"";
	}

	private static string EnsureTrailingDirectorySeparator(string value)
	{
		if (string.IsNullOrEmpty(value))
			return value;

		char trailingSeparator = value[^1];
		if (trailingSeparator == Path.DirectorySeparatorChar || trailingSeparator == Path.AltDirectorySeparatorChar)
			return value;

		return value + Path.DirectorySeparatorChar;
	}

	private static string NormalizeProjectPath(string projectPath)
	{
		return Path.GetFullPath(projectPath)
			.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
	}
}

internal sealed record ProjectBuildServiceOptions(
	IReadOnlyList<string> ProjectFileTypes,
	string WorkspacePath,
	string VsPath,
	string SdkPath,
	string ClrPath,
	string SdkVersion,
	IReadOnlySet<string> ExcludedFolderNames);

internal sealed record BuildProcessStart(string Command, string InitialDirectory);

internal sealed record ActiveBuildContext(
	string ProjectName,
	string ProjectPath,
	bool Debug,
	IReadOnlyList<string> RuntimeDependencySourcePaths);

internal sealed record CleanWorkspaceResult(string Message);

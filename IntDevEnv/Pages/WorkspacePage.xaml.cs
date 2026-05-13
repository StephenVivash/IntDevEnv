using IntDevEnv.Services;
using IntDevEnv.Views;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.Json;

namespace IntDevEnv.Pages;

public partial class WorkspacePage : ContentPage
{
	static List<String> _projectFileTypes = ["cs", "cpp", "c", "hpp", "h"];
	static String _workspacePath = @"C:\Src\IntDevEnv\Workspace";
	static String _workspaceFile = @"Workspace1";

	static String _vsBasePath = @"C:\Program Files\Microsoft Visual Studio\18\Insiders\"; // Community Insiders
	static String _msvcPath = @"VC\Tools\MSVC\";
	//static String _vsVersion = @"14.44.35207";
	//static String _vsVersion = @"14.50.35717";
	static String _vsVersion = @"14.51.36231";
	static String _sdkVersion = @"10.0.19041.0";
	//static String _sdkVersion = @"10.0.26100.0";

	static String _vsPath = $"{_vsBasePath}{_msvcPath}{_vsVersion}\\";
	static String _sdkPath = @"C:\Program Files (x86)\Windows Kits\10\";
	static String _clrPath = @"C:\Program Files (x86)\Windows Kits\NETFXSDK\4.8\";

	private readonly ObservableCollection<WorkspaceItem> _workspace = [];
	private readonly HashSet<string> _projectFileTypeSet =
		_projectFileTypes.Select(extension => $".{extension}".ToLowerInvariant()).ToHashSet();
	private static readonly HashSet<string> _excludedFolderNames =
		new(StringComparer.OrdinalIgnoreCase) { "bin", "obj", "debug", "release" };
	private readonly ProjectBuildService _buildService;
	private readonly ProjectRunService _runService = new();

	private string? _currentSourceFilePath;
	private string _currentSourceLineEnding = Environment.NewLine;
	private WorkspaceItem? _currentProject;

	// /JMC /permissive- /ifcOutput "x64\Debug\" /GS /W3 /Zc:wchar_t /ZI /Gm- /Od /sdl /Fd"x64\Debug\vc145.pdb" /Zc:inline /fp:precise /D "_DEBUG" /D "_CONSOLE" /D "_UNICODE" /D "UNICODE" /errorReport:prompt /WX- /Zc:forScope /RTC1 /Gd /MDd /std:c++20 /FC /Fa"x64\Debug\" /EHsc /nologo /Fo"x64\Debug\" /Fp"x64\Debug\Test1.pch" /diagnostics:column 

#if WINDOWS
	//private readonly ManagedMfc _mfc;
	private readonly ManagedConsole _managedConsole;
	private Microsoft.UI.Xaml.Controls.TextBox? _configuredSourceTextBox;
#endif
	private readonly object _outputLock = new();
	private readonly StringBuilder _pendingOutput = new();
	private readonly StringBuilder _displayedOutput = new();
	private readonly IDispatcherTimer _outputTimer;

	public WorkspacePage()
	{
		Behaviors.Add(new RegisterInViewDirectoryBehavior());
		_buildService = new ProjectBuildService(
			new ProjectBuildServiceOptions(
				_projectFileTypes,
				_workspacePath,
				_vsPath,
				_sdkPath,
				_clrPath,
				_sdkVersion,
				_excludedFolderNames),
			AppendOutput);
		InitializeComponent();
		colWorkspace.ItemsSource = _workspace;
		ConfigureSourceEditor();
		_outputTimer = Dispatcher.CreateTimer();
		_outputTimer.Interval = TimeSpan.FromMilliseconds(50);
		_outputTimer.Tick += (_, _) => FlushPendingOutput();
		_outputTimer.Start();

		horMenu.Create(ePages.eWorkspace, StackOrientation.Horizontal);
		verMenu.Create(ePages.eWorkspace, StackOrientation.Vertical);
		horMenu.ProjectChanged += OnProjectPickerChanged;
		verMenu.ProjectChanged += OnProjectPickerChanged;
		LoadWorkspace();

#if WINDOWS
		try
		{
			//_mfc = new ManagedMfc();
			//_mfc.setString("Hello, World!");
			_managedConsole = new ManagedConsole(new ConsoleHostSink(this));
		}
		catch (Exception ex)
		{
			System.Diagnostics.Debug.WriteLine(ex);
			throw;
		}
#endif
	}

	private void ConfigureSourceEditor()
	{
#if WINDOWS
		edtSource.HandlerChanged += (_, _) => ConfigureWindowsSourceEditor();
		ConfigureWindowsSourceEditor();
#endif
	}

#if WINDOWS
	private void ConfigureWindowsSourceEditor()
	{
		if (edtSource.Handler?.PlatformView is not Microsoft.UI.Xaml.Controls.TextBox textBox)
			return;

		if (!ReferenceEquals(_configuredSourceTextBox, textBox))
		{
			if (_configuredSourceTextBox is not null)
				_configuredSourceTextBox.KeyDown -= SourceTextBox_KeyDown;

			_configuredSourceTextBox = textBox;
			textBox.KeyDown += SourceTextBox_KeyDown;
		}

		textBox.TextWrapping = Microsoft.UI.Xaml.TextWrapping.NoWrap;
		textBox.IsSpellCheckEnabled = false;
		textBox.IsTextPredictionEnabled = false;
	}

	private void SourceTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
	{
		if (e.Key != Windows.System.VirtualKey.Tab ||
			sender is not Microsoft.UI.Xaml.Controls.TextBox textBox)
			return;

		int selectionStart = textBox.SelectionStart;
		string text = textBox.Text ?? string.Empty;
		string updatedText = text.Remove(selectionStart, textBox.SelectionLength).Insert(selectionStart, "\t");
		textBox.Text = updatedText;
		edtSource.Text = updatedText;
		textBox.SelectionStart = selectionStart + 1;
		textBox.SelectionLength = 0;
		e.Handled = true;
	}
#endif

	private bool DebugMode()
	{
		return horMenu.Mode == "Debug";
	}

	protected override void OnSizeAllocated(double width, double height)
	{
		if ((width == -1) || (height == -1))
			return;

#if ANDROID || IOS
		if (width > height)
		{
			horMenu.IsVisible = false;
			verMenu.IsVisible = true;
			//meditationView.WidthRequest = 200;
		}
		else
		{
			horMenu.IsVisible = true;
			verMenu.IsVisible = false;
			//meditationView.WidthRequest = width;
		}
#else
		horMenu.IsVisible = true;
		verMenu.IsVisible = false;
#endif

		base.OnSizeAllocated(width, height);
	}

	private void OnWorkspaceSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is not WorkspaceItem item)
			return;

		if (item.IsWorkspace || item.IsFolder)
		{
			ClearEditor();
			return;
		}

		if (item.IsFile && !string.Equals(item.Path, _currentSourceFilePath, StringComparison.OrdinalIgnoreCase))
		{
			LoadEditorFile(item.Path);
			return;
		}

		if (item.IsProject)
		{
			string? projectFilePath = GetProjectEditorFilePath(item);
			if (string.IsNullOrWhiteSpace(projectFilePath))
				ClearEditor();
			else if (!string.Equals(projectFilePath, _currentSourceFilePath, StringComparison.OrdinalIgnoreCase))
				LoadEditorFile(projectFilePath);

			return;
		}
	}

	public void OnClean()
	{
		if (_buildService.IsBuildRunning)
		{
			ResetOutput("Cannot clean while a build is running.");
			return;
		}

		SaveCurrentSourceFile();

		WorkspaceItem[] projects = _workspace
			.Where(item => item.IsProject)
			.ToArray();

		if (projects.Length == 0)
		{
			ResetOutput("No projects to clean.");
			return;
		}

		ResetOutput($"Cleaning build artifacts for {projects.Length} project(s)...{Environment.NewLine}");
		_ = CleanWorkspaceAsync(projects);
	}

	public void OnBuild()
	{
		Build(false);
	}

	public void OnRebuild()
	{
		Build(true);
	}

	public void OnBuildAll()
	{
		BuildAll(false);
	}

	public void OnRebuildAll()
	{
		BuildAll(true);
	}

	public void OnRun()
	{
		if (_currentProject == null)
			return;

		string cmd = _runService.CreateRunCommand(_currentProject, DebugMode());
		if (string.IsNullOrWhiteSpace(cmd))
		{
			ResetOutput("No executable to run.");
			return;
		}

		ResetOutput("");

#if WINDOWS
		try
		{
			_managedConsole.StartConsole(cmd, 0, 0, 0, _currentProject.Path);
		}
		catch (Exception ex)
		{
			AppendOutput($"[error] {ex}{Environment.NewLine}");
		}
#else
		AppendOutput("ManagedConsole is only available on Windows." + Environment.NewLine);
#endif
	}

	public void Build(bool rebuild)
	{
		if (_currentProject == null)
			return;

		SaveCurrentSourceFile();
		ResetOutput("");
		_buildService.Build(_currentProject, _workspace.Where(item => item.IsProject), DebugMode(), rebuild, TryStartBuildProcess);
	}

	public void BuildAll(bool rebuild)
	{
		SaveCurrentSourceFile();

		List<WorkspaceItem> projects = _workspace
			.Where(item => item.IsProject)
			.ToList();

		if (projects.Count == 0)
		{
			ResetOutput("No projects to build.");
			return;
		}

		ResetOutput("");
		_buildService.BuildAll(projects, DebugMode(), rebuild, TryStartBuildProcess);
	}

	private async Task CleanWorkspaceAsync(IReadOnlyList<WorkspaceItem> projects)
	{
		try
		{
			CleanWorkspaceResult result = await Task.Run(() => _buildService.CleanWorkspaceBuildArtifacts(projects));
			Dispatcher.Dispatch(() => ResetOutput(result.Message));
		}
		catch (Exception ex)
		{
			Dispatcher.Dispatch(() => ResetOutput($"Clean failed.{Environment.NewLine}[error] {ex.Message}{Environment.NewLine}"));
		}
	}

	internal void OnBuildProcessExited(uint exitCode)
	{
		_buildService.OnBuildProcessExited(exitCode, action => Dispatcher.Dispatch(action), TryStartBuildProcess);
	}

	private bool TryStartBuildProcess(BuildProcessStart processStart)
	{
#if WINDOWS
		try
		{
			_managedConsole.StartConsole(processStart.Command, 0, 0, 0, processStart.InitialDirectory);
			return true;
		}
		catch (Exception ex)
		{
#if DEBUG
			AppendOutput($"[error] {ex}{Environment.NewLine}");
#endif
			return false;
		}
#else
		AppendOutput("ManagedConsole is only available on Windows." + Environment.NewLine);
		return false;
#endif
	}

	private void UpdateCurrentProject(WorkspaceItem selectedItem)
	{
		WorkspaceItem? previousProject = _currentProject;

		if (selectedItem.IsWorkspace)
		{
			_currentProject = null;
			SyncProjectPickers();
			return;
		}

		if (selectedItem.IsProject)
		{
			_currentProject = selectedItem;
			SyncProjectPickers();
			SaveSelectedProjectIfChanged(previousProject);
			return;
		}

		_currentProject = _workspace
			.Where(item => item.IsProject && IsSameOrChildPath(item.Path, selectedItem.Path))
			.OrderByDescending(item => item.Path.Length)
			.FirstOrDefault();
		SyncProjectPickers();
		SaveSelectedProjectIfChanged(previousProject);
	}

	private void SaveSelectedProjectIfChanged(WorkspaceItem? previousProject)
	{
		if (_currentProject is null ||
			string.Equals(previousProject?.Path, _currentProject.Path, StringComparison.OrdinalIgnoreCase))
			return;

		SaveSelectedProject(_currentProject.Name);
	}

	private static bool IsSameOrChildPath(string parentPath, string childPath)
	{
		string fullParentPath = Path.GetFullPath(parentPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string fullChildPath = Path.GetFullPath(childPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

		return string.Equals(fullParentPath, fullChildPath, StringComparison.OrdinalIgnoreCase) ||
			fullChildPath.StartsWith(fullParentPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
			fullChildPath.StartsWith(fullParentPath + Path.AltDirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
	}

	internal void AppendOutput(string text)
	{
		if (string.IsNullOrEmpty(text))
			return;

		lock (_outputLock)
			_pendingOutput.Append(text);
	}

	private void ResetOutput(string text)
	{
		lock (_outputLock)
		{
			_pendingOutput.Clear();
			_displayedOutput.Clear();
			_displayedOutput.Append(text);
		}

		edtOutput.Text = text;
		ScrollOutputToEnd();
	}

	private void FlushPendingOutput()
	{
		string? nextChunk = null;

		lock (_outputLock)
		{
			if (_pendingOutput.Length == 0)
				return;

			nextChunk = _pendingOutput.ToString();
			_pendingOutput.Clear();
			_displayedOutput.Append(nextChunk);
		}

		edtOutput.Text = _displayedOutput.ToString();
		ScrollOutputToEnd();
	}

	private void ScrollOutputToEnd()
	{
		edtOutput.CursorPosition = edtOutput.Text.Length;
		edtOutput.SelectionLength = 0;

		Dispatcher.Dispatch(async () =>
		{
			await Task.Yield();
			await scrOutput.ScrollToAsync(edtOutput, ScrollToPosition.End, false);
		});
	}

	private void LoadWorkspace()
	{
		_workspace.Clear();

		string manifestPath = GetWorkspaceManifestPath();
		string workspaceRootName = Path.GetFileNameWithoutExtension(manifestPath);
		_workspace.Add(new WorkspaceItem(workspaceRootName, _workspacePath, WorkspaceItemType.Workspace, 0));

		if (!File.Exists(manifestPath))
			return;

		WorkspaceManifest? manifest = JsonSerializer.Deserialize<WorkspaceManifest>(
			File.ReadAllText(manifestPath),
			new JsonSerializerOptions
			{
				PropertyNameCaseInsensitive = true,
				AllowTrailingCommas = true,
			});

		if (manifest?.Projects is null)
			return;

		List<WorkspaceItem> projects = manifest.Projects
			.Where(project => !string.IsNullOrWhiteSpace(project.Path))
			.Select(project => CreateProjectItem(project.Path))
			.Where(project => project is not null)
			.Select(project => project!)
			.OrderBy(project => project.Name, StringComparer.OrdinalIgnoreCase)
			.ThenBy(project => project.Path, StringComparer.OrdinalIgnoreCase)
			.ToList();

		foreach (WorkspaceItem project in projects)
			AddProjectItems(project);

		_currentProject = GetProjectFromSavedSelection(manifest.Project) ??
			_workspace.FirstOrDefault(item => item.IsProject);
		RefreshProjectPickers();

		if (_currentProject is not null)
			colWorkspace.SelectedItem = _currentProject;
		else
			SyncProjectPickers();
	}

	private void RefreshProjectPickers()
	{
		MenuView.ProjectPickerItem[] projects = _workspace
			.Where(item => item.IsProject)
			.Select(item => new MenuView.ProjectPickerItem(item.Name, item.Path))
			.ToArray();

		horMenu.SetProjects(projects);
		verMenu.SetProjects(projects);
		SyncProjectPickers();
	}

	private void SyncProjectPickers()
	{
		string? selectedProjectPath = _currentProject?.Path;
		horMenu.SetSelectedProject(selectedProjectPath);
		verMenu.SetSelectedProject(selectedProjectPath);
	}

	private void OnProjectPickerChanged(object? sender, MenuView.ProjectPickerItem selectedProject)
	{
		WorkspaceItem? projectItem = _workspace.FirstOrDefault(item =>
			item.IsProject &&
			string.Equals(item.Path, selectedProject.Path, StringComparison.OrdinalIgnoreCase));

		if (projectItem is null)
			return;

		UpdateCurrentProject(projectItem);
	}

	private WorkspaceItem? GetProjectFromSavedSelection(string? projectName)
	{
		if (string.IsNullOrWhiteSpace(projectName))
			return null;

		return _workspace.FirstOrDefault(item =>
			item.IsProject &&
			string.Equals(item.Name, projectName.Trim(), StringComparison.OrdinalIgnoreCase));
	}

	private void SaveSelectedProject(string projectName)
	{
		string manifestPath = GetWorkspaceManifestPath();
		if (!File.Exists(manifestPath))
			return;

		try
		{
			WorkspaceManifest? manifest = JsonSerializer.Deserialize<WorkspaceManifest>(
				File.ReadAllText(manifestPath),
				new JsonSerializerOptions
				{
					PropertyNameCaseInsensitive = true,
					AllowTrailingCommas = true,
				});

			if (manifest?.Projects is null ||
				string.Equals(manifest.Project, projectName, StringComparison.OrdinalIgnoreCase))
				return;

			WorkspaceManifest updatedManifest = manifest with { Project = projectName };
			string json = JsonSerializer.Serialize(updatedManifest, new JsonSerializerOptions
			{
				WriteIndented = true,
			});
			File.WriteAllText(manifestPath, json + Environment.NewLine);
		}
		catch (Exception ex)
		{
			AppendOutput($"[warning] Failed to save selected project: {ex.Message}{Environment.NewLine}");
		}
	}

	private WorkspaceItem? CreateProjectItem(string projectPath)
	{
		string absoluteProjectPath = Path.GetFullPath(Path.Combine(_workspacePath, projectPath));

		if (!Directory.Exists(absoluteProjectPath))
			return null;

		string projectDisplayName = Path.GetFileName(projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
		string projectType = DetectProjectType(absoluteProjectPath);
		return new WorkspaceItem(projectDisplayName, absoluteProjectPath, WorkspaceItemType.Project, 1, projectType);
	}

	private void AddProjectItems(WorkspaceItem projectItem)
	{
		string absoluteProjectPath = projectItem.Path;
		_workspace.Add(projectItem);

		HashSet<string> addedFolders = [];
		IEnumerable<string> projectFiles = EnumerateProjectFiles(absoluteProjectPath)
			.OrderBy(path => Path.GetRelativePath(absoluteProjectPath, path), StringComparer.OrdinalIgnoreCase);

		foreach (string projectFile in projectFiles)
		{
			string relativeFilePath = Path.GetRelativePath(absoluteProjectPath, projectFile);
			string directoryPath = Path.GetDirectoryName(relativeFilePath) ?? string.Empty;

			if (!string.IsNullOrEmpty(directoryPath))
			{
				string[] folders = directoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
				string currentFolderPath = string.Empty;

				for (int i = 0; i < folders.Length; i++)
				{
					currentFolderPath = string.IsNullOrEmpty(currentFolderPath)
						? folders[i]
						: Path.Combine(currentFolderPath, folders[i]);

					if (addedFolders.Add(currentFolderPath))
					{
						string absoluteFolderPath = Path.Combine(absoluteProjectPath, currentFolderPath);
						_workspace.Add(new WorkspaceItem(folders[i], absoluteFolderPath, WorkspaceItemType.Folder, i + 2));
					}
				}
			}

			int depth = string.IsNullOrEmpty(directoryPath)
				? 2
				: directoryPath.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Length + 2;

			_workspace.Add(new WorkspaceItem(Path.GetFileName(projectFile), projectFile, WorkspaceItemType.File, depth));
		}
	}

	private string DetectProjectType(string rootPath)
	{
		List<string> projectFiles = EnumerateProjectFiles(rootPath).ToList();

		if (projectFiles.Any(filePath => string.Equals(Path.GetExtension(filePath), ".cpp", StringComparison.OrdinalIgnoreCase)))
			return "cpp";

		if (projectFiles.Any(filePath => string.Equals(Path.GetExtension(filePath), ".c", StringComparison.OrdinalIgnoreCase)))
			return "c";

		if (projectFiles.Any(filePath => string.Equals(Path.GetExtension(filePath), ".cs", StringComparison.OrdinalIgnoreCase)))
			return "cs";

		return string.Empty;
	}

	private IEnumerable<string> EnumerateProjectFiles(string rootPath)
	{
		foreach (string filePath in Directory.EnumerateFiles(rootPath))
			if (_projectFileTypeSet.Contains(Path.GetExtension(filePath).ToLowerInvariant()))
				yield return filePath;

		foreach (string directoryPath in Directory.EnumerateDirectories(rootPath))
		{
			if (_excludedFolderNames.Contains(Path.GetFileName(directoryPath)))
				continue;

			foreach (string filePath in EnumerateProjectFiles(directoryPath))
				yield return filePath;
		}
	}

	private void LoadEditorFile(string filePath)
	{
		SaveCurrentSourceFile();

		string sourceText = File.ReadAllText(filePath);
		_currentSourceLineEnding = DetectLineEnding(sourceText);
		edtSource.Text = sourceText;
		edtSource.CursorPosition = 0;
		_currentSourceFilePath = filePath;
	}

	private void ClearEditor()
	{
		SaveCurrentSourceFile();
		edtSource.Text = string.Empty;
		edtSource.CursorPosition = 0;
		_currentSourceFilePath = null;
		_currentSourceLineEnding = Environment.NewLine;
	}

	private static string? GetProjectEditorFilePath(WorkspaceItem project)
	{
		string projectSettingsPath = Path.Combine(project.Path, "project.json");
		if (File.Exists(projectSettingsPath))
			return projectSettingsPath;

		if (project.ProjectType != "cs")
			return null;

		string namedProjectPath = Path.Combine(project.Path, $"{project.Name}.csproj");
		if (File.Exists(namedProjectPath))
			return namedProjectPath;

		return Directory.EnumerateFiles(project.Path, "*.csproj").FirstOrDefault();
	}

	private void SaveCurrentSourceFile()
	{
		if (string.IsNullOrWhiteSpace(_currentSourceFilePath))
			return;

		string sourceText = NormalizeLineEndings(edtSource.Text ?? string.Empty, _currentSourceLineEnding);
		File.WriteAllText(_currentSourceFilePath, sourceText);
	}

	private static string DetectLineEnding(string text)
	{
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '\r')
			{
				if ((i + 1) < text.Length && text[i + 1] == '\n')
					return "\r\n";
				return "\r";
			}

			if (text[i] == '\n')
				return "\n";
		}

		return Environment.NewLine;
	}

	private static string NormalizeLineEndings(string text, string lineEnding)
	{
		if (string.IsNullOrEmpty(text))
			return text;

		string normalizedText = text.Replace("\r\n", "\n").Replace('\r', '\n');
		return normalizedText.Replace("\n", lineEnding);
	}

	private static string GetWorkspaceManifestPath()
	{
		return Path.Combine(_workspacePath, Path.HasExtension(_workspaceFile) ? _workspaceFile : $"{_workspaceFile}.json");
	}

#if WINDOWS
	internal sealed class ConsoleHostSink(WorkspacePage page) : IManagedConsoleSink
	{
		private int _processId = 0;

		public void OnStarted(int processId)
		{
			_processId = processId;
#if DEBUG
			page.AppendOutput($"[started] pid={processId}{Environment.NewLine}{Environment.NewLine}");
#endif
		}

		public void OnOutput(byte[] data)
		{
			string s = Encoding.UTF8.GetString(data);
			page.AppendOutput(s);
		}

		public void OnExited(uint exitCode)
		{
#if DEBUG
			page.AppendOutput($"{Environment.NewLine}[exited] code={exitCode}{Environment.NewLine}");
#endif
			page.OnBuildProcessExited(exitCode);
		}

		public void OnError(uint errorCode)
		{
			page.AppendOutput($"Failed to run process with error code={errorCode}.{Environment.NewLine}");
		}
	}
#endif
}



/*
	new(@"cmd.exe /c dir /s c:\Src\IntDevEnv\ManagedConsole"),

    <!--WebView x:Name="PreviewWebView" /-->

	PreviewWebView.Source = new HtmlWebViewSource { Html = BuildPreviewHtml(null) };

	private static string BuildPreviewHtml(string? commandText)
	{
		string body = commandText is null
			? "<p>Select a command from the left pane.</p>"
			: $"<h3>Selected Command</h3><pre>{System.Net.WebUtility.HtmlEncode(commandText)}</pre>";

		return $$"""
<!DOCTYPE html>
<html>
<head>
<meta charset="utf-8">
<style>
body {
	font-family: Segoe UI, sans-serif;
	margin: 0;
	padding: 16px;
	background: #1f1f1f;
	color: #ffffff;
}
pre {
	white-space: pre-wrap;
	background: #1f1f1f;
	border: 1px solid #d0d7de;
	border-radius: 6px;
	padding: 12px;
}
</style>
</head>
<body>
{{body}}
</body>
</html>
""";
	}
}
  
*/

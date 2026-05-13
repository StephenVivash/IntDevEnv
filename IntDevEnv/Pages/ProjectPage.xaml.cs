using System.Collections.ObjectModel;
using System.Text;

namespace IntDevEnv.Pages;

public partial class ProjectPage : ContentPage
{
	static String workspacePath = @"C:\Src\IntDevEnv\Workspace";
	static String projectName = @"Test1";
	static String projectExtension = @"cpp";
	static String compilerFlags = $"/EHsc /std:c++20 /Fe:{projectName}.exe"; // /Fo:main.obj

	//static String vsPath = @"C:\Program Files\Microsoft Visual Studio\18\Community";
	static String vsPath = @"C:\Program Files\Microsoft Visual Studio\18\Insiders";
	static String sdkPath = @"C:\Program Files (x86)\Windows Kits\10";
	static String vsVersion = @"14.50.35717";
	static String sdkVersion = @"10.0.19041.0";
	//static String sdkVersion = @"10.0.26100.0";

	private readonly ObservableCollection<CommandItem> _commands =
	[
		new("dotnet --info"), // cmd.exe /c 
		new(@"cmd.exe /c dir /s c:\Src\IntDevEnv\ManagedConsole"),

		new($"\"{vsPath}\\VC\\Tools\\MSVC\\{vsVersion}\\bin\\Hostx64\\x64\\cl\" {compilerFlags} " +

			$"/I \"{vsPath}\\VC\\Tools\\MSVC\\{vsVersion}\\include\" " +
			$"/I \"{sdkPath}\\include\\{sdkVersion}\\ucrt\" " +

			$"\"{workspacePath}\\{projectName}\\{projectName}.{projectExtension}\" " +

			$"/link " +
			$"\"{vsPath}\\VC\\Tools\\MSVC\\{vsVersion}\\lib\\x64\\LIBCMT.lib\" " +
			$"\"{vsPath}\\VC\\Tools\\MSVC\\{vsVersion}\\lib\\x64\\LIBCpMT.lib\" " +
			$"\"{vsPath}\\VC\\Tools\\MSVC\\{vsVersion}\\lib\\x64\\OLDNAMES.lib\" " +
			$"\"{vsPath}\\VC\\Tools\\MSVC\\{vsVersion}\\lib\\x64\\libvcruntime.lib\" " +
			$"\"{sdkPath}\\lib\\{sdkVersion}\\ucrt\\x64\\libucrt.lib\" " +
			$"\"{sdkPath}\\lib\\{sdkVersion}\\um\\x64\\kernel32.lib\" " +
			$"\"{sdkPath}\\lib\\{sdkVersion}\\um\\x64\\uuid.lib\""),

		//$"cmd.exe /c \"cd /d \"{projectsPath}\\{projectName}\" "
	];

	// /JMC /permissive- /ifcOutput "x64\Debug\" /GS /W3 /Zc:wchar_t /ZI /Gm- /Od /sdl /Fd"x64\Debug\vc145.pdb" /Zc:inline /fp:precise /D "_DEBUG" /D "_CONSOLE" /D "_UNICODE" /D "UNICODE" /errorReport:prompt /WX- /Zc:forScope /RTC1 /Gd /MDd /std:c++20 /FC /Fa"x64\Debug\" /EHsc /nologo /Fo"x64\Debug\" /Fp"x64\Debug\Test1.pch" /diagnostics:column 

#if WINDOWS
	//private readonly ManagedMfc _mfc;
	private readonly ManagedConsole _managedConsole;
#endif
	private readonly object _outputLock = new();
	private readonly StringBuilder _pendingOutput = new();
	private readonly StringBuilder _displayedOutput = new();
	private readonly IDispatcherTimer _outputTimer;

	public ProjectPage()
	{
		InitializeComponent();
		CommandList.ItemsSource = _commands;
		//PreviewWebView.Source = new HtmlWebViewSource { Html = BuildPreviewHtml(null) };
		_outputTimer = Dispatcher.CreateTimer();
		_outputTimer.Interval = TimeSpan.FromMilliseconds(50);
		_outputTimer.Tick += (_, _) => FlushPendingOutput();
		_outputTimer.Start();

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

	private void OnCommandSelectionChanged(object? sender, SelectionChangedEventArgs e)
	{
		if (e.CurrentSelection.FirstOrDefault() is not CommandItem item)
		{
			return;
		}

		//PreviewWebView.Source = new HtmlWebViewSource { Html = BuildPreviewHtml(item.CommandText) };
		ResetOutput(""); // $"{item.CommandText}{Environment.NewLine}{Environment.NewLine}"

#if WINDOWS
		try
		{
			string initialDir = $"{workspacePath}\\{projectName}";
			//::SetCurrentDirectory(initialDir);
			_managedConsole.StartConsole(item.CommandText, 0, 0, 0, initialDir);
		}
		catch (Exception ex)
		{
			AppendOutput($"[error] {ex}{Environment.NewLine}");
		}
#else
		AppendOutput("ManagedConsole is only available on Windows." + Environment.NewLine);
#endif
	}

	internal void AppendOutput(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return;
		}

		lock (_outputLock)
		{
			_pendingOutput.Append(text);
		}
	}

	private void ResetOutput(string text)
	{
		lock (_outputLock)
		{
			_pendingOutput.Clear();
			_displayedOutput.Clear();
			_displayedOutput.Append(text);
		}

		OutputEditor.Text = text;
		OutputEditor.CursorPosition = text.Length;
	}

	private void FlushPendingOutput()
	{
		string? nextChunk = null;

		lock (_outputLock)
		{
			if (_pendingOutput.Length == 0)
			{
				return;
			}

			nextChunk = _pendingOutput.ToString();
			_pendingOutput.Clear();
			_displayedOutput.Append(nextChunk);
		}

		OutputEditor.Text = _displayedOutput.ToString();
		OutputEditor.CursorPosition = OutputEditor.Text.Length;
	}

#if WINDOWS
	internal sealed class ConsoleHostSink(ProjectPage page) : IManagedConsoleSink
	{
		public void OnStarted(int processId)
		{
			page.AppendOutput($"[started] pid={processId}{Environment.NewLine}{Environment.NewLine}");
		}

		public void OnOutput(byte[] data)
		{
			string s = Encoding.UTF8.GetString(data);
			page.AppendOutput(s);
		}

		public void OnExited(uint exitCode)
		{
			page.AppendOutput($"{Environment.NewLine}[exited] code={exitCode}{Environment.NewLine}");
		}

		public void OnError(uint errorCode)
		{
			page.AppendOutput($"{Environment.NewLine}[error] code={errorCode}{Environment.NewLine}");
		}
	}
#endif

	internal sealed record CommandItem(string CommandText);
}

/*
    <!--WebView x:Name="PreviewWebView" /-->

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

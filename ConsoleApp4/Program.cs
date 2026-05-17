using System.Text;

internal sealed class ConsoleHostSink : IManagedConsoleSink
{
	public void OnStarted(int processId)
	{
		Console.WriteLine($"Started process {processId}");
	}

	public void OnOutput(byte[] data)
	{
		Console.Write(Encoding.UTF8.GetString(data));
	}

	public void OnExited(uint exitCode)
	{
		Console.WriteLine();
		Console.WriteLine($"Exited with code {exitCode}");
	}

	public void OnError(uint errorCode)
	{
		Console.Error.WriteLine($"Console error: {errorCode}");
	}
}

internal static class Program
{
	private static void Main()
	{
		/* Demonstrate MFC string marshalling
		 * 
        string str = "Hello, World!";
        ManagedMfc mfc = new ManagedMfc();
        mfc.setString(str);
        Console.WriteLine($"getString: {mfc.getString()}");
        str = "C:\\Test\\Filename.exe";
        Console.WriteLine($"Filename: {ManagedMfc.GetFileName(str)}");
        */

		ManagedConsole mc = new ManagedConsole(new ConsoleHostSink());
		mc.StartConsole("cmd.exe /c dotnet --info", 0, 0, 0, "");
		while (mc.IsConsoleActive())
		{
			Thread.Sleep(100);
		}
	}
}

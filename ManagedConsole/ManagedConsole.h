#pragma once

#include "..\MonitorConsole\MonitorConsole.h"
#include <vcclr.h>

using namespace System;

ref class ManagedConsole;

class CCallback : public CConsoleCallback
{
public:
	CCallback(ManagedConsole^ owner);
	virtual bool InitCallback(CMonitorConsole* pMC);
	virtual bool RunCallback(CMonitorConsole* pMC);
	virtual bool DataCallback(CMonitorConsole* pMC, LPVOID pchBuffer, DWORD dwBytesAvailable);
	virtual void EndCallback(CMonitorConsole* pMC);
	virtual void ErrorCallback(CMonitorConsole* pMC);

private:
	gcroot<ManagedConsole^> _owner;
};

public interface class IManagedConsoleSink
{
	void OnStarted(int processId);
	void OnOutput(array<Byte>^ data);
	void OnExited(unsigned int exitCode);
	void OnError(unsigned int errorCode);
};

public ref class ConsoleStartedEventArgs : EventArgs
{
public:
	ConsoleStartedEventArgs(int processId)
	{
		ProcessId = processId;
	}

	property int ProcessId;
};

public ref class ConsoleOutputEventArgs : EventArgs
{
public:
	ConsoleOutputEventArgs(array<Byte>^ data)
	{
		Data = data;
	}

	property array<Byte>^ Data;
};

public ref class ConsoleExitedEventArgs : EventArgs
{
public:
	ConsoleExitedEventArgs(unsigned int exitCode)
	{
		ExitCode = exitCode;
	}

	property unsigned int ExitCode;
};

public ref class ConsoleErrorEventArgs : EventArgs
{
public:
	ConsoleErrorEventArgs(unsigned int errorCode)
	{
		ErrorCode = errorCode;
	}

	property unsigned int ErrorCode;
};

public ref class ManagedConsole
{
public:
	ManagedConsole();
	ManagedConsole(IManagedConsoleSink^ sink);
	~ManagedConsole();
	!ManagedConsole();
	bool StartConsole(String^ commandLine,
		//CStringList* plssEnvironment,
		//CConsoleCallback* pConsoleCallback,
		int nThreadPriority,
		DWORD dwProcessPriority,
		DWORD dwCreationFlags,
		String^ lpszInitialDir);

	bool IsConsoleActive() { return _pconsole != nullptr && _pconsole->IsConsoleActive(); }

	String^ ReadOutput();

	event EventHandler<ConsoleStartedEventArgs^>^ Started
	{
		void add(EventHandler<ConsoleStartedEventArgs^>^ handler) { _started += handler; }
		void remove(EventHandler<ConsoleStartedEventArgs^>^ handler) { _started -= handler; }
		void raise(Object^ sender, ConsoleStartedEventArgs^ args)
		{
			if (_started != nullptr)
				_started(sender, args);
		}
	}

	event EventHandler<ConsoleOutputEventArgs^>^ OutputReceived
	{
		void add(EventHandler<ConsoleOutputEventArgs^>^ handler) { _outputReceived += handler; }
		void remove(EventHandler<ConsoleOutputEventArgs^>^ handler) { _outputReceived -= handler; }
		void raise(Object^ sender, ConsoleOutputEventArgs^ args)
		{
			if (_outputReceived != nullptr)
				_outputReceived(sender, args);
		}
	}

	event EventHandler<ConsoleExitedEventArgs^>^ Exited
	{
		void add(EventHandler<ConsoleExitedEventArgs^>^ handler) { _exited += handler; }
		void remove(EventHandler<ConsoleExitedEventArgs^>^ handler) { _exited -= handler; }
		void raise(Object^ sender, ConsoleExitedEventArgs^ args)
		{
			if (_exited != nullptr)
				_exited(sender, args);
		}
	}

	event EventHandler<ConsoleErrorEventArgs^>^ Error
	{
		void add(EventHandler<ConsoleErrorEventArgs^>^ handler) { _error += handler; }
		void remove(EventHandler<ConsoleErrorEventArgs^>^ handler) { _error -= handler; }
		void raise(Object^ sender, ConsoleErrorEventArgs^ args)
		{
			if (_error != nullptr)
				_error(sender, args);
		}
	}

	property IManagedConsoleSink^ Sink
	{
		IManagedConsoleSink^ get() { return _sink; }
		void set(IManagedConsoleSink^ value) { _sink = value; }
	}

internal:
	void OnNativeStarted(int processId);
	bool OnNativeTick();
	bool OnNativeOutput(LPVOID pchBuffer, DWORD dwBytesAvailable);
	void OnNativeExited(unsigned int exitCode);
	void OnNativeError(unsigned int errorCode);

private:
	CMonitorConsole* _pconsole = nullptr;
	CCallback* _pcallback = nullptr;
	IManagedConsoleSink^ _sink = nullptr;
	EventHandler<ConsoleStartedEventArgs^>^ _started = nullptr;
	EventHandler<ConsoleOutputEventArgs^>^ _outputReceived = nullptr;
	EventHandler<ConsoleExitedEventArgs^>^ _exited = nullptr;
	EventHandler<ConsoleErrorEventArgs^>^ _error = nullptr;
};
/*
public ref class ManagedMfc
{
public:
	ManagedMfc();
	~ManagedMfc();

	void setString(String^ str);
	String^ getString();

	static String^ GetFileName(String^ path);

private:
	CString* _pstr;
};
*/
//String^ GetFileName(String^ path);

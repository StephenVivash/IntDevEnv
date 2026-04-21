#pragma once

#include <string>
#include <vcclr.h>

#include "ManagedConsole.h"

using namespace System;
using namespace System::Runtime::InteropServices;

CCallback::CCallback(ManagedConsole^ owner)
	: _owner(owner)
{
}

bool CCallback::InitCallback(CMonitorConsole* pMC) 
{
	_owner->OnNativeStarted(static_cast<int>(pMC->GetProcessId()));
	return true;
}

bool CCallback::RunCallback(CMonitorConsole* pMC) 
{
	return _owner->OnNativeTick();
}

bool CCallback::DataCallback(CMonitorConsole* pMC, LPVOID pchBuffer, DWORD dwBytesAvailable)
{
	return _owner->OnNativeOutput(pchBuffer, dwBytesAvailable);
}

void CCallback::EndCallback(CMonitorConsole* pMC) 
{
	_owner->OnNativeExited(pMC->GetExitCode());
}

void CCallback::ErrorCallback(CMonitorConsole* pMC) 
{
	_owner->OnNativeError(pMC->GetExitCode());
}

ManagedConsole::ManagedConsole()
	: ManagedConsole(nullptr)
{
}

ManagedConsole::ManagedConsole(IManagedConsoleSink^ sink)
{
	_pconsole = new CMonitorConsole();
	_sink = sink;
	_pcallback = new CCallback(this);
}

ManagedConsole::~ManagedConsole()
{
	this->!ManagedConsole();
}

ManagedConsole::!ManagedConsole()
{
	if (_pconsole != nullptr)
	{
		delete _pconsole;
		_pconsole = nullptr;
	}
	if (_pcallback != nullptr)
	{
		delete _pcallback;
		_pcallback = nullptr;
	}
}

bool ManagedConsole::StartConsole(String^ commandLine, 
	//CStringList* plssEnvironment,
	//CConsoleCallback* pConsoleCallback, 
	int nThreadPriority, 
	DWORD dwProcessPriority,
	DWORD dwCreationFlags, 
	String^ lpszInitialDir)
{
	if (_pconsole == nullptr || _pcallback == nullptr)
		return false;

	if (String::IsNullOrWhiteSpace(commandLine))
		return false;

	pin_ptr<const wchar_t> pinnedCommandLine = PtrToStringChars(commandLine);
	//LPCTSTR nativeCommandLine = pinnedCommandLine;

	//Console::WriteLine("managed: {0}", commandLine);
	//Console::WriteLine("pinned: {0}", gcnew String(pinnedCommandLine));
	//Console::WriteLine("native: {0}", gcnew String(nativeCommandLine));
	//std::wcout << L"pinned: " << (const wchar_t*)pinnedCommandLine << std::endl;
	//std::wcout << L"native: " << nativeCommandLine << std::endl;

	//const wchar_t* pinnedText = pinnedCommandLine;
	//std::wcout << L"pinned: ";
	//std::wcout.write(pinnedText, wcslen(pinnedText));
	//std::wcout << std::endl;

	LPCTSTR nativeInitialDir = nullptr;
	pin_ptr<const wchar_t> pinnedInitialDir;
	if (!String::IsNullOrWhiteSpace(lpszInitialDir))
	{
		pinnedInitialDir = PtrToStringChars(lpszInitialDir);
		nativeInitialDir = pinnedInitialDir;
	}

	return _pconsole->StartConsole(pinnedCommandLine, nullptr, _pcallback,
		nThreadPriority, dwProcessPriority, dwCreationFlags, nativeInitialDir);
}

String^ ManagedConsole::ReadOutput()
{
	std::wstring s = L""; // _native->ReadOutput();
	return gcnew String(s.c_str());
}

void ManagedConsole::OnNativeStarted(int processId)
{
	if (_sink != nullptr)
		_sink->OnStarted(processId);

	if (_started != nullptr)
		_started(this, gcnew ConsoleStartedEventArgs(processId));
}

bool ManagedConsole::OnNativeTick()
{
	return true;
}

bool ManagedConsole::OnNativeOutput(LPVOID pchBuffer, DWORD dwBytesAvailable)
{
	if (pchBuffer == nullptr || dwBytesAvailable == 0)
		return true;

	array<Byte>^ data = gcnew array<Byte>(dwBytesAvailable);
	Marshal::Copy(IntPtr(pchBuffer), data, 0, static_cast<int>(dwBytesAvailable));

	if (_sink != nullptr)
		_sink->OnOutput(data);

	if (_outputReceived != nullptr)
		_outputReceived(this, gcnew ConsoleOutputEventArgs(data));

	return true;
}

void ManagedConsole::OnNativeExited(unsigned int exitCode)
{
	if (_sink != nullptr)
		_sink->OnExited(exitCode);

	if (_exited != nullptr)
		_exited(this, gcnew ConsoleExitedEventArgs(exitCode));
}

void ManagedConsole::OnNativeError(unsigned int errorCode)
{
	if (_sink != nullptr)
		_sink->OnError(errorCode);

	if (_error != nullptr)
		_error(this, gcnew ConsoleErrorEventArgs(errorCode));
}
/*
ManagedMfc::ManagedMfc() 
{
	_pstr = nullptr;
}

ManagedMfc::~ManagedMfc() 
{
	if (_pstr != nullptr)
		delete _pstr;
}

void ManagedMfc::setString(String^ str) 
{
	pin_ptr<const wchar_t> pinnedStr = PtrToStringChars(str);
	//Console::WriteLine("pinned: {0}", gcnew String(pinnedStr));
	_pstr = new CString(pinnedStr);
}

String^ ManagedMfc::getString() 
{
	if (_pstr == nullptr)
		return "";
	return gcnew String(static_cast<LPCTSTR>(*_pstr));
}

String^ ManagedMfc::GetFileName(String^ path)
{
	pin_ptr<const wchar_t> pinnedPath = PtrToStringChars(path);
	CString strFileName = ::GetFileName(pinnedPath);
	return gcnew String(static_cast<LPCTSTR>(strFileName));
}
/ *
String^ GetFileName(String^ path)
{
	pin_ptr<const wchar_t> pinnedPath = PtrToStringChars(path);
	CString strFileName = ::GetFileName(pinnedPath);
	return gcnew String(static_cast<LPCTSTR>(strFileName));
}
*/

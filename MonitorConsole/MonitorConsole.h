#if !defined(AFX_MONITORCONSOLE_H__90C2C425_D9A8_4F41_B37B_80CD35AC62EE__INCLUDED_)
#define AFX_MONITORCONSOLE_H__90C2C425_D9A8_4F41_B37B_80CD35AC62EE__INCLUDED_

#pragma once

#include "MFC.h"
//#include "Misc.h"

#include <atomic>
#include <tlhelp32.h>

#ifdef MONITORCONSOLE_EXPORTS
#define MONITORCONSOLE_API __declspec(dllexport)
#else
#define MONITORCONSOLE_API __declspec(dllimport)
#endif

class CMonitorConsole;

class MONITORCONSOLE_API CProcessEntry
{
public:
	CProcessEntry(DWORD dwProcessId, DWORD dwParentProcessId, LPCTSTR lpszExeFile)
		{m_dwProcessId = dwProcessId; m_dwParentProcessId = dwParentProcessId;
		m_strExeFile = lpszExeFile; m_bChild = false;}

    DWORD m_dwProcessId;
    DWORD m_dwParentProcessId;
    CString m_strExeFile;
	bool m_bChild;
};

class MONITORCONSOLE_API CProcessControl
{
public:
	CProcessControl();
	virtual ~CProcessControl();
	bool IsEmpty() {return m_rgnProcesses.GetSize() == 0;}
	bool RemoveAll();
	bool TakeSnapShot(DWORD dwProcessId);
	bool KillProcess(DWORD dwProcessId);
	bool KillProcessTree(DWORD dwProcessId, bool bRoot);

protected:
	CProcessEntry* GetEntry(DWORD dwProcessId);
	bool FindChildren(DWORD dwProcessId);
	bool CanKillProcess(CProcessEntry* ppe, bool bRoot);

	CPtrArray m_rgnProcesses;
};

class MONITORCONSOLE_API CConsoleCallback
{
public:
	virtual bool InitCallback(CMonitorConsole* pMC) = 0;
	virtual bool RunCallback(CMonitorConsole* pMC) = 0;
	virtual bool DataCallback(CMonitorConsole* pMC, LPVOID pchBuffer, DWORD dwBytesAvailable) = 0;
	virtual void EndCallback(CMonitorConsole* pMC) = 0;
	virtual void ErrorCallback(CMonitorConsole* pMC) = 0;
};

class MONITORCONSOLE_API CMonitorConsole
{
public:
	CMonitorConsole();
	virtual ~CMonitorConsole();
	bool StartConsole(LPCTSTR lpszCommandLine, CStringList* plssEnvironment,
		CConsoleCallback* pConsoleCallback, int nThreadPriority = THREAD_PRIORITY_NORMAL,
		DWORD dwProcessPriority = NORMAL_PRIORITY_CLASS,
		DWORD dwCreationFlags = 0, LPCTSTR lpszInitialDir = NULL);
	bool StopConsole(bool bProcessTree, bool bRoot);
	bool WriteToConsole(LPVOID pBuffer, DWORD dwBytesToWrite);
	bool TakeSnapShot();

	void SetProcessId(DWORD dwProcessId) {m_dwProcessId = dwProcessId;}
	DWORD GetProcessId() {return m_dwProcessId;}
	void SetProcessHandle(HANDLE hProcess) {m_hProcess = hProcess;}
	HANDLE GetProcessHandle() {return m_hProcess;}
	void SetProcessThreadHandle(HANDLE hProcessThread) {m_hProcessThread = hProcessThread;}
	HANDLE GetProcessThreadHandle() {return m_hProcessThread;}
	void SetMonitorOut(HANDLE hMonitorOut) {m_hMonitorOut = hMonitorOut;}
	HANDLE GetMonitorOut() {return m_hMonitorOut;}
	LPCTSTR GetCommandLine() {return m_strCommandLine;}
	void SetExitCode(DWORD dwExitCode) {m_dwExitCode = dwExitCode;}
	DWORD GetExitCode() {return m_dwExitCode;}
	int GetThreadPriority() {return m_nThreadPriority;}
	DWORD GetProcessPriority() {return m_dwProcessPriority;}
	DWORD GetCreationFlags() {return m_dwCreationFlags;}
	LPCTSTR GetInitialDir() {return m_strInitialDir;}
	LPCTSTR GetEnvironment() {return m_szEnvironment;}
	bool IsConsoleActive() {return m_bMonitorThreadActive.load() || (m_hProcess != INVALID_HANDLE_VALUE);}
	CConsoleCallback* GetConsoleCallback() {return m_pConsoleCallback;}

protected:
	friend UINT MonitorThread(LPVOID pParam);

	CString m_strCommandLine;
	TCHAR m_szEnvironment[1024];
	CConsoleCallback* m_pConsoleCallback;
	int m_nThreadPriority;
	DWORD m_dwProcessPriority;
	DWORD m_dwCreationFlags;
	CString m_strInitialDir;
	CWinThread* m_pMonitorThread;
	DWORD m_dwProcessId;
	HANDLE m_hProcess;
	HANDLE m_hProcessThread;
	HANDLE m_hMonitorOut;
	DWORD m_dwExitCode;
	std::atomic<bool> m_bMonitorThreadActive;
	CProcessControl m_ProcessControl;
};

#endif

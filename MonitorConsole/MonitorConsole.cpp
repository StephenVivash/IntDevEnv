#include "pch.h"
#include "MonitorConsole.h"

static CFile g_VerboseFile;
static int g_nVerboseCount = 0;

static void WriteVerboseBytes(const void* data, UINT byteCount)
{
	if((data == NULL) || (byteCount == 0) || (g_VerboseFile.m_hFile == INVALID_HANDLE_VALUE))
		return;

	g_VerboseFile.Write(data, byteCount);
}

static void WriteVerboseText(LPCTSTR text)
{
	if((text == NULL) || (g_VerboseFile.m_hFile == INVALID_HANDLE_VALUE))
		return;

#ifdef UNICODE
	const int byteCount = WideCharToMultiByte(CP_UTF8, 0, text, -1, nullptr, 0, nullptr, nullptr);
	if(byteCount <= 1)
		return;

	std::vector<char> buffer(static_cast<size_t>(byteCount));
	if(WideCharToMultiByte(CP_UTF8, 0, text, -1, buffer.data(), byteCount, nullptr, nullptr) <= 0)
		return;

	WriteVerboseBytes(buffer.data(), static_cast<UINT>(byteCount - 1));
#else
	WriteVerboseBytes(text, static_cast<UINT>(strlen(text)));
#endif
}

///////////////////////////////////////////////////////////////////////////////////////
// CProcessControl

CProcessControl::CProcessControl() 
{
}

CProcessControl::~CProcessControl()
{
	RemoveAll();
}

bool CProcessControl::RemoveAll()
{
	int nLength = m_rgnProcesses.GetSize();
	for(int i = 0; i < nLength; ++i)
		delete (CProcessEntry*) m_rgnProcesses[i];
	m_rgnProcesses.RemoveAll();
	return true;
}

bool CProcessControl::TakeSnapShot(DWORD dwProcessId)
{
	bool bRC = false;
	PROCESSENTRY32 pe;
	HANDLE hSnapshot;
	bool bFound = false;
	RemoveAll();
	if((hSnapshot = ::CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS,0)) != INVALID_HANDLE_VALUE)
	{
		pe.dwSize = sizeof PROCESSENTRY32;
		if(::Process32First(hSnapshot,&pe))
			do
			{
				m_rgnProcesses.Add(new CProcessEntry(pe.th32ProcessID,
					pe.th32ParentProcessID,pe.szExeFile));
				bRC = true;
			} while(::Process32Next(hSnapshot,&pe));
		::CloseHandle(hSnapshot);
		if(dwProcessId > 0)
		{
			int nLength = m_rgnProcesses.GetSize();
			for(int i = 0; i < nLength; ++i)
				if(((CProcessEntry*) m_rgnProcesses[i])->m_dwProcessId == dwProcessId)
					bFound = true;
			if(!bFound)
				m_rgnProcesses.Add(new CProcessEntry(dwProcessId,0,_T("<DummyParent>")));
		}
	}
	return bRC;
}

bool CProcessControl::KillProcess(DWORD dwProcessId)
{
	bool bRC = false;
	HANDLE hProcess;
	if(dwProcessId > 0)
		if((hProcess = OpenProcess(PROCESS_TERMINATE,FALSE,dwProcessId)) != NULL)
		{
			if(TerminateProcess(hProcess,0))
			{
				WaitForSingleObject(hProcess,INFINITE);
				bRC = true;
			}
			::CloseHandle(hProcess);
		}
	return bRC;
}

bool CProcessControl::KillProcessTree(DWORD dwProcessId, bool bRoot)
{
	int nLength = m_rgnProcesses.GetSize();
	CProcessEntry* ppe;
	if((ppe = GetEntry(dwProcessId)) != NULL)
	{
		if(bRoot)
			ppe->m_bChild = true;
		FindChildren(dwProcessId);
		for(int i = 0; i < nLength; ++i)
			if((ppe = (CProcessEntry*) m_rgnProcesses[i]) != NULL)
				if(ppe->m_bChild && CanKillProcess(ppe,bRoot))
					KillProcess(ppe->m_dwProcessId);
		return true;
	}
	return false;
}

CProcessEntry* CProcessControl::GetEntry(DWORD dwProcessId)
{
	int nLength = m_rgnProcesses.GetSize();
	CProcessEntry* ppe;
	for(int i = 0; i < nLength; ++i)
		if((ppe = (CProcessEntry*) m_rgnProcesses[i]) != NULL)
			if(ppe->m_dwProcessId == dwProcessId)
				return (CProcessEntry*) m_rgnProcesses[i];
	return NULL;
}

bool CProcessControl::FindChildren(DWORD dwProcessId)
{
	CProcessEntry* ppe;
	int nLength = m_rgnProcesses.GetSize();
	if(dwProcessId > 0)
		for(int i = 0; i < nLength; ++i)
			if((ppe = (CProcessEntry*) m_rgnProcesses[i]) != NULL)
				if((ppe->m_dwParentProcessId == dwProcessId) && !ppe->m_bChild &&
					(ppe->m_dwParentProcessId != ppe->m_dwProcessId))
				{
					ppe->m_bChild = true;
					FindChildren(ppe->m_dwProcessId);
				}
	return true;
}

bool CProcessControl::CanKillProcess(CProcessEntry* ppe, bool bRoot)
{
	if(_tcsicmp(ppe->m_strExeFile,_T("<DummyParent>")) == 0)
		return false;
	//if(!::GetApp()->IsNovaVersion272())
	//	return true;
	if(!bRoot && (_tcsnicmp(ppe->m_strExeFile,_T("sys_loader"),10) == 0))
		return false;
//	(_tcsicmp(ppe->m_strExeFile,_T("vsim.exe")) == 0)
	return true;
}

///////////////////////////////////////////////////////////////////////////////////////
// MonitorThread

UINT MonitorThread(LPVOID pParam)
{
	CMonitorConsole* pMC = (CMonitorConsole*) pParam;
	HANDLE hProcess = GetCurrentProcess();
	HANDLE hMonitorOut = INVALID_HANDLE_VALUE;
	HANDLE hMonitorIn = INVALID_HANDLE_VALUE;
	HANDLE hChildStdIn = INVALID_HANDLE_VALUE;
	HANDLE hChildStdOut = INVALID_HANDLE_VALUE;

	SECURITY_ATTRIBUTES sa;
	STARTUPINFO si;
	PROCESS_INFORMATION pi;

	//if(!AfxOleInit())
	//{
	//	::AppMessage(_T("OLE Initialisation failed in MonitorThread."),MB_OK | MB_ICONSTOP,::GetMainFrame());
	//	return -1;
	//}

	bool bContinue = true;
	int nTimerCount = -1;
	DWORD dwAvailable,dwBytesRead,dwExitCode = 0;
	char* pchBuffer;

	LPCTSTR lpszInitialDir = pMC->GetInitialDir();
	if((lpszInitialDir == NULL) || (_tcslen(lpszInitialDir) == 0))
		lpszInitialDir = NULL;

	LPCTSTR lpszEnvironment = pMC->GetEnvironment();
	if((lpszEnvironment == NULL) || (_tcslen(lpszEnvironment) == 0))
		lpszEnvironment = NULL;

	memset(&sa,0,sizeof SECURITY_ATTRIBUTES);
	memset(&si,0,sizeof STARTUPINFO);
	memset(&pi,0,sizeof PROCESS_INFORMATION);

	sa.nLength = sizeof(SECURITY_ATTRIBUTES); 
	sa.lpSecurityDescriptor = NULL; 
	sa.bInheritHandle = TRUE; 

	if(CreatePipe(&hChildStdIn,&hMonitorOut,&sa,0))
		if(CreatePipe(&hMonitorIn,&hChildStdOut,&sa,0))
			;
		else
			dwExitCode = GetLastError();
	else
		dwExitCode = GetLastError();

//	SetConsoleCtrlHandler((PHANDLER_ROUTINE) SrvCtrlHand,TRUE); 

	si.cb = sizeof(STARTUPINFO); 
	si.dwFlags = STARTF_USESTDHANDLES | STARTF_USESHOWWINDOW; 
	si.hStdInput = hChildStdIn; 
	si.hStdOutput= hChildStdOut; 
	si.hStdError = hChildStdOut;
	si.wShowWindow = SW_HIDE; 

	if(::GetApp()->IsVerboseMode() && (++g_nVerboseCount == 1) && g_VerboseFile.Open(::GetApp()->GetVerboseFileName(),
		CFile::modeWrite | CFile::modeCreate | CFile::modeNoTruncate))
	{
		static const unsigned char utf8Bom[] = { 0xEF, 0xBB, 0xBF };
		CString strTemp;
		strTemp.Format(_T("\nCommandLine: %s\nInitialDir: %s\n\n"),
			pMC->GetCommandLine(),lpszInitialDir);
		g_VerboseFile.Seek(0,CFile::end);
		if(g_VerboseFile.Seek(0, CFile::end) == 0)
			WriteVerboseBytes(utf8Bom, sizeof utf8Bom);
		WriteVerboseText(strTemp);
	}

	if(dwExitCode == 0)
		if(CreateProcess(NULL,(LPTSTR) pMC->GetCommandLine(),NULL,NULL,TRUE,CREATE_DEFAULT_ERROR_MODE | 
			pMC->GetCreationFlags() | pMC->GetProcessPriority(),(LPVOID) lpszEnvironment,
			lpszInitialDir,&si,&pi))
		{
			pMC->SetProcessId(pi.dwProcessId);
			pMC->SetProcessHandle(pi.hProcess);
			pMC->SetProcessThreadHandle(pi.hThread);
			pMC->SetMonitorOut(hMonitorOut);

			if(pMC->GetConsoleCallback()->InitCallback(pMC))
				while(bContinue)
				{
					Sleep(10);
					//WaitForSingleObject(hChildStdOut,500);
					if(!GetExitCodeProcess(pi.hProcess,&dwExitCode) || (dwExitCode != STILL_ACTIVE))
						bContinue = false;
					if((nTimerCount = (++nTimerCount % 10)) == 0)
						if(!pMC->GetConsoleCallback()->RunCallback(pMC))
							bContinue = false;

					while(PeekNamedPipe(hMonitorIn,NULL,0,NULL,&dwAvailable,NULL) && (dwAvailable > 0))
						if((pchBuffer = new char[dwAvailable + 1]) != NULL)
						{
							if(ReadFile(hMonitorIn,(LPVOID) pchBuffer,dwAvailable,&dwBytesRead,NULL))
							{
								pchBuffer[dwBytesRead] = 0;

								if(::GetApp()->IsVerboseMode() && (g_VerboseFile.m_hFile != INVALID_HANDLE_VALUE))
									g_VerboseFile.Write(pchBuffer,dwBytesRead);

								if(!pMC->GetConsoleCallback()->DataCallback(pMC,(LPVOID) 
									pchBuffer,dwBytesRead))
									bContinue = false;
							}
							else
								bContinue = false;
							delete [] pchBuffer;
						}
						else
							bContinue = false;
				}
			else
				bContinue = false;
		}
		else
		{
			dwExitCode = GetLastError();
			pMC->SetExitCode(dwExitCode);
			pMC->GetConsoleCallback()->ErrorCallback(pMC);
		}
	else
	{
		pMC->SetExitCode(dwExitCode);
		pMC->GetConsoleCallback()->ErrorCallback(pMC);
	}

	if(::GetApp()->IsVerboseMode() && (--g_nVerboseCount == 0) && (g_VerboseFile.m_hFile != INVALID_HANDLE_VALUE))
	{
		g_VerboseFile.Close();
		Sleep(100);
	}
	pMC->SetExitCode(dwExitCode);
	pMC->SetProcessHandle(INVALID_HANDLE_VALUE);
	pMC->SetProcessThreadHandle(INVALID_HANDLE_VALUE);
	pMC->SetMonitorOut(INVALID_HANDLE_VALUE);

	CloseHandle(pi.hProcess); 
	CloseHandle(pi.hThread); 
	CloseHandle(hChildStdIn); 
	CloseHandle(hChildStdOut); 
	CloseHandle(hMonitorOut); 
	CloseHandle(hMonitorIn); 
	pMC->m_bMonitorThreadActive = false;
	pMC->GetConsoleCallback()->EndCallback(pMC);
	pMC->SetProcessId(0);
	return dwExitCode;
} 

///////////////////////////////////////////////////////////////////////////////////////
// CMonitorConsole

CMonitorConsole::CMonitorConsole()
{
//	*m_szCommandLine = 0;
	m_pConsoleCallback = NULL;
	m_nThreadPriority = 0;
	m_dwProcessPriority = 0;
	m_dwCreationFlags = 0;
	m_pMonitorThread = NULL;
	m_dwProcessId = 0;
	m_hProcess = INVALID_HANDLE_VALUE;
	m_hProcessThread = INVALID_HANDLE_VALUE;
	m_hMonitorOut = INVALID_HANDLE_VALUE;
	m_dwExitCode = 0;
	m_bMonitorThreadActive = false;
}

CMonitorConsole::~CMonitorConsole()
{
	StopConsole(false,true);
	if(m_pMonitorThread != NULL)
	{
		WaitForSingleObject(m_pMonitorThread->m_hThread, INFINITE);
		delete m_pMonitorThread;
		m_pMonitorThread = NULL;
	}
}

bool CMonitorConsole::StartConsole(LPCTSTR lpszCommandLine, CStringList* plssEnvironment, 
	CConsoleCallback* pConsoleCallback, int nThreadPriority, DWORD dwProcessPriority, 
	DWORD dwCreationFlags, LPCTSTR lpszInitialDir)
{
	CString strTemp;
	CString strPath;
	int nIndex1 = 0,nIndex2 = -1;
	bool bRC = false;
	LPTSTR lp;
	TCHAR** environment = _tenviron;

	StopConsole(false,true);
	m_strCommandLine = lpszCommandLine;
	m_bMonitorThreadActive = true;

	m_szEnvironment[0] = 0;
	if((plssEnvironment != NULL) && !plssEnvironment->IsEmpty())
	{
		POSITION Pos = plssEnvironment->GetHeadPosition();
		while(Pos)
		{
			strTemp = plssEnvironment->GetNext(Pos);
			if(_tcsnicmp(strTemp,_T("Path="),5) == 0)
				strPath = strTemp.Right(strTemp.GetLength() - 5);
			else
			{
				_tcscpy_s(&m_szEnvironment[nIndex1], _countof(m_szEnvironment) - nIndex1, strTemp);
				nIndex1 += strTemp.GetLength() + 1;
			}
		}
		while((lp = environment[++nIndex2]) != NULL) 
		{
			if(_tcsnicmp(lp,_T("Path="),5) == 0)
			{
				strTemp = _T("Path=") + strPath + _T(";") + &lp[5];
				lp = (LPTSTR) (LPCTSTR) strTemp;
			}
			_tcscpy_s(&m_szEnvironment[nIndex1], _countof(m_szEnvironment) - nIndex1, lp);
			nIndex1 += _tcslen(lp) + 1;
		}
		m_szEnvironment[nIndex1] = 0;
	}

	m_pConsoleCallback = pConsoleCallback;
	m_nThreadPriority = nThreadPriority;
	m_dwProcessPriority = dwProcessPriority;
	m_dwCreationFlags = dwCreationFlags;
	m_strInitialDir = lpszInitialDir;
	if((m_pMonitorThread = AfxBeginThread(MonitorThread,(LPVOID) this,m_nThreadPriority)) != NULL)
		bRC = true;
	else
		m_bMonitorThreadActive = false;
	return bRC;
}
/*
bool CMonitorConsole::StopConsole(bool bProcessTree, bool bRoot)
{
	bool bRC = false;
	if(m_dwProcessId != 0)
	{
		if(bProcessTree)
		{
			if(m_ProcessControl.IsEmpty())
				m_ProcessControl.TakeSnapShot(m_dwProcessId);
			bRC = m_ProcessControl.KillProcessTree(m_dwProcessId,bRoot);
		}
		else
			bRC = m_ProcessControl.KillProcess(m_dwProcessId);
	}
	m_ProcessControl.RemoveAll();
	return bRC;
}
*/
bool CMonitorConsole::StopConsole(bool bProcessTree, bool bRoot)
{
	bool bRC = false;
	if((m_dwProcessId != 0) && bRoot)
		bRC = m_ProcessControl.KillProcess(m_dwProcessId);
	m_ProcessControl.RemoveAll();
	return bRC;
}

bool CMonitorConsole::WriteToConsole(LPVOID pBuffer, DWORD dwBytesToWrite)
{
	DWORD dwBytesWritten;
	if(::GetApp()->IsVerboseMode() && (g_VerboseFile.m_hFile != INVALID_HANDLE_VALUE))
		g_VerboseFile.Write(pBuffer,dwBytesToWrite);
	if(m_hMonitorOut != INVALID_HANDLE_VALUE)
		if(WriteFile(m_hMonitorOut,pBuffer,dwBytesToWrite,&dwBytesWritten,NULL))
			return dwBytesWritten == dwBytesToWrite;
	return false;
}

bool CMonitorConsole::TakeSnapShot()
{
	return m_ProcessControl.TakeSnapShot(0);
}

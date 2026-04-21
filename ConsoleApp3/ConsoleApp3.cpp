#include <iostream>

#include "..\MonitorConsole\MonitorConsole.h"

class CCallback : public CConsoleCallback
{
public:
	virtual bool InitCallback(CMonitorConsole* pMC) { return true; }
	virtual bool RunCallback(CMonitorConsole* pMC) { return true; }
	virtual bool DataCallback(CMonitorConsole* pMC, LPVOID pchBuffer, DWORD dwBytesAvailable);
	virtual void EndCallback(CMonitorConsole* pMC) {}
	virtual void ErrorCallback(CMonitorConsole* pMC) {}
};

bool CCallback::DataCallback(CMonitorConsole* pMC, LPVOID pchBuffer, DWORD dwBytesAvailable)
{
	std::cout.write(static_cast<const char*>(pchBuffer), dwBytesAvailable);
	std::cout.flush();
	return true;
}

int main()
{
	CString str1 = "Hello World!\n";
	std::wcout << static_cast<const wchar_t*>(str1) << std::endl;

	CStringList lssDrvs;
	CStringList lssFiles;
	//bool b = EnumurateNetDevices(lssDrvs);
	//str1 = GetWinVersion();
	//CLoadFileList lfl(&lssFiles);
	//lfl.Go(_T("C:\\Src\\IntDevEnv"), _T("*.exe"), true, false);

	POSITION pos = lssFiles.GetHeadPosition();
	while (pos)
	{
		str1 = lssFiles.GetNext(pos);
		std::wcout << L"Version: " << static_cast<const wchar_t*>(str1);
		//str1 = GetFileVersion(str1);
		std::wcout << static_cast<const wchar_t*>(str1) << std::endl;
	}

	//str1 = GetWinVersion();
	std::wcout << L"Windows Version: " << static_cast<const wchar_t*>(str1) << std::endl << std::endl;

	CCallback callback;
	CMonitorConsole console;
	console.StartConsole(_T("cmd.exe /c dotnet --info"), NULL, &callback, 0, 0, 0, _T("C:\\Src\\IntDevEnv\\Projects\\Test1"));
	while (true)
	{
		if (!console.IsConsoleActive())
			break;
		Sleep(100);
	}
}

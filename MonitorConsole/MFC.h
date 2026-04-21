#pragma once

#include <cstdarg>
#include <string>
#include <vector>

#include <direct.h>
#include <io.h>
#include <shlwapi.h>
#include <shobjidl.h>
#include <tchar.h>

#include <windows.h>

#pragma warning(disable:4996)

#ifndef MONITORCONSOLE_EXPORTS
#ifdef PROJECT2_EXPORTS
#define MONITORCONSOLE_EXPORTS
#endif
#endif

#ifndef MONITORCONSOLE_API
#ifdef MONITORCONSOLE_EXPORTS
#define MONITORCONSOLE_API __declspec(dllexport)
#else
#define MONITORCONSOLE_API __declspec(dllimport)
#endif
#endif

class MONITORCONSOLE_API App
{
public:
	bool IsVerboseMode();
	LPCTSTR GetVerboseFileName();
	bool IsOldCygwin();
};

extern App app;
MONITORCONSOLE_API App* GetApp();

using POSITION = size_t;

class MONITORCONSOLE_API CString
{
public:
	CString();
	CString(TCHAR ch);
	CString(LPCTSTR lpsz);
	CString(const char* lpsz);
	CString(const CString&) = default;
	CString(CString&&) noexcept = default;
	~CString() = default;

	CString& operator=(const CString&) = default;
	CString& operator=(TCHAR ch);
	CString& operator=(LPCTSTR lpsz);
	CString& operator=(const char* lpsz);

	operator LPCTSTR() const;

	int GetLength() const;
	bool IsEmpty() const;
	void Empty();
	TCHAR GetAt(int nIndex) const;
	void SetAt(int nIndex, TCHAR ch);
	TCHAR operator[](int nIndex) const;
	CString Right(int nCount) const;
	CString Left(int nCount) const;
	CString Mid(int nFirst) const;
	CString Mid(int nFirst, int nCount) const;
	int Find(TCHAR ch, int nStart = 0) const;
	int Find(LPCTSTR lpszSub, int nStart = 0) const;
	int ReverseFind(TCHAR ch) const;
	int CompareNoCase(LPCTSTR lpsz) const;
	void Delete(int nIndex, int nCount = 1);
	void TrimLeft();
	void TrimRight();
	void Format(LPCTSTR lpszFormat, ...);
	void Format(const char* lpszFormat, ...);
	void FormatV(LPCTSTR lpszFormat, va_list args);
	LPTSTR GetBuffer(int nMinBufLength);
	void ReleaseBuffer(int nNewLength = -1);

	CString& operator+=(const CString& rhs);
	CString& operator+=(LPCTSTR rhs);
	CString& operator+=(const char* rhs);
	CString& operator+=(TCHAR ch);

	friend CString operator+(const CString& lhs, const CString& rhs);
	friend CString operator+(const CString& lhs, LPCTSTR rhs);
	friend CString operator+(LPCTSTR lhs, const CString& rhs);
	friend CString operator+(const CString& lhs, const char* rhs);
	friend CString operator+(const char* lhs, const CString& rhs);
	friend CString operator+(const CString& lhs, TCHAR ch);
	friend CString operator+(TCHAR ch, const CString& rhs);
	friend bool operator==(const CString& lhs, const CString& rhs);
	friend bool operator==(const CString& lhs, LPCTSTR rhs);
	friend bool operator==(LPCTSTR lhs, const CString& rhs);
	friend bool operator==(const CString& lhs, const char* rhs);
	friend bool operator==(const char* lhs, const CString& rhs);
	friend bool operator!=(const CString& lhs, const CString& rhs);
	friend bool operator!=(const CString& lhs, LPCTSTR rhs);
	friend bool operator!=(LPCTSTR lhs, const CString& rhs);
	friend bool operator!=(const CString& lhs, const char* rhs);
	friend bool operator!=(const char* lhs, const CString& rhs);

private:
	static std::basic_string<TCHAR> FromNarrow(const char* lpsz);
	std::basic_string<TCHAR> m_str;
};

class MONITORCONSOLE_API CStringList
{
public:
	bool IsEmpty() const;
	POSITION GetHeadPosition() const;
	CString GetNext(POSITION& pos) const;
	void AddTail(LPCTSTR value);
	POSITION Find(LPCTSTR value, POSITION startAfter = 0) const;
	void RemoveAt(POSITION pos);

private:
	std::vector<CString> m_items;
};

class CFile
{
public:
	enum OpenFlags
	{
		modeWrite = 0x0002,
		modeCreate = 0x1000,
		modeNoTruncate = 0x2000
	};

	enum SeekPosition
	{
		begin = FILE_BEGIN,
		current = FILE_CURRENT,
		end = FILE_END
	};

	CFile();
	~CFile();

	bool Open(LPCTSTR lpszFileName, unsigned int nOpenFlags);
	DWORD Seek(LONG lOff, unsigned int nFrom);
	void Write(const void* lpBuf, UINT nCount);
	void Close();

	HANDLE m_hFile;
};

class CWinThread
{
public:
	CWinThread();
	~CWinThread();

	HANDLE m_hThread;
	DWORD m_nThreadID;
};

class CPtrArray
{
public:
	int GetSize() const;
	void Add(void* p);
	void RemoveAll();
	void* operator[](int index) const;

private:
	std::vector<void*> m_items;
};

using AFX_THREADPROC = UINT (*)(LPVOID);

CWinThread* AfxBeginThread(AFX_THREADPROC pfnThreadProc, LPVOID pParam,
	int nPriority = THREAD_PRIORITY_NORMAL);

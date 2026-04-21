#include "pch.h"
#include "MFC.h"

#include <cstdio>

App app;

bool App::IsVerboseMode()
{
	return true;
}

LPCTSTR App::GetVerboseFileName()
{
	return _T("MonitorConsole.log");
}

bool App::IsOldCygwin()
{
	return false;
}

App* GetApp()
{
	return &app;
}

CString::CString()
{
}

CString::CString(TCHAR ch)
{
	m_str.assign(1, ch);
}

CString::CString(LPCTSTR lpsz) : m_str(lpsz ? lpsz : _T(""))
{
}

CString::CString(const char* lpsz) : m_str(FromNarrow(lpsz))
{
}

CString& CString::operator=(LPCTSTR lpsz)
{
	m_str = lpsz ? lpsz : _T("");
	return *this;
}

CString& CString::operator=(TCHAR ch)
{
	m_str.assign(1, ch);
	return *this;
}

CString& CString::operator=(const char* lpsz)
{
	m_str = FromNarrow(lpsz);
	return *this;
}

CString::operator LPCTSTR() const
{
	return m_str.c_str();
}

int CString::GetLength() const
{
	return static_cast<int>(m_str.length());
}

bool CString::IsEmpty() const
{
	return m_str.empty();
}

void CString::Empty()
{
	m_str.clear();
}

TCHAR CString::GetAt(int nIndex) const
{
	return m_str[static_cast<size_t>(nIndex)];
}

void CString::SetAt(int nIndex, TCHAR ch)
{
	m_str[static_cast<size_t>(nIndex)] = ch;
}

TCHAR CString::operator[](int nIndex) const
{
	return GetAt(nIndex);
}

CString CString::Right(int nCount) const
{
	if(nCount <= 0)
		return CString();

	const size_t count = static_cast<size_t>(nCount);
	if(count >= m_str.length())
		return *this;

	return CString(m_str.substr(m_str.length() - count).c_str());
}

CString CString::Left(int nCount) const
{
	if(nCount <= 0)
		return CString();

	const size_t count = static_cast<size_t>(nCount);
	if(count >= m_str.length())
		return *this;

	return CString(m_str.substr(0, count).c_str());
}

CString CString::Mid(int nFirst) const
{
	if(nFirst <= 0)
		return *this;

	const size_t first = static_cast<size_t>(nFirst);
	if(first >= m_str.length())
		return CString();

	return CString(m_str.substr(first).c_str());
}

CString CString::Mid(int nFirst, int nCount) const
{
	if(nFirst < 0 || nCount <= 0)
		return CString();

	const size_t first = static_cast<size_t>(nFirst);
	if(first >= m_str.length())
		return CString();

	return CString(m_str.substr(first, static_cast<size_t>(nCount)).c_str());
}

int CString::Find(TCHAR ch, int nStart) const
{
	if(nStart < 0)
		nStart = 0;

	const size_t pos = m_str.find(ch, static_cast<size_t>(nStart));
	return pos == std::basic_string<TCHAR>::npos ? -1 : static_cast<int>(pos);
}

int CString::Find(LPCTSTR lpszSub, int nStart) const
{
	if(lpszSub == nullptr)
		return -1;
	if(nStart < 0)
		nStart = 0;

	const size_t pos = m_str.find(lpszSub, static_cast<size_t>(nStart));
	return pos == std::basic_string<TCHAR>::npos ? -1 : static_cast<int>(pos);
}

int CString::ReverseFind(TCHAR ch) const
{
	const size_t pos = m_str.rfind(ch);
	return pos == std::basic_string<TCHAR>::npos ? -1 : static_cast<int>(pos);
}

int CString::CompareNoCase(LPCTSTR lpsz) const
{
	const LPCTSTR rhs = lpsz ? lpsz : _T("");
	return _tcsicmp(m_str.c_str(), rhs);
}

void CString::Delete(int nIndex, int nCount)
{
	if(nIndex < 0 || nCount <= 0)
		return;

	const size_t index = static_cast<size_t>(nIndex);
	if(index >= m_str.length())
		return;

	m_str.erase(index, static_cast<size_t>(nCount));
}

void CString::TrimLeft()
{
	size_t pos = 0;
	while(pos < m_str.length() && _istspace(static_cast<unsigned short>(m_str[pos])))
		++pos;
	m_str.erase(0, pos);
}

void CString::TrimRight()
{
	size_t pos = m_str.length();
	while(pos > 0 && _istspace(static_cast<unsigned short>(m_str[pos - 1])))
		--pos;
	m_str.erase(pos);
}

void CString::Format(LPCTSTR lpszFormat, ...)
{
	va_list args;
	va_start(args, lpszFormat);
	FormatV(lpszFormat, args);
	va_end(args);
}

void CString::Format(const char* lpszFormat, ...)
{
	va_list args;
	va_start(args, lpszFormat);
	const CString wideFormat(lpszFormat);
	FormatV(wideFormat, args);
	va_end(args);
}

void CString::FormatV(LPCTSTR lpszFormat, va_list args)
{
	if(lpszFormat == nullptr)
	{
		m_str.clear();
		return;
	}

	va_list argsCopy;
	va_copy(argsCopy, args);

	const int length = _vsctprintf(lpszFormat, args);
	if(length < 0)
	{
		va_end(argsCopy);
		m_str.clear();
		return;
	}

	std::vector<TCHAR> buffer(static_cast<size_t>(length) + 1);
	_vstprintf_s(buffer.data(), buffer.size(), lpszFormat, argsCopy);
	va_end(argsCopy);
	m_str.assign(buffer.data());
}

LPTSTR CString::GetBuffer(int nMinBufLength)
{
	if(nMinBufLength < 0)
		nMinBufLength = 0;

	m_str.resize(static_cast<size_t>(nMinBufLength));
	return m_str.data();
}

void CString::ReleaseBuffer(int nNewLength)
{
	if(nNewLength >= 0)
	{
		m_str.resize(static_cast<size_t>(nNewLength));
		return;
	}

	m_str.resize(_tcslen(m_str.c_str()));
}

CString& CString::operator+=(const CString& rhs)
{
	m_str += rhs.m_str;
	return *this;
}

CString& CString::operator+=(LPCTSTR rhs)
{
	if(rhs != nullptr)
		m_str += rhs;
	return *this;
}

CString& CString::operator+=(const char* rhs)
{
	m_str += FromNarrow(rhs);
	return *this;
}

CString& CString::operator+=(TCHAR ch)
{
	m_str += ch;
	return *this;
}

CString operator+(const CString& lhs, const CString& rhs)
{
	return CString((lhs.m_str + rhs.m_str).c_str());
}

CString operator+(const CString& lhs, LPCTSTR rhs)
{
	CString result(lhs);
	result += rhs;
	return result;
}

CString operator+(LPCTSTR lhs, const CString& rhs)
{
	CString result(lhs);
	result += rhs;
	return result;
}

CString operator+(const CString& lhs, const char* rhs)
{
	CString result(lhs);
	result += rhs;
	return result;
}

CString operator+(const char* lhs, const CString& rhs)
{
	CString result(lhs);
	result += rhs;
	return result;
}

CString operator+(const CString& lhs, TCHAR ch)
{
	CString result(lhs);
	result += ch;
	return result;
}

CString operator+(TCHAR ch, const CString& rhs)
{
	CString result;
	result += ch;
	result += rhs;
	return result;
}

bool operator==(const CString& lhs, const CString& rhs)
{
	return lhs.CompareNoCase(rhs) == 0;
}

bool operator==(const CString& lhs, LPCTSTR rhs)
{
	return lhs.CompareNoCase(rhs) == 0;
}

bool operator==(LPCTSTR lhs, const CString& rhs)
{
	return rhs.CompareNoCase(lhs) == 0;
}

bool operator==(const CString& lhs, const char* rhs)
{
	return lhs.CompareNoCase(CString(rhs)) == 0;
}

bool operator==(const char* lhs, const CString& rhs)
{
	return rhs.CompareNoCase(CString(lhs)) == 0;
}

bool operator!=(const CString& lhs, const CString& rhs)
{
	return !(lhs == rhs);
}

bool operator!=(const CString& lhs, LPCTSTR rhs)
{
	return !(lhs == rhs);
}

bool operator!=(LPCTSTR lhs, const CString& rhs)
{
	return !(lhs == rhs);
}

bool operator!=(const CString& lhs, const char* rhs)
{
	return !(lhs == rhs);
}

bool operator!=(const char* lhs, const CString& rhs)
{
	return !(lhs == rhs);
}

std::basic_string<TCHAR> CString::FromNarrow(const char* lpsz)
{
	if(lpsz == nullptr)
		return {};

#ifdef UNICODE
	const int required = MultiByteToWideChar(CP_ACP, 0, lpsz, -1, nullptr, 0);
	if(required <= 1)
		return {};

	std::wstring buffer(static_cast<size_t>(required - 1), L'\0');
	MultiByteToWideChar(CP_ACP, 0, lpsz, -1, buffer.data(), required);
	return buffer;
#else
	return std::string(lpsz);
#endif
}

bool CStringList::IsEmpty() const
{
	return m_items.empty();
}

POSITION CStringList::GetHeadPosition() const
{
	return m_items.empty() ? 0 : 1;
}

CString CStringList::GetNext(POSITION& pos) const
{
	if(pos == 0 || pos > m_items.size())
		return CString();

	const size_t index = pos - 1;
	pos = (index + 1 < m_items.size()) ? (index + 2) : 0;
	return m_items[index];
}

void CStringList::AddTail(LPCTSTR value)
{
	m_items.emplace_back(value);
}

POSITION CStringList::Find(LPCTSTR value, POSITION startAfter) const
{
	size_t startIndex = 0;
	if(startAfter > 0)
		startIndex = startAfter;

	for(size_t i = startIndex; i < m_items.size(); ++i)
		if(m_items[i].CompareNoCase(value) == 0)
			return i + 1;

	return 0;
}

void CStringList::RemoveAt(POSITION pos)
{
	if(pos == 0 || pos > m_items.size())
		return;

	m_items.erase(m_items.begin() + static_cast<std::ptrdiff_t>(pos - 1));
}

CFile::CFile() : m_hFile(INVALID_HANDLE_VALUE)
{
}

CFile::~CFile()
{
	Close();
}

bool CFile::Open(LPCTSTR lpszFileName, unsigned int nOpenFlags)
{
	Close();

	DWORD creationDisposition = OPEN_EXISTING;
	if((nOpenFlags & modeCreate) != 0)
		creationDisposition = ((nOpenFlags & modeNoTruncate) != 0) ? OPEN_ALWAYS : CREATE_ALWAYS;

	const DWORD desiredAccess = ((nOpenFlags & modeWrite) != 0) ? GENERIC_WRITE : GENERIC_READ;
	m_hFile = ::CreateFile(lpszFileName, desiredAccess, FILE_SHARE_READ, nullptr, creationDisposition,
		FILE_ATTRIBUTE_NORMAL, nullptr);
	return m_hFile != INVALID_HANDLE_VALUE;
}

DWORD CFile::Seek(LONG lOff, unsigned int nFrom)
{
	return ::SetFilePointer(m_hFile, lOff, nullptr, nFrom);
}

void CFile::Write(const void* lpBuf, UINT nCount)
{
	if(m_hFile == INVALID_HANDLE_VALUE)
		return;

	DWORD written = 0;
	::WriteFile(m_hFile, lpBuf, nCount, &written, nullptr);
}

void CFile::Close()
{
	if(m_hFile != INVALID_HANDLE_VALUE)
	{
		::CloseHandle(m_hFile);
		m_hFile = INVALID_HANDLE_VALUE;
	}
}

CWinThread::CWinThread() : m_hThread(nullptr), m_nThreadID(0)
{
}

CWinThread::~CWinThread()
{
	if(m_hThread != nullptr)
		::CloseHandle(m_hThread);
}

int CPtrArray::GetSize() const
{
	return static_cast<int>(m_items.size());
}

void CPtrArray::Add(void* p)
{
	m_items.push_back(p);
}

void CPtrArray::RemoveAll()
{
	m_items.clear();
}

void* CPtrArray::operator[](int index) const
{
	return m_items[static_cast<size_t>(index)];
}

CWinThread* AfxBeginThread(AFX_THREADPROC pfnThreadProc, LPVOID pParam, int nPriority)
{
	if(pfnThreadProc == nullptr)
		return nullptr;

	CWinThread* pThread = new CWinThread();
	pThread->m_hThread = ::CreateThread(nullptr, 0,
		reinterpret_cast<LPTHREAD_START_ROUTINE>(pfnThreadProc), pParam, 0, &pThread->m_nThreadID);
	if(pThread->m_hThread == nullptr)
	{
		delete pThread;
		return nullptr;
	}

	::SetThreadPriority(pThread->m_hThread, nPriority);
	return pThread;
}

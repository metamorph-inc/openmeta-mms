
#include <vector>
#include <algorithm>
#include <iterator>

class PathUtil {
	std::vector<CString> 				m_vecPaths;
	bool								m_bArePathsValid;

	void addLibraryPaths(std::vector<CString>& vecPathes, IMgaProject* Project) {
		std::vector<CString> newPaths;
		CComBSTR ProjectConnStr;
		COMTHROW(Project->get_ProjectConnStr(&ProjectConnStr));
		CString projectPath = CString(static_cast<const wchar_t*>(ProjectConnStr));
		projectPath = projectPath.Right(projectPath.GetLength() - wcslen(L"MGA="));
		wchar_t projectFullPath[MAX_PATH] = { 0 };
		wchar_t* filepart;
		if (GetFullPathNameW(static_cast<const wchar_t*>(projectPath), _countof(projectFullPath), projectFullPath, &filepart) && filepart)
		{
			*filepart = L'\0';
		}

		CComPtr<IMgaFolder> rootFolder;
		COMTHROW(Project->get_RootFolder(&rootFolder));
		CComPtr<IMgaFolders> rootFolders;
		COMTHROW(rootFolder->get_ChildFolders(&rootFolders));
		long count;
		COMTHROW(rootFolders->get_Count(&count));
		for (int i = 1; i <= count; i++) {
			CComPtr<IMgaFolder> rootFolder;
			COMTHROW(rootFolders->get_Item(i, &rootFolder));
			CComBSTR libraryName;
			rootFolder->get_LibraryName(&libraryName);
			if (libraryName && libraryName.Length()) {
				CString path;
				wchar_t fullPath[MAX_PATH] = { 0 };
				if (GetFullPathNameW(static_cast<const wchar_t*>(libraryName), _countof(fullPath), fullPath, nullptr)) {
					if (wcscmp(static_cast<const wchar_t*>(libraryName), fullPath) == 0) {
						path = fullPath;
					}
					else {
						path = CString(projectFullPath) + static_cast<const wchar_t*>(libraryName);
					}
					wchar_t* filepart;
					if (GetFullPathNameW(static_cast<const wchar_t*>(path), _countof(fullPath), fullPath, &filepart) && filepart) {
						*filepart = 0;
						newPaths.push_back(fullPath);
						newPaths.push_back(CString(fullPath) + L"icons\\");
					}
				}
			}
		}

		std::sort(newPaths.begin(), newPaths.end());
		newPaths.erase(std::unique(newPaths.begin(), newPaths.end()), newPaths.end());
		std::copy(newPaths.begin(), newPaths.end(), std::back_inserter(vecPathes));
	}

public:

	PathUtil() : m_bArePathsValid( false ) {}

	std::vector<CString> getPaths() const
	{
		return m_vecPaths;
	}

	bool arePathsValid() const
	{
		return m_bArePathsValid;
	}

	bool loadPaths(IMgaProject* pProject, bool bRefresh)
	{
		CString m_strParadigmPath;
		CString	m_strProjectPath;

		if ( bRefresh ) {
			m_vecPaths.clear();
			m_bArePathsValid = false;

			if ( pProject ) {
				long lStatus;
				COMTHROW( pProject->get_ProjectStatus( &lStatus ) );
				if ( (lStatus & 0x01L) != 0 ) {
					CComBSTR bstrParadigm;
					COMTHROW( pProject->get_ParadigmConnStr( &bstrParadigm ) );
					m_strParadigmPath = CString( bstrParadigm );
					if ( m_strParadigmPath.Find( _T("MGA=") ) == 0 ) {
						int iPos = m_strParadigmPath.ReverseFind( _T('\\') );
						if( iPos >= 4 ) {
							m_strParadigmPath = m_strParadigmPath.Mid( 4, iPos - 4 );
							if( m_strParadigmPath.IsEmpty() )
								m_strParadigmPath = '\\';
						}
					}

					CComBSTR bstrProject;
					COMTHROW( pProject->get_ProjectConnStr( &bstrProject ) );
					m_strProjectPath = CString( bstrProject );
					if ( m_strProjectPath.Find( _T("MGA=") ) == 0 ) {
						int iPos = m_strProjectPath.ReverseFind( _T('\\') );
						if( iPos >= 4 ) {
							m_strProjectPath = m_strProjectPath.Mid( 4, iPos - 4 );
							if( m_strProjectPath.IsEmpty() )
								m_strProjectPath = '\\';
						}
					}
				}
			}
		}

		if ( ! m_bArePathsValid ) {

			CString strPath;
			try {
				CComPtr<IMgaRegistrar> spRegistrar;
				COMTHROW( spRegistrar.CoCreateInstance( OLESTR( "Mga.MgaRegistrar" ) ) );
				CComBSTR bstrPath;
				COMTHROW( spRegistrar->get_IconPath( REGACCESS_BOTH, &bstrPath ) );

				strPath = bstrPath;
			}
			catch ( hresult_exception & ) {
			}

			strPath.Replace( _T("$PARADIGMDIR"), m_strParadigmPath );
			strPath.Replace( _T("$PROJECTDIR"), m_strProjectPath );

			while( ! strPath.IsEmpty() ) {
				int iPos = strPath.Find( ';' );
				if( iPos == 0) // zolmol: if accidentaly there are two semicolons, or the path starts with a semicolon
				{
					strPath = strPath.Right( strPath.GetLength() - 1 );
					continue;
				}
				CString strDir;
				if ( iPos != -1 ) {
					strDir = strPath.Left( iPos );
					strPath = strPath.Right( strPath.GetLength() - iPos - 1 );
				}
				else {
					strDir = strPath;
					strPath.Empty();
				}
				strDir.Replace( '/', '\\' );
				if ( strDir.GetAt( strDir.GetLength() - 1 ) != '\\' )
					strDir += _T("\\");
				m_vecPaths.push_back( strDir );
			}
			m_vecPaths.push_back( _T(".\\") );

			long status = 0;
			pProject->get_ProjectStatus(&status);
			// need to be in a transaction to read library paths
			if (status & 0x8) {
				addLibraryPaths(m_vecPaths, pProject);

				m_bArePathsValid = true;
			}
		}

		return m_bArePathsValid;
	}
};

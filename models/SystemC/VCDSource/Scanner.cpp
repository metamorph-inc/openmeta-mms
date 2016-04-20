/**********************************************************************
* author£ºGavin
* time£º11-08-2009
* location£ºShanxi Xi'an NWPU Computer Science Building 218#
* file description£º
*		This is the implement file of CScanner class  
***********************************************************************/

#include "Scanner.h"

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CScanner default constructor
CScanner::CScanner(void)
: m_hVcdFile( NULL )
, m_pBuf( NULL )
, m_pCur( NULL )
, m_pTail( NULL )
{
	// allocate memory for buffer
	m_pBuf = new char[BUF_SIZE];
	if ( m_pBuf )
		exit(-1);
	memset( m_pBuf, 0, BUF_SIZE );
	m_pCur = m_pTail = m_pBuf;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CScanner constructor
CScanner::CScanner( const char* path )
: m_hVcdFile( NULL )
, m_pBuf( NULL )
, m_pCur( NULL )
, m_pTail( NULL )
{
	// open the file
	if ( m_hVcdFile = fopen( path, "r"))
	{
		strcpy( m_strFilePath, path );

		// allocate memory for buffer
		if ( (m_pBuf = new char[BUF_SIZE]) == NULL )
			exit( -1 );
		memset( m_pBuf, 0, BUF_SIZE );
		m_pCur = m_pTail = m_pBuf;
	}
	else
	{
		cerr << "can't open file " << path << endl; 
	}
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CScanner default destructor
CScanner::~CScanner(void)
{
	delete m_pBuf;
	if ( m_hVcdFile )
		fclose(m_hVcdFile);
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CScanner reopen method
//		close current file if exist, and open file by path 
// Input:
//			path - file path 
// Output:
//			none
// Return:
//			zero if success, otherwise none zero
int CScanner::reopen( const char* newPath )
{
	if ( !strcmp( newPath, m_strFilePath ) )
		return 0;

	FILE* old = m_hVcdFile;

	// reopen another file and set buffer initial state
	if ( m_hVcdFile = fopen( newPath, "r") )
	{
		fclose(old);
		strcmp( m_strFilePath, newPath );
		memset( m_pBuf, 0, BUF_SIZE );
		m_pCur = m_pTail = m_pBuf;
	}
	else
	{
		m_hVcdFile = old;
		cerr << "can't open file " << newPath << endl;
		return -1;
	}

	return 0;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CScanner get_stream method
//		fetch stream from file to buffer
// Input:
//			none 
// Output:
//			none
// Return:
//			the first character's ASCII value, 
//			-1 if reach the end of file or error occured when read file
int CScanner::get_stream()
{
//	errno = 0;

	if ( !m_hVcdFile )
		return (-2);
	
	if( feof( m_hVcdFile ) ) 
		return (-1);

	size_t num = fread( m_pBuf, sizeof(char), BUF_SIZE, m_hVcdFile );
	m_pCur = m_pBuf;
	m_pTail = m_pCur + num;

	if((!num)/*||(errno)*/)
		return (-3);

	return (int)(*m_pCur);
}


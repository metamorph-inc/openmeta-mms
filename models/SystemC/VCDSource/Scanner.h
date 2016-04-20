/**********************************************************************
* author£ºGavin
* time£º11-08-2009
* location£ºShanxi Xi'an NWPU Computer Science Building 218#
* file description£º
*		This is the header file of CScanner class  
***********************************************************************/
#ifndef VCDPARSER_SCANNER_H_
#define VCDPARSER_SCANNER_H_

#include <stdio.h>
#include <stdlib.h>
#include <iostream>
using namespace std;

#define BUF_SIZE	4096

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CUncopyable 
// as base class only, prevent derived class from copying 
//
class CUncopyable
{
protected:					// protected, allow derived constructing & destructing
	CUncopyable(){};
	~CUncopyable(){};		// non-virtual destructor, due to there is noting in it
private:					// private, prevent derived copying
	CUncopyable(const CUncopyable&);
	CUncopyable& operator=(const CUncopyable&);
};

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer
// scan the input VCD file and get every single character, send to tokenizer
// this is the interface to IO of the parser system
//
class CScanner : public CUncopyable
{
public:
	CScanner(void);
	CScanner( const char* );
	virtual ~CScanner(void);

	/*
	 * no need to declaration copy constructor & assignment operator
	CScanner(const CScanner&);
	CScanner& operator=(const CScanner&);
	*/

private:
	int get_stream();					// get string stream from file to buffer

public:
	int reopen(const char*);			// close current file and reopen another file
	inline signed char get_char();		// send the next char to tokenizer
	inline signed char get_peek();		// get the char and stay in there, does move the character pointer

private:
	char	m_strFilePath[FILENAME_MAX];// the full path of the current file
	FILE*	m_hVcdFile;					// the input vcd file handler
	char*	m_pBuf;						// buffer, stored max BUF_SIZE characters
	char*	m_pCur;						// pointer to current position in the buffer
	char*	m_pTail;					// the tail of buffer, just the next position after the last one of buffer
};

inline signed char CScanner::get_char()
{
	signed char ch = (m_pCur != m_pTail) ? (*m_pCur) : (get_stream());
	if ( ch >= 0 )
		m_pCur++;
	return ch;
}

/* add by Gavin 2009-11-10 BOF */
// for next_var_token()
// sometimes it need getting the next character
// and then determine what to do presently
// the next character can't be get by get_char()
// or else the current position of file will move to next position
// that's not what we wanted
inline signed char CScanner::get_peek()
{
	signed char ch = (m_pCur != m_pTail) ? (*m_pCur) : (get_stream());
/*
 * don't increase the pointer 

	if ( ch >= 0 )
		m_pCur++;
*
*/
	return ch;
}
/* add by Gavin 2009-11-10 EOF */

#endif
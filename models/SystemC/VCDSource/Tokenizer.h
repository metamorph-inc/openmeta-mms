/**********************************************************************
* author£ºGavin
* time£º11-09-2009
* location£ºShanxi Xi'an NWPU Computer Science Building 218#
* file description£º
*		This is the header file of CTokenzier class  
***********************************************************************/

#ifndef VCDPARSER_TOKENIZER_H_
#define VCDPARSER_TOKENIZER_H_

#include "Scanner.h"
#include "Keyword.h"

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer
// scan the input VCD file and get every token word separate by nature separator
// return the type of the token word
// 
class CTokenizer : private CScanner
{
public:
	CTokenizer(void);
	CTokenizer( const char* );
	~CTokenizer(void);

	/*
	 * inheritance from CUncopyable
	CTokenizer( const CTokenizer& );
	CTokenizer& operator=( const CTokenizer& );
	*/

private:
	inline void put_char( signed char, int& );	// put ch into m_strTokenWord

public:
	const char* get_word() const{ return m_strTokenWord ;}
	inline void to_end();					// get next token until $end or eof
	int next_token();						// analyze the normal token word, return it's token type
	int next_scope_token();					// get the TT_STRING string from next_token(), return the scope token type
	int next_tscale_token();				// get the TT_STRING string from next_token(), return the time scale token type
	int next_var_token(bool is_key=false);	// analyze the input file text, return the var token type
	int next_str_token();					// get the value change signal name, most simple method, separated by white space

private:
	static int m_nMaxTokenLen;		// max length of the token word
	char*	m_strTokenWord;			// the lexical token word
	int		m_nTokenLen;			// the length of the token
	int		m_nPrevChar;			// the prevenient char used in next_var_token
};

inline void CTokenizer::put_char( signed char ch, int& len )
{
	if ( len == m_nMaxTokenLen )
	{
		char* old = m_strTokenWord;
		m_strTokenWord = new char[m_nMaxTokenLen = m_nMaxTokenLen * 2 + 1];
		memcpy( m_strTokenWord, old, len );
		delete old;
	}
	m_strTokenWord[len++] = ch;
}

inline void CTokenizer::to_end()
{
	while(int tok=next_token())if(tok ==TT_END||tok==TT_EOF)return;
}
#endif
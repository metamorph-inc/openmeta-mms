/**********************************************************************
* author£ºGavin
* time£º11-09-2009
* location£ºShanxi Xi'an NWPU Computer Science Building 218#
* file description£º
*		This is the implement file of CScanner class
*
*	|-->	11-10	catching cold at the sensitive time, trapped myself in bedroom
*	|-->			complete next_token() to next_str_token();
***********************************************************************/

#include "Tokenizer.h"

int CTokenizer::m_nMaxTokenLen = 20;	// initial with 20, maybe get longer, can't be a constant

// the skim_over_space macro, used with {} just like "{skim_over_space(ch);}" or there maybe something you won't want happen
#define  skim_over_space(ch) while((ch = get_char())<=' ')if(ch==-1)return TT_EOF

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer default constructor
CTokenizer::CTokenizer(void)
: m_strTokenWord( NULL )
, m_nTokenLen( 0 )
, m_nPrevChar( 0 )
{
	if ( !(m_strTokenWord = new char[m_nMaxTokenLen + 1]) )
		exit(-1);
	memset( m_strTokenWord, 0, m_nMaxTokenLen + 1 );
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer constructor
CTokenizer::CTokenizer( const char* path )
: CScanner( path )
, m_strTokenWord( NULL )
, m_nTokenLen( 0 )
, m_nPrevChar( 0 )
{
	if ( !(m_strTokenWord = new char[m_nMaxTokenLen + 1]) )
		exit(-1);
	memset( m_strTokenWord, 0, m_nMaxTokenLen + 1 );
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer destructor
CTokenizer::~CTokenizer(void)
{
	delete m_strTokenWord;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer next_token method
//		get a lexical token, and return it's type
// Input:
//			none 
// Output:
//			none
// Return:
//			enum token_type 
//			
int CTokenizer::next_token()
{
	signed char ch;
	bool is_keyword = false;
	int len = 0;

	// skim over the space, get_char() return the value of text character
	// ch <= ' ' can be used to quickly check white space such as spaces, tabs, newlines, formfeeds ect
	// get_char() return -1 means reaching the end of file
/*
	while ( (ch = get_char()) <= ' ' )
	{
		if ( ch == -1 )
			return TT_EOF;
	}
*/
	{skim_over_space(ch);}

	// is this string begin with $
	if ( ch == '$' )
		is_keyword = true;

	// copy the string into m_strTokenWord
	do 
	{
		/*
		if ( len == m_nMaxTokenLen )
		{
			char* old = m_strTokenWord;
			m_strTokenWord = new char[m_nMaxTokenLen = m_nMaxTokenLen * 2 + 1];
			memcpy( m_strTokenWord, old, len + 1);
			delete old;
		}
		m_strTokenWord[len++] = ch;
		*/
		put_char( ch, len );
		ch = get_char();
	} while ( ch > ' ' );	// copy string until a space or eof
	m_strTokenWord[len] = 0;

	if ( !is_keyword )
	{
		m_nTokenLen = len;
		return TT_STRING;
	}

	// search the string begin with $ in the keyword list
	int index = map_tokens( m_strTokenWord + 1 );
	return index < NUM_TOKENS ? index : TT_NONE;

/*
	for ( int i = 0; i < NUM_TOKENS; ++i )
		if ( !strcmp( m_strTokenWord + 1, tokens[i] ))
			return (i);

	return TT_NONE;
*/
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer next_scope_token method
//		get a scope token, and return it's type
// Input:
//			none 
// Output:
//			none
// Return:
//			enum scope_token_type 
//			
int CTokenizer::next_scope_token()
{
	int tok = next_token();

	if ( tok != TT_STRING )
		return tok;

	int index = map_scope_tokens( m_strTokenWord );
	return index < NUM_SCOPE_TOKENS ? index : ST_IDENTIFIER;
/*
	for ( int i = 0; i < NUM_SCOPE_TOKENS; ++i )
		if ( !strcmp( m_strTokenWord, scope_tokens[i]))
			return (i);

	return ST_IDENTIFIER;
*/
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer next_tscale_token method
//		get a time scale token, and return it's type
// Input:
//			none 
// Output:
//			none
// Return:
//			enum tscale_token_type 
//			
int CTokenizer::next_tscale_token()
{
	int tok = next_token();

	if ( tok != TT_STRING )
		return tok;

	int index = map_tscale_tokens( m_strTokenWord );
	return index < NUM_TSCALE_TOKENS ? index : TT_NONE;

/*
	for ( int i = 0; i < NUM_TSCALE_TOKENS; ++i )
		if ( !strcmp( m_strTokenWord, tscale_tokens[i]))
			return (i);

	return TT_NONE;
*/
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer next_var_token method
//		get a var token, and return it's type
// Input:
//			none 
// Output:
//			none
// Return:
//			enum var_token_type 
//			
int CTokenizer::next_var_token( bool is_key )
{
	signed char ch;
	int len = 0;

	// skim over space if exist
	if ( !m_nPrevChar )
	{
		skim_over_space(ch);
	}
	else
	{
		ch = m_nPrevChar;
		m_nPrevChar = 0;
	}

	if ( ch == '[' )return VT_LSQR;
	if ( ch == ':' )return VT_COLON;
	if ( ch == ']' )return VT_RSQR;

	// is this a decimal number, begin with 1~9
	// sequence of digits
	if ( ch <= '9' && ch >= '0')
	{
		do
		{
			put_char( ch, len );
			ch = get_char();
			if ( ch <= ' ' || ch == ':' || ch == ']')	// the legal postfix of decimal number
			{
				m_nPrevChar = ch > ' ' ? ch : 0;		// record the character as prev char in next var token
				m_strTokenWord[len] = 0;				// put end character into token word
				m_nTokenLen = len;						// get the length of token 
				return VT_DECIM;
			}
		}
		while ( ch <= '9' && ch >= '0' );
		return VT_NONE;		// illegal postfix of decimal number
	}

	// is this an identifier or keyword, 
	// begin with letters or underscore characters(_)
	// any sequence of letters, digits, dollars($) and underscore characters(_)
	signed char ch2;
	do 
	{
		put_char( ch, len );
		if ( (ch2 = get_peek()) == '[' )
		{
			m_nPrevChar = get_char();
			break;
		}
		else
			ch = get_char();
	} while ( ch > ' ' );
	m_strTokenWord[len] = 0;

	if ( !is_key )
	{
		m_nTokenLen = len;
		return VT_IDENTIFER;
	}

	int index = map_var_tokens( m_strTokenWord );
	return index < NUM_VAR_TOKENS ? index : VT_NONE;
}

/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// class CTokenizer next_str_token method
//		get a value change string
// Input:
//			none 
// Output:
//			none
// Return:
//			length of the value change string 
//			
int CTokenizer::next_str_token()
{
	signed char ch;
	int len = 0;

	{skim_over_space(ch);}

	do 
	{
		put_char(ch, len);
		ch = get_char();
	} while (ch > ' ');
	m_strTokenWord[len] = 0;
	m_nTokenLen = len;
	return len;
}

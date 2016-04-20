/**********************************************************************
* author£ºGavin
* time£º11-09-2009
* location£ºShanxi Xi'an NWPU Computer Science Building 218#
* file description£º
*		This is keyword type definition file  
***********************************************************************/

#ifndef VCDPARSER_KEYWORD_H_
#define VCDPARSER_KEYWORD_H_

inline int map_tokens_base( const char* key,
						    const char* token_lib[],
							int num )
{
	int index = 0;
	for ( ; index < num; ++index )
		if (!strcmp(key, token_lib[index]))
			return index;
	return index;
}

///////////////////////////////////////////////////////////////////////
// token type

#define NUM_TOKENS	13		// numbers of keyword, only the keyword, not all types

typedef enum token_type
{
	TT_END, TT_VAR, TT_COMMENT, TT_SCOPE, 
	TT_UPSCOPE,	TT_DATE, TT_TIMESCALE, 
	TT_ENDDEFINITIONS, TT_VERSION, TT_DUMPALL, 
	TT_DUMPOFF, TT_DUMPON,TT_DUMPVARS, 
	TT_STRING, TT_EOF, TT_NONE
}T_Tokens;

static const char *tokens[] = 
{
	"end",	"var",	"comment",	"scope",
	"upscope",	"date",	"timescale",
	"enddefinitions",	"version",	"dumpall",
	"dumpoff",	"dumpon",	"dumpvars",
	"",	"",	""
};

inline int map_tokens( const char* key )
{ 
	return map_tokens_base( key, tokens, NUM_TOKENS ); 
}

/////////////////////////////////////////////////////////////////////////
// scope token type

#define NUM_SCOPE_TOKENS	5
typedef enum scope_token_type
{
	ST_MODULE,	ST_FUNCTION,	ST_TASK,	
	ST_FORK,	ST_BEGIN,	
	ST_IDENTIFIER
}T_ScopeTokens;

static const char *scope_tokens[] = 
{
	"module",	"function",	"task",
	"fork",		"begin",	
	""
};

inline int map_scope_tokens( const char* key )
{
	return map_tokens_base( key, scope_tokens, NUM_SCOPE_TOKENS ); 
}

/////////////////////////////////////////////////////////////////////////
// timescale tokens

#define	NUM_TSCALE_TOKENS	9

typedef enum timescale_token_type
{
	TST_ONE,	TST_TEN,	TST_HUND,
	TST_SEC,	TST_M,	TST_U,
	TST_N,	TST_P,	TST_F
}T_TScaleTokens;

static const char *tscale_tokens[] = 
{
	"1",	"10",	"100",
	"s",	"ms",	"us",
	"ns",	"ps",	"fs"
};

inline int map_tscale_tokens( const char* key )
{
	return map_tokens_base( key, tscale_tokens, NUM_TSCALE_TOKENS ); 
}

///////////////////////////////////////////////////////////////////////
// var tokens

#define NUM_VAR_TOKENS	17	

typedef enum var_token_type
{
	VT_REG,		VT_PARAMETER,VT_INTEGER,	VT_REAL,
	VT_EVENT,	VT_SUPPLY0,	VT_SUPPLY1,	VT_TIME,
	VT_TRI,		VT_TRIAND,	VT_TRIOR,	VT_TRIREG,	
	VT_TRI0,	VT_TRI1,	VT_WAND,	VT_WIRE,	VT_WOR,
	VT_LSQR,	VT_RSQR,	VT_COLON,	
	VT_DECIM,	VT_IDENTIFER,	VT_NONE	
}T_VarTokens;

static const char* var_tokens[] = 
{
	"reg",	"parameter",	"integer",	"real",
	"event",	"supply0",	"supply1",	"time",
	"tri",	"triand",		"trior",	"trireg",
	"tri0",	"tri1",	"wand",	"wire",	"wor",
	"[",	"]",	":",	
	"",	"",	""
};

inline int map_var_tokens( const char* key )
{
	return map_tokens_base( key, var_tokens, NUM_VAR_TOKENS ); 
}
#endif


﻿using System;

namespace Ruby {

	public enum ASTNodeType {
		METHOD,
		SCOPE,
		BLOCK,
		IF,
		CASE,
		WHEN,
		WHILE,
		UNTIL,
		ITER,
		FOR,
		BREAK,
		NEXT,
		REDO,
		RETRY,
		BEGIN,
		RESCUE,
		ENSURE,
		AND,
		OR,
		NOT,
		MASGN,
		ASGN,
		CDECL,
		CVASGN,
		CVDECL,
		OP_ASGN,
		CALL,
		SCALL,
		FCALL,
		SUPER,
		ZSUPER,
		ARRAY,
		ZARRAY,
		HASH,
		KW_HASH,
		RETURN,
		YIELD,
		LVAR,
		DVAR,
		GVAR,
		IVAR,
		CONST,
		CVAR,
		NTH_REF,
		BACK_REF,
		MATCH,
		INT,
		FLOAT,
		NEGATE,
		LAMBDA,
		SYM,
		STR,
		DSTR,
		XSTR,
		DXSTR,
		REGX,
		DREGX,
		DREGX_ONCE,
		ARG,
		ARGS_TAIL,
		KW_ARG,
		KW_REST_ARGS,
		SPLAT,
		TO_ARY,
		SVALUE,
		BLOCK_ARG,
		DEF,
		SDEF,
		ALIAS,
		UNDEF,
		CLASS,
		MODULE,
		SCLASS,
		COLON2,
		COLON3,
		DOT2,
		DOT3,
		SELF,
		NIL,
		TRUE,
		FALSE,
		DEFINED,
		POSTEXE,
		DSYM,
		HEREDOC,
		LITERAL_DELIM,
		WORDS,
		SYMBOLS,
		LAST
	}
}

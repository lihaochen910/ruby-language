﻿using System;

namespace Ruby {

	public enum ASTNodeType {
		SCOPE,
		BLOCK,
		IF,
		UNLESS,
		CASE,
		CASE2,
		WHEN,
		WHILE,
		UNTIL,
		ITER,
		FOR,
		FOR_MASGN,
		BREAK,
		NEXT,
		REDO,
		RETRY,
		BEGIN,
		RESCUE,
		RESBODY,
		ENSURE,
		AND,
		OR,
		MASGN,
		LASGN,
		DASGN,
		DASGN_CURR,
		GASGN,
		IASGN,
		CDECL,
		CVASGN,
		OP_ASGN1,
		OP_ASGN2,
		OP_ASGN_AND,
		OP_ASGN_OR,
		OP_CDECL,
		CALL,
		OPCALL,
		FCALL,
		VCALL,
		QCALL,
		SUPER,
		ZSUPER,
		ARRAY,
		ZARRAY,
		VALUES,
		HASH,
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
		MATCH2,
		MATCH3,
		LIT,
		STR,
		DSTR,
		XSTR,
		DXSTR,
		EVSTR,
		DREGX,
		ONCE,
		ARGS,
		ARGS_AUX,
		OPT_ARG,
		KW_ARG,
		POSTARG,
		ARGSCAT,
		ARGSPUSH,
		SPLAT,
		BLOCK_PASS,
		DEFN,
		DEFS,
		ALIAS,
		VALIAS,
		UNDEF,
		CLASS,
		MODULE,
		SCLASS,
		COLON2,
		COLON3,
		DOT2,
		DOT3,
		FLIP2,
		FLIP3,
		SELF,
		NIL,
		TRUE,
		FALSE,
		ERRINFO,
		DEFINED,
		POSTEXE,
		DSYM,
		ATTRASGN,
		LAMBDA,
		LAST
	}
}
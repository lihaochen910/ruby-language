using System;
using System.Linq;
using System.Text;

namespace Diggins.Jigsaw {

	/// <summary>
	/// See: https://ruby-doc.org/docs/ruby-doc-bundle/Manual/man-1.4/syntax.html
	/// </summary>
	public class RubyGrammar : Grammar {

		//----------------------------------------------------------------//
		/**	@ruby	Recursive rules defined at the top
		*/
		public static Rule RecBlock					= Recursive ( () => Block );
		public static Rule RecExpr					= Recursive ( () => Expr );
		public static Rule RecStatement				= Recursive ( () => Statement );
		public static Rule Literal					= Recursive ( () => String | Float | Fixnum | Hash | Array | True | False | Nil | Symbol );

		//----------------------------------------------------------------//
		/**	@ruby	字符串Token
			@text	foobar
					ruby_is_simple
		*/
		public static Rule Letter					= MatchChar ( Char.IsLetter );
		public static Rule LetterOrDigit			= MatchChar ( Char.IsLetterOrDigit );
		public static Rule WS						= Pattern ( @"\s*" );

		public static Rule Comma					= CharToken ( ',' );
		public static Rule Eos						= CharToken ( ';' );
		public static Rule Eq						= CharToken ( '=' );
		public static Rule Dot						= CharToken ( '.' );

		//----------------------------------------------------------------//
		/**	@ruby	注释
			@text	# 这是单行注释
			@text	=begin
					这是多行注释
					=end
		*/
		public static Rule LineComment				= MatchString ( "#" ) + AdvanceWhileNot ( MatchChar ( '\n' ) );
		public static Rule FullComment				= MatchString ( "=begin" ) + AdvanceWhileNot ( MatchString ( "=end" ) );

		//----------------------------------------------------------------//
		/**	@ruby	数字
			@text	123
						integer
					-123
						integer(signed)
					1_234
						integer(underscore within decimal numbers ignored)
					123.45
						floating point number
					1.2e-3
						floating point number
					0xffff
						hexadecimal integer
					0b01011
						binary integer
					0377
						octal integer
					?a
						ASCII code for character `a'(97)
					?\C-a
						Control-a(1)
					?\M-a
						Meta-a(225)
					?\M-\C-a
						Meta-Control-a(129)
					:symbol
						Integer corresponding identifiers, variable names, and operators.
		*/
		public static Rule Digit					= MatchChar ( Char.IsDigit );
		public static Rule Digits					= OneOrMore ( Digit );
		public static Rule E						= ( MatchChar ( 'e' ) | MatchChar ( 'E' ) ) + Opt ( MatchChar ( '+' ) | MatchChar ( '-' ) );
		public static Rule Exp						= E + Digits;
		public static Rule Frac						= MatchChar ( '.' ) + Digits;
		public static Rule Fixnum					= Digits + Not ( MatchChar ( '.' ) );
		public static Rule Float					= Digits + ( ( Frac + Opt ( Exp ) ) | Exp );
		public static Rule HexDigit					= MatchString ( "0x" ) + Digit | CharRange ( 'a', 'f' ) | CharRange ( 'A', 'F' );
		public static Rule Number					= Float | Fixnum | HexDigit;


		//----------------------------------------------------------------//
		/**	@ruby	变量和常量
			@text	foobar
					ruby_is_simple
		*/
		public static Rule IdentFirstChar			= MatchChar ( c => Char.IsLetter ( c ) || c == '_' );
		public static Rule IdentNextChar			= MatchChar ( c => Char.IsLetterOrDigit ( c ) || c == '_' );
		public static Rule Identifier				= IdentFirstChar + ZeroOrMore ( IdentNextChar );
		public static Rule IdentifierList			= Node ( Opt ( CharToken ( '(' ) ) + CommaDelimited ( Identifier + WS ) + Opt ( CharToken ( ')' ) ) );
		public static Rule Name						= Node ( Identifier );
		public static Rule NameList					= Node ( Name + ZeroOrMore ( Comma + Name ) );
		public static Rule LocalVariable			= Node ( Identifier + WS );
		public static Rule GlobalVariable			= Node ( MatchChar ( '$' ) + Identifier + WS );
		public static Rule InstanceVariable			= Node ( MatchChar ( '@' ) + Identifier + WS );
		public static Rule Constants				= Node ( CharRange ( 'A', 'Z' ) + ZeroOrMore ( CharRange ( 'A', 'Z' ) ) + WS );
		public static Rule Variable					= LocalVariable | GlobalVariable | InstanceVariable | Constants;

		//----------------------------------------------------------------//
		/**	@ruby	基础类型Token
			@text	foobar
					ruby_is_simple
		*/
		public static Rule True						= Node ( MatchString ( "true" ) );
		public static Rule False					= Node ( MatchString ( "false" ) );
		public static Rule Nil						= Node ( MatchString ( "nil" ) );
		public static Rule UnicodeChar				= MatchString ( "\\u" ) + HexDigit + HexDigit + HexDigit + HexDigit;
		public static Rule ControlChar				= MatchChar ( '\\' ) + CharSet ( "\"\'\\/bfnt" );
		public static Rule ParamChar				= MatchString ( "#{" ) + Identifier + MatchChar ( '}' );
		public static Rule DoubleQuotedString		= Node ( MatchChar ( '"' ) + ZeroOrMore ( UnicodeChar | ControlChar | ExceptCharSet ( "\"\\" ) | ParamChar ) + MatchChar ( '"' ) );
		public static Rule SingleQuotedString		= Node ( MatchChar ( '\'' ) + ZeroOrMore ( UnicodeChar | ControlChar | ExceptCharSet ( "'\\" ) ) + MatchChar ( '\'' ) );
		public static Rule LineOrientedString		= Node ( MatchString ( "<<EOF" ) + ZeroOrMore ( UnicodeChar | ControlChar | ExceptCharSet ( "\"\\" ) | ParamChar ) + MatchString ( "EOF" ) );
		public static Rule String					= Node ( DoubleQuotedString | SingleQuotedString | LineOrientedString );
		public static Rule Symbol					= Node ( MatchChar ( ':' ) + Identifier );
		public static Rule Value					= Node ( Recursive ( () => String | Number | Hash | Array | True | False | Nil | Symbol ) );
		public static Rule Array					= Node ( CharToken ( '[' ) + CommaDelimited ( Value ) + WS + CharToken ( ']' ) );
		public static Rule PairName					= Identifier | DoubleQuotedString | SingleQuotedString;
		public static Rule Pair						= Node ( PairName + WS + ( StringToken ( "=>" ) | CharToken ( ':' ) | Eq ) + RecExpr + WS );
		public static Rule Hash						= Node ( CharToken ( '{' ) + CommaDelimited ( Pair ) + WS + CharToken ( '}' ) );

		//----------------------------------------------------------------//
		/**	@ruby	方法表达式
			@text	foobar
					ruby_is_simple
		*/
		public static Rule Ellipsis					= MatchChar ( '*' ) + Name;
		public static Rule Params					= Node ( OptParenthesize ( ( NameList + Opt ( Comma + Ellipsis ) ) | Ellipsis ) );
		public static Rule NamedFunc				= Node ( Keyword ( "def" ) + Identifier + WS + Params + RecStatement );
		public static Rule ClassFunc				= Node ( Keyword ( "def" ) + Identifier + ( MatchChar ( '.' ) | MatchString ( "::" ) )  + Params + RecStatement );
		public static Rule Function					= NamedFunc | ClassFunc;

		//----------------------------------------------------------------//
		/**	@ruby	表达式Token
			@text	foobar
					ruby_is_simple
		*/
		public static Rule FuncName					= Node ( Name + ZeroOrMore ( CharToken ( '.' ) + Name ) + Opt ( ( MatchChar ( '.' ) | MatchString ( "::" ) ) + Name ) );
		public static Rule ArgList					= Node ( Opt ( CharToken ( '(' ) ) + CommaDelimited ( RecExpr ) + Opt ( CharToken ( ')' ) ) );
		public static Rule Index					= Node ( CharToken ( '[' ) + RecExpr + CharToken ( ']' ) );
		public static Rule Field					= Node ( CharToken ( '.' ) + Identifier );
		public static Rule PrefixOp					= Node ( MatchStringSet ( "! - ~ not" ) );
		public static Rule ParenExpr				= Node ( CharToken ( '(' ) + RecExpr + WS + CharToken ( ')' ) );
		public static Rule LeafExpr					= ParenExpr | Literal | Variable;
		public static Rule PrefixExpr				= Node ( PrefixOp + Recursive ( () => PrefixOrLeafExpr ) );
		public static Rule PrefixOrLeafExpr			= PrefixExpr | LeafExpr;
		public static Rule PostfixOp				= Field | Index | ArgList;
		public static Rule PostfixExpr				= Node ( PrefixOrLeafExpr + WS + OneOrMore ( PostfixOp + WS ) );
		public static Rule UnaryExpr				= PostfixExpr | PrefixOrLeafExpr;
		public static Rule BinaryOp					= Node ( MatchStringSet ( "<= >= <=> == != === << >> && || and or < > & | ^ ~ + - * ** % / .. ..." ) );
		public static Rule BinaryExpr				= Node ( UnaryExpr + WS + BinaryOp + WS + RecExpr );
		public static Rule AssignOp					= Node ( MatchStringSet ( "&&= ||= >>= <<= += -= *= **= %= /= |= ^= =" ) );
		public static Rule AssignExpr				= Node ( ( Variable | PostfixExpr ) + WS + AssignOp + WS + RecExpr );
		public static Rule TertiaryExpr				= Node ( ( AssignExpr | BinaryExpr | UnaryExpr ) + WS + CharToken ( '?' ) + RecExpr + CharToken ( ':' ) + RecExpr + WS );
		public static Rule Expr						= Node ( ( TertiaryExpr | AssignExpr | BinaryExpr | UnaryExpr ) + WS );
		public static Rule ExprList					= Node ( Expr + ZeroOrMore ( Comma + Expr ) );

		public static Rule ParanExpr				= Node ( Parenthesize ( Expr ) );

		public static Rule Var						= Node ( Name | ( PrefixExpr + CharToken ( '[' ) + Exp + CharToken ( ']' ) ) | ( PrefixExpr + CharToken ( '.' ) + Name ) );
		public static Rule VarList					= Node ( Var + ZeroOrMore ( Comma + Var ) );
		public static Rule FunCall					= Node ( PrefixExpr + Opt ( CharToken ( '.' ) + Name ) );


		//----------------------------------------------------------------//
		/**	@ruby	Assignment
			@text	variable '=' expr
					constant '=' expr
					expr`['expr..`]' '=' expr
					expr`.'identifier '=' expr
		*/
		public static Rule AssignmentExpr			= ( LocalVariable | GlobalVariable | InstanceVariable | Constants | Expr ) + MatchChar ( '=' ) + Expr + Opt ( Eos );

		//----------------------------------------------------------------//
		/**	@ruby	Statement rules
		*/
		public static Rule Alias					= Node ( Keyword ( "alias" ) + ( Identifier | Symbol ) + WS + ( Identifier | Symbol ) + WS + Opt ( Eos ) );
		public static Rule Undef					= Node ( Keyword ( "undef" ) + ( Identifier | Symbol ) + WS + Opt ( Eos ) );
		public static Rule Raise					= Node ( Keyword ( "raise" ) + Expr + WS + Opt ( Eos ) );
		public static Rule Defined					= Node ( Keyword ( "defined?" ) + OptParenthesize ( Expr ) + WS + Opt ( Eos ) );
		public static Rule Next						= Node ( Keyword ( "next" ) + WS + Opt ( Eos ) );
		public static Rule Redo						= Node ( Keyword ( "redo" ) + WS + Opt ( Eos ) );
		public static Rule Break					= Node ( Keyword ( "break" ) + WS + Opt ( Eos ) );
		public static Rule Return					= Node ( Keyword ( "return" ) + Opt ( Expr ) + WS + Opt ( Eos ) );
		public static Rule LastStat					= Node ( ( StringToken ( "return" ) + Opt ( ExprList ) ) | Break | Next | Redo | ExprList );
		public static Rule Class					= Node ( Keyword ( "class" ) + ( ( Identifier + Opt ( Keyword ( "<" ) + Identifier ) ) | ( Keyword ( "<<" ) + Expr ) )  + RecExpr + WS + Keyword ( "end" ) );
		public static Rule Module					= Node ( Keyword ( "module" ) + Identifier + RecExpr + WS + Keyword ( "end" ) );
		//public static Rule Range					= Node ( Expr + ( StringToken ( ".." ) | StringToken ( "..." ) )  + Expr );
		public static Rule VarDecl					= Node ( Keyword ( "var" ) + Identifier + WS + Opt ( Eq + Expr ) + Opt ( Eos ) );
		public static Rule While					= Node ( Keyword ( "while" ) + OptParenthesize ( Expr ) + Opt ( Keyword ( "do" ) ) + RecStatement + WS + Keyword ( "end" ) );
		public static Rule Until					= Node ( Keyword ( "until" ) + OptParenthesize ( Expr ) + Opt ( Keyword ( "do" ) ) + RecStatement + WS + Keyword ( "end" ) );
		public static Rule For						= Node ( Keyword ( "for" ) + Identifier + WS + Keyword ( "in" ) + Expr + WS + Opt ( Keyword ( "do" ) ) + RecStatement + WS + Keyword ( "end" ) );
		public static Rule Else						= Node ( Keyword ( "else" ) + RecStatement );
		public static Rule ElseIf					= Node ( Keyword ( "elsif" ) + OptParenthesize ( Expr ) + Opt ( Keyword ( "then" ) ) );
		public static Rule If						= Node ( Keyword ( "if" ) + OptParenthesize ( Expr ) + Opt ( Keyword ( "then" ) ) + RecStatement + Opt ( Else | ElseIf ) + WS + Keyword ( "end" ) );
		public static Rule Case						= Node ( Keyword ( "case" ) + Expr + OneOrMore ( Keyword ( "when" ) + RecStatement + Opt ( Keyword ( "then" ) ) )  + Opt ( Else ) + WS + Keyword ( "end" ) );
		public static Rule Unless					= Node ( Keyword ( "unless" ) + OptParenthesize ( Expr ) + Opt ( Keyword ( "then" ) ) + RecStatement + Opt ( Else ) + WS + Keyword ( "end" ) );
		public static Rule ExprStatement			= Node ( Expr + WS + Opt ( Eos ) );
		public static Rule Empty					= Node ( WS + Opt ( Eos ) );
		public static Rule Statement				= Case | For | While | Until | Unless | If | Return | Raise | Next | Redo | Break | VarDecl | ExprStatement | Empty;

		public static Rule Block					= Node ( ZeroOrMore ( Statement + Opt ( Eos ) ) + Opt ( LastStat + Opt ( Eos ) ) );
		public static Rule FuncBody					= Node ( OptParenthesize ( Opt ( Params ) ) + Block + WS + Keyword ( "end" ) );
		public static Rule FunDefStatement			= Node ( Keyword ( "def" ) + FuncName + WS + FuncBody );
		public static Rule FunExpr					= Node ( Keyword ( "def" ) + FuncBody );

		public static Rule CharToken (char c) { return MatchChar ( c ) + WS; }
		public static Rule StringToken (string s) { return MatchString ( s ) + WS; }
		public static Rule CommaDelimited (Rule r) { return Opt ( r + ( ZeroOrMore ( CharToken ( ',' ) + r ) + Opt ( CharToken ( ',' ) ) ) ); }

		public static Rule MatchAnyString (params string[] xs) { return Choice ( xs.Select ( x => MatchString ( x ) ).ToArray () ); }
		public static Rule MatchStringSet (string s) { return MatchAnyString ( s.Split ( ' ' ) ); }
		public static Rule Keyword (string s) { return MatchString ( s ) + Not ( LetterOrDigit ) + WS; }
		public static Rule Parenthesize (Rule r) { return CharToken ( '(' ) + r + WS + CharToken ( ')' ); }
		public static Rule OptParenthesize (Rule r) { return Opt ( CharToken ( '(' ) )  + r + WS + Opt ( CharToken ( ')' ) ); }


		//----------------------------------------------------------------//
		/**	@ruby	The top-level rule
		*/
		public static Rule Script					= Node ( ZeroOrMore ( Statement ) + WS + End );


		// Grammar initiatlization
		static RubyGrammar () {
			InitGrammar ( typeof ( RubyGrammar ) );
		}
	}
}

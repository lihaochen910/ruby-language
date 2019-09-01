using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peg;

namespace Ruby {

	/// <summary>
	/// Contains grammar rules. Any circular references require the usage of a "Delay" function which prevents 
	/// the parser construction to lead to a stack overflow.
	/// </summary>
	public class RubyGrammar : Grammar {

		public static Rule RubyAstNode ( ASTNodeType nodeType, Rule x ) {
			return new AstNodeRule ( nodeType, x );
		}
		
		#region Comment
		public static Rule LineComment () {
			return Seq ( CharSeq ( "#" ), UntilEndOfLine () );
		}
		public static Rule BlockComment () {
			return Seq ( CharSeq ( "=begin" ), NoFail ( WhileNot ( AnyChar (), CharSeq ( "=end" ) ), "expected a new line" ) );
		}
		public static Rule Comment () {
			return Choice ( BlockComment (), LineComment () );
		}
		#endregion

		#region 数字
		public static Rule FloatLiteral () {
			return RubyAstNode ( ASTNodeType.FLOAT, Seq ( Opt ( SingleChar ( '-' ) ), Plus ( Digit () ), SingleChar ( '.' ), Plus ( Digit () ) ) );
		}
		public static Rule FixnumLiteral () {
			return RubyAstNode ( ASTNodeType.INT, Seq ( Opt ( SingleChar ( '-' ) ), Plus ( Digit () ), Not ( CharSet ( "." ) ) ) );
		}
		public static Rule BinaryLiteral () {
			return RubyAstNode ( ASTNodeType.INT, Seq ( CharSeq ( "0b" ), Plus ( CharRange ( '0', '1' ) ), Not ( CharSet ( "." ) ) ) );
		}
		public static Rule HexLiteral () {
			return RubyAstNode ( ASTNodeType.INT, Seq ( CharSeq ( "0x" ), Plus ( Choice ( Digit (), CharRange ( 'a', 'f' ), CharRange ( 'A', 'F' ) ) ), Not ( CharSet ( "." ) ) ) );
		}
		public static Rule NumberLiteral () {
			return Choice ( FixnumLiteral (), FloatLiteral (), BinaryLiteral (), HexLiteral () );
		}
		#endregion

		#region 变量常量
		public static Rule Identifier () {
			return Seq ( IdentFirstChar (), Star ( IdentNextChar () ) );
		}
		public static Rule LocalVariableReference () {
			return RubyAstNode ( ASTNodeType.LVAR, Seq ( IdentFirstChar (), Star ( IdentNextChar () ) ) );
		}
		public static Rule InstanceVariableReference () {
			return RubyAstNode ( ASTNodeType.IVAR, Seq ( SingleChar ( '@' ), IdentFirstChar (), Star ( IdentNextChar () ) ) );
		}
		public static Rule ConstantReference () {
			return RubyAstNode ( ASTNodeType.CONST, Plus ( CharRange ( 'A', 'Z' ) ) );
		}
		public static Rule ClassVariableReference () {
			return RubyAstNode ( ASTNodeType.CVAR, Seq ( CharSeq ( "@@" ), IdentFirstChar (), Star ( IdentNextChar () ) ) );
		}
		public static Rule GlobalVariableReference () {
			return RubyAstNode ( ASTNodeType.GVAR, Seq ( SingleChar ( '$' ), IdentFirstChar (), Star ( IdentNextChar () ) ) );
		}
		public static Rule Variable () {
			return Choice ( LocalVariableReference (), InstanceVariableReference (), ConstantReference (), ClassVariableReference (), GlobalVariableReference () );
		}
		#endregion

		#region 基础类型
		public static Rule TrueLiteral () {
			return RubyAstNode ( ASTNodeType.TRUE, Word ( "true" ) );
		}
		public static Rule FalseLiteral () {
			return RubyAstNode ( ASTNodeType.FALSE, Word ( "false" ) );
		}
		public static Rule NilLiteral () {
			return RubyAstNode ( ASTNodeType.NIL, Word ( "nil" ) );
		}
		public static Rule SelfLiteral () {
			return RubyAstNode ( ASTNodeType.SELF, Word ( "self" ) );
		}
		public static Rule SymbolLiteral () {
			return RubyAstNode ( ASTNodeType.SYM, Seq ( SingleChar ( ':' ), IdentFirstChar (), Star ( IdentNextChar () ) ) );
		}
		public static Rule UnicodeChar () { 
			return Seq ( CharSeq ( "\\u" ), HexDigit (), HexDigit (), HexDigit (), HexDigit () );
		}
		public static Rule ControlChar () { 
			return SingleChar ( '\\' ) + CharSet ( "\"\'\\/befnrst" );
		}
		//public static Rule ParamChar = Node ( StringToken ( "#{" ) + RecExpr + CharToken ( '}' ) );
		public static Rule DoubleQuotedString () {
			return RubyAstNode ( ASTNodeType.STR, Seq ( SingleChar ( '\"' ), Star ( Choice ( UnicodeChar (), ControlChar (), Not ( CharSet ( "\"\\" ) ) )  /*| ParamChar*/ ), SingleChar ( '\"' ) ) );
		}
		public static Rule SingleQuotedString () {
			return RubyAstNode ( ASTNodeType.STR, Seq ( SingleChar ( '\'' ) + Star ( Choice ( UnicodeChar (), ControlChar (), Not ( CharSet ( "\"\\" ) ) )  /*| ParamChar*/ ), SingleChar ( '\'' ) ) );
		}
		//public static Rule LineOrientedString = Node ( MatchString ( "<<EOF" ) + ZeroOrMore ( UnicodeChar | ControlChar | ParamChar ) + WS + MatchString ( "EOF" ) );
		public static Rule StringLiteral () {
			return RubyAstNode ( ASTNodeType.STR, Choice ( DoubleQuotedString (), SingleQuotedString () ) );
		}
		public static Rule ArrayLiteral () {
			return RubyAstNode ( ASTNodeType.ARRAY, Seq ( Token ( "[" ), Star ( Delay ( Expr ) ), NoFail ( Token ( "]" ), "missing ']'" ) ) );
		}
		public static Rule PairName () {
			return Choice ( Identifier (), SymbolLiteral (), StringLiteral () );
		}
		public static Rule Pair () {
			return PairName () + WS () + ( Choice ( Token ( "=>" ), Token ( ":" ), Token ( "=" ) ) ) + Delay ( Expr ) + WS ();
		}
		public static Rule HashLiteral () {
			return RubyAstNode ( ASTNodeType.HASH, Token ( "{" ) + CommaDelimited ( Pair () ) + WS () + Token ( "}" ) );
		}
		public static Rule LambdaLiteral () {
			return RubyAstNode ( ASTNodeType.LAMBDA, Seq ( Token ( "->" ), Token ( "{" ), Opt ( AnonymousArgs () ), Expr (), WS (), Token ( "}" ) ) );
		}
		public static Rule ProcLiteral_1 () {
			return RubyAstNode ( ASTNodeType.LAMBDA, Seq ( Token ( "{" ), Opt ( AnonymousArgs () ), Star ( Delay ( Expr ) ), WS (), Token ( "}" ) ) );
		}
		public static Rule ProcLiteral_2 () {
			return RubyAstNode ( ASTNodeType.LAMBDA, Seq ( Word ( "do" ), Opt ( AnonymousArgs () ), Star ( Delay ( Expr ) ), WS (), Word ( "end" ) ) );
		}
		#endregion

		#region 表达式
		public static Rule Literal () {
			return Choice ( NumberLiteral (), StringLiteral (), SymbolLiteral (), TrueLiteral (), FalseLiteral (), NilLiteral (), ArrayLiteral (), HashLiteral () );
		}
		public static Rule DefaultParam () {
			return Seq ( Identifier (), Token ( "=" ), Delay ( Expr ) );
		}
		public static Rule Args () {
			return Seq ( Token ( "(" ), CommaDelimited ( Delay ( Expr ) ), Token ( ")" ) );
		}
		public static Rule ArgsWithBlockArg () {
			return Seq ( Token ( "(" ), CommaDelimited ( Delay ( Expr ) ), Opt ( Choice ( Seq ( SingleChar ( '&' ), Identifier () ), Seq ( SingleChar ( '*' ), Identifier () ) ) ), Token ( ")" ) );
		}
		public static Rule AnonymousArgs () {
			return Seq ( Token ( "|" ), CommaDelimited ( Choice ( Identifier (), DefaultParam () ) ), Token ( "|" ) );
		}
		public static Rule Index () {
			return Seq ( Token ( "[" ), Delay ( Expr ), Token ( "]" ) );
		}
		public static Rule Field () {
			return Seq ( Token ( "." ), Identifier () );
		}
		public static Rule ParenExpr () {
			return Seq ( Token ( "(" ), Delay ( Expr ), WS (), Token ( ")" ) );
		}
		public static Rule LeafExpr () {
			return Choice ( Literal (), Variable (), ParenExpr (), CallExpr (), OpCallExpr (), FunctionCallExpr () );
		}
		public static Rule PrefixOp () {
			return CharSet ( "! not - ~" );
		}
		public static Rule PrefixExpr () {
			return Seq ( PrefixOp (), Delay ( PrefixOrLeafExpr ) );
		}
		public static Rule PrefixOrLeafExpr () {
			return Choice ( PrefixExpr (), LeafExpr () );
		}
		public static Rule PostfixOp () {
			return Choice ( Field (), Index (), Args (), ArgsWithBlockArg () );
		}
		public static Rule PostfixExpr () {
			return Seq ( PrefixOrLeafExpr (), WS (), Star ( Seq ( PostfixOp (), WS () ) ) );
		}
		public static Rule UnaryExpr () {
			return Choice ( PostfixExpr (), PrefixOrLeafExpr () );
		}
		public static Rule BinaryOp () {
			return CharSet ( "<= >= <=> == != === << >> && || and or < > & | ^ ~ + - * ** % / .. ..." );
		}
		public static Rule BinaryExpr () {
			return Seq ( UnaryExpr (), WS (), BinaryOp (), WS (), Star ( Delay ( Expr ) ) );
		}
		public static Rule AssignOp () {
			return CharSet ( "&&= ||= >>= <<= += -= *= **= %= /= |= ^= =" );
		}
		public static Rule AssignExpr () {
			return Seq ( Choice ( Variable (), PostfixExpr () ), WS (), AssignOp (), WS (), Star ( Delay ( Expr ) ) );
		}
		public static Rule TertiaryExpr () {
			return Seq ( Choice ( AssignExpr (), BinaryExpr (), UnaryExpr () ), WS (), Token ( "?" ), Delay ( Expr ), Token ( ":" ), Delay ( Expr ), WS () );
		}
		public static Rule CallExpr () {
			return RubyAstNode ( ASTNodeType.CALL, Delay ( Expr ) + SingleChar ( '.' ) + Opt ( Token ( "(" ) + CommaDelimited ( Delay ( Expr ) ) + Token ( ")" ) ) );
		}
		public static Rule OpCallExpr () {
			return RubyAstNode ( ASTNodeType.SCALL, Delay ( Expr ) + Opt ( SingleChar ( '.' ) ) + StringSet ( "! % + - * / = | < > []" ) + Opt ( Token ( "(" ) + CommaDelimited ( Delay ( Expr ) ) + Token ( ")" ) ) );
		}
		public static Rule FunctionCallExpr () {
			return RubyAstNode ( ASTNodeType.FCALL, Identifier () + Opt ( Token ( "(" ) + CommaDelimited ( Delay ( Expr ) ) + Token ( ")" ) ) );
		}
		public static Rule Expr () {
			return Token ( Choice ( AssignExpr (), UnaryExpr (), BinaryExpr (), TertiaryExpr (), LambdaLiteral (), ProcLiteral_1 (), ProcLiteral_2 () ) );
		}
		public static Rule ExprList () {
			return Seq ( Expr (), Star ( Seq ( Token ( "," ), Expr () ) ) );
		}
		#endregion

		#region Statement
		public static Rule Else () {
			return Seq ( Word ( "else" ), Star ( Delay ( Statement ) ) );
		}
		public static Rule ElseIf () {
			return Seq ( Word ( "elsif" ), OptParenthesize ( Expr () ), Opt ( Word ( "then" ) ) );
		}
		public static Rule If () {
			return RubyAstNode ( ASTNodeType.IF, Seq ( Word ( "if" ), OptParenthesize ( Expr () ), Opt ( Word ( "then" ) ), Star ( Delay ( Statement ) ), Opt ( Choice ( Else (), ElseIf () ) ), WS (), Word ( "end" ) ) );
		}
		public static Rule IfModifier () {
			return RubyAstNode ( ASTNodeType.IF, Seq ( LastStatement (), WS (), Word ( "if" ), Expr (), UntilEndOfLine () ) );
		}
		public static Rule For () {
			return RubyAstNode ( ASTNodeType.FOR, Seq ( Word ( "for" ), LocalVariableReference (), WS (), Word ( "in" ), Expr (), WS () + Opt ( Word ( "do" ) ), Star ( Delay ( Statement ) ), WS () + Word ( "end" ) ) );
		}
		public static Rule While () {
			return RubyAstNode ( ASTNodeType.WHILE, Seq ( Word ( "while" ), OptParenthesize ( Expr () ) + Opt ( Word ( "do" ) ), Star ( Delay ( Statement ) ), WS (), Word ( "end" ) ) );
		}
		public static Rule Until () {
			return RubyAstNode ( ASTNodeType.UNTIL, Seq ( Word ( "until" ), OptParenthesize ( Expr () ) + Opt ( Word ( "do" ) ), Star ( Delay ( Statement ) ), WS (), Word ( "end" ) ) );
		}
		public static Rule Unless () {
			return RubyAstNode ( ASTNodeType.IF, Seq ( Word ( "unless" ), OptParenthesize ( Expr () ), Opt ( Word ( "then" ) ), Star ( Delay ( Statement ) ), Opt ( Else () ), WS (), Word ( "end" ) ) );
		}
		public static Rule UnlessModifier () {
			return RubyAstNode ( ASTNodeType.IF, Seq ( LastStatement (), WS (), Word ( "unless" ), Expr (), UntilEndOfLine () ) );
		}
		public static Rule Case () {
			return RubyAstNode ( ASTNodeType.CASE, Seq ( 
				Word ( "case" ), Expr (), 
				Plus ( Seq ( Word ( "when" ), Plus ( Expr () ), Opt ( Word ( "then" ) ),
						Star ( Delay ( Statement ) ) ) ),
				Opt ( Else () ), WS (), 
					Star ( Delay ( Statement ) ), WS (),
				Word ( "end" ) ) );
		}
		public static Rule Return () {
			return RubyAstNode ( ASTNodeType.RETURN, Seq ( Word ( "return" ), Opt ( ExprList () ) ) );
		}
		public static Rule Break () {
			return RubyAstNode ( ASTNodeType.BREAK, Word ( "break" ) );
		}
		public static Rule Next () {
			return RubyAstNode ( ASTNodeType.NEXT, Word ( "next" ) );
		}
		public static Rule Redo () {
			return RubyAstNode ( ASTNodeType.REDO, Word ( "redo" ) );
		}
		public static Rule LastStatement () {
			return Choice ( Return (), Break (), Next (), Redo (), ExprList () );
		}
		public static Rule LastStatementUntilEndOfLine () {
			return Seq ( Choice ( Return (), Break (), Next (), Redo (), ExprList () ), UntilEndOfLine () );
		}
		public static Rule ExprStatement () {
			return Seq ( Expr (), WS (), Opt ( UntilEndOfLine () ) );
		}
		public static Rule ClassDefinitions () {
			return RubyAstNode ( ASTNodeType.CLASS, Seq (
				Word ( "class" ), Identifier (), Opt ( Word ( "<" ) + Identifier () ) | ( Word ( "<<" ) + Delay ( Expr ) ),
					Star ( Choice ( Delay ( Expr ), Delay ( ClassDefinitions ), Delay ( ModuleDefinitions ) ) ), WS (),
				Word ( "end" ) ) );
		}
		public static Rule ModuleDefinitions () {
			return RubyAstNode ( ASTNodeType.CLASS, Seq (
				Word ( "module" ), Identifier (),
					Star ( Choice ( Delay ( Expr ), Delay ( ClassDefinitions ), Delay ( ModuleDefinitions ) ) ), WS (),
				Word ( "end" ) ) );
		}
		public static Rule FunctionDefinitionsArgsWithBlockArg () {
			return Seq ( Token ( "(" ), CommaDelimited ( Choice ( Identifier (), DefaultParam () ) ), Opt ( Choice ( Seq ( SingleChar ( '&' ), Identifier () ), Seq ( SingleChar ( '*' ), Identifier () ) ) ), Token ( ")" ) );
		}
		public static Rule FunctionDefinitions () {
			return RubyAstNode ( ASTNodeType.METHOD, Seq (
				Word ( "def" ), Identifier (), Opt ( FunctionDefinitionsArgsWithBlockArg () ),
					Star ( Choice ( Delay ( Expr ), Delay ( Statement ) ) ), WS (),
				Word ( "end" ) ) );
		}
		public static Rule ClassFunctionDefinitions () {
			return RubyAstNode ( ASTNodeType.METHOD, Seq (
				Word ( "def" ), Identifier (), Choice ( SingleChar ( '.' ), CharSeq ( "::" ) ), Identifier (), Opt ( FunctionDefinitionsArgsWithBlockArg () ),
					Star ( Choice ( Delay ( Expr ), Delay ( Statement ) ) ), WS (),
				Word ( "end" ) ) );
		}
		public static Rule EmptyStatement () {
			return Seq ( WS (), UntilEndOfLine () );
		}
		public static Rule Statement () {
			return Choice ( If (), IfModifier (), For (), While (), Until (), Unless (), UnlessModifier (), Case (), Return (), Next (), Redo (), Break (), ExprStatement (), ClassDefinitions (), ModuleDefinitions (), FunctionDefinitions (), ClassFunctionDefinitions (), LastStatementUntilEndOfLine (), EmptyStatement () );
		}
		#endregion

		public static Rule UntilEndOfLine () {
			return NoFail ( WhileNot ( AnyChar (), NL () ), "expected a new line" );
		}
		public static Rule WS () {
			return Star ( Choice ( CharSet ( " \t\n\r" ), Comment () ) );
		}
		public static Rule Token (string s) {
			return Token ( CharSeq ( s ) );
		}
		public static Rule Token (Rule r) {
			return Seq ( r, WS () );
		}
		public static Rule CommaDelimited ( Rule r ) {
			return Opt ( r + ( Star ( Token ( "," ) + r ) + Opt ( Token ( "," ) ) ) );
		}
		public static Rule Word (string s) {
			return Seq ( CharSeq ( s ), EOW (), WS () );
		}
		public static Rule EscapeChar () {
			return Seq ( SingleChar ( '\\' ), AnyChar () );
		}
		public static Rule StringCharLiteral () {
			return Choice ( EscapeChar (), NotChar ( '"' ) );
		}
		public static Rule CodeBlock () {
			return Seq ( Opt ( Token ( "{" ) ), Star ( Choice ( Statement (), Expr () ) ), Opt ( Token ( "}" ) ) );
		}
		public static Rule Param () {
			return Token ( RubyAstNode ( ASTNodeType.ARG, Identifier () ) );
		}
		public static Rule Params () {
			return Seq ( Token ( "(" ), Star ( Param () ), NoFail ( Token ( ")" ), "missing ')'" ) );
		}
		public static Rule Parenthesize ( Rule r ) {
			return Seq ( Token ( "(" ), r, WS (), Token ( ")" ) );
		}
		public static Rule OptParenthesize ( Rule r ) {
			return Seq ( Opt ( Token ( "(" ) ), r, WS (), Opt ( Token ( ")" ) ) );
		}

		public static Rule RubyScript () {
			return Seq ( WS (), Star ( Choice ( Expr (), Statement () ) ), WS (), NoFail ( EndOfInput (), "expected macro or function defintion" ) );
		}
	}
}

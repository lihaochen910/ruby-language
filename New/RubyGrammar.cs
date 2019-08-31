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
	public class RubyGrammar : Peg.Grammar {

		public static Rule RubyAstNode (ASTNodeType nodeType, Rule x) {
			return new AstNodeRule ( nodeType, x );
		}
		public static Rule UntilEndOfLine () {
			return NoFail ( WhileNot ( AnyChar (), NL () ), "expected a new line" );
		}
		public static Rule LineComment () {
			return Seq ( CharSeq ( "//" ), UntilEndOfLine () );
		}
		public static Rule BlockComment () {
			return Seq ( CharSeq ( "/*" ), NoFail ( WhileNot ( AnyChar (), CharSeq ( "*/" ) ), "expected a new line" ) );
		}
		public static Rule MetaDataContent () {
			return RubyAstNode ( ASTNodeType.MetaDataContent, UntilEndOfLine () );
		}
		public static Rule MetaDataLabel () {
			return RubyAstNode ( ASTNodeType.MetaDataLabel, Seq ( Star ( CharSet ( " \t" ) ), Ident (), SingleChar ( ':' ) ) );
		}
		public static Rule MetaDataEntry () {
			return Seq ( Opt ( MetaDataLabel () ), Star ( CharSet ( " \t" ) ), MetaDataContent () );
		}
		public static Rule StartMetaDataBlock () {
			return Seq ( WS (), CharSeq ( "{{" ), UntilEndOfLine () );
		}
		public static Rule EndMetaDataBlock () {
			return Seq ( WS (), CharSeq ( "}}" ), UntilEndOfLine () );
		}
		public static Rule MetaDataBlock () {
			return Seq ( RubyAstNode ( ASTNodeType.MetaDataBlock, Seq ( StartMetaDataBlock (), WhileNot ( MetaDataEntry (), EndMetaDataBlock () ) ) ), WS () );
		}
		public static Rule Comment () {
			return Choice ( BlockComment (), LineComment () );
		}
		public static Rule WS () {
			return Star ( Choice ( CharSet ( " \t\n\r" ), Comment () ) );
		}
		public static Rule CatIdentChar () {
			return Choice ( IdentNextChar (), CharSet ( "~`!@#$%^&*-+=|:;<>.?/" ) );
		}
		public static Rule CatIdent () {
			return Plus ( CatIdentChar () ); ;
		}
		public static Rule Token (string s) {
			return Token ( CharSeq ( s ) );
		}
		public static Rule Token (Rule r) {
			return Seq ( r, WS () );
		}
		public static Rule Word (string s) {
			return Seq ( CharSeq ( s ), EOW (), WS () );
		}
		public static Rule Quote () {
			// Note the usage of Delay which breaks circular references in the grammar
			return RubyAstNode ( ASTNodeType.Quote, Seq ( Token ( "[" ), Star ( Delay ( Expr ) ), NoFail ( Token ( "]" ), "missing ']'" ) ) );
		}
		public static Rule IntegerLiteral () {
			return RubyAstNode ( ASTNodeType.Int, Seq ( Opt ( SingleChar ( '-' ) ), Plus ( Digit () ), Not ( CharSet ( "." ) ) ) );
		}
		public static Rule EscapeChar () {
			return Seq ( SingleChar ( '\\' ), AnyChar () );
		}
		public static Rule StringCharLiteral () {
			return Choice ( EscapeChar (), NotChar ( '"' ) );
		}
		public static Rule CharLiteral () {
			return RubyAstNode ( ASTNodeType.Char, Seq ( SingleChar ( '\'' ), StringCharLiteral (), SingleChar ( '\'' ) ) );
		}
		public static Rule StringLiteral () {
			return RubyAstNode ( ASTNodeType.String, Seq ( SingleChar ( '\"' ), Star ( StringCharLiteral () ), SingleChar ( '\"' ) ) );
		}
		public static Rule FloatLiteral () {
			return RubyAstNode ( ASTNodeType.Float, Seq ( Opt ( SingleChar ( '-' ) ), Plus ( Digit () ), SingleChar ( '.' ), Plus ( Digit () ) ) );
		}
		public static Rule HexValue () {
			return RubyAstNode ( ASTNodeType.Hex, Plus ( HexDigit () ) );
		}
		public static Rule HexLiteral () {
			return Seq ( CharSeq ( "0x" ), NoFail ( HexValue (), "expected at least one hexadecimal digit" ) );
		}
		public static Rule BinaryValue () {
			return RubyAstNode ( ASTNodeType.Bin, Plus ( BinaryDigit () ) );
		}
		public static Rule BinaryLiteral () {
			return Seq ( CharSeq ( "0b" ), NoFail ( BinaryValue (), "expected at least one binary digit" ) );
		}
		public static Rule NumLiteral () {
			return Choice ( HexLiteral (), BinaryLiteral (), FloatLiteral (), IntegerLiteral () );
		}
		public static Rule Literal () {
			return Choice ( StringLiteral (), CharLiteral (), NumLiteral () );
		}
		public static Rule Symbol () {
			// The "()" together is treated as a single symbol
			return Choice ( CharSeq ( "()" ), CharSet ( "()," ) );
		}
		public static Rule Name () {
			return Token ( RubyAstNode ( ASTNodeType.Name, Choice ( Symbol (), CatIdent () ) ) );
		}
		public static Rule Lambda () {
			return RubyAstNode ( ASTNodeType.Lambda, Seq ( CharSeq ( "\\" ), NoFail ( Seq ( Param (), CharSeq ( "." ), Choice ( Delay ( Lambda ),
				NoFail ( Quote (), "expected a quotation or lambda expression" ) ) ), "expected a lambda expression" ) ) );
		}
		public static Rule Expr () {
			return Token ( Choice ( Lambda (), Literal (), Quote (), Name () ) );
		}
		public static Rule Statement () {
			return Delay ( FxnDef );
		}
		public static Rule CodeBlock () {
			return Seq ( Token ( "{" ), Star ( Choice ( Statement (), Expr () ) ), NoFail ( Token ( "}" ), "missing '}'" ) );
		}
		public static Rule Param () {
			return Token ( RubyAstNode ( ASTNodeType.Param, Ident () ) );
		}
		public static Rule Params () {
			return Seq ( Token ( "(" ), Star ( Param () ), NoFail ( Token ( ")" ), "missing ')'" ) );
		}
		public static Rule TypeVar () {
			return RubyAstNode ( ASTNodeType.TypeVar, Seq ( Opt ( CharSeq ( "$" ) ), LowerCaseLetter (), Star ( IdentNextChar () ) ) );
		}
		public static Rule StackVar () {
			return RubyAstNode ( ASTNodeType.StackVar, Seq ( Opt ( CharSeq ( "$" ) ), UpperCaseLetter (), Star ( IdentNextChar () ) ) );
		}
		public static Rule TypeOrStackVar () {
			return Seq ( SingleChar ( '\'' ), NoFail ( Choice ( TypeVar (), StackVar () ), "invalid type or stack variable name" ), WS () );
		}
		public static Rule TypeName () {
			return Token ( RubyAstNode ( ASTNodeType.TypeName, Ident () ) );
		}
		public static Rule TypeComponent () {
			return Choice ( TypeName (), TypeOrStackVar (), Delay ( FxnType ) );
		}
		public static Rule Production () {
			return RubyAstNode ( ASTNodeType.Stack, Token ( Star ( TypeComponent () ) ) );
		}
		public static Rule Consumption () {
			return RubyAstNode ( ASTNodeType.Stack, Token ( Star ( TypeComponent () ) ) );
		}
		public static Rule Arrow () {
			return RubyAstNode ( ASTNodeType.Arrow, Choice ( Token ( "->" ), Token ( "~>" ) ) );
		}
		public static Rule FxnType () {
			return RubyAstNode ( ASTNodeType.FxnType, Seq ( Token ( "(" ), Production (), NoFail ( Arrow (), "expected either -> or ~>" ), Consumption (), NoFail ( Token ( ")" ), "expected closing paranthesis" ) ) );
		}
		public static Rule TypeDecl () {
			return Seq ( Token ( ":" ), NoFail ( FxnType (), "expected function type declaration" ), WS () );
		}
		public static Rule FxnDef () {
			return RubyAstNode ( ASTNodeType.Def, Seq ( Word ( "define" ), NoFail ( Name (), "expected name" ),
				Opt ( Params () ), Opt ( TypeDecl () ), Opt ( MetaDataBlock () ), NoFail ( CodeBlock (), "expected a code block" ) ) );
		}
		public static Rule FxnDecl () {
			return RubyAstNode ( ASTNodeType.Decl, Seq ( Word ( "declare" ), NoFail ( Name (), "expected name" ),
				Opt ( Seq ( TypeDecl (), Opt ( MetaDataBlock () ) ) ) ) );
		}
		#region macros
		public static Rule MacroTypeVarName () {
			return RubyAstNode ( ASTNodeType.MacroTypeVarName, Seq ( LowerCaseLetter (), Star ( IdentNextChar () ) ) );
		}
		public static Rule MacroTypeVar () {
			return RubyAstNode ( ASTNodeType.MacroTypeVar, MacroTypeVarName () );
		}
		public static Rule MacroStackVarName () {
			return RubyAstNode ( ASTNodeType.MacroStackVarName, Seq ( UpperCaseLetter (), Star ( IdentNextChar () ) ) );
		}
		public static Rule MacroStackVar () {
			return RubyAstNode ( ASTNodeType.MacroStackVar, Seq ( MacroStackVarName (), WS (), Opt ( TypeDecl () ) ) );
		}
		public static Rule MacroVar () {
			return Seq ( SingleChar ( '$' ), NoFail ( Choice ( MacroTypeVar (), MacroStackVar () ), "expected a valid macro type variable or stack variable" ) );
		}
		public static Rule MacroName () {
			return RubyAstNode ( ASTNodeType.MacroName, Choice ( Symbol (), CatIdent () ) );
		}
		public static Rule MacroTerm () {
			return Token ( Choice ( MacroQuote (), MacroVar (), MacroName () ) );
		}
		public static Rule MacroQuote () {
			return RubyAstNode ( ASTNodeType.MacroQuote, Seq ( Token ( "[" ), Star ( Delay ( MacroTerm ) ), NoFail ( Token ( "]" ), "missing ']'" ) ) );
		}
		public static Rule MacroPattern () {
			return RubyAstNode ( ASTNodeType.MacroPattern, Seq ( Token ( "{" ), Star ( MacroTerm () ), NoFail ( Token ( "}" ), "missing '}'" ) ) );
		}
		public static Rule MacroDef () {
			return RubyAstNode ( ASTNodeType.MacroRule, Seq ( Word ( "rule" ), Seq ( MacroPattern (), Token ( "=>" ), MacroPattern () ) ) );
		}
		public static Rule MacroProp () {
			return RubyAstNode ( ASTNodeType.MacroProp, Seq ( Word ( "rule" ), Seq ( MacroPattern (), Token ( "==" ), MacroPattern () ) ) );
		}
		#endregion
		public static Rule RubyScript () {
			return Seq ( WS (), Star ( Choice ( MetaDataBlock (), FxnDef (), MacroDef (), Expr () ) ), WS (), NoFail ( EndOfInput (), "expected macro or function defintion" ) );
		}
	}
}

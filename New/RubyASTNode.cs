using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peg;

namespace Ruby {

	/// <summary>
	/// An AstNode is used as a base class for a typed abstract syntax tree for Cat programs.
	/// CatAstNodes are created from a Peg.Ast. Apart from being typed, the big difference
	/// is the a CatAstNode can be modified. This makes rewriting algorithms much easier. 
	/// </summary>
	public abstract class RubyAstNode {

		string msText;
		ASTNodeType mLabel;

		public RubyAstNode (PegAstNode node) {
			if ( node.GetLabel () != null )
				mLabel = (ASTNodeType)node.GetLabel ();
			else
				mLabel = ASTNodeType.AstRoot;

			msText = node.ToString ();
		}

		public RubyAstNode (ASTNodeType label, string sText) {
			mLabel = label;
			msText = sText;
		}

		public static RubyAstNode Create (PegAstNode node) {
			ASTNodeType label = (ASTNodeType)node.GetLabel ();
			switch ( label ) {
				case ASTNodeType.AstRoot:
					return new AstRoot ( node );
				case ASTNodeType.Def:
					return new AstDef ( node );
				case ASTNodeType.Name:
					return new AstName ( node );
				case ASTNodeType.Param:
					return new AstParam ( node );
				case ASTNodeType.Lambda:
					return new AstLambda ( node );
				case ASTNodeType.Quote:
					return new AstQuote ( node );
				case ASTNodeType.Char:
					return new AstChar ( node );
				case ASTNodeType.String:
					return new AstString ( node );
				case ASTNodeType.Float:
					return new AstFloat ( node );
				case ASTNodeType.Int:
					return new AstInt ( node );
				case ASTNodeType.Bin:
					return new AstBin ( node );
				case ASTNodeType.Hex:
					return new AstHex ( node );
				case ASTNodeType.Stack:
					return new AstStack ( node );
				case ASTNodeType.FxnType:
					return new AstFxnType ( node );
				case ASTNodeType.TypeVar:
					return new AstTypeVar ( node );
				case ASTNodeType.TypeName:
					return new AstSimpleType ( node );
				case ASTNodeType.StackVar:
					return new AstStackVar ( node );
				case ASTNodeType.MacroRule:
					return new AstMacro ( node );
				case ASTNodeType.MacroProp:
					return new AstMacro ( node );
				case ASTNodeType.MacroPattern:
					return new AstMacroPattern ( node );
				case ASTNodeType.MacroQuote:
					return new AstMacroQuote ( node );
				case ASTNodeType.MacroTypeVar:
					return new AstMacroTypeVar ( node );
				case ASTNodeType.MacroStackVar:
					return new AstMacroStackVar ( node );
				case ASTNodeType.MacroName:
					return new AstMacroName ( node );
				case ASTNodeType.MetaDataContent:
					return new AstMetaDataContent ( node );
				case ASTNodeType.MetaDataLabel:
					return new AstMetaDataLabel ( node );
				case ASTNodeType.MetaDataBlock:
					return new AstMetaDataBlock ( node );
				default:
					throw new Exception ( "unrecognized node type in AST tree: " + label );
			}
		}

		public void CheckIsLeaf (PegAstNode node) {
			CheckChildCount ( node, 0 );
		}

		public void CheckLabel (ASTNodeType label) {
			if ( !GetLabel ().Equals ( label ) )
				throw new Exception ( "Expected label " + label.ToString () + " but instead have label " + GetLabel ().ToString () );
		}

		public void CheckChildCount (PegAstNode node, int n) {
			if ( node.GetNumChildren () != n )
				throw new Exception ( "expected " + n.ToString () + " children, instead found " + node.GetNumChildren ().ToString () );
		}

		public ASTNodeType GetLabel () {
			return mLabel;
		}

		public override string ToString () {
			return msText;
		}

		public void SetText (string s) {
			msText = s;
		}

		public string IndentedString (int nIndent, string s) {
			if ( nIndent > 0 )
				return new String ( '\t', nIndent ) + s;
			else
				return s;
		}

		public virtual void Output (TextWriter writer, int nIndent) {
			writer.Write ( ToString () );
		}
	}


	/// <summary>
	/// A program consists of a sequence of statements 
	/// TODO: reintroduce declarations and macros
	/// </summary>
	public class RubyScript : RubyAstNode {
		public List<RubyAstNode> mStatements = new List<RubyAstNode> ();

		public RubyScript (Peg.PegAstNode node)
			: base ( node ) {
			foreach ( Peg.PegAstNode child in node.GetChildren () ) {
				RubyAstNode statement = RubyAstNode.Create ( child );
				mStatements.Add ( statement );
			}
		}
	}
}

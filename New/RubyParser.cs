using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Peg;

namespace Ruby {

	/// <summary>
	/// The CatParser constructs a typed AST representing Cat source code.
	/// This library is based on the PEG parsing library
	/// </summary>
	public class RubyParser {

		public static List<RubyAstNode> Parse (string s) {

			Parser parser = new Parser ( s );

			try {
				bool bResult = parser.Parse ( RubyGrammar.RubyScript () );
				if ( !bResult )
					throw new Exception ( "failed to parse input" );
			}
			catch ( Exception e ) {
				Console.WriteLine ( "Parsing error occured with message: " + e.Message );
				Console.WriteLine ( parser.ParserPosition );
				throw e;
			}

			RubyScript tmp = new RubyScript ( parser.GetAst () );
			return tmp.mStatements;
		}

		public static List<RubyAstNode> ParseByRule (string s, Grammar.Rule rule) {

			Parser parser = new Parser ( s );

			try {
				bool bResult = parser.Parse ( rule );
				if ( !bResult )
					throw new Exception ( "failed to parse input" );
			}
			catch ( Exception e ) {
				Console.WriteLine ( "Parsing error occured with message: " + e.Message );
				Console.WriteLine ( parser.ParserPosition );
				throw e;
			}

			RubyScript tmp = new RubyScript ( parser.GetAst () );
			return tmp.mStatements;
		}
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Peg;

namespace Ruby
{
    public class RubyTests
    {
        public static void TestPrint(string s)
        {
            try
            {
                Console.WriteLine("input:");
                Console.WriteLine(s);
                //var n = RubyGrammar.RubyScript().(s)[0];
                //Console.WriteLine("nodes");
                //Console.WriteLine(n.ToXml);
                //var p = JavaScriptSourcePrinter.ToString(n);
                //Console.WriteLine("output:");
                //Console.WriteLine(p);
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR occured " + e.Message);
            }
        }

        public static void Test(string s, dynamic expected = null)
        {
            try
            {
                if (expected != null)
                {
                    Console.WriteLine("Testing: {0}", s);
                    Console.WriteLine("Expecting {0}", expected);
                    //dynamic result = RubyEvaluator.RunScript(s);
                    //Console.WriteLine("Result {0}", result);
                    //if (result != expected)
                    //    Console.WriteLine("ERROR!");
                }
                else
                {
                    Console.WriteLine("Testing: {0}", s);
					//RubyEvaluator.RunScript(s);                    
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Error occured: " + e.Message);
            }
        }

        public static void TestParse(string s, Grammar.Rule r)
        {
            GrammarTest.Test(s, r);
        }

        public static void TimingTests()
        {
			Utilities.TimeExecution ( () =>
				  TestParse ( @"x = 1", RubyGrammar.RubyScript() ) );

			return;

			Utilities.TimeExecution(() =>
                TestParse( @"x = { a => 10, b => 20, c => { x => 1, y => 2, z => 3 } }", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : 10, b : 20, c :  { x : 1, y : 2, z : a[1][2][3][4][5][6] } };", RubyGrammar.RubyScript()));
            
            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : 10, b : 20, c :  { x : 1, y : 2, z : a[1][2][3][4][5][6], w : { a:1, b:2, c:3, d:4, e:5 } } };", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : [1,2,3,4,5,6,7,8,9], b : [1,2,3,4,5,6,7,8,9], c : [1,2,3,4,5,6,7,8,9] };", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : { b : { c : [1,2,3,4,5,6,7,8,9] } } };", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : { b : [1,2,3,4,5,6,7,8,9], c : [1,2,3,4,5,6,7,8,9], d : [1,2,3,4,5,6,7,8,9] } };", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : { b : { c : [1,2,3,4,5,6,7,8,9], d : [1,2,3,4,5,6,7,8,9], e : [1,2,3,4,5,6,7,8,9] } } };", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : { b : { c:1, d:1, e:1, f:1, g:1, h:1, i:1, j:1, k:1, l:1, m:1, n:1 }, o:1, p:1, q:1, r:1 }, s:1, t:1, u:1 };", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
                TestParse(@"var x = { a : { b : { c : [1,2,3,4,5,6,7,8,9], d : [1,2,3,4,5,6,7,8,9], e : [1,2,3,4,5,6,7,8,9] }, f:1, g:1, h:1, i:1, j:1, k:1, l:1, m:1, n:1 }, o:1, p:1, q:1, r:1 };", RubyGrammar.RubyScript()));

            Utilities.TimeExecution(() =>
				TestParse(
					@"var x = { 
						a : 10, 
						b : 20, 
						c : { 
							x : 1, y : 2, z : 
							[ 
								'a', 'b', 'c', 'd', 'e', 'f', 'g', 
								[1, 2, 3, 4, 5, 6], 
								[7, 8, 9, 10, 11, 12], 
								{ a:1, b:2, c:3, d:4 } 
							] },
						d : a[1][2][3],
						'e' : f(a, b, {a:1, b:2, c:3}),
						""f"" : f(),
						h : f([a, b, c, d, e, e])
						};", RubyGrammar.RubyScript() ) );
		}

        public static void Tests()
        {
			//TimingTests ();

			//TestParse ( "# line comment s\n", RubyGrammar.Comment () );
			//TestParse ( "=begin full comment\n\n=end", RubyGrammar.Comment () );

			TestParse ( "a", RubyGrammar.LocalVariableReference () );
			TestParse ( "@a", RubyGrammar.InstanceVariableReference () );
			TestParse ( "$FILE", RubyGrammar.GlobalVariableReference () );
			TestParse ( "FILE", RubyGrammar.ConstantReference () );

			//TestParse ( "#{ x }", RubyGrammar.ParamChar );

			//TestParse ( "<<EOF text here\n text EOF", RubyGrammar.LineOrientedString );
			TestParse ( "\"hello\"", RubyGrammar.StringLiteral () );
			TestParse ( "\'hello\'", RubyGrammar.StringLiteral () );
			TestParse ( "99.9", RubyGrammar.Literal () );
			TestParse ( "99", RubyGrammar.Literal () );
			TestParse ( "true", RubyGrammar.Literal () );
			TestParse ( "nil", RubyGrammar.Literal () );
			TestParse ( ":symbol", RubyGrammar.Literal () );
			TestParse ( "[ 1, 2.0, 'nice', false, :symbol ]", RubyGrammar.ArrayLiteral () );
			TestParse ( "{ a => 3, b => 4 }", RubyGrammar.HashLiteral () );

			TestParse ( "3 + 3", RubyGrammar.Expr () );
			TestParse ( "3 + 3", RubyGrammar.BinaryExpr () );
			TestParse ( "x + 3", RubyGrammar.BinaryExpr () );
			TestParse ( "1 .. 3", RubyGrammar.BinaryExpr () );
			TestParse ( "x = 3", RubyGrammar.Expr () );
			//TestParse ( "x = 3", RubyGrammar.AssignExpr );
			//TestParse ( "(3)", RubyGrammar.ParenExpr );
			//TestParse ( "(3 + 3)", RubyGrammar.ParenExpr );
			//TestParse ( "42;", RubyGrammar.ExprStatement );
			//TestParse ( "\"hello\"", RubyGrammar.Expr );
			//TestParse ( "\"\"", RubyGrammar.Expr );
			//TestParse ( "\"\\\"\"", RubyGrammar.Expr );
			//TestParse ( "'hello'", RubyGrammar.Expr );
			//TestParse ( "''", RubyGrammar.Expr );
			//TestParse ( "'\"'", RubyGrammar.Expr );
			//TestParse ( "\"'\"", RubyGrammar.Expr );
			//TestParse ( "{}", RubyGrammar.Hash );
			//TestParse ( "{ }", RubyGrammar.Hash );
			//TestParse ( "f", RubyGrammar.Expr );
			//TestParse ( "()", RubyGrammar.ArgList );
			//TestParse ( "(  )", RubyGrammar.ArgList );
			//TestParse ( "(a)", RubyGrammar.ArgList );
			//TestParse ( "( a )", RubyGrammar.ArgList );
			//TestParse ( "(a,b)", RubyGrammar.ArgList );
			//TestParse ( "( a , b )", RubyGrammar.ArgList );
			//TestParse ( "f()", RubyGrammar.Expr );
			//TestParse ( "f(a)", RubyGrammar.Expr );
			//TestParse ( "f(a, b)", RubyGrammar.Expr );
			//TestParse ( "f(a)(b)", RubyGrammar.Expr );
			//TestParse ( "(f)(b)", RubyGrammar.Expr );
			//TestParse ( "a[1]", RubyGrammar.Expr );
			//TestParse ( "a[1](2)", RubyGrammar.Expr );
			//TestParse ( "[]", RubyGrammar.Expr );
			//TestParse ( "[a]", RubyGrammar.Expr );
			//TestParse ( "[a,1,2]", RubyGrammar.Expr );
			//TestParse ( "{}", RubyGrammar.Expr );
			//TestParse ( "{a:b}", RubyGrammar.Expr );
			//TestParse ( "{ :a : b }", RubyGrammar.Expr );
			//TestParse ( "{a:b,\"b\":42}", RubyGrammar.Expr );
			//TestParse ( "{ a : { b : c }, d : [1, 2, 3] }", RubyGrammar.Expr );
			//TestParse ( "def f()\n\nend", RubyGrammar.FunExpr );
			//TestParse ( "function f() { }", RubyGrammar.Expr );
			//TestParse ( "function () { }", RubyGrammar.FunExpr );
			TestParse ( "def f() end", RubyGrammar.FunctionDefinitions () );
			//TestParse ( "def f(a) end", RubyGrammar.Function );
			//TestParse ( "def f(a, &b) end", RubyGrammar.Function );
			//TestParse ( "def f(a, b) end", RubyGrammar.Function );
			//TestParse ( "def f(a, b) end;", RubyGrammar.Statement );
			//TestParse ( "(def f(a, b) end)()", RubyGrammar.Expr );
			//TestParse ( "(def f(a, b) end)();", RubyGrammar.Statement );
			//TestParse ( "return 42", RubyGrammar.Statement );
			TestParse ( "return 13 if false", RubyGrammar.IfModifier () );
			TestParse ( "if x 42 end", RubyGrammar.Statement () );
			TestParse ( "if x 42 else return 43", RubyGrammar.Statement () );
			TestParse ( "if (y) return x + 5; else { var z = 6; return z + x; }", RubyGrammar.Statement () );
			TestParse ( "def f(x, y) { if (y) return x + 5; else { var z = 6; return z + x; } }", RubyGrammar.FunctionDefinitions () );

			TestParse ( "2", RubyGrammar.RubyScript() );

			Test ( "42;", 42 );
			Test ( "6 * 7;", 42 );
			Test ( "(3 + 3) * (3 + 4);", 42 );
			Test ( "2 + 5 * 8;", 42 );
			Test ( "2 + (5 * 8);", 42 );
			Test ( "(2 + 5) * 8;", 35 );
			Test ( "13; 42;", 42 );
			Test ( "{ 13; 42; }", 42 );
			Test ( "var i = 42; i;", 42 );
			Test ( "(function() { return 42; })();", 42 );
			Test ( "var i = 1; i = 5; 3; i + 2;", 7 );
			Test ( "i = 42; i;", 42 );
			Test ( "var i = 1; i = 5; 3; i += 2;", 7 );
			Test ( "function f(x) { return x + 1; }; f(41);", 42 );
			Test ( "var f = function(x) { return x + 1; }; f(41);", 42 );
			Test ( "f = function(x) { return x + 1; }; f(41);", 42 );
			Test ( "var x = 41; f = function() { return x + 1; }; f();", 42 );
			Test ( "function f(a, b) { return a * b; }; f(6, 7);", 42 );
			Test ( "var a = 6; function f(b) { return a * b; }; f(7);", 42 );
			Test ( "var a = 6; function f(b) { return a * b; }; { a = 7; f(7); }", 49 );
			Test ( "var a = 6; function f(b) { return a * b; }; { var a = 7; f(7); }", 42 );
			Test ( "var x = { a : 42 }; x.a;", 42 );
			Test ( "alert('Hello world');" );
			Test ( "alert(\"Hello world\");" );

			TestPrint ( "var a; a = 10; a += 5; function f(x, y) { if (y) return x + 5; else { var z = 6; return z + x; } }; alert(f(9));" );
			TestPrint ( "var x = { a : 10, b : 20, c : { x : 1, y : 2, z : [ 'a', 'b', 'c'] } };" );

		}
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Diggins.Jigsaw
{
    public class GrammarTest
    {
        public static void Test(string s, Rule r)
        {
            try
            {
				Print ( string.Format ( "Using rule {0} to parse string {1}", r.Name, s ) );

                var nodes = r.Parse(s);
                if (nodes == null || nodes.Count != 1) {
					Print ( "Parsing failed!", ConsoleColor.Red );
				}
				else if (nodes[0].Text != s) {
					Print ( "Parsing partially succeeded", ConsoleColor.Yellow );
				}
                else {
					Print ( "Parsing suceeded", ConsoleColor.Green );
				}

				if ( nodes != null && nodes.Count > 0 ) {
					Console.WriteLine ( nodes[0].Text + "\n" );
				}
				else {
					Console.WriteLine ();
				}
			}
            catch (Exception e)
            {
				Print ( "Parsing failed with exception:", ConsoleColor.Red );
				Print ( e.Message + "\n" + e.StackTrace + "\n", ConsoleColor.Gray );
            }
        }

		public static void Print ( string text, ConsoleColor color = ConsoleColor.White ) {
			var old = Console.ForegroundColor;
			Console.ForegroundColor = color;
			Console.WriteLine ( text );
			Console.ForegroundColor = old;
		}
    }
}

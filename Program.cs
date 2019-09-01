using System;
using System.Linq;
using System.Linq.Expressions;
using System.Dynamic;

namespace Ruby
{
    class Program
    {
        //static object AddTest(object a, object b)
        //{
        //    return Primitives.add(a, b);
        //}

        static void Main(string[] args)
        {
            //ArithmeticTests.Tests();
            //SExprTests.Tests();
            //JsonTests.Tests();
            //UnifierTests.Tests();
            //CatTests.Tests();
            //ILTests.Tests();
            //LambdaCalculus.Test();
            //EmbeddedScheme.Tests();
            //CodeDOMCompilerTests.Tests();
            //CSharpFunctionCompilerTests.Tests();
            //JavaScriptTests.Tests();
            RubyTests.Tests();
            //CodeProjectArticleSnippets.Tests();
            Console.WriteLine("And that's it. Press any key to go home ...");
            Console.ReadKey();
        }
    }
}

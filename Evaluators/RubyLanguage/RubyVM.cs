using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Diggins.Jigsaw {

	public class Machine
    {
        private Context rootcontext = new Context();
        private IList<string> requirepaths = new List<string>();
        private IList<string> required = new List<string>();

		internal RubyClass basicobjectclass;
		internal RubyClass objectclass;
		internal RubyClass moduleclass;
		internal RubyClass classclass;

		public Machine()
        {
            this.requirepaths.Add(".");
			this.basicobjectclass = new RubyClass("BasicObject", null);
			this.objectclass = new RubyClass("Object", basicobjectclass);
			this.moduleclass = new RubyClass("Module", objectclass);
			this.classclass = new RubyClass("Class", moduleclass);

            this.rootcontext.SetLocalValue("BasicObject", basicobjectclass);
            this.rootcontext.SetLocalValue("Object", objectclass);
            this.rootcontext.SetLocalValue("Module", moduleclass);
            this.rootcontext.SetLocalValue("Class", classclass);

            basicobjectclass.SetClass(classclass);
            objectclass.SetClass(classclass);
            moduleclass.SetClass(classclass);
            classclass.SetClass(classclass);

            basicobjectclass.SetInstanceMethod("class", new LambdaFunction(GetClass));
            basicobjectclass.SetInstanceMethod("methods", new LambdaFunction(GetMethods));
            basicobjectclass.SetInstanceMethod("singleton_methods", new LambdaFunction(GetSingletonMethods));

            moduleclass.SetInstanceMethod("superclass", new LambdaFunction(GetSuperClass));
            moduleclass.SetInstanceMethod("name", new LambdaFunction(GetName));

            classclass.SetInstanceMethod("new", new LambdaFunction(NewInstance));

            //this.rootcontext.SetLocalValue("Fixnum", new FixnumClass(this));
            //this.rootcontext.SetLocalValue("Float", new FloatClass(this));
            //this.rootcontext.SetLocalValue("String", new StringClass(this));
            //this.rootcontext.SetLocalValue("NilClass", new NilClass(this));
            //this.rootcontext.SetLocalValue("FalseClass", new FalseClass(this));
            //this.rootcontext.SetLocalValue("TrueClass", new TrueClass(this));
            //this.rootcontext.SetLocalValue("Array", new ArrayClass(this));
            //this.rootcontext.SetLocalValue("Hash", new HashClass(this));
            //this.rootcontext.SetLocalValue("Range", new RangeClass(this));

            this.rootcontext.Self = objectclass.CreateInstance();

            //this.rootcontext.Self.Class.SetInstanceMethod("puts", new PutsFunction(System.Console.Out));
            //this.rootcontext.Self.Class.SetInstanceMethod("print", new PrintFunction(System.Console.Out));
            //this.rootcontext.Self.Class.SetInstanceMethod("require", new RequireFunction(this));
        }

        public Context RootContext { get { return this.rootcontext; } }

        public object ExecuteText(string text)
        {
			//Parser parser = new Parser(text);
			//object result = null;

			//for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
			//    result = command.Evaluate(this.rootcontext);

			//return result;
			return RubyEvaluator.RunScript ( text );
		}

        public object ExecuteFile(string filename)
        {
            string path = Path.GetDirectoryName(filename);

            this.requirepaths.Insert(0, path);

            try
            {
                return this.ExecuteText(System.IO.File.ReadAllText(filename));
            }
            finally
            {
                this.requirepaths.RemoveAt(0);
            }
        }

        public bool RequireFile(string filename)
        {
            if (!Path.IsPathRooted(filename))
            {
                foreach (var path in this.requirepaths)
                {
                    string newfilename = Path.Combine(path, filename);
                    if (!File.Exists(newfilename))
                        if (File.Exists(newfilename + ".rb"))
                            newfilename += ".rb";
                        else if (File.Exists(newfilename + ".dll"))
                            newfilename += ".dll";

                    if (File.Exists(newfilename))
                    {
                        filename = newfilename;
                        break;
                    }
                }
            }
            else
            {
                string newfilename = Path.GetFullPath(filename);

                if (!File.Exists(newfilename))
                    if (File.Exists(newfilename + ".rb"))
                        newfilename += ".rb";
                    else if (File.Exists(newfilename + ".dll"))
                        newfilename += ".dll";

                filename = newfilename;
            }

            if (this.required.Contains(filename))
                return false;

            if (filename.EndsWith(".dll"))
            {
                Assembly.LoadFrom(filename);
                return true;
            }

            this.ExecuteFile(filename);
            this.required.Add(filename);

            return true;
        }

        //public object ExecuteReader(TextReader reader)
        //{
        //    Parser parser = new Parser(reader);
        //    object result = null;

        //    for (var command = parser.ParseCommand(); command != null; command = parser.ParseCommand())
        //        result = command.Evaluate(this.rootcontext);

        //    return result;
        //}

        private static object NewInstance(RubyObject obj, Context context, IList<object> values)
        {
            var newobj = ((RubyClass)obj).CreateInstance();

            var initialize = newobj.GetMethod("initialize");

            if (initialize != null)
                initialize.Apply(newobj, context, values);

            return newobj;
        }

        private static object GetName(RubyObject obj, Context context, IList<object> values)
        {
            return ((RubyClass)obj).Name;
        }

        private static object GetSuperClass(RubyObject obj, Context context, IList<object> values)
        {
            return ((RubyClass)obj).SuperClass;
        }

        private static object GetClass(RubyObject obj, Context context, IList<object> values)
        {
            return obj.Class;
        }

        private static object GetMethods(RubyObject obj, Context context, IList<object> values)
        {
            //var result = new RubyObject();

            //for (var @class = obj.SingletonClass; @class != null; @class = @class.SuperClass)
            //{
            //    var names = @class.GetOwnInstanceMethodNames();

            //    foreach (var name in names)
            //    {
            //        Symbol symbol = new Symbol(name);

            //        if (!result.AsCollection.Contains(symbol))
            //            result.Add(symbol);
            //    }
            //}

            //return result;
			throw new NotImplementedException ();
		}

        private static object GetSingletonMethods(RubyObject obj, Context context, IList<object> values)
        {
			//var result = new RubyArray();

			//var names = obj.SingletonClass.GetOwnInstanceMethodNames();

			//foreach (var name in names)
			//{
			//    Symbol symbol = new Symbol(name);

			//    if (!result.Contains(symbol))
			//        result.Add(symbol);
			//}

			//return result;
			throw new NotImplementedException ();
        }
    }
}

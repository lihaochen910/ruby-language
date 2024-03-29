﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Linq.Expressions;

namespace Diggins.Jigsaw
{
    public static class Utilities
    {
        public static Type GetType(string s)
        {
            s = s.Trim();
            switch (s)
            {
                case "int": return typeof(int);
                case "uint": return typeof(uint);
                case "char": return typeof(char);
                case "float": return typeof(float);
                case "double": return typeof(double);
                case "bool": return typeof(bool);
                case "byte": return typeof(byte);
                case "sbyte": return typeof(sbyte);
                case "object": return typeof(object);
                case "decimal": return typeof(decimal);
                case "string": return typeof(string);
                default:
                    var r = Type.GetType(s);
                    if (r != null) return r;
                    r = Type.GetType("System." + s);
                    if (r != null) return r;
                    throw new Exception("Could not find type " + s);
            }
        }

        public static void TimeExecution(Action a)
        {
            Console.WriteLine("Starting execution");
            var start = DateTime.Now;
            a();
            var elapsed = DateTime.Now - start;
            Console.WriteLine("Time of execution is {0} sec, {1} msec", elapsed.TotalSeconds, elapsed.Milliseconds);
        }

        public static void RunMain(Assembly asm, params string[] args)
        {
            MethodInfo main = null;
            foreach (var t in asm.GetTypes())
                foreach (var mi in t.GetMethods(BindingFlags.Public | BindingFlags.Static))
                    if (mi.Name == "Main")
                       main = mi;
            main.Invoke(null, new object[] { args });
        }

        public static string Unquote(string s)
        {
            return s.Substring(1, s.Length - 2);
        }

        public static void Test()
        {
            Func<object, object> f = x => x;
            Delegate d = f;
        }
    }
}

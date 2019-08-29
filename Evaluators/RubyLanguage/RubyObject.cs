using System;                    
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;

namespace Diggins.Jigsaw
{
    /// <summary>
    /// This is a dynamic class for represent JSON object data. It can be used 
    /// as a "dynamic" variable, and can be constructed from a string using the static Parse
    /// method. 
    /// There is additional built-in support for implementing features of JavaScript 
    /// such as prototype chain look-up and cloning.
    /// </summary>
    public class RubyObject : DynamicObject, IEnumerable<KeyValuePair<string, object>>
    {
        #region 
        ExpandoObject x = new ExpandoObject();
		#endregion

		private RubyClass @class;
		private RubyClass singletonclass;

		public RubyObject(RubyClass @class)
        {
			this.@class = @class;
		}

        public RubyObject(RubyObject prototype)
        {
            this["prototype"] = prototype;
        }

        public RubyObject Clone()
        {
            return new RubyObject(this);
        }

		public RubyClass @Class { get { return this.@class; } }

		public RubyClass SingletonClass {
			get
			{
				if ( this.singletonclass == null ) {
					this.singletonclass = new RubyClass ( string.Format ( "#<Class:{0}>", this.ToString () ), this.@class );
					this.singletonclass.SetClass ( this.@class.Class );
				}

				return this.singletonclass;
			}
		}

		public void Add(string name, dynamic value)
        {
            this[name] = value;
        }

        static string Unquote(string s)
        {
            int n = s.Length;
            return (n > 2 && s.First() == '"' && s[n - 1] == '"')
                ? s.Substring(1, n - 2)
                : s;
        }

        public IDictionary<string, Object> AsDictionary
        {
            get { return x; }
        }

		public ICollection<KeyValuePair<string, object>> AsCollection {
			get { return x; }
		}

		public bool HasField(string s)
        {
            return AsDictionary.ContainsKey(Unquote(s));
        }

        public dynamic this[string name]
        {
            get 
            {
                if (HasField(name))
                    return AsDictionary[Unquote(name)];
                else if (HasField("prototype")) {
                    dynamic d = AsDictionary["prototype"];
                    return d[name];
                }
                else
                    throw new Exception(String.Format("Could not find the name {0} or a prototype", name));
            }
            set
            {
                AsDictionary[Unquote(name)] = value;
            }
        }

        public dynamic this[int index]
        {
            get
            {
                return x.ElementAt(index).Value;
            }
            set
            {
                var key = x.AsCollection().ElementAt(index).Key;
                this[key] = value;
            }
        }

        public IEnumerable<KeyValuePair<string, Object>> KeyValues
        {
            get
            {
                return x;
            }
        }

        public StringBuilder BuildString(StringBuilder sb)
        {
            sb.AppendLine("{");
            int n = 0;
            foreach (var kv in KeyValues)
            {
                if (n++ > 0) sb.Append(", ");
                sb.AppendFormat("\"{0}\" : ", kv.Key);
                JsonValueToString(kv.Value, sb);
                sb.AppendLine();
            }
            sb.AppendLine("}");
            return sb;
        }

		public virtual IFunction GetMethod ( string name ) {

			if ( this.singletonclass != null )
				return this.singletonclass.GetInstanceMethod ( name );

			if ( this.@class != null )
				return this.@class.GetInstanceMethod ( name );

			return null;
		}

		public override string ToString()
        {
			return string.Format ( "#<{0}:0x{1}>", this.Class.Name, this.GetHashCode ().ToString ( "x" ) );
		}

		internal void SetClass ( RubyClass @class ) {
			this.@class = @class;
		}

		public static StringBuilder JsonValueToString(dynamic value, StringBuilder sb)
        {
            if (value is RubyObject)
            {
                return value.BuildString(sb);
            }
            else if (value is List<dynamic>)
            {
                var xs = (List<dynamic>)value;
                sb.Append("[");
                for (int i = 0; i < xs.Count; ++i)
                {
                    if (i > 0) sb.Append(", ");
                    JsonValueToString(xs[i], sb);
                }
                sb.Append("]");
            }
            else
            {
                sb.Append(value.ToString());
                sb.Append(" ");
            }
            return sb;
        }

        public static dynamic Eval(Node n)
        {
            switch (n.Label)
            {
                case "Name": return Eval(n[0]);
                case "Value": return Eval(n[0]);
                case "Fixnum": return Int32.Parse(n.Text);
                case "Float": return Double.Parse(n.Text);
                case "String": return n.Text.Substring(1, n.Text.Length - 2);
                case "True": return true;
                case "False": return false;
                case "Nil": return null;
                case "Symbol": return new Symbol(n.Text.Replace(":", string.Empty));
                case "Array": return n.Nodes.Select(Eval).ToList();
                case "Hash": return n.Nodes.Select(Eval).ToDictionary(var => var);
                //case "Object":
                //    {
                //        var r = new RubyObject();
                //        foreach (var pair in n.Nodes)
                //        {
                //            var name = pair[0].Text;
                //            var value = Eval(pair[1]);
                //            r[name] = value;
                //        }
                //        return r;
                //    }
                default:
                    throw new Exception("Unexpected node type " + n.Label);
            }
        }

        public override IEnumerable<string> GetDynamicMemberNames()
        {
            return KeyValues.Select(kv => kv.Key);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            result = null;
            if (!HasField(binder.Name)) return false;
            result = this[binder.Name];
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = value;
            return true;
        }

        public IEnumerator<KeyValuePair<string, dynamic>> GetEnumerator()
        {
            return AsDictionary.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return AsDictionary.GetEnumerator();
        }
    }
}

﻿
using System;
using System.Collections.Generic;
using System.Linq;

namespace Diggins.Jigsaw {

	public class RubyClass : RubyObject {

		private string name;
		private RubyClass superclass;
		private RubyClass parent;
		private IDictionary<string, IFunction> methods = new Dictionary<string, IFunction> ();
		private Context constants = new Context ();

		public RubyClass (string name, RubyClass superclass = null)
			: this ( null, name, superclass ) {
		}

		public RubyClass (RubyClass @class, string name, RubyClass superclass = null, RubyClass parent = null)
			: base ( @class ) {
			this.name = name;
			this.superclass = superclass;
			this.parent = parent;
		}

		public string Name { get { return this.name; } }

		public RubyClass SuperClass { get { return this.superclass; } }

		public Context Constants { get { return this.constants; } }

		public string FullName {
			get
			{
				if ( this.parent == null )
					return this.Name;

				return this.parent.FullName + "::" + this.Name;
			}
		}

		public void SetInstanceMethod (string name, IFunction method) {
			this.methods[name] = method;
		}

		public IFunction GetInstanceMethod (string name) {
			if ( this.methods.ContainsKey ( name ) )
				return this.methods[name];

			if ( this.superclass != null )
				return this.superclass.GetInstanceMethod ( name );

			return null;
		}

		public RubyObject CreateInstance () {
			return new RubyObject ( this );
		}

		public override IFunction GetMethod (string name) {
			return base.GetMethod ( name );
		}

		public IList<string> GetOwnInstanceMethodNames () {
			return this.methods.Keys.ToList ();
		}

		public override string ToString () {
			return this.FullName;
		}

		internal void SetSuperClass (RubyClass superclass) {
			this.superclass = superclass;
		}
	}
}

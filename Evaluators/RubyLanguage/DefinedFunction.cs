using System;
using System.Collections.Generic;

namespace Diggins.Jigsaw {

	public class DefinedFunction : IFunction
    {
        private Node body;
        private IList<string> parameters;
        private Context context;

        public DefinedFunction(Node body, IList<string> parameters, Context context)
        {
            this.body = body;
            this.context = context;
            this.parameters = parameters;
        }

        public object Apply(RubyObject self, Context context, IList<object> values)
        {
            Context newcontext = new Context(self, this.context);

            int k = 0;
            int cv = values.Count;

            foreach (var parameter in this.parameters) 
            {
                newcontext.SetLocalValue(parameter, values[k]);
                k++;
            }

			return RubyObject.Eval ( body );

            //return this.body.Evaluate(newcontext);
        }
    }
}

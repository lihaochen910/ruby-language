using System;
using System.Collections.Generic;

namespace Diggins.Jigsaw {

	public class LambdaFunction : IFunction
    {
        private Func<RubyObject, Context, IList<object>, object> lambda;

        public LambdaFunction(Func<RubyObject, Context, IList<object>, object> lambda)
        {
            this.lambda = lambda;
        }

        public object Apply(RubyObject self, Context context, IList<object> values)
        {
            return this.lambda(self, context, values);
        }
    }
}

namespace Diggins.Jigsaw {

    using System;
    using System.Collections.Generic;

    public interface IFunction
    {
        object Apply(RubyObject self, Context context, IList<object> values);
    }
}

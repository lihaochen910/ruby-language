﻿namespace Diggins.Jigsaw {

    using System;

    public class Symbol
    {
        private static int hashcode = typeof(Symbol).GetHashCode();
        private string name;

        public Symbol(string name)
        {
            this.name = name;
        }

		public string Name { get => name; }

        public override string ToString()
        {
            return ":" + this.name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (obj is Symbol)
            {
                var symbol = (Symbol)obj;

                return this.name == symbol.name;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return this.name.GetHashCode() + hashcode;
        }
    }
}

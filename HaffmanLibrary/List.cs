using System;
using System.Collections.Generic;
using System.Text;

namespace HaffmanLibrary
{
    class List : IComparable<List>
    {
        private char _value = '\0';
        private int _weight = 0;
        private List left = null;
        private List right = null;
        private string _code = null;

        public List Left
        {
            get => left;
            set => left = value;
        }

        public List Right
        {
            get => right;
            set => right = value;
        }

        public string Code
        {
            get => _code;
            set => _code = value;
        }

        public int Weight { get => _weight; }
        public char Value { get => _value; }

        public List(in char value, in int weight)
        {
            this._value = value;
            this._weight = weight;
        }

        public List(in int weight)
        {
            this._weight = weight;
        }

        public int CompareTo(List obj)
        {
            return this._weight.CompareTo(obj._weight);
        }
    }
}

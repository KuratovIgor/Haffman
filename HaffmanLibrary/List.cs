using System;
using System.Collections.Generic;
using System.Text;

namespace HaffmanLibrary
{
    class List : IComparable<List>
    {
        private char _value = '\0'; //Symbol from text
        private readonly int _weight = 0; //Quantity of symbol in text
        private List left = null; //Left list of tree
        private List right = null; //Right list of tree
        private string _code = null; //New binary code of this symbol

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
        public char Value { get => _value; set => _value = value; }

        public List() { }

        public List(in char value, in int weight)
        {
            this._value = value;
            this._weight = weight;
        }

        public List(in int weight)
        {
            this._weight = weight;
        }

        public List(in char value)
        {
            this._value = value;
        }

        //Method from interface IComporable for sort
        public int CompareTo(List obj)
        {
            return this._weight.CompareTo(obj._weight);
        }
    }
}

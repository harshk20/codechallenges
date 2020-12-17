using System;
using System.Collections.Generic;

namespace followthewhiterabbit
{
    // Class for fast traversal of words
    public class WordTree
    {
        private CharNode _root { get; set; }
        public CharNode Root { get { return _root; } }

        public WordTree()
        {
            _root = new CharNode(' ');
        }

        // Break down the word in tree of characters
        public void CompileWord (string word)
        {
            CharNode currentNode = Root;
            foreach (var ch in word)
                currentNode = currentNode.InsertCharInChild(ch);

            // Mark last character node as word
            currentNode.UpdateAsWord(true);
        }
        
    }

    // Class to hold data in tree node
    public class CharNode {

        private char _value { get; set; }
        public char Value { get { return this._value; } }
        private ICollection<CharNode> _children { get; set; }
        public IEnumerable<CharNode> Children { get { return this._children; } }

        public bool IsWord { get; set; }

        public CharNode (char c)
        {
            this._value = c;
            this.IsWord = false;
            this._children = new List<CharNode>();
        }

        public void UpdateAsWord (bool isWord)
        {
            this.IsWord = isWord;
        }

        // Insert a child character node and return it.
        public CharNode InsertCharInChild (char value)
        {
            CharNode newNode = FindCharInChild(value);
            if (newNode == null)
            {
                newNode = new CharNode(value);
                this._children.Add(newNode);
            }
            return newNode;
        }

        // See if character is there as a child
        public CharNode FindCharInChild (char value)
        {
            foreach (CharNode chn in this.Children)
            {
                if (chn.Value.Equals(value))
                    return chn;
            }

            return null;
        }

    }
}

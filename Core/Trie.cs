using System;
using System.Collections.Generic;

namespace Decomp.Core
{
    public class SimpleTrieNode<T>
    {
        public SimpleTrieNode<T>[] Next;
        public bool Leaf;
        public T Value;

        public SimpleTrieNode()
        {
            Next = new SimpleTrieNode<T>[27];
            Leaf = false;
            Value = default(T);
        }
    }

    public class SimpleTrie<T>
    {
        private SimpleTrieNode<T> _root;

        public SimpleTrie() => _root = new SimpleTrieNode<T>();

        public SimpleTrie(IEnumerable<KeyValuePair<string, T>> enumerable)
        {
            _root = new SimpleTrieNode<T>();
            foreach (var pair in enumerable) Add(pair); 
        }

        public void Add(KeyValuePair<string, T> pair) => Add(_root, pair.Key.ToLower(), pair.Value, 0);
        public void Add(string key, T value) => Add(_root, key.ToLower(), value, 0);

        private static void Add(SimpleTrieNode<T> node, string s, T value, int index)
        {
            while (true)
            {
                if (index == s.Length)
                {
                    node.Leaf = true;
                    node.Value = value;
                    return;
                }

                var nextIndex = Char.IsLetter(s, index) ? s[index] - 'a' : 26;
                var b = node.Next[nextIndex];
                if (b != null) node = b; 
                else
                {
                    var go = new SimpleTrieNode<T>();
                    node.Next[nextIndex] = go;
                    node = go;
                }
                index++;
            }
        }

        public bool ContainsKey(string key)
        {
            var b = _root;
            var s = key.ToLower();
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return false;
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'a' : 26];
            }

            return b?.Leaf ?? false;
        }

        public T GetValue(string key)
        {
            var b = _root;
            var s = key.ToLower();
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return default(T);
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'a' : 26];
            }

            return b == null ? default(T) : (b.Leaf ? b.Value : default(T));
        }

        public bool TryGetValue(string key, out T value)
        {
            var b = _root;
            var s = key.ToLower();
            value = default(T);
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return false;
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'a' : 26];
            }

            if (b == null) return false;
            value = b.Leaf ? b.Value : default(T);
            return true;
        }

        public T this[string key]
        {
            set => Add(new KeyValuePair<string, T>(key, value));
            get => GetValue(key);
        }

        public void Clear() => _root = new SimpleTrieNode<T>();

        public bool Remove(string key)
        {
            var b = _root;
            var s = key.ToLower();
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return false;
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'a' : 26];
            }

            if (b == null) return false;
            if (b.Leaf == false) return false;
            b.Leaf = false;
            return true;
        }
    }
}

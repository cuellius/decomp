using System;
using System.Collections.Generic;

namespace Decomp.Core
{
    public class SimpleTrieNode<T>
    {
#pragma warning disable CA1819 // Properties should not return arrays
        public SimpleTrieNode<T>[] Next { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
        public bool Leaf { get; set; }
        public T Value { get; set; }

        public SimpleTrieNode()
        {
            Next = new SimpleTrieNode<T>[27];
            Leaf = false;
            Value = default;
        }
    }

    public class SimpleTrie<T>
    {
        private SimpleTrieNode<T> _root;

        public SimpleTrie() => _root = new SimpleTrieNode<T>();

        public SimpleTrie(IEnumerable<KeyValuePair<string, T>> enumerable)
        {
            _root = new SimpleTrieNode<T>();

            if (enumerable == null) return;
            foreach (var pair in enumerable) Add(pair);
        }

        public void Add(KeyValuePair<string, T> pair) => Add(_root, pair.Key.ToUpperInvariant(), pair.Value, 0);
        public void Add(string key, T value) => Add(_root, key?.ToUpperInvariant(), value, 0);

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

                var nextIndex = Char.IsLetter(s, index) ? s[index] - 'A' : 26;
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
            if (key == null) return false;

            var b = _root;
            var s = key.ToUpperInvariant();
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return false;
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'A' : 26];
            }

            return b?.Leaf ?? false;
        }

        public T GetValue(string key)
        {
            if (key == null) return default;

            var b = _root;
            var s = key.ToUpperInvariant();
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return default;
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'A' : 26];
            }

            return b == null ? default : b.Leaf ? b.Value : default;
        }

        public bool TryGetValue(string key, out T value)
        {
            if (key == null)
            {
                value = default;
                return false;
            }

            var b = _root;
            var s = key.ToUpperInvariant();
            value = default;
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return false;
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'A' : 26];
            }

            if (b == null) return false;
            value = b.Leaf ? b.Value : default;
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
            if (key == null) return false;

            var b = _root;
            var s = key.ToUpperInvariant();
            for (int i = 0; i < s.Length; i++)
            {
                if (b == null) return false;
                b = b.Next[Char.IsLetter(s, i) ? s[i] - 'A' : 26];
            }

            if (b == null) return false;
            if (b.Leaf == false) return false;
            b.Leaf = false;
            return true;
        }
    }
}

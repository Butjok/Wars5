using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Butjok.CommandLine
{
    [CreateAssetMenu(menuName = nameof(Theme))]
    public class Theme : ScriptableObject
    {
        public Style normal = new Style();
        public Style error = new Style();
        [SerializeField] private List<Record> tokens = new List<Record>();
        private readonly Dictionary<Token, Style> cache = new Dictionary<Token, Style>();

        public bool TryGetStyle(Token token, out Style style) {
            if (cache.TryGetValue(token, out style))
                return true;
            var record = tokens.SingleOrDefault(record => record.token == token);
            style = record ?? normal;
            cache.Add(token, style);
            return record != null;
        }

        public void Reset() {
            var bold = new[] {
                Token.False,
                Token.Float2,
                Token.Float3,
                Token.Int2,
                Token.Int3,
                Token.Null,
                Token.Rgb,
                Token.String,
                Token.True
            };
            foreach (var token in bold)
                tokens.Add(new Record { token = token, bold = true });
        }

        [Serializable]
        private class Record : Style
        {
#pragma warning disable 0649
            public Token token;
#pragma warning restore 0649
        }
    }

    [Serializable]
    public class Style
    {
        public Color color = Color.white;
        public bool bold, italic;
    }
}
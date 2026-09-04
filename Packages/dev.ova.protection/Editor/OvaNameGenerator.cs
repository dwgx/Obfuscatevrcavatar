using System;
using System.Collections.Generic;
using System.Text;

namespace Ova.Editor
{
    /// <summary>
    /// Homoglyph names from Latin-1 I-with-diacritics (U+00CC–U+00CF).
    /// Same public alphabet many VRC obfuscators use; this class is OVA's own.
    /// Unique per instance. Scene assets are never renamed.
    /// </summary>
    internal sealed class OvaNameGenerator
    {
        static readonly char[] Alphabet = { 'Ì', 'Í', 'Î', 'Ï' };

        readonly Random _rng;
        readonly int _length;
        readonly HashSet<string> _used = new HashSet<string>();

        public OvaNameGenerator(int seed, int length)
        {
            if (seed == 0)
                seed = unchecked((int)(DateTime.UtcNow.Ticks ^ Environment.TickCount));
            _rng = new Random(seed);
            _length = Math.Max(8, length);
        }

        public void Reserve(string name)
        {
            if (!string.IsNullOrEmpty(name))
                _used.Add(name);
        }

        public string Next()
        {
            var sb = new StringBuilder();
            int extra = 0;
            for (int attempt = 0; attempt < 1024; attempt++)
            {
                sb.Length = 0;
                int n = _length + extra;
                for (int i = 0; i < n; i++)
                    sb.Append(Alphabet[_rng.Next(Alphabet.Length)]);
                var s = sb.ToString();
                if (_used.Add(s))
                    return s;
                if (attempt > 0 && (attempt & 31) == 0)
                    extra++;
            }
            sb.Length = 0;
            for (int i = 0; i < 24; i++)
                sb.Append(Alphabet[_rng.Next(Alphabet.Length)]);
            var fb = sb.ToString() + Guid.NewGuid().ToString("N").Substring(0, 4);
            _used.Add(fb);
            return fb;
        }
    }
}

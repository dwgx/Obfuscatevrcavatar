using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Ova
{
    /// <summary>
    /// Watermark / settings identity hash. Not a decrypt key. Safe to publish.
    /// </summary>
    public static class OvaFingerprint
    {
        public const string Algo = "ova-fp-v1";

        public static string Compute(OvaSettings settings)
        {
            if (settings == null) settings = new OvaSettings();
            var wm = settings.watermark ?? new OvaWatermarkLayer();
            var payload = new StringBuilder(128);
            payload.Append(Algo).Append('\n');
            payload.Append("seed=").Append(settings.seed.ToString(CultureInfo.InvariantCulture)).Append('\n');
            payload.Append("nameLength=").Append(settings.nameLength.ToString(CultureInfo.InvariantCulture)).Append('\n');
            payload.Append("watermark.enabled=").Append(wm.enabled ? "1" : "0").Append('\n');
            payload.Append("watermark.amplitude=")
                .Append(wm.amplitude.ToString("R", CultureInfo.InvariantCulture))
                .Append('\n');

            using (var sha = SHA256.Create())
            {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(payload.ToString()));
                var hex = new StringBuilder(hash.Length * 2);
                for (var i = 0; i < hash.Length; i++)
                    hex.Append(hash[i].ToString("x2"));
                return hex.ToString();
            }
        }
    }
}

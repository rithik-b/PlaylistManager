using System;
using SiraUtil.Affinity;
using TMPro;

namespace PlaylistManager.AffinityPatches
{
    internal class Amogus : IAffinity
    {
        [AffinityPrefix]
        [AffinityPatch(typeof(TMP_Text), nameof(TMP_Text.text), AffinityMethodType.Setter)]
        private void Joke(ref string value)
        {
            if (value == null)
            {
                return;
            }

            if (value.EndsWith("er", StringComparison.Ordinal) || value.EndsWith("tor", StringComparison.Ordinal) || value.EndsWith("ear", StringComparison.Ordinal))
            {
                value += "? I hardly know her!";
            }
            else
            {
                value = value.Replace("er ", "er? I hardly know her! ");
                value = value.Replace("tor ", "tor? I hardly know her! ");
                value = value.Replace("ear ", "ear? I hardly know her! ");
            }
        }
    }
}
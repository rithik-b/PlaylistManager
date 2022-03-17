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
            if (value.EndsWith("er") || value.EndsWith("tor") || value.EndsWith("ear"))
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
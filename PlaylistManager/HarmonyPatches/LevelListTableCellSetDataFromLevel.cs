using BeatSaberPlaylistsLib.Types;
using HarmonyLib;
using TMPro;

namespace PlaylistManager.HarmonyPatches
{
    [HarmonyPatch(typeof(LevelListTableCell))]
    [HarmonyPatch("SetDataFromLevelAsync", MethodType.Normal)]
    public class LevelListTableCellSetDataFromLevel
    {
        static void Postfix(IPreviewBeatmapLevel level, bool isFavorite, ref TextMeshProUGUI ____songAuthorText, TextMeshProUGUI ____songDurationText)
        {
            if (level is IPlaylistSong song)
            {
                ____songAuthorText.richText = true;
                if (!string.IsNullOrWhiteSpace(song.levelAuthorName))
                    ____songAuthorText.text = song.songAuthorName + " <size=80%>[" + song.levelAuthorName.Replace(@"<", "<\u200B").Replace(@">", ">\u200B") + "]</size>";
            }
        }
    }
}

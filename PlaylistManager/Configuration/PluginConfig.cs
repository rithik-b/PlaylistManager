using System.Runtime.CompilerServices;
using IPA.Config.Stores;
using IPA.Config.Stores.Attributes;
using IPA.Config.Stores.Converters;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace PlaylistManager.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; } = null!;
        public virtual string AuthorName { get; set; } = nameof(PlaylistManager);
        public virtual bool AutomaticAuthorName { get; set; } = true;
        public virtual bool DefaultImageDisabled { get; set; } = false;
        public virtual bool FoldersDisabled { get; set; } = false;
        public virtual bool DefaultAllowDuplicates { get; set; } = true;
        public virtual bool PlaylistHoverHints { get; set; } = true;
        public virtual bool BlurredArt { get; set; } = true;
        public virtual float PlaylistScrollSpeed { get; set; } = 1;
        public virtual bool HighlightDifficulty { get; set; } = true;
        public virtual bool EasterEggs { get; set; } = false;
        public virtual bool DownloadDuringGameplay { get; set; } = false;
        public virtual bool DriveFullProtection { get; set; } = true;
        public virtual bool ShowDownloadIcon { get; set; } = true;

        [UseConverter(typeof(EnumConverter<SyncOptions>))]
        [NonNullable]
        public virtual SyncOptions SyncOption { get; set; } = SyncOptions.On;

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
        }

        public enum SyncOptions
        {
            Off,
            On,
            Ask
        }
    }
}

namespace PlaylistManager.Interfaces
{
    interface ILevelCollectionUpdater
    {
        void LevelCollectionUpdated();
        void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory);
    }
}

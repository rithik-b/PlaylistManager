namespace PlaylistManager.Interfaces
{
    interface ILevelCategoryUpdater
    {
        public void LevelCategoryUpdated(SelectLevelCategoryViewController.LevelCategory levelCategory, bool viewControllerActivated);
    }
}

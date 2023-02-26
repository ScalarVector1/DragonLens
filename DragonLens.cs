using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using Terraria.ModLoader;

namespace DragonLens
{
	public class DragonLens : Mod
	{
		public override void PostAddRecipes()
		{
			if (ModContent.GetInstance<ToolConfig>().preloadSpawners)
			{
				UILoader.GetUIState<ItemBrowser>().Refresh();
				UILoader.GetUIState<ItemBrowser>().initialized = true;

				UILoader.GetUIState<ProjectileBrowser>().Refresh();
				UILoader.GetUIState<ProjectileBrowser>().initialized = true;

				UILoader.GetUIState<NPCBrowser>().Refresh();
				UILoader.GetUIState<NPCBrowser>().initialized = true;

				UILoader.GetUIState<BuffBrowser>().Refresh();
				UILoader.GetUIState<BuffBrowser>().initialized = true;

				UILoader.GetUIState<TileBrowser>().Refresh();
				UILoader.GetUIState<TileBrowser>().initialized = true;

				UILoader.GetUIState<ToolBrowser>().Refresh();
				UILoader.GetUIState<ToolBrowser>().initialized = true;
			}
		}
	}
}
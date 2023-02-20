using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using Terraria.ModLoader;

namespace DragonLens
{
	public class DragonLens : Mod
	{
		public override void PostSetupContent()
		{
			if (ModContent.GetInstance<ToolConfig>().preloadSpawners)
			{
				UILoader.GetUIState<ItemBrowser>().Refresh();
				UILoader.GetUIState<ProjectileBrowser>().Refresh();
				UILoader.GetUIState<NPCBrowser>().Refresh();
				UILoader.GetUIState<BuffBrowser>().Refresh();
				UILoader.GetUIState<ToolBrowser>().Refresh();
			}
		}
	}
}
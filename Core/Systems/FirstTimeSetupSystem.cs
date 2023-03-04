using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems
{
	internal class FirstTimeSetupSystem : ModSystem
	{
		public static bool trueFirstTime;

		public override void PostUpdateEverything()
		{
			if (trueFirstTime)
			{
				UILoader.GetUIState<LayoutPresetBrowser>().visible = true;

				if (!UILoader.GetUIState<LayoutPresetBrowser>().initialized)
				{
					UILoader.GetUIState<LayoutPresetBrowser>().Refresh();
					UILoader.GetUIState<LayoutPresetBrowser>().initialized = true;
				}
			}
		}
	}
}

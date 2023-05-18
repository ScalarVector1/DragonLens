using System.ComponentModel;
using Terraria.ModLoader.Config;
namespace DragonLens.Configs
{
	[Label("$Mods.DragonLens.ToolConfig.Title")]
	public class ToolConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("$Mods.DragonLens.ToolConfig.preloadSpawners.Label")]
		[Tooltip("$Mods.DragonLens.ToolConfig.preloadSpawners.Tooltip")]
		[DefaultValue(true)]
		public bool preloadSpawners;

		[Label("$Mods.DragonLens.ToolConfig.preloadAssets.Label")]
		[Tooltip("$Mods.DragonLens.ToolConfig.preloadAssets.Tooltip")]
		[DefaultValue(true)]
		public bool preloadAssets;
	}
}
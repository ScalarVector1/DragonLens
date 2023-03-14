using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DragonLens.Configs
{
	public class ToolConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Preload spawners")]
		[Tooltip("Load entries for spawners at mod load instead of when used. Prevents in-game freezes but may increase load time.")]
		[DefaultValue(true)]
		public bool preloadSpawners;

		[Label("Preload assets")]
		[Tooltip("Load assets for spawners at mod load instead of when used. Prevents in-game freezes but may increase load time.")]
		[DefaultValue(true)]
		public bool preloadAssets;
	}
}
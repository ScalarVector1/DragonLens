using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace DragonLens.Configs
{
	public class ToolConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;

		[Label("Godmode default")]
		[Tooltip("If godmode should be turned on by default at startup")]
		public bool defaultGodmode;

		[Label("Default spawnrate")]
		[Tooltip("Set the default enemy spawn rate")]
		[Range(0f, 10f)]
		[DefaultValue(1f)]
		public float defaultSpawnrate;

		[Label("Preload spawners")]
		[Tooltip("Load entries for spawners at mod load instead of when used. Prevents in-game freezes but may increase load time.")]
		[DefaultValue(true)]
		public bool preloadSpawners;
	}
}
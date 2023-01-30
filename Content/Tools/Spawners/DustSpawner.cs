using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Spawners
{
	internal class DustSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/DustSpawner";

		public override string DisplayName => "Dust spawner";

		public override string Description => "Spawn dust, with options to preview different spawning methods and parameters";

		public override void OnActivate()
		{
			Main.NewText("Implement me...");
		}
	}
}

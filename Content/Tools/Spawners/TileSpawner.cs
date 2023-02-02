using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Spawners
{
	internal class TileSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/TileSpawner";

		public override string DisplayName => "Tile spawner";

		public override string Description => "Place tiles without items! Spawns tile entities aswell";

		public override void OnActivate()
		{
			Main.NewText("Implement me...");
		}
	}
}

using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Spawners
{
	internal class ItemSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ItemSpawner";

		public override string Name => "Item spawner";

		public override string Description => "Spawn items, with options to customize their stats!";

		public override void OnActivate()
		{
			Main.NewText("Implement me...");
		}
	}
}

using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Spawners
{
	internal class BuffSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/BuffSpawner";

		public override string DisplayName => "Buff spawner";

		public override string Description => "Allows you to apply buffs to yourself or NPCs";

		public override void OnActivate()
		{
			Main.NewText("Implement me...");
		}
	}
}

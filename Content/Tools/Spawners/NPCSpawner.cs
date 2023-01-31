using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Spawners
{
	internal class NPCSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/NPCSpawner";

		public override string Name => "NPC spawner";

		public override string Description => "Spawn NPCs, from villagers to skeletons to bosses";

		public override void OnActivate()
		{
			Main.NewText("Implement me...");
		}
	}
}

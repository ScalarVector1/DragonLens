using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Despawners
{
	internal class NPCDespawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/NPCDespawner";

		public override string DisplayName => "Clear NPCs";

		public override string Description => "Removes all NPCs currently in the world. Right click for a more traditional 'butcher'";

		public override void OnActivate()
		{
			if (Main.mouseRight)
			{
				foreach (NPC npc in Main.npc)
				{
					npc.StrikeNPC(int.MaxValue, 0, 0);
				}
			}
			else
			{
				foreach (NPC npc in Main.npc)
				{
					npc.active = false;
				}
			}
		}
	}
}

using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Despawners
{
	internal class NPCDespawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/NPCDespawner";

		public override string DisplayName => "Clear NPCs";

		public override string Description => "Removes all hostile NPCs currently in the world. Right click for a more traditional 'butcher'. NEWBLOCK hold SHIFT to clear/butcher friendly NPCs aswell";

		public override bool HasRightClick => true;

		public override string RightClickName => "Traditional butcher";

		public override void OnActivate()
		{
			foreach (NPC npc in Main.npc)
			{
				if (!npc.friendly || Main.keyState.PressingShift())
					npc.active = false;
			}
		}

		public override void OnRightClick()
		{
			foreach (NPC npc in Main.npc)
			{
				if (!npc.friendly || Main.keyState.PressingShift())
					npc.StrikeNPC(int.MaxValue, 0, 0);
			}
		}
	}
}

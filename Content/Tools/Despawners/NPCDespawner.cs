using DragonLens.Core.Systems.ToolSystem;
using System.IO;
using Terraria.ID;
namespace DragonLens.Content.Tools.Despawners
{
	internal class NPCDespawner : Tool
	{
		private static int lastUsedClear = 0; //Used for net sync
		private static bool clearFriendly = false;

		public override string IconKey => "NPCDespawner";

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

			lastUsedClear = 0;
			clearFriendly = Main.keyState.PressingShift();
			NetSend();
		}

		public override void OnRightClick()
		{
			foreach (NPC npc in Main.npc)
			{
				if (!npc.friendly || Main.keyState.PressingShift())
					npc.StrikeNPC(new NPC.HitInfo() { Damage = int.MaxValue });
			}

			lastUsedClear = 1;
			clearFriendly = Main.keyState.PressingShift();
			NetSend();
		}

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(lastUsedClear);
			writer.Write(clearFriendly);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			lastUsedClear = reader.ReadInt32();
			clearFriendly = reader.ReadBoolean();

			if (lastUsedClear == 0)
			{
				foreach (NPC npc in Main.npc)
				{
					if (!npc.friendly || clearFriendly)
						npc.active = false;
				}
			}
			else
			{
				foreach (NPC npc in Main.npc)
				{
					if (!npc.friendly || clearFriendly)
						npc.StrikeNPC(new NPC.HitInfo() { Damage = int.MaxValue });
				}
			}

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}
	}
}
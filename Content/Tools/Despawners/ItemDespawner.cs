using DragonLens.Core.Systems.ToolSystem;
using System.IO;
using Terraria.ID;
namespace DragonLens.Content.Tools.Despawners
{
	internal class ItemDespawner : Tool
	{
		public override string IconKey => "ItemDespawner";

		public override void OnActivate()
		{
			foreach (Item item in Main.item)
			{
				item.active = false;
			}

			NetSend();
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			foreach (Item item in Main.item)
			{
				item.active = false;
			}

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}
	}
}
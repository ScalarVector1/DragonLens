using DragonLens.Core.Systems.ToolSystem;
using System.IO;
using Terraria.ID;

namespace DragonLens.Content.Tools.Despawners
{
	internal class ProjectileDespawner : Tool
	{
		public override string IconKey => "ProjectileDespawner";

		public override void OnActivate()
		{
			foreach (Projectile proj in Main.projectile)
			{
				proj.active = false;
			}
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			foreach (Projectile proj in Main.projectile)
			{
				proj.active = false;
			}

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}
	}
}
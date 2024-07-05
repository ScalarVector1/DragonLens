global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.ModLoader;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ToolSystem;
using System.IO;

namespace DragonLens
{
	public class DragonLens : Mod
	{
		public override void HandlePacket(BinaryReader reader, int whoAmI)
		{
			string type = reader.ReadString();

			if (type == "ToolPacket")
				ToolHandler.HandlePacket(reader, whoAmI);

			if (type == "AdminUpdate")
				PermissionHandler.HandlePacket(reader);

			if (type == "ToolDataRequest")
				PermissionHandler.SendToolData(whoAmI);
		}
	}
}
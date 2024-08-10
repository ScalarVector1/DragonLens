global using Microsoft.Xna.Framework;
global using Microsoft.Xna.Framework.Graphics;
global using Terraria;
global using Terraria.ModLoader;
global using ReLogic.Content;
using DragonLens.Configs;
using DragonLens.Content.Tools;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ToolSystem;
using ReLogic.Content;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Terraria.GameContent;
using Terraria.ID;
using DragonLens.Content.Tools.Multiplayer;

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
				PermissionHandler.HandlePacket(reader, whoAmI);

			if (type == "ToolDataRequest")
				PermissionHandler.SendToolData(whoAmI);

			if (type == "PlayerManager")
				PlayerManagerNetHandler.HandlePacket(reader, whoAmI);
		}
	}
}
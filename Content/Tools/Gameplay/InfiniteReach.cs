using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class InfiniteReach : Tool
	{
		public static bool active = false;

		public override string IconKey => "InfiniteReach";

		public override bool IsHighlighted => active;

		public override void ResetForNonAdmin(Player player)
		{
			active = false;
		}

		public override void OnActivate()
		{
			active = !active;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["active"] = active;
		}

		public override void LoadData(TagCompound tag)
		{
			active = tag.GetBool("active");
		}
	}

	internal class InfiniteReachPlayer : ModPlayer
	{
		public override void UpdateEquips()
		{
			if (InfiniteReach.active && PermissionHandler.CanUseTools(Player))
			{
				if (Main.SmartCursorWanted)
				{
					Main.SmartCursorWanted_Mouse = false;
					Main.SmartCursorWanted_GamePad = false;
					Main.NewText("Smart cursor is disabled with omnipotent building");
				}

				Player.tileRangeX = int.MaxValue / 32 - 20;
				Player.tileRangeY = int.MaxValue / 32 - 20;
			}
		}

		public override float UseTimeMultiplier(Item item)
		{
			if (InfiniteReach.active && PermissionHandler.CanUseTools(Player) && (item.createTile != -1 || item.createWall != -1 || item.pick > 0 || item.axe > 0 || item.hammer > 0))
				return 0.1f;

			return 1;
		}

		public override float UseAnimationMultiplier(Item item)
		{
			if (InfiniteReach.active && PermissionHandler.CanUseTools(Player) && (item.createTile != -1 || item.createWall != -1 || item.pick > 0 || item.axe > 0 || item.hammer > 0))
				return 0.1f;

			return 1;
		}
	}
}
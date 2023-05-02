using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Despawners
{
	internal class BuffDespawner : Tool
	{
		public override string IconKey => "BuffDespawner";

		public override string DisplayName => "Clear buffs";

		public override string Description => "Removes all buffs and debuffs from yourself";

		public override void OnActivate()
		{
			for (int k = 0; k < Player.MaxBuffs; k++)
			{
				Main.LocalPlayer.buffTime[k] = 0;
				Main.LocalPlayer.buffType[k] = 0;
			}
		}
	}
}
using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Despawners
{
	internal class DustDespawner : Tool
	{
		public override string IconKey => "DustDespawner";

		public override string DisplayName => "Clear dusts";

		public override string Description => "Removes all dust currently in the world";

		public override void OnActivate()
		{
			foreach (Dust dust in Main.dust)
			{
				dust.active = false;
			}
		}
	}
}

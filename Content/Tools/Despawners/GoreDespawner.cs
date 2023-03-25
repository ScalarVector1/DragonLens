using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Despawners
{
	internal class GoreDespawner : Tool
	{
		public override string IconKey => "GoreDespawner";

		public override string DisplayName => "Clear gores";

		public override string Description => "Removes all gores currently in the world";

		public override void OnActivate()
		{
			foreach (Gore gore in Main.gore)
			{
				gore.active = false;
			}
		}
	}
}
using DragonLens.Core.Systems.ToolSystem;
namespace DragonLens.Content.Tools.Despawners
{
	internal class DustDespawner : Tool
	{
		public override string IconKey => "DustDespawner";

		public override void OnActivate()
		{
			foreach (Dust dust in Main.dust)
			{
				dust.active = false;
			}
		}
	}
}
using DragonLens.Core.Systems.ToolSystem;
namespace DragonLens.Content.Tools.Despawners
{
	internal class GoreDespawner : Tool
	{
		public override string IconKey => "GoreDespawner";

		public override void OnActivate()
		{
			foreach (Gore gore in Main.gore)
			{
				gore.active = false;
			}
		}
	}
}
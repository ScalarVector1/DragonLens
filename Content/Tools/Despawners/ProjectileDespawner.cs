using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Despawners
{
	internal class ProjectileDespawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ProjectileDespawner";

		public override string DisplayName => "Clear projectiles";

		public override string Description => "Removes all projectiles currently in the world";

		public override void OnActivate()
		{
			foreach (Projectile proj in Main.projectile)
			{
				proj.active = false;
			}
		}
	}
}

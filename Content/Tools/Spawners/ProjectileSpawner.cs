using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools.Spawners
{
	internal class ProjectileSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ProjectileSpawner";

		public override string Name => "Projectile spawner";

		public override string Description => "Spawn projectiles, with options for setting velocity and other parameters";

		public override void OnActivate()
		{
			Main.NewText("Implement me...");
		}
	}
}

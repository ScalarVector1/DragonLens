using Terraria.ModLoader;

namespace DragonLens
{
	public class DragonLens : Mod
	{
		public static DragonLens instance;

		public override void Load()
		{
			instance = this;
		}
	}
}
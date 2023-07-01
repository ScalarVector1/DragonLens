using DragonLens.Core.Systems.ToolSystem;

namespace DragonLens.Content.Tools.Map
{
	internal class RevealMap : Tool
	{
		public override string IconKey => "RevealMap";

		public override void OnActivate()
		{
			for (int i = 0; i < Main.maxTilesX; i++)
			{
				for (int j = 0; j < Main.maxTilesY; j++)
				{
					if (WorldGen.InWorld(i, j))
						Main.Map.Update(i, j, 255);
				}
			}

			Main.refreshMap = true;
		}
	}
}
using DragonLens.Core.Systems.ToolSystem;
using System.Reflection;
using Terraria.Map;

namespace DragonLens.Content.Tools.Map
{
	internal class HideMap : Tool
	{
		public override string IconKey => "HideMap";

		private FieldInfo tilesArray;

		public override void Load()
		{
			base.Load();

			tilesArray = typeof(WorldMap).GetField("_tiles", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public override void OnActivate()
		{
			// Attempt to reinitialize the MapTile array through reflection. If,
			// for some reason, this fails, just clear the map (slower).
			if (tilesArray is not null)
				tilesArray.SetValue(Main.Map, new MapTile[Main.Map.MaxWidth, Main.Map.MaxHeight]);
			else
				Main.Map.Clear();

			Main.refreshMap = true;
			Main.clearMap = true;
		}
	}
}
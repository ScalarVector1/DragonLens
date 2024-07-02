using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Systems.ThemeSystem;

namespace DragonLens.Content.Filters.TileFilters
{
	internal class TileModFilter : Filter
	{
		public Mod mod;

		public TileModFilter(Mod mod) : base("", "", n => FilterByMod(n, mod))
		{
			this.mod = mod;
			isModFilter = true;
		}

		public override string Name => mod.DisplayName;

		public override string Description => TileSpawner.GetText("Filters.Mod.Description", mod.DisplayName);

		public static bool FilterByMod(BrowserButton button, Mod mod)
		{
			if (button is TileButton)
			{
				var tb = button as TileButton;

				if (ModContent.GetModTile(tb.tileType)?.Mod == mod)
					return false;
			}

			return true;
		}

		public override void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = null;

			string path = $"{mod.Name}/icon_small";

			if (mod.Name == "ModLoader")
				tex = Assets.Filters.tModLoader.Value;
			else if (ModContent.HasAsset(path))
				tex = ModContent.Request<Texture2D>(path).Value;

			if (tex != null)
			{
				int widest = tex.Width > tex.Height ? tex.Width : tex.Height;
				spriteBatch.Draw(tex, target.Center.ToVector2(), null, Color.White, 0, tex.Size() / 2f, target.Width / (float)widest, 0, 0);
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, mod.DisplayName[..2], target.Center.ToVector2(), Color.White, 1, 0.5f, 0.4f);
			}
		}
	}
}
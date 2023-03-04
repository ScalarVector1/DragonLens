using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Systems.ThemeSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace DragonLens.Content.Filters.DustFilters
{
	internal class DustModFilter : Filter
	{
		public Mod mod;

		public DustModFilter(Mod mod) : base("", mod.DisplayName, $"Dust added by the mod {mod.DisplayName}", n => FilterByMod(n, mod))
		{
			this.mod = mod;
		}

		public static bool FilterByMod(BrowserButton button, Mod mod)
		{
			if (button is DustButton)
			{
				var db = button as DustButton;

				if (db.dust.type > DustID.Count && DustLoader.GetDust(db.dust.type).Mod == mod)
					return false;
			}

			return true;
		}

		public override void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = null;

			string path = $"{mod.Name}/icon_small";

			if (mod.Name == "ModLoader")
				tex = ThemeHandler.GetIcon("Customize");
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

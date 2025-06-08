using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Developer;
using DragonLens.Content.Tools.Spawners;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DragonLens.Content.Filters.AssetFilters
{
	internal class AssetModFilter : Filter
	{
		public Mod mod;

		public AssetModFilter(Mod mod) : base(null, "", n => FilterByMod(n, mod))
		{
			this.mod = mod;
			isModFilter = true;
		}

		public override string Name => mod.DisplayName;

		public override string Description => $"Assets from {mod.DisplayName}";

		public static bool FilterByMod(BrowserButton button, Mod mod)
		{
			if (button is TextureAssetButton)
			{
				var ib = button as TextureAssetButton;

				if (ib.mod != null && ib.mod == mod)
					return false;
			}

			if (button is ShaderAssetButton)
			{
				var ib = button as ShaderAssetButton;

				if (ib.mod != null && ib.mod == mod)
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

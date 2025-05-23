using DragonLens.Content.GUI;
using DragonLens.Content.Tools;

namespace DragonLens.Content.Filters.ToolFilters
{
	internal class ToolModFilter : Filter
	{
		public Mod mod;

		public ToolModFilter(Mod mod) : base(null, "", n => FilterByMod(n, mod))
		{
			this.mod = mod;
			isModFilter = true;
		}

		public override string Name => mod.DisplayName;

		public override string Description => Helpers.LocalizationHelper.GetText("Tools.CustomizeTool.Filters.Mod.Description", mod.DisplayName);

		public static bool FilterByMod(BrowserButton button, Mod mod)
		{
			if (button is ToolBrowserButton)
			{
				var tb = button as ToolBrowserButton;

				if (tb.tool.Mod != null && tb.tool.Mod == mod)
					return false;
			}

			return true;
		}

		public override void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = null;

			string path = $"{mod.Name}/icon_small";

			//if (mod.Name == "DragonLens")
				//tex = ModContent.Request<Texture2D>(dlPath).Value;

			if (ModContent.HasAsset(path))
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

using DragonLens.Content.GUI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace DragonLens.Content.Filters
{
	internal delegate bool FilterDelegate(BrowserButton button);

	internal class Filter
	{
		public string texture;
		public string name;
		public string description;

		public FilterDelegate shouldFilter;

		public Filter(string texture, string name, string description, FilterDelegate shouldFilter)
		{
			this.texture = texture;
			this.name = name;
			this.description = description;
			this.shouldFilter = shouldFilter;
		}

		public virtual void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = ModContent.Request<Texture2D>(texture).Value;
			int widest = tex.Width > tex.Height ? tex.Width : tex.Height;

			spriteBatch.Draw(tex, target.Center.ToVector2(), null, Color.White, 0, tex.Size() / 2f, target.Width / (float)widest, 0, 0);
		}
	}
}

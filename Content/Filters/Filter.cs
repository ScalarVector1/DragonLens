using DragonLens.Content.GUI;
namespace DragonLens.Content.Filters
{
	internal delegate bool FilterDelegate(BrowserButton button);

	/// <summary>
	/// The logical backend for FilterButton elements. Defines the data for the element as well as the filtering logic.
	/// </summary>
	internal class Filter
	{
		/// <summary>
		/// The texture used to draw this filters icon if no custom drawing is defined.
		/// </summary>
		public string texture;

		/// <summary>
		/// The name of this filter, shown on the tooltip to the user when hovered over.
		/// </summary>
		public string name;

		/// <summary>
		/// The description of this filter, shown on the tooltip to the user when hovered over.
		/// </summary>
		public string description;

		/// <summary>
		/// Defines which elements should be filtered by this filter. Return true to filter out, false to leave in.
		/// </summary>
		public FilterDelegate shouldFilter;

		public Filter(string texture, string name, string description, FilterDelegate shouldFilter)
		{
			this.texture = texture;
			this.name = name;
			this.description = description;
			this.shouldFilter = shouldFilter;
		}

		/// <summary>
		/// Allows you to change how the filter's icon should draw
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target">The bounding box of the filter button</param>
		public virtual void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = ModContent.Request<Texture2D>(texture).Value;
			int widest = tex.Width > tex.Height ? tex.Width : tex.Height;

			spriteBatch.Draw(tex, target.Center.ToVector2(), null, Color.White, 0, tex.Size() / 2f, target.Width / (float)widest, 0, 0);
		}
	}
}
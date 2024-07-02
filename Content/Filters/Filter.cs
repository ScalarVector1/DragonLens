using DragonLens.Content.GUI;
using DragonLens.Helpers;
using ReLogic.Content;

namespace DragonLens.Content.Filters
{
	internal delegate bool FilterDelegate(BrowserButton button);

	/// <summary>
	/// The logical backend for FilterButton elements. Defines the data for the element as well as the filtering logic.
	/// </summary>
	internal class Filter
	{
		/// <summary>
		/// Whether this filter is a mod filter. You can only activate one mod filter an once.
		/// </summary>
		public bool isModFilter;

		/// <summary>
		/// The texture used to draw this filters icon if no custom drawing is defined.
		/// </summary>
		public Asset<Texture2D> texture;

		/// <summary>
		/// The localization key of this filter, used to get localized text for name and description
		/// </summary>
		public string localizationKey;

		/// <summary>
		/// The name of this filter, shown on the tooltip to the user when hovered over.
		/// </summary>
		public virtual string Name => LocalizationHelper.GetText($"{localizationKey}.Name");

		/// <summary>
		/// The description of this filter, shown on the tooltip to the user when hovered over.
		/// </summary>
		public virtual string Description => LocalizationHelper.GetText($"{localizationKey}.Description");

		/// <summary>
		/// Defines which elements should be filtered by this filter. Return true to filter out, false to leave in.
		/// </summary>
		public FilterDelegate shouldFilter;

		public Filter(Asset<Texture2D> texture, string localizationKey, FilterDelegate shouldFilter)
		{
			this.texture = texture;
			this.localizationKey = localizationKey;
			this.shouldFilter = shouldFilter;
		}

		/// <summary>
		/// Allows you to change how the filter's icon should draw
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target">The bounding box of the filter button</param>
		public virtual void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = texture.Value;
			int widest = tex.Width > tex.Height ? tex.Width : tex.Height;

			spriteBatch.Draw(tex, target.Center.ToVector2(), null, Color.White, 0, tex.Size() / 2f, target.Width / (float)widest, 0, 0);
		}
	}
}
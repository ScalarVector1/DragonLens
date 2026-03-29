namespace DragonLens.Content.Filters.PlayerManagerFilters
{

	/// <summary>
	/// Draw filter with a custom spritesheet
	/// Used for team icon filters
	/// </summary>
	internal class TeamFilter : Filter
	{
		public Rectangle sourceRect;
		public Point drawSize;
		public Color color = Color.White;

		public TeamFilter(Asset<Texture2D> texture, string localizationKey, FilterDelegate shouldFilter, Rectangle sourceRect, Point drawSize)
			: base(texture, localizationKey, shouldFilter)
		{
			this.sourceRect = sourceRect;
			this.drawSize = drawSize;
		}

		public override void Draw(SpriteBatch spriteBatch, Rectangle target)
		{
			Rectangle drawRect = new(
				target.X + (target.Width - drawSize.X) / 2,
				target.Y + (target.Height - drawSize.Y) / 2,
				drawSize.X,
				drawSize.Y
			);

			spriteBatch.Draw(texture.Value, drawRect, sourceRect, color);
		}
	}
}
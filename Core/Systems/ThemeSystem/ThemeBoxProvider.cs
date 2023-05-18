namespace DragonLens.Core.Systems.ThemeSystem
{
	/// <summary>
	/// A class which provides the methods used to render boxes for the GUI.
	/// </summary>
	public abstract class ThemeBoxProvider
	{
		/// <summary>
		/// The name of this box provider
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// The description for the box provider
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// Draws a simple box. Used for most buttons and smaller backgrounds.
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target"></param>
		/// <param name="color"></param>
		public abstract void DrawBox(SpriteBatch spriteBatch, Rectangle target, Color color);

		/// <summary>
		/// Draws a smaller box, used by tiny things like slider ticks.
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target"></param>
		/// <param name="color"></param>
		public abstract void DrawBoxSmall(SpriteBatch spriteBatch, Rectangle target, Color color);

		/// <summary>
		/// Draws a 'fancy' box. Used by things like popout UI backgrounds.
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target"></param>
		/// <param name="color"></param>
		public abstract void DrawBoxFancy(SpriteBatch spriteBatch, Rectangle target, Color color);

		/// <summary>
		/// Draws the outline of a box. Used for things like placement previews.
		/// </summary>
		/// <param name="spriteBatch"></param>
		/// <param name="target"></param>
		/// <param name="color"></param>
		public abstract void DrawOutline(SpriteBatch spriteBatch, Rectangle target, Color color);
	}
}
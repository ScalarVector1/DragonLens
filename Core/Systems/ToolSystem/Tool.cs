using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace DragonLens.Core.Systems.ToolSystem
{
	/// <summary>
	/// A singleton type which can be extended to add new tools. Tools function primarily as a 'kick-off-point' for other GUIs or state changes.
	/// </summary>
	internal abstract class Tool : ModTexturedType
	{
		/// <summary>
		/// A path to the texture of the icon used for this tool
		/// </summary>
		public abstract override string Texture { get; }

		/// <summary>
		/// The display name of the tool to the end user
		/// </summary>
		public abstract string DisplayName { get; }

		/// <summary>
		/// The description that should show up when queried for more information about this tool
		/// </summary>
		public abstract string Description { get; }

		protected sealed override void Register()
		{
			ModTypeLookup<Tool>.Register(this);
			ToolLoader.AddTool(this);
		}

		public sealed override void SetupContent()
		{
			SetStaticDefaults();
		}

		/// <summary>
		/// What happens when the user activates this tool, either by clicking on it or using it's hotkey
		/// </summary>
		public abstract void OnActivate();

		/// <summary>
		/// Allows you to load or initialize other things when this tool loads.
		/// </summary>
		public virtual void Load() { }

		/// <summary>
		/// Draws this tool's icon at a given position. Can be overridden to change how it draws or add effects like opacity based on state.
		/// </summary>
		/// <param name="spriteBatch">Spritebatch used to draw the icon</param>
		/// <param name="position">Where the icon should be drawn on the UI</param>
		public virtual void DrawIcon(SpriteBatch spriteBatch, Vector2 position)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			spriteBatch.Draw(tex, position, Color.White);
		}
	}
}

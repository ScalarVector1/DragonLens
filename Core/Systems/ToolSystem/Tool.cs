using DragonLens.Core.Systems.ThemeSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ToolSystem
{
	/// <summary>
	/// A singleton type which can be extended to add new tools. Tools function primarily as a 'kick-off-point' for other GUIs or state changes.
	/// </summary>
	internal abstract class Tool : ModType
	{
		/// <summary>
		/// The hotkey keybind for this tool
		/// </summary>
		public ModKeybind keybind;

		/// <summary>
		/// The hotkey used for the right click function of this tool
		/// </summary>
		public ModKeybind altKeybind;

		/// <summary>
		/// The icon key to retrieve the icon for this tool
		/// </summary>
		public abstract string IconKey { get; }

		/// <summary>
		/// The display name of the tool to the end user
		/// </summary>
		public abstract string DisplayName { get; }

		/// <summary>
		/// The description that should show up when queried for more information about this tool
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// What happens when the user activates this tool, either by clicking on it or using it's hotkey
		/// </summary>
		public abstract void OnActivate();

		/// <summary>
		/// If this tool has functionality on right click.
		/// </summary>
		public virtual bool HasRightClick => false;

		/// <summary>
		/// The name of this tools right click funcitonality. Used for hotkeys.
		/// </summary>
		public virtual string RightClickName => "";

		/// <summary>
		/// What happens if this tool is right clicked. Only used if HasRightClick is true.
		/// </summary>
		public virtual void OnRightClick() { }

		public virtual void SaveData(TagCompound tag) { }

		public virtual void LoadData(TagCompound tag) { }

		protected sealed override void Register()
		{
			ModTypeLookup<Tool>.Register(this);
			ToolHandler.AddTool(this);

			keybind = KeybindLoader.RegisterKeybind(Mod, DisplayName, Keys.None);

			if (HasRightClick)
				altKeybind = KeybindLoader.RegisterKeybind(Mod, RightClickName, Keys.None);
		}

		/// <summary>
		/// Draws this tool's icon at a given position. Can be overridden to change how it draws or add effects like opacity based on state.
		/// </summary>
		/// <param name="spriteBatch">Spritebatch used to draw the icon</param>
		/// <param name="position">Where the icon should be drawn on the UI</param>
		public virtual void DrawIcon(SpriteBatch spriteBatch, Rectangle target)
		{
			Texture2D tex = ThemeHandler.GetIcon(IconKey);
			float scale = 1;

			if (tex.Width > target.Width || tex.Height > target.Height)
				scale = tex.Width > tex.Height ? target.Width / tex.Width : target.Height / tex.Height;

			spriteBatch.Draw(tex, target.Center(), null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);
		}
	}
}

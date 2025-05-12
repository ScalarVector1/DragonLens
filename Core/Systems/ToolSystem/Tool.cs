using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework.Input;
using System.IO;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;

namespace DragonLens.Core.Systems.ToolSystem
{
	/// <summary>
	/// A singleton type which can be extended to add new tools. Tools function primarily as a 'kick-off-point' for other GUIs or state changes.
	/// </summary>
	public abstract class Tool : ModType
	{
		/// <summary>
		/// The hotkey keybind for this tool.
		/// </summary>
		public ModKeybind keybind;

		/// <summary>
		/// The hotkey used for the right click function of this tool.
		/// </summary>
		public ModKeybind altKeybind;

		/// <summary>
		/// The icon key to retrieve the icon for this tool.
		/// </summary>
		public abstract string IconKey { get; }

		/// <summary>
		/// The localization key to retrieve the localized text for this tool.
		/// </summary>
		public virtual string LocalizationKey => Name;

		/// <summary>
		/// The display name of the tool to the end user.
		/// Auto-assigned to a localization key if not overridden.
		/// </summary>
		public virtual string DisplayName => Language.GetText($"Mods.{Mod.Name}.Tools.{LocalizationKey}.DisplayName").Value;

		/// <summary>
		/// The description that should show up when queried for more information about this tool.
		/// Auto-assigned to a localization key if not overridden.
		/// </summary>
		public virtual string Description => Language.GetText($"Mods.{Mod.Name}.Tools.{LocalizationKey}.Description").Value;

		/// <summary>
		/// What happens when the user activates this tool, either by clicking on it or using it's hotkey.
		/// </summary>
		public abstract void OnActivate();

		/// <summary>
		/// If this tool has functionality on right click.
		/// </summary>
		public virtual bool HasRightClick => false;

		/// <summary>
		/// The name of this tools right click funcitonality. Used for hotkeys.
		/// Auto-assigned to a localization key if not overridden.
		/// </summary>
		public virtual string RightClickName => Language.GetText($"Mods.{Mod.Name}.Tools.{LocalizationKey}.RightClickName").Value;

		/// <summary>
		/// What happens if this tool is right clicked. Only used if HasRightClick is true.
		/// </summary>
		public virtual void OnRightClick() { }

		/// <summary>
		/// Allows you to save persistent data for this tool.
		/// </summary>
		/// <param name="tag"></param>
		public virtual void SaveData(TagCompound tag) { }

		/// <summary>
		/// Allows you to load the persistent data for this tool, if that data exists.
		/// </summary>
		/// <param name="tag"></param>
		public virtual void LoadData(TagCompound tag) { }

		/// <summary>
		/// Allows you to define the data to be sent with this tools packet. Every tool gets its own packet that can be easily sent by calling NetSend.
		/// </summary>
		/// <param name="writer"></param>
		public virtual void SendPacket(BinaryWriter writer) { }

		/// <summary>
		/// Allows you to define the behavior for recieving this tools packet. This should unpack the data you send in SendPacket.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="sender"></param>
		public virtual void RecievePacket(BinaryReader reader, int sender) { }

		/// <summary>
		/// This should be called when your tool needs to send its packet.
		/// </summary>
		public void NetSend(int toClient = -1, int ignoreClient = -1)
		{
			if (Main.netMode == NetmodeID.SinglePlayer) //single player dosent care about packets
				return;

			if (Main.netMode == NetmodeID.Server)
				Mod.Logger.Info($"Sending packet for tool {DisplayName} ({Name}) from server");

			if (Main.netMode == NetmodeID.MultiplayerClient)
				Mod.Logger.Info($"Sending packet for tool {DisplayName} ({Name}) from {Main.LocalPlayer.whoAmI}");

			ModPacket packet = Mod.GetPacket();
			packet.Write("ToolPacket");
			packet.Write(Name);
			SendPacket(packet);

			packet.Send(toClient, ignoreClient);
		}

		protected sealed override void Register()
		{
			ModTypeLookup<Tool>.Register(this);

			keybind = KeybindLoader.RegisterKeybind(Mod, LocalizationKey, Keys.None);
			Language.GetOrRegister($"Mods.{Mod.Name}.Tools.{LocalizationKey}.DisplayName", () => Name);
			Language.GetOrRegister($"Mods.{Mod.Name}.Tools.{LocalizationKey}.Description", () => "The tool has no description! Add one in .hjson files!");

			if (HasRightClick)
			{
				altKeybind = KeybindLoader.RegisterKeybind(Mod, $"{LocalizationKey}RightClick", Keys.None);
				Language.GetOrRegister($"Mods.{Mod.Name}.Tools.{LocalizationKey}.RightClickName", () => $"{Name} Right Click");
			}
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
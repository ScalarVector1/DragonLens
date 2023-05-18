using DragonLens.Core.Systems;
using DragonLens.Core.Systems.ThemeSystem;
using ReLogic.Graphics;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using Terraria.ModLoader.Config;
using Terraria.UI;
namespace DragonLens.Helpers
{
	internal static class GUIHelper
	{
		/// <summary>
		/// Draws a simple box in the style of the DragonLens GUI.
		/// </summary>
		/// <param name="sb">the spriteBatch to draw the box with</param>
		/// <param name="target">where/how big the box should be drawn</param>
		/// <param name="color"><the color of the box/param>
		public static void DrawBox(SpriteBatch sb, Rectangle target, Color color = default)
		{
			if (ThemeHandler.currentBoxProvider is null)
			{
				FirstTimeSetupSystem.PanicToSetup();
				return;
			}

			if (target.Width < 16 || target.Height < 16)
				ThemeHandler.currentBoxProvider.DrawBoxSmall(sb, target, color);

			ThemeHandler.currentBoxProvider.DrawBox(sb, target, color);
		}

		/// <summary>
		/// Draws a fancy box in the style of the DragonLens GUI.
		/// </summary>
		/// <param name="sb">the spriteBatch to draw the box with</param>
		/// <param name="target">where/how big the box should be drawn</param>
		/// <param name="color"><the color of the box/param>
		public static void DrawBoxFancy(SpriteBatch sb, Rectangle target, Color color = default)
		{
			if (ThemeHandler.currentBoxProvider is null)
			{
				FirstTimeSetupSystem.PanicToSetup();
				return;
			}

			ThemeHandler.currentBoxProvider.DrawBoxFancy(sb, target, color);
		}

		/// <summary>
		/// Draws the outline of a box in the style of the DragonLens GUI.
		/// </summary>
		/// <param name="sb">the spriteBatch to draw the outline with</param>
		/// <param name="target">where/how big the outline should be drawn</param>
		/// <param name="color">the color of the outline</param>
		public static void DrawOutline(SpriteBatch sb, Rectangle target, Color color = default)
		{
			if (ThemeHandler.currentBoxProvider is null)
			{
				FirstTimeSetupSystem.PanicToSetup();
				return;
			}

			ThemeHandler.currentBoxProvider.DrawOutline(sb, target, color);
		}

		/// <summary>
		/// Gets the inverse of a color. Used for 'constrasting' elements of the DragonLens GUI, such as outlines indicating something being on.
		/// </summary>
		/// <param name="color">the color to invert</param>
		/// <returns>the inverted color</returns>
		public static Color InvertColor(this Color color)
		{
			return new Color(255 - color.R, 255 - color.G, 255 - color.B, color.A);
		}

		/// <summary>
		/// Wraps a string to a given maximum width, by forcibly adding newlines. Normal newlines will be removed, put the text 'NEWBLOCK' in your string to break a paragraph if needed.
		/// </summary>
		/// <param name="input">The input string to be wrapped</param>
		/// <param name="length">The maximum width of the text</param>
		/// <param name="font">The font the text will be drawn in, to calculate its size</param>
		/// <param name="scale">The scale the text will be drawn at, to calculate its size</param>
		/// <returns>Input text with linebreaks inserted so it obeys the width constraint.</returns>
		public static string WrapString(string input, int length, DynamicSpriteFont font, float scale)
		{
			string output = "";
			string[] words = input.Split();

			string line = "";
			foreach (string str in words)
			{
				if (str == "NEWBLOCK")
				{
					output += "\n\n";
					line = "";
					continue;
				}

				if (str == "NEWLN")
				{
					output += "\n";
					line = "";
					continue;
				}

				if (font.MeasureString(line).X * scale < length)
				{
					output += " " + str;
					line += " " + str;
				}
				else
				{
					output += "\n" + str;
					line = str;
				}
			}

			return output[1..];
		}

		/// <summary>
		/// Uses reflection to forcibly open the TModLoader configuration UI to a given ModConfig's screen.
		/// </summary>
		/// <param name="config">The config to open up</param>
		public static void OpenConfig(ModConfig config)
		{
			IngameFancyUI.CoverNextFrame();
			Main.playerInventory = false;
			Main.editChest = false;
			Main.npcChatText = "";
			Main.inFancyUI = true;

			Type interfaceType = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.UI.Interface");
			FieldInfo modConfigList = interfaceType.GetField("modConfigList", BindingFlags.Static | BindingFlags.NonPublic);

			Type uiModConfig = typeof(ModLoader).Assembly.GetType("Terraria.ModLoader.Config.UI.UIModConfig");
			FieldInfo modConfig = interfaceType.GetField("modConfig", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo setMod = uiModConfig.GetMethod("SetMod", BindingFlags.Instance | BindingFlags.NonPublic);

			var ui = (UIState)modConfig.GetValue(null);

			setMod.Invoke(ui, new object[] { ModContent.GetInstance<DragonLens>(), config });
			Main.InGameUI.SetState(ui);
		}

		/// <summary>
		/// Opens a URL in the web browser
		/// </summary>
		/// <param name="url">The URL to open to</param>
		public static void OpenUrl(string url)
		{
			try
			{
				Process.Start(url);
			}
			catch
			{
				if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
				{
					url = url.Replace("&", "^&");
					Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
				{
					Process.Start("xdg-open", url);
				}
				else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
				{
					Process.Start("open", url);
				}
				else
				{
					throw;
				}
			}
		}
	}
}
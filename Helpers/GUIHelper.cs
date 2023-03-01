using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
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
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Box").Value;

			if (color == default)
				color = new Color(49, 84, 141) * 0.9f;

			var sourceCorner = new Rectangle(0, 0, 6, 6);
			var sourceEdge = new Rectangle(6, 0, 4, 6);
			var sourceCenter = new Rectangle(6, 6, 4, 4);

			Rectangle inner = target;
			inner.Inflate(-4, -4);

			sb.Draw(tex, inner, sourceCenter, color);

			sb.Draw(tex, new Rectangle(target.X + 2, target.Y, target.Width - 4, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y - 2 + target.Height, target.Height - 4, 6), sourceEdge, color, -(float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X - 2 + target.Width, target.Y + target.Height, target.Width - 4, 6), sourceEdge, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 2, target.Height - 4, 6), sourceEdge, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);

			sb.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI * 1.5f, Vector2.Zero, 0, 0);
		}

		/// <summary>
		/// Draws the outline of a box in the style of the DragonLens GUI.
		/// </summary>
		/// <param name="sb">the spriteBatch to draw the outline with</param>
		/// <param name="target">where/how big the outline should be drawn</param>
		/// <param name="color">the color of the outline</param>
		public static void DrawOutline(SpriteBatch sb, Rectangle target, Color color = default)
		{
			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Box").Value;

			if (color == default)
				color = new Color(49, 84, 141) * 0.9f;

			var sourceCorner = new Rectangle(0, 0, 6, 6);
			var sourceEdge = new Rectangle(6, 0, 4, 6);
			var sourceCenter = new Rectangle(6, 6, 4, 4);

			Rectangle inner = target;
			inner.Inflate(-4, -4);

			sb.Draw(tex, new Rectangle(target.X + 2, target.Y, target.Width - 4, 6), sourceEdge, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y - 2 + target.Height, target.Height - 4, 6), sourceEdge, color, -(float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X - 2 + target.Width, target.Y + target.Height, target.Width - 4, 6), sourceEdge, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + 2, target.Height - 4, 6), sourceEdge, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);

			sb.Draw(tex, new Rectangle(target.X, target.Y, 6, 6), sourceCorner, color, 0, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y, 6, 6), sourceCorner, color, (float)Math.PI * 0.5f, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X + target.Width, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI, Vector2.Zero, 0, 0);
			sb.Draw(tex, new Rectangle(target.X, target.Y + target.Height, 6, 6), sourceCorner, color, (float)Math.PI * 1.5f, Vector2.Zero, 0, 0);
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
			FieldInfo modConfigForUiModConfig = uiModConfig.GetField("modConfig", BindingFlags.Instance | BindingFlags.NonPublic);
			MethodInfo setMod = uiModConfig.GetMethod("SetMod", BindingFlags.Instance | BindingFlags.NonPublic);

			var ui = (UIState)modConfig.GetValue(null);

			modConfigForUiModConfig.SetValue(ui, config);

			Main.InGameUI.SetState(ui);
			setMod.Invoke(ui, new object[] { ModContent.GetInstance<DragonLens>(), config });
		}
	}
}

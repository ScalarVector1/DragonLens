using DragonLens.Configs;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Helpers;
using System.Text.RegularExpressions;
using Terraria.GameInput;
using Terraria.UI;

namespace DragonLens.Content.GUI.FieldEditors
{
	public enum InputType
	{
		text,
		integer,
		number
	}

	internal class TextField : SmartUIElement
	{
		public bool typing;
		public bool updated;
		public InputType inputType;

		public string currentValue = "";

		public TextField(InputType inputType = InputType.text)
		{
			this.inputType = inputType;
			Width.Set(130, 0);
			Height.Set(24, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			typing = true;
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			typing = true;
			currentValue = "";
			updated = true;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (updated)
				updated = false;

			if (Main.mouseLeft && !IsMouseHovering)
				typing = false;
		}

		public void HandleText()
		{
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				typing = false;

			PlayerInput.WritingText = true;
			Main.instance.HandleIME();

			string newText = Main.GetInputText(currentValue);

			if (inputType == InputType.integer && Regex.IsMatch(newText, "[0-9]*$"))
			{
				if (newText != currentValue)
				{
					currentValue = newText;
					updated = true;
				}
			}
			else if (inputType == InputType.number && Regex.IsMatch(newText, "(?<=^| )[0-9]+(.[0-9]+)?(?=$| )|(?<=^| ).[0-9]+(?=$| )")) //I found this regex on SO so no idea if it works right lol
			{
				if (newText != currentValue)
				{
					currentValue = newText;
					updated = true;
				}
			}
			else
			{
				if (newText != currentValue)
				{
					currentValue = newText;
					updated = true;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			if (typing)
			{
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());
				HandleText();
			}

			Vector2 pos = GetDimensions().Position() + Vector2.One * 4;

			string displayed = currentValue;

			if (typing && Main.GameUpdateCount % 20 < 10)
				displayed += "|";

			Utils.DrawBorderString(spriteBatch, displayed, pos, Color.White, 0.75f);
		}
	}
}
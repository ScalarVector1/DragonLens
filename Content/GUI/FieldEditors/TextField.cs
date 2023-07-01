using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using ReLogic.Localization.IME;
using ReLogic.OS;
using System.Text.RegularExpressions;
using Terraria.GameContent;
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
		public bool reset;
		public InputType inputType;

		public string currentValue = "";

		// Composition string is handled at the very beginning of the update
		// In order to check if there is a composition string before backspace is typed, we need to check the previous state
		private bool _oldHasCompositionString;

		public TextField(InputType inputType = InputType.text)
		{
			this.inputType = inputType;
			Width.Set(130, 0);
			Height.Set(24, 0);
		}

		public void SetTyping()
		{
			typing = true;
			Main.blockInput = true;
		}
		
		public void SetNotTyping()
		{
			typing = false;
			Main.blockInput = false;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			SetTyping();
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			SetTyping();
			currentValue = "";
			updated = true;
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (reset)
			{
				updated = false;
				reset = false;
			}

			if (updated)
				reset = true;

			if (Main.mouseLeft && !IsMouseHovering)
				SetNotTyping();
		}

		public void HandleText()
		{
			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
				SetNotTyping();

			PlayerInput.WritingText = true;
			Main.instance.HandleIME();

			string newText = Main.GetInputText(currentValue);
			
			// GetInputText() handles typing operation, but there is a issue that it doesn't handle backspace correctly when the composition string is not empty. It will delete a character both in the text and the composition string instead of only the one in composition string. We'll fix the issue here to provide a better user experience
			if (_oldHasCompositionString && Main.inputText.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Back))
				newText = currentValue; // force text not to be changed

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

			_oldHasCompositionString = Platform.Get<IImeService>().CompositionString is {Length: > 0};
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			if (typing)
			{
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor.InvertColor());
				HandleText();

				// draw ime panel, note that if there's no composition string then it won't draw anything
				Main.instance.DrawWindowsIMEPanel(GetDimensions().Position());
			}

			Vector2 pos = GetDimensions().Position() + Vector2.One * 4;

			const float scale = 0.75f;
			string displayed = currentValue;

			Utils.DrawBorderString(spriteBatch, displayed, pos, Color.White, scale);

			// composition string + cursor drawing below
			if (!typing)
				return;

			pos.X += FontAssets.MouseText.Value.MeasureString(displayed).X * scale;
			string compositionString = Platform.Get<IImeService>().CompositionString;

			if (compositionString is {Length: > 0})
			{
				Utils.DrawBorderString(spriteBatch, compositionString, pos, new Color(255, 240, 20), scale);
				pos.X += FontAssets.MouseText.Value.MeasureString(compositionString).X * scale;
			}

			if (Main.GameUpdateCount % 20 < 10)
				Utils.DrawBorderString(spriteBatch, "|", pos, Color.White, scale);
		}
	}
}
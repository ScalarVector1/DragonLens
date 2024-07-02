using DragonLens.Content.GUI;
using DragonLens.Helpers;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class SoundSpawner : BrowserTool<SoundBrowser>
	{
		public override string IconKey => "SoundSpawner";

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.SoundSpawner.{key}", args);
		}
	}

	internal class SoundBrowser : Browser
	{
		public override string Name => SoundSpawner.GetText("DisplayName");

		public override string IconTexture => "SoundSpawner";

		public override Vector2 DefaultPosition => new(0.5f, 0.4f);

		public override void PostInitialize()
		{
			listMode = true;
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<SoundButton>();

			System.Reflection.FieldInfo[] fields = typeof(SoundID).GetFields();

			for (int k = 0; k < fields.Length; k++)
			{
				object obj = fields[k].GetValue(null);

				if (obj is SoundStyle sound)
				{
					if (sound != default)
						buttons.Add(new SoundButton(sound, fields[k].Name, this));
				}
			}

			grid.AddRange(buttons);
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			base.DraggableUdpate(gameTime);

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll($"DragonLens: {Name}");
		}
	}

	internal class SoundButton : BrowserButton
	{
		public SoundStyle Sound;

		public string name;

		public override string Identifier => name;

		public SoundButton(SoundStyle Sound, string name, Browser browser) : base(browser)
		{
			this.Sound = Sound;
			this.name = name;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Texture2D tex = Assets.Misc.Sound.Value;

			spriteBatch.Draw(tex, iconBox.Center(), null, Color.White, 0, tex.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, name[..2], iconBox.TopLeft() + Vector2.One * 6, Color.Gray);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			SoundEngine.PlaySound(Sound);
		}

		public override int CompareTo(object obj)
		{
			return name.CompareTo((obj as SoundButton).name);
		}
	}
}
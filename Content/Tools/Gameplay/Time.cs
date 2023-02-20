using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Time : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/Time";

		public override string DisplayName => "Time tool";

		public override string Description => "Adjust or pause the day/night cycle";

		public override void OnActivate()
		{
			TimeWindow state = UILoader.GetUIState<TimeWindow>();
			state.visible = !state.visible;
		}
	}

	internal class TimePauseSystem : ModSystem
	{
		public static int savedTime = -1;

		public override void PreUpdateTime()
		{
			if (savedTime != -1)
				Main.time = savedTime;
		}
	}

	internal class TimeWindow : DraggableUIState
	{
		public TimeSlider slider;
		public TimePauseButton pause;
		public MoonPhaseButton[] moonButtons;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 32);

		public override Vector2 DefaultPosition => new(0.5f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 400;
			height = 200;

			slider = new TimeSlider();
			Append(slider);

			pause = new TimePauseButton();
			Append(pause);

			moonButtons = new MoonPhaseButton[8];

			for (int k = 0; k < 8; k++)
			{
				moonButtons[k] = new MoonPhaseButton(k);
				Append(moonButtons[k]);
			}
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			slider.Left.Set(basePos.X + 25, 0);
			slider.Top.Set(basePos.Y + 65, 0);

			pause.Left.Set(basePos.X + 340, 0);
			pause.Top.Set(basePos.Y + 52, 0);

			for (int k = 0; k < 8; k++)
			{
				moonButtons[k].Left.Set(basePos.X + 18 + k * 46, 0);
				moonButtons[k].Top.Set(basePos.Y + 138, 0);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, 400, 200), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ModContent.Request<Texture2D>("DragonLens/Assets/Tools/Time").Value;
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, "Set time", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}

	internal class TimeSlider : UIElement
	{
		public bool dragging;
		public float progress;

		public TimeSlider()
		{
			Width.Set(300, 0);
			Height.Set(16, 0);
		}

		public override void Update(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);

				if (progress < 0.5f)
				{
					Main.dayTime = true;
					Main.time = progress * 2 * Main.dayLength;
				}
				else
				{
					Main.dayTime = false;
					Main.time = (progress - 0.5f) * 2 * Main.nightLength;
				}

				if (TimePauseSystem.savedTime != -1) //updates time even while paused
					TimePauseSystem.savedTime = (int)Main.time;

				if (!Main.mouseLeft)
					dragging = false;
			}
			else
			{
				if (Main.dayTime)
					progress = (float)(Main.time / Main.dayLength * 0.5f);
				else
					progress = (float)(0.5f + Main.time / Main.nightLength / 2);
			}

			base.Update(gameTime);
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public string GetTimeString()
		{
			string AmPm = Language.GetTextValue("GameUI.TimeAtMorning");
			double time = Main.time;
			if (!Main.dayTime)
				time += 54000.0;

			time = time / 86400.0 * 24.0;
			double timeSubtractor = 7.5;
			time = time - timeSubtractor - 12.0;
			if (time < 0.0)
				time += 24.0;

			if (time >= 12.0)
				AmPm = Language.GetTextValue("GameUI.TimePastMorning");

			int hoursString = (int)time;
			double secondRemainder = time - hoursString;
			secondRemainder = (int)(secondRemainder * 60.0);
			string minutesString = secondRemainder.ToString() ?? "";
			if (secondRemainder < 10.0)
				minutesString = "0" + minutesString;

			if (hoursString > 12)
				hoursString -= 12;

			if (hoursString == 0)
				hoursString = 12;

			return Language.GetTextValue("CLI.Time", hoursString + ":" + minutesString + " " + AmPm);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/TimeScale").Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 6, dims.Y - 8, 12, 24);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ModContent.GetInstance<GUIConfig>().buttonColor);

			string dayString = Main.dayTime ? "Day" : "Night";
			int maxTicks = Main.dayTime ? (int)Main.dayLength : (int)Main.nightLength;

			Utils.DrawBorderString(spriteBatch, GetTimeString(), dims.TopLeft() + new Vector2(0, 20), Color.White, 0.8f);
			Utils.DrawBorderString(spriteBatch, $"({dayString}: {(int)Main.time}/{maxTicks} frames)", dims.TopLeft() + new Vector2(0, 36), Color.White, 0.8f);
		}
	}

	internal class TimePauseButton : UIElement
	{
		public TimePauseButton()
		{
			Width.Set(42, 0);
			Height.Set(42, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D icon = TimePauseSystem.savedTime == -1 ?
				ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Pause").Value :
				ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Play").Value;

			spriteBatch.Draw(icon, dims.TopLeft() + Vector2.One * 5, Color.White);
		}

		public override void Click(UIMouseEvent evt)
		{
			if (TimePauseSystem.savedTime == -1)
				TimePauseSystem.savedTime = (int)Main.time;
			else
				TimePauseSystem.savedTime = -1;
		}
	}

	internal class MoonPhaseButton : UIElement
	{
		readonly int moonPhase;

		public MoonPhaseButton(int moonPhase)
		{
			Width.Set(42, 0);
			Height.Set(42, 0);

			this.moonPhase = moonPhase;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			if (Main.moonPhase == moonPhase)
				GUIHelper.DrawOutline(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

			Texture2D icon = Terraria.GameContent.TextureAssets.Moon[Main.moonType].Value;

			var source = new Rectangle(0, moonPhase * 50, 50, 50);

			spriteBatch.Draw(icon, dims.Center(), source, Color.White, 0, Vector2.One * 25, 0.65f, 0, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			Main.moonPhase = moonPhase;
		}
	}
}
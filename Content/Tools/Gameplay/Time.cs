using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Time : Tool
	{
		public override string IconKey => "Time";

		public override void OnActivate()
		{
			TimeWindow state = UILoader.GetUIState<TimeWindow>();
			state.visible = !state.visible;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["savedTime"] = TimePauseSystem.savedTime;
			tag["savedDay"] = TimePauseSystem.savedDay;
		}

		public override void LoadData(TagCompound tag)
		{
			TimePauseSystem.savedTime = tag.GetInt("savedTime");
			TimePauseSystem.savedDay = tag.GetBool("savedDay");
		}

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(Main.time);
			writer.Write(Main.dayTime);
			writer.Write(TimePauseSystem.savedTime);
			writer.Write(TimePauseSystem.savedDay);
			writer.Write(Main.moonPhase);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			Main.time = reader.ReadDouble(); 
			Main.dayTime = reader.ReadBoolean();
			TimePauseSystem.savedTime = reader.ReadInt32();
			TimePauseSystem.savedDay = reader.ReadBoolean();
			Main.moonPhase = reader.ReadInt32();

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}

		public override void ResetForNonAdmin(Player player)
		{
			TimePauseSystem.savedTime = -1;
			TimePauseSystem.savedDay = false;
		}
	}

	internal class TimePauseSystem : ModSystem
	{
		public static int savedTime = -1;
		public static bool savedDay = false;

		public override void PreUpdateTime()
		{
			if (savedTime != -1)
			{
				Main.time = savedTime;
				Main.dayTime = savedDay;
			}
		}
	}

	internal class TimeWindow : DraggableUIState
	{
		public TimeSlider slider;
		public TimePauseButton pause;
		public MoonPhaseButton[] moonButtons;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 54);

		public override Vector2 DefaultPosition => new(0.5f, 0.5f);

		public override string HelpLink => "https://github.com/ScalarVector1/DragonLens/wiki/Time-tool";

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
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, 400, 200), ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("Time");
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetToolText("Time.UITitle"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}

	internal class TimeSlider : SmartUIElement
	{
		public bool dragging;
		public float progress;

		public TimeSlider()
		{
			Width.Set(300, 0);
			Height.Set(16, 0);
		}

		public override void SafeUpdate(GameTime gameTime)
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
				{
					TimePauseSystem.savedDay = Main.dayTime;
					TimePauseSystem.savedTime = (int)Main.time;
				}

				if (!Main.mouseLeft)
				{
					dragging = false;
					ToolHandler.NetSend<Time>();
				}
			}
			else
			{
				if (Main.dayTime)
					progress = (float)(Main.time / Main.dayLength * 0.5f);
				else
					progress = (float)(0.5f + Main.time / Main.nightLength / 2);
			}
		}

		public override void SafeMouseDown(UIMouseEvent evt)
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
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Texture2D tex = Assets.GUI.TimeScale.Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 6, dims.Y - 8, 12, 24);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ThemeHandler.ButtonColor);

			string dayString = LocalizationHelper.GetText($"Tools.Time.{(Main.dayTime ? "Day" : "Night")}");
			int maxTicks = Main.dayTime ? (int)Main.dayLength : (int)Main.nightLength;
			string curTimeString = LocalizationHelper.GetToolText("Time.CurrentTime", dayString, (int)Main.time, maxTicks);

			Utils.DrawBorderString(spriteBatch, GetTimeString(), dims.TopLeft() + new Vector2(0, 20), Color.White, 0.8f);
			Utils.DrawBorderString(spriteBatch, curTimeString, dims.TopLeft() + new Vector2(0, 36), Color.White, 0.8f);
		}
	}

	internal class TimePauseButton : SmartUIElement
	{
		public TimePauseButton()
		{
			Width.Set(42, 0);
			Height.Set(42, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Texture2D icon = TimePauseSystem.savedTime == -1 ?
				Assets.GUI.Pause.Value :
				Assets.GUI.Play.Value;

			spriteBatch.Draw(icon, dims.TopLeft() + Vector2.One * 5, Color.White);

			if (IsMouseHovering)
			{
				string name = LocalizationHelper.GetText($"Tools.Time.{(TimePauseSystem.savedTime == -1 ? "Freeze" : "Resume")}");
				Tooltip.SetName(name);
				Tooltip.SetTooltip(LocalizationHelper.GetToolText("Time.FreezeTooltip"));
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (TimePauseSystem.savedTime == -1)
			{
				TimePauseSystem.savedTime = (int)Main.time;
				TimePauseSystem.savedDay = Main.dayTime;
			}
			else
			{
				TimePauseSystem.savedTime = -1;
			}

			ToolHandler.NetSend<Time>();
		}
	}

	internal class MoonPhaseButton : SmartUIElement
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
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			if (Main.moonPhase == moonPhase)
				GUIHelper.DrawOutline(spriteBatch, dims, ThemeHandler.ButtonColor.InvertColor());

			Texture2D icon = Terraria.GameContent.TextureAssets.Moon[Main.moonType].Value;

			var source = new Rectangle(0, moonPhase * 50, 50, 50);

			spriteBatch.Draw(icon, dims.Center(), source, Color.White, 0, Vector2.One * 25, 0.65f, 0, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Main.moonPhase = moonPhase;

			ToolHandler.NetSend<Time>();
		}
	}
}
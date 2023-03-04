using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Weather : Tool
	{
		public override string IconKey => "Weather";

		public override string DisplayName => "Weather tool";

		public override string Description => "Adjust the weather";

		public override void OnActivate()
		{
			WeatherWindow state = UILoader.GetUIState<WeatherWindow>();
			state.visible = !state.visible;
		}
	}

	internal class WeatherWindow : DraggableUIState
	{
		public CloudSlider cloudSlider;
		public WindSlider windSlider;

		public RainButton rainButton;
		public SandstormButton sandstormButton;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 32);

		public override Vector2 DefaultPosition => new(0.7f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			cloudSlider = new();
			Append(cloudSlider);

			windSlider = new();
			Append(windSlider);

			rainButton = new();
			Append(rainButton);

			sandstormButton = new();
			Append(sandstormButton);

			width = 400;
			height = 170;
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			cloudSlider.Left.Set(basePos.X + 25, 0);
			cloudSlider.Top.Set(basePos.Y + 70, 0);

			windSlider.Left.Set(basePos.X + 25, 0);
			windSlider.Top.Set(basePos.Y + 126, 0);

			rainButton.Left.Set(basePos.X + 340, 0);
			rainButton.Top.Set(basePos.Y + 57, 0);

			sandstormButton.Left.Set(basePos.X + 340, 0);
			sandstormButton.Top.Set(basePos.Y + 111, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, 400, 170), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("Weather");
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, "Set weather", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y + 180, 400, 80), ModContent.GetInstance<GUIConfig>().backgroundColor);

			string tips = "Useful combinations:\n  High Rain + Wind over 20 mph = Thunderstorm\n  Natural Rain + Snow biome = Blizzard";
			Utils.DrawBorderString(spriteBatch, tips, basePos + new Vector2(24, 190), Color.White, 0.8f);

			base.Draw(spriteBatch);
		}
	}

	internal class CloudSlider : UIElement
	{
		public bool dragging;
		public float progress;

		public CloudSlider()
		{
			Width.Set(300, 0);
			Height.Set(16, 0);
		}

		public override void Update(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);

				Main.cloudAlpha = progress;
				Main.maxRaining = progress;

				if (!Main.mouseLeft)
					dragging = false;
			}
			else
			{
				progress = Main.cloudAlpha;
			}

			base.Update(gameTime);
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/CloudScale").Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var tickTarget = new Rectangle(dims.X + 60 - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.Yellow);

			tickTarget = new Rectangle(dims.X + 180 - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.Orange);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 6, dims.Y - 8, 12, 24);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ModContent.GetInstance<GUIConfig>().buttonColor);

			Utils.DrawBorderString(spriteBatch, "Rain intensity", dims.TopLeft() + new Vector2(0, -22), Color.White, 0.8f);

			Utils.DrawBorderString(spriteBatch, "low", dims.TopLeft() + new Vector2(30, 14), Color.White, 0.7f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "med", dims.TopLeft() + new Vector2(120, 14), Color.White, 0.7f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "high", dims.TopLeft() + new Vector2(240, 14), Color.White, 0.7f, 0.5f);
		}
	}

	internal class WindSlider : UIElement
	{
		public bool dragging;
		public float progress;

		public WindSlider()
		{
			Width.Set(300, 0);
			Height.Set(16, 0);
		}

		public override void Update(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);

				Main.windSpeedCurrent = (progress - 0.5f) * 2.4f;
				Main.windSpeedTarget = (progress - 0.5f) * 2.4f;

				if (!Main.mouseLeft)
					dragging = false;
			}
			else
			{
				progress = (Main.windSpeedCurrent + 1.2f) / 2.4f;
			}

			base.Update(gameTime);
		}

		public override void MouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/WindScale").Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 6, dims.Y - 8, 12, 24);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ModContent.GetInstance<GUIConfig>().buttonColor);

			var tickTarget = new Rectangle(dims.X + (int)(4 / 6f * dims.Width) - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.Red);

			tickTarget = new Rectangle(dims.X + (int)(2 / 6f * dims.Width) - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.Red);

			Utils.DrawBorderString(spriteBatch, "Wind strength", dims.TopLeft() + new Vector2(0, -22), Color.White, 0.8f);

			Utils.DrawBorderString(spriteBatch, "60w", dims.TopLeft() + new Vector2(10, 14), Color.White, 0.7f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "60e", dims.TopLeft() + new Vector2(290, 14), Color.White, 0.7f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "20w", dims.TopLeft() + new Vector2(100, 14), Color.White, 0.7f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "20e", dims.TopLeft() + new Vector2(200, 14), Color.White, 0.7f, 0.5f);
		}
	}

	internal class RainButton : UIElement
	{
		public RainButton()
		{
			Width.Set(42, 0);
			Height.Set(42, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D icon = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Rain").Value;

			if (Main.raining)
				GUIHelper.DrawOutline(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

			if (IsMouseHovering)
			{
				Tooltip.SetName("Rain");
				Tooltip.SetTooltip("Enables or disables natural rain. Note that you may still see residual rain during the 'fade-out' of a natural rain event.");
			}

			spriteBatch.Draw(icon, dims.Center.ToVector2(), null, Color.White, 0, icon.Size() / 2, 1, 0, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			if (!Main.raining)
			{
				Main.StartRain();
			}
			else
			{
				Main.raining = false;
			}
		}
	}

	internal class SandstormButton : UIElement
	{
		public SandstormButton()
		{
			Width.Set(42, 0);
			Height.Set(42, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D icon = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Sandstorm").Value;

			if (Sandstorm.Happening)
				GUIHelper.DrawOutline(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());

			if (IsMouseHovering)
			{
				Tooltip.SetName("Sandstorm");
				Tooltip.SetTooltip("Start or stop a sandstorm. Only visible in the desert.");
			}

			spriteBatch.Draw(icon, dims.Center.ToVector2(), null, Color.White, 0, icon.Size() / 2, 1, 0, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			if (!Sandstorm.Happening)
				Sandstorm.StartSandstorm();
			else
				Sandstorm.Happening = false;
		}
	}
}
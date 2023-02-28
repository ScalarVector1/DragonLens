using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace DragonLens.Content.Tools.Visualization
{
	internal class Hitboxes : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/Hitboxes";

		public override string DisplayName => "Hitbox visualizer";

		public override string Description => "Shows the hitboxes of various entities";

		public override void OnActivate()
		{
			HitboxWindow state = UILoader.GetUIState<HitboxWindow>();
			state.visible = !state.visible;
		}
	}

	internal class HitboxSystem : ModSystem
	{
		public override void Load()
		{
			On.Terraria.Main.DrawInterface += DrawHitboxes;
		}

		private void DrawHitboxes(On.Terraria.Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			HitboxWindow state = UILoader.GetUIState<HitboxWindow>();

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			state.NPCOption.DrawBoxes(Main.spriteBatch);
			state.ProjectileOption.DrawBoxes(Main.spriteBatch);
			state.PlayerOption.DrawBoxes(Main.spriteBatch);
			state.ItemOption.DrawBoxes(Main.spriteBatch);

			Main.spriteBatch.End();

			orig(self, gameTime);
		}
	}

	internal class HitboxWindow : DraggableUIState
	{
		public HitboxOption NPCOption;
		public HitboxOption ProjectileOption;
		public HitboxOption PlayerOption;
		public HitboxOption ItemOption;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 220, 32);

		public override Vector2 DefaultPosition => new(0.7f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 264;
			height = 260;

			NPCOption = new HitboxOption(() =>
			{
				var list = new List<Rectangle>();

				foreach (NPC npc in Main.npc)
				{
					if (npc.active)
					{
						Rectangle box = npc.Hitbox;
						box.Offset((-Main.screenPosition).ToPoint());
						list.Add(box);
					}
				}

				return list;
			}, "NPC Hitboxes", 0);
			Append(NPCOption);

			ProjectileOption = new HitboxOption(() =>
			{
				var list = new List<Rectangle>();

				foreach (Projectile proj in Main.projectile)
				{
					if (proj.active)
					{
						Rectangle box = proj.Hitbox;
						box.Offset((-Main.screenPosition).ToPoint());
						list.Add(box);
					}
				}

				return list;
			}, "Projectile Hitboxes", 0.1f);
			Append(ProjectileOption);

			PlayerOption = new HitboxOption(() =>
			{
				var list = new List<Rectangle>();

				foreach (Player player in Main.player)
				{
					if (player.active)
					{
						Rectangle box = player.Hitbox;
						box.Offset((-Main.screenPosition).ToPoint());
						list.Add(box);
					}
				}

				return list;
			}, "Player Hitboxes", 0.5f);
			Append(PlayerOption);

			ItemOption = new HitboxOption(() =>
			{
				var list = new List<Rectangle>();

				foreach (Item item in Main.item)
				{
					if (item.active)
					{
						Rectangle box = item.Hitbox;
						box.Offset((-Main.screenPosition).ToPoint());
						list.Add(box);
					}
				}

				return list;
			}, "Item Hitboxes", 0.6f);
			Append(ItemOption);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			NPCOption.Left.Set(basePos.X + 8, 0);
			NPCOption.Top.Set(basePos.Y + 56, 0);

			ProjectileOption.Left.Set(basePos.X + 134, 0);
			ProjectileOption.Top.Set(basePos.Y + 56, 0);

			PlayerOption.Left.Set(basePos.X + 8, 0);
			PlayerOption.Top.Set(basePos.Y + 150, 0);

			ItemOption.Left.Set(basePos.X + 134, 0);
			ItemOption.Top.Set(basePos.Y + 150, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, width, height), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, width, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ModContent.Request<Texture2D>("DragonLens/Assets/Tools/Hitboxes").Value;
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, "Hitboxes", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}

	internal class HitboxOption : UIElement
	{
		public enum BoxType
		{
			none,
			outline,
			filled
		}

		public ColorSlider slider;

		public BoxType boxState;
		public Func<List<Rectangle>> getBoxes;
		public string text;

		public Color BoxColor => slider != null ? slider.Color : Color.Red;

		public HitboxOption(Func<List<Rectangle>> getBoxes, string text, float defaultColor)
		{
			this.getBoxes = getBoxes;
			this.text = text;

			Width.Set(122, 0);
			Height.Set(90, 0);

			slider = new(defaultColor);
			slider.Left.Set(11, 0);
			slider.Top.Set(64, 0);
			Append(slider);

			var button = new ToggleButton("DragonLens/Assets/GUI/NoBox", () => boxState == BoxType.none);
			button.Left.Set(10, 0);
			button.Top.Set(28, 0);
			button.OnClick += (a, b) => boxState = BoxType.none;
			Append(button);

			button = new ToggleButton("DragonLens/Assets/GUI/BorderBox", () => boxState == BoxType.outline);
			button.Left.Set(46, 0);
			button.Top.Set(28, 0);
			button.OnClick += (a, b) => boxState = BoxType.outline;
			Append(button);

			button = new ToggleButton("DragonLens/Assets/GUI/FillBox", () => boxState == BoxType.filled);
			button.Left.Set(82, 0);
			button.Top.Set(28, 0);
			button.OnClick += (a, b) => boxState = BoxType.filled;
			Append(button);
		}

		public void DrawBoxes(SpriteBatch sb)
		{
			List<Rectangle> boxes = getBoxes();

			boxes.ForEach(n =>
			{
				switch (boxState)
				{
					case BoxType.outline:
						GUIHelper.DrawOutline(sb, n, BoxColor);
						break;

					case BoxType.filled:
						Texture2D tex = Terraria.GameContent.TextureAssets.MagicPixel.Value;
						sb.Draw(tex, n, BoxColor * 0.5f);
						break;
				}
			});
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();

			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);
			Utils.DrawBorderString(spriteBatch, text, new Vector2(dims.Center.X, dims.Y + 8), Color.White, 0.75f, 0.5f);

			base.Draw(spriteBatch);
		}
	}

	internal class ColorSlider : UIElement
	{
		public bool dragging;
		public float progress;

		public Color Color => Main.hslToRgb(new Vector3(progress, 1, 0.5f));

		public ColorSlider(float defaultValue = 0)
		{
			Width.Set(100, 0);
			Height.Set(16, 0);

			progress = defaultValue;
		}

		public override void Update(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);

				if (!Main.mouseLeft)
					dragging = false;
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

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/ColorScale").Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ModContent.GetInstance<GUIConfig>().buttonColor);
		}
	}
}
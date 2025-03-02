using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace DragonLens.Content.Tools.Visualization
{
	internal class Hitboxes : Tool
	{
		public override string IconKey => "Hitboxes";

		public override void OnActivate()
		{
			HitboxWindow state = UILoader.GetUIState<HitboxWindow>();
			state.visible = !state.visible;
		}

		public override void SaveData(TagCompound tag)
		{
			var npcTag = new TagCompound();
			UILoader.GetUIState<HitboxWindow>().NPCOption.Save(npcTag);
			tag["npcTag"] = npcTag;

			var projTag = new TagCompound();
			UILoader.GetUIState<HitboxWindow>().ProjectileOption.Save(projTag);
			tag["projTag"] = projTag;

			var playerTag = new TagCompound();
			UILoader.GetUIState<HitboxWindow>().PlayerOption.Save(playerTag);
			tag["playerTag"] = playerTag;

			var itemTag = new TagCompound();
			UILoader.GetUIState<HitboxWindow>().ItemOption.Save(itemTag);
			tag["itemTag"] = itemTag;
		}

		public override void LoadData(TagCompound tag)
		{
			TagCompound npcTag = tag.Get<TagCompound>("npcTag");
			if (npcTag != null)
				UILoader.GetUIState<HitboxWindow>().NPCOption.Load(npcTag);

			TagCompound projTag = tag.Get<TagCompound>("projTag");
			if (projTag != null)
				UILoader.GetUIState<HitboxWindow>().ProjectileOption.Load(projTag);

			TagCompound playerTag = tag.Get<TagCompound>("playerTag");
			if (playerTag != null)
				UILoader.GetUIState<HitboxWindow>().PlayerOption.Load(playerTag);

			TagCompound itemTag = tag.Get<TagCompound>("itemTag");
			if (itemTag != null)
				UILoader.GetUIState<HitboxWindow>().ItemOption.Load(itemTag);
		}
	}

	internal class HitboxSystem : ModSystem
	{
		public override void Load()
		{
			Terraria.On_Main.DrawInterface += DrawHitboxes;
		}

		private void DrawHitboxes(Terraria.On_Main.orig_DrawInterface orig, Main self, GameTime gameTime)
		{
			HitboxWindow state = UILoader.GetUIState<HitboxWindow>();

			Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

			state.NPCOption.DrawBoxes(Main.spriteBatch);
			state.ProjectileOption.DrawBoxes(Main.spriteBatch);
			state.PlayerOption.DrawBoxes(Main.spriteBatch);
			state.ItemOption.DrawBoxes(Main.spriteBatch);
			state.MeleeOption.DrawBoxes(Main.spriteBatch);
			state.TileEntityOption.DrawBoxes(Main.spriteBatch);

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
		public HitboxOption MeleeOption;
		public HitboxOption TileEntityOption;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 220, 32);

		public override Vector2 DefaultPosition => new(0.7f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 264;
			height = 350;

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
			}, "NPC", 0);
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
			}, "Projectile", 0.1f);
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
			}, "Player", 0.5f);
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
			}, "Item", 0.6f);
			Append(ItemOption);

			MeleeOption = new HitboxOption(() =>
			{
				var list = new List<Rectangle>();

				foreach (Player player in Main.player)
				{
					if (player.active)
					{
						var mp = player.GetModPlayer<MeleeHitboxTracker>();

						if (mp.fresh)
						{
							Rectangle box = mp.lastMeleeBox;
							box.Offset((-Main.screenPosition).ToPoint());
							list.Add(box);

							mp.fresh = false;
						}
					}
				}

				return list;
			}, "Melee", 0.15f);
			Append(MeleeOption);

			TileEntityOption = new HitboxOption(() =>
			{
				var list = new List<Rectangle>();

				foreach (KeyValuePair<int, TileEntity> pair in TileEntity.ByID)
				{
					var te = TileEntity.ByID[pair.Key];

					Rectangle box = new Rectangle(te.Position.X * 16, te.Position.Y * 16, 16, 16);
					box.Offset((-Main.screenPosition).ToPoint());
					list.Add(box);				
				}

				return list;
			}, "TileEntity", 0.85f);
			Append(TileEntityOption);
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

			MeleeOption.Left.Set(basePos.X + 8, 0);
			MeleeOption.Top.Set(basePos.Y + 244, 0);

			TileEntityOption.Left.Set(basePos.X + 134, 0);
			TileEntityOption.Top.Set(basePos.Y + 244, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, width, height), ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, width, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("Hitboxes");
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetToolText("Hitboxes.DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}

	internal class HitboxOption : SmartUIElement
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
		public string localizationKey;

		public Color BoxColor => slider != null ? slider.Color : Color.Red;

		public HitboxOption(Func<List<Rectangle>> getBoxes, string localizationKey, float defaultColor)
		{
			this.getBoxes = getBoxes;
			this.localizationKey = localizationKey;

			Width.Set(122, 0);
			Height.Set(90, 0);

			slider = new(defaultColor);
			slider.Left.Set(11, 0);
			slider.Top.Set(64, 0);
			Append(slider);

			var button = new ToggleButton("DragonLens/Assets/GUI/NoBox", () => boxState == BoxType.none);
			button.Left.Set(10, 0);
			button.Top.Set(28, 0);
			button.OnLeftClick += (a, b) => boxState = BoxType.none;
			Append(button);

			button = new ToggleButton("DragonLens/Assets/GUI/BorderBox", () => boxState == BoxType.outline);
			button.Left.Set(46, 0);
			button.Top.Set(28, 0);
			button.OnLeftClick += (a, b) => boxState = BoxType.outline;
			Append(button);

			button = new ToggleButton("DragonLens/Assets/GUI/FillBox", () => boxState == BoxType.filled);
			button.Left.Set(82, 0);
			button.Top.Set(28, 0);
			button.OnLeftClick += (a, b) => boxState = BoxType.filled;
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

			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);
			Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetText($"Tools.Hitboxes.Options.{localizationKey}"), new Vector2(dims.Center.X, dims.Y + 8), Color.White, 0.75f, 0.5f);

			base.Draw(spriteBatch);
		}

		public void Save(TagCompound tag)
		{
			tag["color"] = slider.progress;
			tag["setting"] = (int)boxState;
		}

		public void Load(TagCompound tag)
		{
			slider.progress = tag.GetFloat("color");
			boxState = (BoxType)tag.GetInt("setting");
		}
	}

	internal class ColorSlider : SmartUIElement
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

		public override void SafeUpdate(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);

				if (!Main.mouseLeft)
					dragging = false;
			}
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Texture2D tex = Assets.GUI.ColorScale.Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ThemeHandler.ButtonColor);
		}
	}

	internal class MeleeHitboxTracker : ModPlayer
	{
		public Rectangle lastMeleeBox;
		public bool fresh;

		public override void Load()
		{
			On_Player.ItemCheck_MeleeHitNPCs += StealHitbox;
		}

		private void StealHitbox(On_Player.orig_ItemCheck_MeleeHitNPCs orig, Player self, Item sItem, Rectangle itemRectangle, int originalDamage, float knockBack)
		{
			self.GetModPlayer<MeleeHitboxTracker>().lastMeleeBox = itemRectangle;
			self.GetModPlayer<MeleeHitboxTracker>().fresh = true;
			orig(self, sItem, itemRectangle, originalDamage, knockBack);
		}
	}
}
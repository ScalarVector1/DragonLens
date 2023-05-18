using DragonLens.Content.GUI;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
namespace DragonLens.Content.Tools.Editors
{
	internal class PlayerEditorTool : Tool
	{
		public override string IconKey => "PlayerEditor";

		public override string LocalizationKey => "PlayerEditor";

		public override void OnActivate()
		{
			PlayerEditorState state = UILoader.GetUIState<PlayerEditorState>();
			state.visible = !state.visible;

			//We re-initialize because the UserInterface isnt set when loaded so the scroll bars poop out
			state.RemoveAllChildren();
			state.OnInitialize();

			state.player = Main.LocalPlayer;
			state.SetupNewPlayer();
		}
	}

	internal class PlayerEditorPlayer : ModPlayer
	{
		public int minionBoost = 0;
		public int lifeBoost = 0;
		public int manaBoost = 0;
		public int defenseBoost = 0;
		public float enduranceBoost = 0;
		public float speedBoost = 0;

		public override void PostUpdateEquips()
		{
			Player.maxMinions += minionBoost;
			Player.statLifeMax2 += lifeBoost;
			Player.statManaMax2 += manaBoost;
			Player.statDefense += defenseBoost;
			Player.endurance += enduranceBoost;
			Player.moveSpeed += speedBoost;
		}
	}

	internal class PlayerEditorState : DraggableUIState
	{
		public Player player = Main.LocalPlayer;

		public UIGrid basicEditorList;
		public UIGrid modPlayerEditorList;

		public FixedUIScrollbar basicEditorScroll;
		public FixedUIScrollbar modPlayerEditorScroll;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 844, 32);

		public override Vector2 DefaultPosition => new(0.4f, 0.4f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 844;
			height = 648;

			basicEditorScroll = new(UserInterface);
			basicEditorScroll.Height.Set(540, 0);
			basicEditorScroll.Width.Set(16, 0);
			Append(basicEditorScroll);

			basicEditorList = new();
			basicEditorList.Width.Set(320, 0);
			basicEditorList.Height.Set(540, 0);
			basicEditorList.SetScrollbar(basicEditorScroll);
			Append(basicEditorList);

			modPlayerEditorScroll = new(UserInterface);
			modPlayerEditorScroll.Height.Set(540, 0);
			modPlayerEditorScroll.Width.Set(16, 0);
			Append(modPlayerEditorScroll);

			modPlayerEditorList = new();
			modPlayerEditorList.Width.Set(480, 0);
			modPlayerEditorList.Height.Set(540, 0);
			modPlayerEditorList.SetScrollbar(modPlayerEditorScroll);
			modPlayerEditorList.ListPadding = 16;
			Append(modPlayerEditorList);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			basicEditorList.Left.Set(newPos.X + 10, 0);
			basicEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			basicEditorScroll.Left.Set(newPos.X + 320, 0);
			basicEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);

			modPlayerEditorList.Left.Set(newPos.X + 342, 0);
			modPlayerEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			modPlayerEditorScroll.Left.Set(newPos.X + 480 + 338, 0);
			modPlayerEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);
		}

		public void SetupNewPlayer()
		{
			BuildBasicEditor();
			BuildModPlayerEditor();
		}

		private void BuildBasicEditor()
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetText($"Tools.PlayerEditor.Editors.{text}");
			}

			basicEditorList.Add(new IntEditor(GetLocalizedText("MaxLife.Name"), n => player.statLifeMax = n, player.statLifeMax, () => player.statLifeMax, GetLocalizedText("MaxLife.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("MaxMana.Name"), n => player.statManaMax = n, player.statManaMax, () => player.statManaMax, GetLocalizedText("MaxMana.Description")));

			PlayerEditorPlayer mp = player.GetModPlayer<PlayerEditorPlayer>();

			basicEditorList.Add(new IntEditor(GetLocalizedText("ExtraLife.Name"), n => mp.lifeBoost = n, mp.lifeBoost, () => mp.lifeBoost, GetLocalizedText("ExtraLife.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("ExtraMana.Name"), n => mp.manaBoost = n, mp.manaBoost, () => mp.manaBoost, GetLocalizedText("ExtraMana.Description")));

			basicEditorList.Add(new IntEditor(GetLocalizedText("ExtraDefense.Name"), n => mp.defenseBoost = n, mp.defenseBoost, () => mp.defenseBoost, GetLocalizedText("ExtraDefense.Description")));
			basicEditorList.Add(new FloatEditor(GetLocalizedText("ExtraEndurance.Name"), n => mp.enduranceBoost = n, mp.enduranceBoost, () => mp.enduranceBoost, GetLocalizedText("ExtraEndurance.Description")));

			basicEditorList.Add(new IntEditor(GetLocalizedText("ExtraMinionSlots.Name"), n => mp.minionBoost = n, mp.minionBoost, () => mp.minionBoost, GetLocalizedText("ExtraMinionSlots.Description")));
			basicEditorList.Add(new FloatEditor(GetLocalizedText("ExtraSpeed.Name"), n => mp.speedBoost = n, mp.speedBoost, () => mp.speedBoost, GetLocalizedText("ExtraSpeed.Description")));
		}

		private void BuildModPlayerEditor()
		{
			foreach (ModPlayer mp in player.ModPlayers)
			{
				if (mp is PlayerEditorPlayer) //this is our own special thing we want in the other box!
					continue;

				var newContainer = new ModPlayerContainer(mp);

				Main.NewText(mp.Name + ": " + newContainer.modPlayerEditorList.Count);

				if (newContainer.modPlayerEditorList.Count > 1)
					modPlayerEditorList.Add(newContainer);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetText($"Tools.PlayerEditor.{text}");
			}

			Helpers.GUIHelper.DrawBox(spriteBatch, BoundingBox, ThemeHandler.BackgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("PlayerEditor");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, GetLocalizedText("DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			Vector2 pos = basePos;
			Utils.DrawBorderString(spriteBatch, GetLocalizedText("VanillaFields"), pos + new Vector2(120, 80), Color.White, 1, 0f, 0.5f);
			Utils.DrawBorderString(spriteBatch, GetLocalizedText("ModPlayers"), pos + new Vector2(320 + 220, 80), Color.White, 1, 0f, 0.5f);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;

			spriteBatch.Draw(background, basicEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);
			spriteBatch.Draw(background, modPlayerEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);

			base.Draw(spriteBatch);
		}
	}

	internal class ModPlayerSeperator : SmartUIElement
	{
		readonly string message;

		public ModPlayerSeperator(string message)
		{
			this.message = message;
			Width.Set(480, 0);
			Height.Set(32, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = GetDimensions().ToRectangle();
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, message, GetDimensions().ToRectangle().TopLeft() + Vector2.One * 4, Color.White, 0.8f);
		}
	}

	internal class ModPlayerContainer : SmartUIElement
	{
		public UIGrid modPlayerEditorList;

		private float count = 0;
		private float nextHeight = 0;
		private float height = 32;

		public ModPlayerContainer(ModPlayer mp)
		{
			modPlayerEditorList = new();
			modPlayerEditorList.Add(new ModPlayerSeperator(mp.Mod.DisplayName + ": " + mp.Name));

			if (mp != null)
			{
				//TODO: some sort of GetEditor generic or something so we dont have to do... this
				foreach (FieldInfo t in mp.GetType().GetFields())
				{
					TryAddEditor<bool, BoolEditor>(t, mp);
					TryAddEditor<int, IntEditor>(t, mp);
					TryAddEditor<float, FloatEditor>(t, mp);
					TryAddEditor<Vector2, Vector2Editor>(t, mp);
					TryAddEditor<Color, ColorEditor>(t, mp);
					TryAddEditor<string, StringEditor>(t, mp);
					TryAddEditor<NPC, NPCEditor>(t, mp);
					TryAddEditor<Projectile, ProjectileEditor>(t, mp);
					TryAddEditor<Player, PlayerEditor>(t, mp);
				}

				foreach (PropertyInfo t in mp.GetType().GetProperties().Where(n => n.SetMethod != null))
				{
					if (t.Name == "Entity")
						continue;

					TryAddEditor<bool, BoolEditor>(t, mp);
					TryAddEditor<int, IntEditor>(t, mp);
					TryAddEditor<float, FloatEditor>(t, mp);
					TryAddEditor<Vector2, Vector2Editor>(t, mp);
					TryAddEditor<Color, ColorEditor>(t, mp);
					TryAddEditor<string, StringEditor>(t, mp);
					TryAddEditor<NPC, NPCEditor>(t, mp);
					TryAddEditor<Projectile, ProjectileEditor>(t, mp);
					TryAddEditor<Player, PlayerEditor>(t, mp);
				}
			}

			height += nextHeight + modPlayerEditorList.ListPadding;
			nextHeight = 0;
			count = 0;

			Width.Set(480, 0);
			Height.Set(height, 0);

			modPlayerEditorList.Width.Set(480, 0);
			modPlayerEditorList.Height.Set(height, 0);

			Append(modPlayerEditorList);
		}

		private void TryAddEditor<T, E>(FieldInfo t, ModPlayer p) where E : FieldEditor<T>
		{
			if (t.FieldType == typeof(T))
			{
				string message = LocalizationHelper.GetToolText("PlayerEditor.AutogenMsg");

				var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(p, n)), (T)t.GetValue(p), () => (T)t.GetValue(p), message });
				modPlayerEditorList.Add(newEditor);

				if (newEditor.Height.Pixels > nextHeight)
					nextHeight = newEditor.Height.Pixels;

				count++;

				if (count >= 3)
				{
					height += nextHeight + modPlayerEditorList.ListPadding;
					nextHeight = 0;
					count = 0;
				}
			}
		}

		private void TryAddEditor<T, E>(PropertyInfo t, ModPlayer p) where E : FieldEditor<T>
		{
			if (t.PropertyType == typeof(T))
			{
				string message = LocalizationHelper.GetToolText("PlayerEditor.AutogenMsg");

				var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(p, n)), (T)t.GetValue(p), () => (T)t.GetValue(p), message });
				modPlayerEditorList.Add(newEditor);

				if (newEditor.Height.Pixels > nextHeight)
					nextHeight = newEditor.Height.Pixels;

				count++;

				if (count >= 3)
				{
					height += nextHeight + modPlayerEditorList.ListPadding;
					nextHeight = 0;
					count = 0;
				}
			}
		}
	}
}
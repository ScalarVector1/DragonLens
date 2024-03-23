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
using Terraria.GameInput;
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
		public bool wingBoost = false;

		public override void PostUpdateEquips()
		{
			Player.maxMinions += minionBoost;
			Player.statLifeMax2 += lifeBoost;
			Player.statManaMax2 += manaBoost;
			Player.statDefense += defenseBoost;
			Player.endurance += enduranceBoost;
			Player.moveSpeed += speedBoost;
			if (wingBoost)
				Player.wingTime = Player.wingTimeMax;
		}
	}

	internal class PlayerEditorState : DraggableUIState
	{
		public Player player = Main.LocalPlayer;

		public UIGrid basicEditorList;
		public FieldEditorMenu modPlayerEditor;

		public StyledScrollbar basicEditorScroll;


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

			modPlayerEditor = new(UserInterface);
			modPlayerEditor.OnInitialize();
			Append(modPlayerEditor);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			basicEditorList.Left.Set(newPos.X + 10, 0);
			basicEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			basicEditorScroll.Left.Set(newPos.X + 320, 0);
			basicEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);

			modPlayerEditor.Left.Set(newPos.X + 342, 0);
			modPlayerEditor.Top.Set(newPos.Y + 50, 0);
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

			basicEditorList.Add(new BoolEditor(GetLocalizedText("InfWingTime.Name"), n => mp.wingBoost = n, mp.wingBoost, () => mp.wingBoost, GetLocalizedText("InfWingTime.Description")));

		}

		private void BuildModPlayerEditor()
		{
			List<object> modEditorList = new();

			foreach (ModPlayer mp in player.ModPlayers)
			{
				if (mp is PlayerEditorPlayer) //this is our own special thing we want in the other box!
					continue;

				modEditorList.Add(mp);
			}

			modPlayerEditor.SetEditing(modEditorList.ToArray());
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetText($"Tools.PlayerEditor.{text}");
			}

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll("DragonLens: Player Editor");

			Helpers.GUIHelper.DrawBox(spriteBatch, BoundingBox, ThemeHandler.BackgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("PlayerEditor");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, GetLocalizedText("DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			Vector2 pos = basePos;
			Utils.DrawBorderString(spriteBatch, GetLocalizedText("VanillaFields"), pos + new Vector2(120, 80), Color.White, 1, 0f, 0.5f);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;

			spriteBatch.Draw(background, basicEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);

			base.Draw(spriteBatch);
		}
	}
}
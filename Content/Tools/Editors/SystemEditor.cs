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
	internal class SystemEditorTool : Tool
	{
		public override string IconKey => "SystemEditor";

		public override string LocalizationKey => "SystemEditor";

		public override void OnActivate()
		{
			SystemEditorState state = UILoader.GetUIState<SystemEditorState>();
			state.visible = !state.visible;

			//We re-initialize because the UserInterface isnt set when loaded so the scroll bars poop out
			state.RemoveAllChildren();
			state.OnInitialize();

			state.player = Main.LocalPlayer;
			state.SetupNewPlayer();
		}
	}

	internal class SystemEditorState : DraggableUIState
	{
		public Player player = Main.LocalPlayer;

		public UIGrid basicEditorList;
		public FieldEditorMenu modSystemEditor;

		public StyledScrollbar basicEditorScroll;

		public static FieldInfo systemList = typeof(SystemLoader).GetField("Systems", BindingFlags.NonPublic | BindingFlags.Static);

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 844, 54);

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

			modSystemEditor = new(UserInterface);
			modSystemEditor.OnInitialize();
			Append(modSystemEditor);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			basicEditorList.Left.Set(newPos.X + 10, 0);
			basicEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			basicEditorScroll.Left.Set(newPos.X + 320, 0);
			basicEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);

			modSystemEditor.Left.Set(newPos.X + 342, 0);
			modSystemEditor.Top.Set(newPos.Y + 50, 0);
		}

		public void SetupNewPlayer()
		{
			BuildBasicEditor();
			BuildModSystemEditor();
			Recalculate();
			Recalculate();
		}

		private void BuildBasicEditor()
		{
			static string Localize(string text)
			{
				return LocalizationHelper.GetToolText($"SystemEditor.Editors.{text}");
			}

			void AddDownedBoolean(string name, Action<bool> onValueChanged, Func<bool> listenForUpdate)
			{
				basicEditorList.Add(new BoolEditor(Localize($"{name}.Name"), onValueChanged, listenForUpdate.Invoke(), listenForUpdate, Localize($"{name}.Description")));
			}

			AddDownedBoolean("KingSlime", n => NPC.downedSlimeKing = n, () => NPC.downedSlimeKing);
			AddDownedBoolean("EoC", n => NPC.downedBoss1 = n, () => NPC.downedBoss1);
			AddDownedBoolean("EvilBoss", n => NPC.downedBoss2 = n, () => NPC.downedBoss2);
			AddDownedBoolean("Skeletron", n => NPC.downedBoss3 = n, () => NPC.downedBoss3);
			AddDownedBoolean("QueenBee", n => NPC.downedQueenBee = n, () => NPC.downedQueenBee);
			AddDownedBoolean("GoblinArmy", n => NPC.downedGoblins = n, () => NPC.downedGoblins);
			AddDownedBoolean("FrostLegion", n => NPC.downedFrost = n, () => NPC.downedFrost);
			AddDownedBoolean("Hardmode", n => Main.hardMode = n, () => Main.hardMode);
			AddDownedBoolean("QueenSlime", n => NPC.downedQueenSlime = n, () => NPC.downedQueenSlime);
			AddDownedBoolean("Destroyer", n => NPC.downedMechBoss1 = n, () => NPC.downedMechBoss1);
			AddDownedBoolean("Twins", n => NPC.downedMechBoss2 = n, () => NPC.downedMechBoss2);
			AddDownedBoolean("SkeletronPrime", n => NPC.downedMechBoss3 = n, () => NPC.downedMechBoss3);
			AddDownedBoolean("Plantera", n => NPC.downedPlantBoss = n, () => NPC.downedPlantBoss);
			AddDownedBoolean("Golem", n => NPC.downedGolemBoss = n, () => NPC.downedGolemBoss);
			AddDownedBoolean("Fishron", n => NPC.downedFishron = n, () => NPC.downedFishron);
			AddDownedBoolean("EoL", n => NPC.downedEmpressOfLight = n, () => NPC.downedEmpressOfLight);
			AddDownedBoolean("Cultist", n => NPC.downedAncientCultist = n, () => NPC.downedAncientCultist);
			AddDownedBoolean("Moonlord", n => NPC.downedMoonlord = n, () => NPC.downedMoonlord);
			AddDownedBoolean("Pirates", n => NPC.downedPirates = n, () => NPC.downedPirates);
		}

		private void BuildModSystemEditor()
		{
			List<object> modEditorList = new();

			foreach (ModSystem ms in (List<ModSystem>)systemList.GetValue(null))
			{
				if (ms.Mod.Name != "DragonLens")
					modEditorList.Add(ms);
			}

			modSystemEditor.SetEditing(modEditorList.ToArray());
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetText($"Tools.SystemEditor.{text}");
			}

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll("DragonLens: System Editor");

			Helpers.GUIHelper.DrawBox(spriteBatch, BoundingBox, ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("SystemEditor");
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
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
	internal class EntityEditorTool : Tool
	{
		public override string IconKey => "EntityEditor";

		public override string LocalizationKey => "EntityEditor";

		public override void OnActivate()
		{
			EntityEditorState state = UILoader.GetUIState<EntityEditorState>();
			state.visible = !state.visible;

			//We re-initialize because the UserInterface isnt set when loaded so the scroll bars poop out
			state.RemoveAllChildren();
			state.OnInitialize();
		}
	}

	internal class EntityEditorState : DraggableUIState
	{
		public Entity entity;

		public UIGrid basicEditorList;
		public UIGrid modEditorList;

		public FixedUIScrollbar basicEditorScroll;
		public FixedUIScrollbar modEditorScroll;

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

			modEditorScroll = new(UserInterface);
			modEditorScroll.Height.Set(540, 0);
			modEditorScroll.Width.Set(16, 0);
			Append(modEditorScroll);

			modEditorList = new();
			modEditorList.Width.Set(480, 0);
			modEditorList.Height.Set(540, 0);
			modEditorList.SetScrollbar(modEditorScroll);
			modEditorList.ListPadding = 16;
			Append(modEditorList);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			basicEditorList.Left.Set(newPos.X + 10, 0);
			basicEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			basicEditorScroll.Left.Set(newPos.X + 320, 0);
			basicEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);

			modEditorList.Left.Set(newPos.X + 342, 0);
			modEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			modEditorScroll.Left.Set(newPos.X + 480 + 338, 0);
			modEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);
		}

		public void SetupNew()
		{
			BuildBasicEditor();
			BuildModEditor();
		}

		private void BuildBasicEditor()
		{
			basicEditorList.Add(new Vector2Editor("Position", n => entity.position = n, entity.position, () => entity.position, "Where the entity top-left is located in world coordinates"));
			basicEditorList.Add(new Vector2Editor("Velocity", n => entity.velocity = n, entity.velocity, () => entity.velocity, "How much the entity's position changes each tick"));
			basicEditorList.Add(new Vector2Editor("Size", n => entity.Size = n, entity.Size, () => entity.Size, "The width/height of the entities hitbox"));

			if (entity is NPC)
				BuildBasicNPC();
		}

		private void BuildModEditor()
		{

		}

		// TODO: Soemthing here to allow mods to register custom entity types?
		// SLR dummies come to mind...

		#region NPC
		private void BuildBasicNPC()
		{
			NPC npc = entity as NPC;
			basicEditorList.Add(new IntEditor("Type", n => npc.type = n, npc.type, () => npc.type, "The type of NPC this is. See the wiki for what values associate to what NPC."));
			basicEditorList.Add(new StringEditor("Name", n => npc.GivenName = n, npc.GivenOrTypeName, () => npc.GivenOrTypeName, "Given name of the NPC."));
			basicEditorList.Add(new IntEditor("Max Life", n => npc.lifeMax = n, npc.lifeMax, () => npc.lifeMax, "Maximum life of this NPC"));
			basicEditorList.Add(new IntEditor("Life", n => npc.life = n, npc.life, () => npc.life, "Current life of this NPC"));
			basicEditorList.Add(new IntEditor("Defense", n => npc.defense = n, npc.defense, () => npc.defense, "Current defense of this NPC"));
			basicEditorList.Add(new IntEditor("Damage", n => npc.damage = n, npc.damage, () => npc.damage, "Current contact damage of the NPC"));
			basicEditorList.Add(new FloatEditor("KB Resist", n => npc.knockBackResist = n, npc.knockBackResist, () => npc.knockBackResist, "Current 'knockback resistance' of the NPC. Note this is a misnomer, knockback is MULTIPLIED by this value."));
			
			basicEditorList.Add(new IntEditor("AI Style", n => npc.aiStyle = n, npc.aiStyle, () => npc.aiStyle, "The AI style of this NPC. See the wiki for what values represent what NPCs behavior."));

			for (int k = 0; k < npc.ai.Length; k++)
			{
				int i = k; // Else these track k and we get an index OOB because k increments afterwards
				basicEditorList.Add(new FloatEditor("AI " + k, n => npc.ai[i] = n, npc.ai[i], () => npc.ai[i], "AI specific value " + k));
			}

			for (int k = 0; k < npc.localAI.Length; k++)
			{
				int i = k; // Else these track k and we get an index OOB because k increments afterwards
				basicEditorList.Add(new FloatEditor("LAI " + k, n => npc.localAI[i] = n, npc.localAI[i], () => npc.localAI[i], "Local AI specific value " + k));
			}
		}

		private void BuildModNPC()
		{

		}
		#endregion

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!BoundingBox.Contains(Main.MouseScreen.ToPoint()) && entity is null)
			{
				for(int k = 0; k < Main.maxNPCs; k++)
				{
					if (Main.npc[k].active && Main.npc[k].Hitbox.Contains(Main.MouseWorld.ToPoint()))
					{
						entity = Main.npc[k];
						Main.NewText(Main.npc[k].GivenOrTypeName + " selected for editing.");

						SetupNew();

						break;
					}
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetText($"Tools.EntityEditor.{text}");
			}

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll("DragonLens: Entity Editor");

			Helpers.GUIHelper.DrawBox(spriteBatch, BoundingBox, ThemeHandler.BackgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("EntityEditor");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, GetLocalizedText("DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			if (entity != null)
			{
				Vector2 pos = basePos;
				Utils.DrawBorderString(spriteBatch, GetLocalizedText("VanillaFields"), pos + new Vector2(120, 80), Color.White, 1, 0f, 0.5f);
				Utils.DrawBorderString(spriteBatch, GetLocalizedText("ModPlayers"), pos + new Vector2(320 + 220, 80), Color.White, 1, 0f, 0.5f);

				Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;

				spriteBatch.Draw(background, basicEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);
				spriteBatch.Draw(background, modEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);
			}
			else
			{
				Utils.DrawBorderStringBig(spriteBatch, "Click an entity to start", BoundingBox.Center.ToVector2(), Color.White, 1f, 0.5f, 0.5f);
			}

			base.Draw(spriteBatch);

		}
	}
}

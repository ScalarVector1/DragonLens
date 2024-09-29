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
using Terraria.DataStructures;
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
			state.entity = null;
			state.RemoveAllChildren();
			state.OnInitialize();
		}
	}

	internal class EntityEditorState : DraggableUIState
	{
		public Entity entity;
		public TileEntity tileEntity; // Tile entities are not entities. How inconvenient.

		public UIGrid basicEditorList;
		public StyledScrollbar basicEditorScroll;

		public FieldEditorMenu moddedEditor;

		public EntityEditorButton button;

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

			moddedEditor = new(UserInterface);
			moddedEditor.OnInitialize();
			Append(moddedEditor);

			button = new(this);
			Append(button);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			basicEditorList.Left.Set(newPos.X + 10, 0);
			basicEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			basicEditorScroll.Left.Set(newPos.X + 320, 0);
			basicEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);

			moddedEditor.Left.Set(newPos.X + 342, 0);
			moddedEditor.Top.Set(newPos.Y + 50, 0);

			button.Left.Set(newPos.X - 220, 0);
			button.Top.Set(newPos.Y + 220, 0);
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			if ((entity is null || !entity.active) && tileEntity is null)
			{
				basicEditorList.Clear();
				moddedEditor.Clear();

				basicEditorScroll.Remove();

				width = 600;
				height = 130;

				entity = null;
				tileEntity = null;
				PlayerInput.WritingText = false;
				Main.blockInput = false;

				Main.LocalPlayer.mouseInterface = true;
			}
			else
			{
				Append(basicEditorScroll);

				width = 844;
				height = 648;
			}
		}

		public void SetupNew()
		{
			BuildBasicEditor();
			BuildModEditor();
		}

		private void BuildBasicEditor()
		{
			static string Localize(string text)
			{
				return LocalizationHelper.GetToolText($"EntityEditor.BasicEditors.{text}");
			}

			if (entity != null)
			{
				basicEditorList.Add(new Vector2Editor(Localize("Position.Name"), n => entity.position = n, entity.position, () => entity.position, Localize("Position.Description")));
				basicEditorList.Add(new Vector2Editor(Localize("Velocity.Name"), n => entity.velocity = n, entity.velocity, () => entity.velocity, Localize("Velocity.Description")));
				basicEditorList.Add(new Vector2Editor(Localize("Size.Name"), n => entity.Size = n, entity.Size, () => entity.Size, Localize("Size.Description")));

				if (entity is NPC)
					BuildBasicNPC();
			}
			else if (tileEntity != null)
			{
				BuildBasicTE();
			}
		}

		private void BuildModEditor()
		{
			if (entity != null)
			{
				if (entity is NPC)
					BuildModNPC();
			}
			else if (tileEntity != null)
			{
				BuildModTE();
			}
		}

		// TODO: Soemthing here to allow mods to register custom entity types?
		// SLR dummies come to mind...

		#region NPC
		private void BuildBasicNPC()
		{
			static string Localize(string text)
			{
				return LocalizationHelper.GetToolText($"EntityEditor.NPCEditors.{text}");
			}
			
			NPC npc = entity as NPC;
			basicEditorList.Add(new IntEditor(Localize("Type.Name"), n => npc.type = n, npc.type, () => npc.type, Localize("Type.Description")));
			basicEditorList.Add(new StringEditor(Localize("Name.Name"), n => npc.GivenName = n, npc.GivenOrTypeName, () => npc.GivenOrTypeName, Localize("Name.Description")));
			basicEditorList.Add(new IntEditor(Localize("MaxLife.Name"), n => npc.lifeMax = n, npc.lifeMax, () => npc.lifeMax, Localize("MaxLife.Description")));
			basicEditorList.Add(new IntEditor(Localize("Life.Name"), n => npc.life = n, npc.life, () => npc.life, Localize("Life.Description")));
			basicEditorList.Add(new IntEditor(Localize("Defense.Name"), n => npc.defense = n, npc.defense, () => npc.defense, Localize("Defense.Description")));
			basicEditorList.Add(new IntEditor(Localize("Damage.Name"), n => npc.damage = n, npc.damage, () => npc.damage, Localize("Damage.Description")));
			basicEditorList.Add(new FloatEditor(Localize("KBResist.Name"), n => npc.knockBackResist = n, npc.knockBackResist, () => npc.knockBackResist, Localize("KBResist.Description")));

			basicEditorList.Add(new IntEditor(Localize("AIStyle.Name"), n => npc.aiStyle = n, npc.aiStyle, () => npc.aiStyle, Localize("AIStyle.Description")));

			for (int k = 0; k < npc.ai.Length; k++)
			{
				int i = k; // Else these track k and we get an index OOB because k increments afterwards
				basicEditorList.Add(new FloatEditor("AI " + k, n => npc.ai[i] = n, npc.ai[i], () => npc.ai[i], Localize("AI.Description") + k));
			}

			for (int k = 0; k < npc.localAI.Length; k++)
			{
				int i = k; // Else these track k and we get an index OOB because k increments afterwards
				basicEditorList.Add(new FloatEditor("LAI " + k, n => npc.localAI[i] = n, npc.localAI[i], () => npc.localAI[i], Localize("LAI.Description") + k));
			}
		}

		private void BuildModNPC()
		{
			NPC npc = entity as NPC;
			List<object> modEditorList = new();

			if (npc.ModNPC != null)
				modEditorList.Add(npc.ModNPC);

			foreach (GlobalNPC gnpc in npc.Globals)
			{
				if (gnpc.Mod == ModLoader.GetMod("DragonLens"))
					continue;

				modEditorList.Add(gnpc);
			}

			moddedEditor.SetEditing(modEditorList.ToArray());
		}
		#endregion

		#region TE
		public void BuildBasicTE()
		{
			static string Localize(string text)
			{
				return LocalizationHelper.GetToolText($"EntityEditor.TEEditors.{text}");
			}

			basicEditorList.Add(new IntEditor(Localize("Type.Name"), n => tileEntity.type = (byte)n, tileEntity.type, () => tileEntity.type, Localize("Type.Description")));
		}

		public void BuildModTE()
		{
			moddedEditor.SetEditing(new object[] { tileEntity });
		}
		#endregion

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!BoundingBox.Contains(Main.MouseScreen.ToPoint()) && entity is null)
			{
				// NPCs
				for (int k = 0; k < Main.maxNPCs; k++)
				{
					Rectangle box = Main.npc[k].Hitbox;
					box.Inflate(16, 16);

					if (Main.npc[k].active && box.Contains(Main.MouseWorld.ToPoint()))
					{
						entity = Main.npc[k];
						tileEntity = null;

						Main.NewText(LocalizationHelper.GetToolText("EntityEditor.Selected", Main.npc[k].GivenOrTypeName));

						SetupNew();

						return;
					}
				}

				// Projectiles
				for (int k = 0; k < Main.maxProjectiles; k++)
				{
					Rectangle box = Main.projectile[k].Hitbox;
					box.Inflate(16, 16);

					if (Main.projectile[k].active && box.Contains(Main.MouseWorld.ToPoint()))
					{
						entity = Main.projectile[k];
						tileEntity = null;

						Main.NewText(LocalizationHelper.GetToolText("EntityEditor.Selected", Main.projectile[k].Name));

						SetupNew();

						return;
					}
				}

				// Tile entity

				Point16 pos = (Main.MouseWorld / 16).ToPoint16();
				if (TileEntity.ByPosition.ContainsKey(pos))
				{
					TileEntity te = TileEntity.ByPosition[pos];

					tileEntity = te;
					entity = null;

					Main.NewText(LocalizationHelper.GetToolText("EntityEditor.SelectedTE", te.Position));

					SetupNew();

					return;
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetToolText($"EntityEditor.{text}");
			}

			Vector2 pos = basePos;

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll("DragonLens: Entity Editor");

			Helpers.GUIHelper.DrawBox(spriteBatch, BoundingBox, ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("EntityEditor");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, GetLocalizedText("DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			// Preview portrait
			var preview = new Rectangle((int)pos.X - 220, (int)pos.Y, 200, 200);

			GUIHelper.DrawBox(spriteBatch, preview, ThemeHandler.ButtonColor);

			if (entity != null && entity.active)
			{
				preview.Inflate(-4, -4);
				var source = new Rectangle((int)entity.Center.X - 100, (int)entity.Center.Y - 100, 200, 200);
				source.Offset((-Main.screenPosition).ToPoint());
				spriteBatch.Draw(Main.screenTarget, preview, source, Color.White);
			}

			if (tileEntity != null)
			{
				preview.Inflate(-4, -4);
				var source = new Rectangle(tileEntity.Position.X * 16 - 100, tileEntity.Position.Y * 16 - 100, 200, 200);
				source.Offset((-Main.screenPosition).ToPoint());
				spriteBatch.Draw(Main.screenTarget, preview, source, Color.White);
			}

			// Labels
			if ((entity is null || !entity.active) && tileEntity is null)
			{
				Utils.DrawBorderStringBig(spriteBatch, GetLocalizedText("Tutorial"), BoundingBox.Center.ToVector2() + Vector2.UnitY * 38, Color.White, 0.8f, 0.5f, 0.5f);
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, GetLocalizedText("VanillaFields"), pos + new Vector2(120, 80), Color.White, 1, 0f, 0.5f);

				Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;

				spriteBatch.Draw(background, basicEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);
			}

			base.Draw(spriteBatch);

		}
	}

	internal class EntityEditorButton : SmartUIElement
	{
		public EntityEditorState parent;

		public EntityEditorButton(EntityEditorState parent)
		{
			this.parent = parent;
			Width.Set(200, 0);
			Height.Set(42, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			parent.basicEditorList.Clear();
			parent.moddedEditor.Clear();

			parent.entity = null;
			parent.tileEntity = null;
			PlayerInput.WritingText = false;
			Main.blockInput = false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (GetDimensions().ToRectangle().Contains(Main.MouseScreen.ToPoint()))
				Main.LocalPlayer.mouseInterface = true;

			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);
			Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetToolText("EntityEditor.Deselect"), GetDimensions().Center(), Color.LightGray, 1, 0.5f, 0.5f);
		}
	}
}
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
using Terraria.GameContent.UI;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Editors
{
	internal class ItemEditor : Tool
	{
		public override string IconKey => "ItemEditor";

		public override void OnActivate()
		{
			ItemEditorState state = UILoader.GetUIState<ItemEditorState>();
			state.visible = !state.visible;

			//We re-initialize because the UserInterface isnt set when loaded so the scroll bars poop out
			state.RemoveAllChildren();
			state.OnInitialize();
		}
	}

	internal class ItemEditorState : DraggableUIState
	{
		public Item item = new();

		public ItemEditorSlot slot;
		public SetDefaultsButton defaultButton;

		public UIGrid basicEditorList;
		public FieldEditorMenu moddedEditor;
		public Terraria.GameContent.UI.Elements.UIList prefixList;

		public StyledScrollbar basicEditorScroll;
		public StyledScrollbar prefixScroll;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 600, 32);

		public override Vector2 DefaultPosition => new(0.4f, 0.4f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 844;
			height = 648;

			slot = new(this);
			Append(slot);

			defaultButton = new(this);
			Append(defaultButton);

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

			prefixScroll = new(UserInterface);
			prefixScroll.Height.Set(326, 0);
			prefixScroll.Width.Set(16, 0);
			Append(prefixScroll);

			prefixList = new();
			prefixList.Width.Set(180, 0);
			prefixList.Height.Set(326, 0);
			prefixList.SetScrollbar(prefixScroll);
			Append(prefixList);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			slot.Left.Set(newPos.X - 182, 0);
			slot.Top.Set(newPos.Y + 50 + 48, 0);

			defaultButton.Left.Set(newPos.X -212, 0);
			defaultButton.Top.Set(newPos.Y + 190 + 52, 0);

			basicEditorList.Left.Set(newPos.X + 10, 0);
			basicEditorList.Top.Set(newPos.Y + 50 + 48, 0);
			basicEditorScroll.Left.Set(newPos.X + 320, 0);
			basicEditorScroll.Top.Set(newPos.Y + 50 + 48, 0);

			moddedEditor.Left.Set(newPos.X + 342, 0);
			moddedEditor.Top.Set(newPos.Y + 50, 0);

			prefixList.Left.Set(newPos.X -220, 0);
			prefixList.Top.Set(newPos.Y + 260 + 48, 0);
			prefixScroll.Left.Set(newPos.X -220 + 180, 0);
			prefixScroll.Top.Set(newPos.Y + 260 + 48, 0);
		}

		public void SetupNewItem()
		{
			BuildBasicEditor();
			BuildModItemEditor();
			BuildPrefixEditor();
		}

		private void BuildBasicEditor()
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetText($"Tools.ItemEditor.Editors.{text}");
			}

			basicEditorList.Add(new IntEditor(GetLocalizedText("Damage.Name"), n => item.damage = n, item.damage, () => item.damage, GetLocalizedText("Damage.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("CritChance.Name"), n => item.crit = n, item.crit, () => item.crit, GetLocalizedText("CritChance.Description")));
			basicEditorList.Add(new FloatEditor(GetLocalizedText("Knockback.Name"), n => item.knockBack = n, item.knockBack, () => item.knockBack, GetLocalizedText("Knockback.Description")));

			basicEditorList.Add(new IntEditor(GetLocalizedText("UseStyle.Name"), n => item.useStyle = n, item.useStyle, () => item.useStyle, GetLocalizedText("UseStyle.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("UseTime.Name"), n => item.useTime = n, item.useTime, () => item.useTime, GetLocalizedText("UseTime.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("UseAnimation.Name"), n => item.useAnimation = n, item.useAnimation, () => item.useAnimation, GetLocalizedText("UseAnimation.Description")));
			basicEditorList.Add(new BoolEditor(GetLocalizedText("AutoReuse.Name"), n => item.autoReuse = n, item.autoReuse, () => item.autoReuse, GetLocalizedText("AutoReuse.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("ReuseDelay.Name"), n => item.reuseDelay = n, item.reuseDelay, () => item.reuseDelay, GetLocalizedText("ReuseDelay.Description")));

			basicEditorList.Add(new IntEditor(GetLocalizedText("PickaxePower.Name"), n => item.pick = n, item.pick, () => item.pick, GetLocalizedText("PickaxePower.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("AxePower.Name"), n => item.axe = n, item.axe, () => item.axe, GetLocalizedText("AxePower.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("HammerPower.Name"), n => item.hammer = n, item.hammer, () => item.hammer, GetLocalizedText("HammerPower.Description")));

			basicEditorList.Add(new IntEditor(GetLocalizedText("Rarity.Name"), n => item.rare = n, item.rare, () => item.rare, GetLocalizedText("Rarity.Description")));
			basicEditorList.Add(new IntEditor(GetLocalizedText("Value.Name"), n => item.value = n, item.value, () => item.value, GetLocalizedText("Value.Description")));

			basicEditorList.Add(new IntEditor(GetLocalizedText("ProjectileType.Name"), n => item.shoot = n, item.shoot, () => item.shoot, GetLocalizedText("ProjectileType.Description")));
			basicEditorList.Add(new FloatEditor(GetLocalizedText("ShootSpeed.Name"), n => item.shootSpeed = n, item.shootSpeed, () => item.shootSpeed, GetLocalizedText("ShootSpeed.Description")));

			basicEditorList.Add(new ColorEditor(GetLocalizedText("Color.Name"), n => item.color = n, item.color, () => item.color, GetLocalizedText("Color.Description")));

			//TODO: Prefix dropdown
		}

		public void BuildModItemEditor()
		{
			List<object> modEditorList = new();

			if (item.ModItem != null)
				modEditorList.Add(item.ModItem);

			foreach (GlobalItem gi in item.Globals)
			{
				modEditorList.Add(gi);
			}

			moddedEditor.SetEditing(modEditorList.ToArray());
		}

		private void BuildPrefixEditor()
		{
			prefixList.Clear();

			for (int k = 1; k < PrefixLoader.PrefixCount; k++)
			{
				for (int i = 0; i < 200; i++)
				{
					Item clone = item.Clone();
					clone.SetDefaults(item.type);
					clone.Prefix(-2);

					if (clone.prefix == k)
					{
						prefixList.Add(new PrefixButton(this, k));
						break;
					}
				}
			}
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			Rectangle side = new Rectangle(BoundingBox.X - 230, BoundingBox.Y + 50, 218, 598);

			if (side.Contains(Main.MouseScreen.ToPoint()))
				Main.LocalPlayer.mouseInterface = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			static string GetLocalizedText(string text)
			{
				return LocalizationHelper.GetText($"Tools.ItemEditor.{text}");
			}

			Rectangle side = new Rectangle(BoundingBox.X - 230, BoundingBox.Y + 50, 218, 598);
			Helpers.GUIHelper.DrawBox(spriteBatch, side, ThemeHandler.BackgroundColor);

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()) || side.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll("DragonLens: Item Editor");

			Helpers.GUIHelper.DrawBox(spriteBatch, BoundingBox, ThemeHandler.BackgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("ItemEditor");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, GetLocalizedText("DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			Vector2 pos = basePos;
			Utils.DrawBorderString(spriteBatch, GetLocalizedText("VanillaFields"), pos + new Vector2(120, 80), Color.White, 1, 0f, 0.5f);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;

			spriteBatch.Draw(background, basicEditorList.GetDimensions().ToRectangle(), Color.Black * 0.25f);
			spriteBatch.Draw(background, prefixList.GetDimensions().ToRectangle(), Color.Black * 0.25f);

			base.Draw(spriteBatch);
		}
	}

	internal class ItemEditorSlot : SmartUIElement
	{
		public ItemEditorState parent;

		public ItemEditorSlot(ItemEditorState parent)
		{
			this.parent = parent;
			Width.Set(120, 0);
			Height.Set(120, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!Main.mouseItem.IsAir && parent.item.IsAir)
			{
				parent.item = Main.mouseItem.Clone();
				Main.LocalPlayer.HeldItem.TurnToAir();
				Main.mouseItem.TurnToAir();

				parent.SetupNewItem();
			}
			else if (Main.mouseItem.IsAir && !parent.item.IsAir)
			{
				Main.mouseItem = parent.item.Clone();
				parent.basicEditorList.Clear();
				parent.moddedEditor.Clear();
				parent.item.TurnToAir();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			if (!parent.item.IsAir)
			{
				Main.inventoryScale = 36 / 52f * 120 / 36f;
				ItemSlot.Draw(spriteBatch, ref parent.item, 21, GetDimensions().Position());

				if (IsMouseHovering)
				{
					Main.LocalPlayer.mouseInterface = true;
					Main.HoverItem = parent.item.Clone(); // Fix knockback issue. Issue #73
					Main.hoverItemName = "a";
				}
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetToolText("ItemEditor.PlaceItemHere"), GetDimensions().Center(), Color.LightGray, 1, 0.5f, 0.5f);
			}
		}
	}

	internal class SetDefaultsButton : SmartUIElement
	{
		public ItemEditorState parent;

		public SetDefaultsButton(ItemEditorState parent)
		{
			this.parent = parent;
			Width.Set(180, 0);
			Height.Set(42, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!parent.item.IsAir)
			{
				parent.item.SetDefaults(parent.item.type);

				parent.moddedEditor.Clear();
				parent.BuildModItemEditor();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!parent.item.IsAir)
			{
				GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);
				Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetToolText("ItemEditor.SetDefaults"), GetDimensions().Center(), Color.LightGray, 1, 0.5f, 0.5f);
			}
		}
	}

	internal class PrefixButton : SmartUIElement
	{
		public ItemEditorState parent;
		public int prefixID;

		private readonly Item dummy;

		public PrefixButton(ItemEditorState parent, int prefixID)
		{
			this.parent = parent;
			this.prefixID = prefixID;

			Width.Set(180, 0);
			Height.Set(32, 0);

			dummy = new();
			dummy.SetDefaults(parent.item.type);
			dummy.Prefix(prefixID);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!parent.item.IsAir)
			{
				parent.item.SetDefaults(parent.item.type);
				parent.item.Prefix(prefixID);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!parent.item.IsAir)
			{
				Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

				string name = PrefixLoader.GetPrefix(prefixID) is null ? PrefixID.Search.GetName(prefixID) : PrefixLoader.GetPrefix(prefixID).DisplayName.Value;

				Utils.DrawBorderString(spriteBatch, name, GetDimensions().Center() + Vector2.UnitY * 2, ItemRarity.GetColor(dummy.rare), 1, 0.5f, 0.5f);

				ModPrefix prefix = PrefixLoader.GetPrefix(prefixID);

				if (prefix != null)
				{
					string path = $"{prefix.Mod.Name}/icon_small";
					Texture2D tex = ModContent.Request<Texture2D>(path).Value;

					spriteBatch.Draw(tex, GetDimensions().ToRectangle().TopLeft() + new Vector2(16, 16), null, Color.White, 0, tex.Size() / 2f, 0.5f, 0, 0);
				}

				if (IsMouseHovering)
				{
					Tooltip.SetName(name);
					Tooltip.SetTooltip("From mod: " + (prefix != null ? prefix.Mod.DisplayName : "Vanilla"));
				}
			}
		}

		public override int CompareTo(object obj)
		{ 
			if (obj is PrefixButton button)
			{
				if (button.dummy.rare != dummy.rare)
					return dummy.rare.CompareTo(button.dummy.rare);

				string name = PrefixLoader.GetPrefix(prefixID) is null ? PrefixID.Search.GetName(prefixID) : PrefixLoader.GetPrefix(prefixID).DisplayName.Value;
				string otherName = PrefixLoader.GetPrefix(button.prefixID) is null ? PrefixID.Search.GetName(button.prefixID) : PrefixLoader.GetPrefix(button.prefixID).DisplayName.Value;

				return name.CompareTo(otherName);
			}

			return base.CompareTo(obj);
		}
	}
}
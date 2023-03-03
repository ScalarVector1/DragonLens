using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Editors
{
	internal class ItemEditor : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ItemEditor";

		public override string DisplayName => "Item Editor";

		public override string Description => "Change the stats (and other fields) of items!";

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

		public UIGrid basicEditorList;
		public Terraria.GameContent.UI.Elements.UIList modItemEditorList;

		public FixedUIScrollbar basicEditorScroll;
		public FixedUIScrollbar modItemEditorScroll;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 600, 32);

		public override Vector2 DefaultPosition => new(0.4f, 0.4f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 800;
			height = 600;

			slot = new(this);
			Append(slot);

			basicEditorScroll = new(UserInterface);
			basicEditorScroll.Height.Set(540, 0);
			basicEditorScroll.Width.Set(16, 0);
			Append(basicEditorScroll);

			basicEditorList = new();
			basicEditorList.Width.Set(320, 0);
			basicEditorList.Height.Set(540, 0);
			basicEditorList.SetScrollbar(basicEditorScroll);
			Append(basicEditorList);

			modItemEditorScroll = new(UserInterface);
			modItemEditorScroll.Height.Set(540, 0);
			modItemEditorScroll.Width.Set(16, 0);
			Append(modItemEditorScroll);

			modItemEditorList = new();
			modItemEditorList.Width.Set(160, 0);
			modItemEditorList.Height.Set(540, 0);
			modItemEditorList.SetScrollbar(modItemEditorScroll);
			Append(modItemEditorList);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			slot.Left.Set(newPos.X + 594, 0);
			slot.Top.Set(newPos.Y + 100, 0);

			basicEditorList.Left.Set(newPos.X + 10, 0);
			basicEditorList.Top.Set(newPos.Y + 50, 0);
			basicEditorScroll.Left.Set(newPos.X + 320, 0);
			basicEditorScroll.Top.Set(newPos.Y + 50, 0);

			modItemEditorList.Left.Set(newPos.X + 342, 0);
			modItemEditorList.Top.Set(newPos.Y + 50, 0);
			modItemEditorScroll.Left.Set(newPos.X + 160 + 338, 0);
			modItemEditorScroll.Top.Set(newPos.Y + 50, 0);
		}

		public void SetupNewItem()
		{
			BuildBasicEditor();
			BuildModItemEditor();
		}

		private void BuildBasicEditor()
		{
			basicEditorList.Add(new IntEditor("Damage", n => item.damage = n, item.damage, () => item.damage, "How much damage this item or the projectiles it fires does."));
			basicEditorList.Add(new IntEditor("Use Style", n => item.useStyle = n, item.useStyle, () => item.useStyle, "0: None NEWLN 1: Swing NEWLN 3: Thrust NEWLN 4: HoldUp NEWLN 5: Shoot NEWLN 6: DrinkLong NEWLN 7: DrinkOld NEWLN 8: GolfPlay NEWLN 9: DrinkLiquid NEWLN 10: HiddenAnimation NEWLN 11: MowTheLawn NEWLN 12: Guitar NEWLN 13: Rapier NEWLN 14: RaiseLamp"));
			basicEditorList.Add(new IntEditor("Use Time", n => item.useTime = n, item.useTime, () => item.useTime, "How many ticks between item uses. Ignores input."));
			basicEditorList.Add(new IntEditor("Use Animation", n => item.useAnimation = n, item.useAnimation, () => item.useTime, "How many ticks before you can cancel this items use with input/use it again."));
			basicEditorList.Add(new BoolEditor("Auto Reuse", n => item.autoReuse = n, item.autoReuse, () => item.autoReuse, "If this item is automatically re-used while holding LMB."));
			basicEditorList.Add(new IntEditor("Crit Chance", n => item.crit = n, item.crit, () => item.crit, "4% base crit chance is added to this value."));

			basicEditorList.Add(new IntEditor("Pickaxe Power", n => item.pick = n, item.pick, () => item.pick, "The items ability to destroy tiles."));
			basicEditorList.Add(new IntEditor("Axe Power", n => item.axe = n, item.axe, () => item.axe, "The items ability to destroy trees. Actual value is multiplied by 5."));
			basicEditorList.Add(new IntEditor("Hammer Power", n => item.hammer = n, item.hammer, () => item.hammer, "The items ability to destroy walls."));

			basicEditorList.Add(new IntEditor("Rarity", n => item.rare = n, item.rare, () => item.rare, "The color of this item's name."));
			basicEditorList.Add(new IntEditor("Value", n => item.value = n, item.value, () => item.value, "Sell price in copper coins."));

			basicEditorList.Add(new IntEditor("Projectile type", n => item.shoot = n, item.shoot, () => item.shoot, "The kind of projectile this item will shoot on use. NEWBLOCK Use the projectile spawner to preview differnet projectile types."));
			basicEditorList.Add(new FloatEditor("Shoot speed", n => item.shootSpeed = n, item.shootSpeed, () => item.shootSpeed, "The velocity at which this item fires projectiles."));

			basicEditorList.Add(new ColorEditor("Color", n => item.color = n, item.color, () => item.color, "The color of this item's sprite."));

			//TODO: Prefix dropdown
		}

		private void BuildModItemEditor()
		{
			if (item.ModItem != null)
			{
				string message = "This field editor was auto-generated via reflection. Changing it may have unknowable consequences depending on what the mod this item is from uses it for.";

				//TODO: some sort of GetEditor generic or something so we dont have to do... this
				foreach (FieldInfo t in item.ModItem.GetType().GetFields())
				{
					TryAddEditor<bool, BoolEditor>(t);
					TryAddEditor<int, IntEditor>(t);
					TryAddEditor<float, FloatEditor>(t);
					TryAddEditor<Vector2, Vector2Editor>(t);
					TryAddEditor<Color, ColorEditor>(t);
					TryAddEditor<string, StringEditor>(t);
					TryAddEditor<NPC, NPCEditor>(t);
					TryAddEditor<Projectile, ProjectileEditor>(t);
					TryAddEditor<Player, PlayerEditor>(t);
				}

				message = "This property editor was auto-generated via reflection. Changing it may have unknowable consequences depending on what the mod this item is from uses it for.";

				foreach (PropertyInfo t in item.ModItem.GetType().GetProperties().Where(n => n.SetMethod != null))
				{
					if (t.Name == "SacrificeTotal")
						continue;

					TryAddEditor<bool, BoolEditor>(t);
					TryAddEditor<int, IntEditor>(t);
					TryAddEditor<float, FloatEditor>(t);
					TryAddEditor<Vector2, Vector2Editor>(t);
					TryAddEditor<Color, ColorEditor>(t);
					TryAddEditor<string, StringEditor>(t);
					TryAddEditor<NPC, NPCEditor>(t);
					TryAddEditor<Projectile, ProjectileEditor>(t);
					TryAddEditor<Player, PlayerEditor>(t);
				}

				Main.NewText("Foo!");
			}
		}

		private void TryAddEditor<T, E>(FieldInfo t) where E : FieldEditor<T>
		{
			if (t.FieldType == typeof(T))
			{
				var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(item.ModItem, n)), (T)t.GetValue(item.ModItem), () => (T)t.GetValue(item.ModItem), "" });
				modItemEditorList.Add(newEditor);
			}
		}

		private void TryAddEditor<T, E>(PropertyInfo t) where E : FieldEditor<T>
		{
			if (t.PropertyType == typeof(T))
			{
				var newEditor = (E)Activator.CreateInstance(typeof(E), new object[] { t.Name, (Action<T>)(n => t.SetValue(item.ModItem, n)), (T)t.GetValue(item.ModItem), () => (T)t.GetValue(item.ModItem), "" });
				modItemEditorList.Add(newEditor);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, BoundingBox, ModContent.GetInstance<GUIConfig>().backgroundColor);

			Vector2 pos = basePos;
			Utils.DrawBorderString(spriteBatch, "Vanilla Fields", pos + new Vector2(120, 25), Color.White, 1, 0f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "Modded", pos + new Vector2(320 + 70, 25), Color.White, 1, 0f, 0.5f);

			Utils.DrawBorderString(spriteBatch, "Item Editor", pos + new Vector2(320 + 130 + 160, 25), Color.White, 1, 0f, 0.5f);

			base.Draw(spriteBatch);
		}
	}

	internal class ItemEditorSlot : UIElement
	{
		public ItemEditorState parent;

		public ItemEditorSlot(ItemEditorState parent)
		{
			this.parent = parent;
			Width.Set(120, 0);
			Height.Set(120, 0);
		}

		public override void Click(UIMouseEvent evt)
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
				parent.modItemEditorList.Clear();
				parent.item.TurnToAir();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Helpers.GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			if (!parent.item.IsAir)
			{
				Main.inventoryScale = 36 / 52f * 120 / 36f;
				ItemSlot.Draw(spriteBatch, ref parent.item, 21, GetDimensions().Position());

				if (IsMouseHovering)
				{
					Main.LocalPlayer.mouseInterface = true;
					Main.HoverItem = parent.item;
					Main.hoverItemName = "a";
				}
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, "Place item\nhere!", GetDimensions().Center(), Color.LightGray, 1, 0.5f, 0.5f);
			}
		}
	}
}

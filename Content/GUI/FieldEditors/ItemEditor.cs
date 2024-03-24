using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class ItemEditor : FieldEditor<Item>
	{
		public MiniItemSlot slot;

		public ItemEditor(string name, Action<Item> onValueChanged, Item initialValue, Func<Item> listenForUpdate = null, string description = "") : base(70, name, onValueChanged, listenForUpdate, initialValue, description)
		{
			slot = new(this);
			slot.Left.Set(10, 0);
			slot.Top.Set(32, 0);
			Append(slot);
		}
	}

	internal class MiniItemSlot : SmartUIElement
	{
		public ItemEditor parent;

		public bool noItem => parent.value is null || parent.value.IsAir;

		public MiniItemSlot(ItemEditor parent)
		{
			this.parent = parent;
			Width.Set(32, 0);
			Height.Set(32, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!Main.mouseItem.IsAir && noItem)
			{
				var newItem = Main.mouseItem.Clone();
				parent.onValueChanged(newItem);

				Main.LocalPlayer.HeldItem.TurnToAir();
				Main.mouseItem.TurnToAir();
			}
			else if (Main.mouseItem.IsAir && !noItem)
			{
				Main.mouseItem = parent.value.Clone();
				parent.value.TurnToAir();
			}
			else if (!Main.mouseItem.IsAir && !noItem)
			{
				var temp = parent.value;

				var newItem = Main.mouseItem.Clone();
				parent.onValueChanged(newItem);

				Main.mouseItem = temp.Clone();
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			if (!noItem)
			{
				Main.inventoryScale = 36 / 52f * 32 / 36f;
				Terraria.UI.ItemSlot.Draw(spriteBatch, ref parent.value, 21, GetDimensions().Position());

				if (IsMouseHovering)
				{
					Main.LocalPlayer.mouseInterface = true;
					Main.HoverItem = parent.value.Clone(); // Fix knockback issue. Issue #73
					Main.hoverItemName = "a";

					parent.hideTooltip = true;
				}

				if (Main.LocalPlayer.inventory.Contains(parent.value))
					Utils.DrawBorderString(spriteBatch, "In inventory", GetDimensions().Center() + new Vector2(20, 2), Color.White, 0.8f, 0f, 0.5f);
				else if (Main.LocalPlayer.armor.Contains(parent.value))
					Utils.DrawBorderString(spriteBatch, "Equipped", GetDimensions().Center() + new Vector2(20, 2), Color.White, 0.8f, 0f, 0.5f);
			}
			else
			{
				Utils.DrawBorderString(spriteBatch, "null", GetDimensions().Center(), Color.Red, 0.6f, 0.5f, 0.5f);
			}
		}
	}
}

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
using System.Text;
using System.Threading.Tasks;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Multiplayer
{
	internal class InventoryManagerWindow : DraggableUIState
	{
		public Player player;

		public UIGrid slots = new();
		public StyledScrollbar slotScroll;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 580, 32);

		public override Vector2 DefaultPosition => new(0.5f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 580;
			height = 456;

			slotScroll = new(UserInterface);
			slotScroll.Width.Set(32, 0);
			slotScroll.Height.Set(380, 0);
			Append(slotScroll);

			slots = new();
			slots.Width.Set(530, 0);
			slots.Height.Set(380, 0);
			slots.SetScrollbar(slotScroll);
			Append(slots);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			slots.Left.Set(newPos.X + 10, 0);
			slots.Top.Set(newPos.Y + 64, 0);

			slotScroll.Left.Set(newPos.X + 542, 0);
			slotScroll.Top.Set(newPos.Y + 64, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var target = new Rectangle((int)basePos.X, (int)basePos.Y, 580, 456);

			GUIHelper.DrawBox(spriteBatch, target, ThemeHandler.BackgroundColor);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 450, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D gridBack = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var gridBackTarget = slots.GetDimensions().ToRectangle();
			gridBackTarget.Inflate(4, 4);
			spriteBatch.Draw(gridBack, gridBackTarget, Color.Black * 0.25f);

			Texture2D icon = ThemeHandler.GetIcon("PlayerManager");
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, $"{player.name}'s Inventory", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			base.Draw(spriteBatch);
		}

		public void SetInventory()
		{
			slots.Clear();

			for (int k = 0; k < player.inventory.Length; k++)
			{
				slots.Add(new ItemSlot(player.inventory, k, 0));
			}

			slots.Add(new Seperator("Equipment", 1));

			for (int k = 0; k < player.armor.Length; k++)
			{
				slots.Add(new ItemSlot(player.armor, k, 2));
			}

			slots.Add(new Seperator("Piggy bank", 3));

			for (int k = 0; k < player.bank.item.Length; k++)
			{
				slots.Add(new ItemSlot(player.bank.item, k, 4));
			}

			slots.Add(new Seperator("Safe", 5));

			for (int k = 0; k < player.bank2.item.Length; k++)
			{
				slots.Add(new ItemSlot(player.bank2.item, k, 6));
			}

			slots.Add(new Seperator("Defenders forge", 7));

			for (int k = 0; k < player.bank3.item.Length; k++)
			{
				slots.Add(new ItemSlot(player.bank3.item, k, 8));
			}

			slots.Add(new Seperator("Void vault", 9));

			for (int k = 0; k < player.bank4.item.Length; k++)
			{
				slots.Add(new ItemSlot(player.bank4.item, k, 10));
			}
		}
	}

	internal class Seperator : SmartUIElement
	{
		readonly string text;
		public int invIndex;

		public Seperator(string text, int invIndex)
		{
			this.text = text;
			this.invIndex = invIndex;

			Width.Set(520, 0);
			Height.Set(24, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = GetDimensions().ToRectangle();
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Utils.DrawBorderString(spriteBatch, text, GetDimensions().Position() + new Vector2(12, 4), Color.White, 0.8f);
		}

		public override int CompareTo(object obj)
		{
			if (obj is ItemSlot slot)
			{
				return invIndex.CompareTo(slot.invIndex);
			}

			if (obj is Seperator sep)
			{
				return invIndex.CompareTo(sep.invIndex);
			}

			return base.CompareTo(obj);
		}
	}

	internal class ItemSlot : SmartUIElement
	{
		readonly Item[] inventory;
		readonly int index;
		public int invIndex;

		Item item => inventory[index];

		public bool noItem => item is null || item.IsAir;

		public ItemSlot(Item[] inventory, int index, int invIndex)
		{
			this.inventory = inventory;
			this.index = index;
			this.invIndex = invIndex;

			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!Main.mouseItem.IsAir && noItem)
			{
				Item newItem = Main.mouseItem.Clone();
				inventory[index] = newItem;

				Main.LocalPlayer.HeldItem.TurnToAir();
				Main.mouseItem.TurnToAir();
			}
			else if (Main.mouseItem.IsAir && !noItem)
			{
				Main.mouseItem = item.Clone();
				item.TurnToAir();
			}
			else if (!Main.mouseItem.IsAir && !noItem)
			{
				Item temp = item.Clone();

				Item newItem = Main.mouseItem.Clone();
				inventory[index] = newItem;

				Main.mouseItem = temp;
			}

			PlayerManagerNetHandler.SendItem(UILoader.GetUIState<InventoryManagerWindow>().player.whoAmI, index, invIndex, item);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);

			if (!noItem)
			{
				Main.inventoryScale = 36 / 52f * 48 / 36f;
				Item clone = item.Clone();
				Terraria.UI.ItemSlot.Draw(spriteBatch, ref clone, 21, GetDimensions().Position());

				if (IsMouseHovering)
				{
					Main.LocalPlayer.mouseInterface = true;
					Main.HoverItem = item.Clone(); // Fix knockback issue. Issue #73
					Main.hoverItemName = "a";
				}
			}
			else if (invIndex == 0) // Poor hack for this but indicates the main inventory to highlight hotbar and special slots
			{
				if (index <= 9)
				{
					Texture2D tex = Terraria.GameContent.TextureAssets.Item[ItemID.BoneGlove].Value;
					spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Gray * 0.35f, 0f, tex.Size() / 2f, 1, 0, 0);
				}

				if (index > 49 && index <= 53)
				{
					Texture2D tex = Terraria.GameContent.TextureAssets.Item[ItemID.GoldCoin].Value;
					spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Gray * 0.35f, 0f, tex.Size() / 2f, 1, 0, 0);
				}

				if (index > 53 && index <= 57)
				{
					Texture2D tex = Terraria.GameContent.TextureAssets.Item[ItemID.WoodenArrow].Value;
					spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Gray * 0.35f, 0f, tex.Size() / 2f, 1, 0, 0);
				}

				if (index == 58)
				{
					Texture2D tex = Terraria.GameContent.TextureAssets.Item[ItemID.FeralClaws].Value;
					spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Gray * 0.35f, 0f, tex.Size() / 2f, 1, 0, 0);
				}
			}
			else if (invIndex == 2) // Equipment
			{
				int id = 0;

				id = index switch // textures for backgrounds
				{
					0 => ItemID.IronHelmet,
					1 => ItemID.IronChainmail,
					2 => ItemID.IronGreaves,
					3 or 4 or 5 or 6 or 7 or 8 or 9 => ItemID.BandofRegeneration,
					10 => ItemID.TopHat,
					11 => ItemID.TaxCollectorSuit,
					12 => ItemID.TaxCollectorPants,
					13 or 14 or 15 or 16 or 17 or 18 or 19 => ItemID.GiantBow,
					_ => 0,
				};

				Texture2D tex = Terraria.GameContent.TextureAssets.Item[id].Value;
				spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.Gray * 0.35f, 0f, tex.Size() / 2f, 1, 0, 0);
			}
		}

		public override int CompareTo(object obj)
		{
			if (obj is ItemSlot slot)
			{
				if (invIndex != slot.invIndex)
					return invIndex.CompareTo(slot.invIndex);

				return index.CompareTo(slot.index);
			}

			if (obj is Seperator sep)
			{
				return invIndex.CompareTo(sep.invIndex);
			}

			return base.CompareTo(obj);
		}
	}
}
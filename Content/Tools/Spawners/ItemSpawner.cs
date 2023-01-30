﻿using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class ItemSpawner : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/ItemSpawner";

		public override string DisplayName => "Item spawner";

		public override string Description => "Spawn items, with options to customize their stats";

		public override void OnActivate()
		{
			ItemBrowser state = UILoader.GetUIState<ItemBrowser>();
			state.visible = !state.visible;

			if (!state.initialized)
			{
				UILoader.GetUIState<ItemBrowser>().Refresh();
				state.initialized = true;
			}
		}
	}

	internal class ItemBrowser : Browser
	{
		public override string Name => "Item spawner";

		public override string IconTexture => "DragonLens/Assets/Tools/ItemSpawner";

		public override void PopulateGrid(UIGrid grid)
		{
			for (int k = 0; k < ItemLoader.ItemCount; k++)
			{
				var item = new Item();
				item.SetDefaults(k);

				grid.Add(new ItemButton(item));
			}
		}
	}

	internal class ItemButton : BrowserButton
	{
		public Item item;

		public override string Identifier => item.Name;

		public ItemButton(Item item)
		{
			this.item = item;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);

			Main.inventoryScale = 36 / 52f;
			ItemSlot.Draw(spriteBatch, ref item, 21, GetDimensions().Position());

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = item;
				Main.hoverItemName = "a";
			}
		}

		public override void Click(UIMouseEvent evt)
		{
			if (Main.mouseItem.IsAir)
			{
				if (!Main.playerInventory)
					Main.playerInventory = true;

				Main.mouseItem = item.Clone();
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (Main.mouseItem.IsAir)
			{
				if (!Main.playerInventory)
					Main.playerInventory = true;

				Main.mouseItem = item.Clone();
				Main.mouseItem.stack = Main.mouseItem.maxStack;
			}
		}

		public override int CompareTo(object obj)
		{
			return item.type - (obj as ItemButton).item.type;
		}
	}
}

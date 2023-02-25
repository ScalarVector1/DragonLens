using DragonLens.Content.Filters;
using DragonLens.Content.Filters.ItemFilters;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
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

		public override Vector2 DefaultPosition => new(0.3f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<ItemButton>();
			for (int k = 0; k < ItemLoader.ItemCount; k++)
			{
				var item = new Item();
				item.SetDefaults(k);

				buttons.Add(new ItemButton(item, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Mod filters");

			foreach (Mod mod in ModLoader.Mods)
			{
				filters.AddFilter(new ItemModFilter(mod));
			}

			filters.AddSeperator("Damage filters");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Unknown", "Any damage", "Any item that deals damage", n => !(n is ItemButton && (n as ItemButton).item.damage > 0)));
			filters.AddFilter(new DamageClassFilter(DamageClass.Melee, "DragonLens/Assets/Filters/Melee"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Ranged, "DragonLens/Assets/Filters/Ranged"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Magic, "DragonLens/Assets/Filters/Magic"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Summon, "DragonLens/Assets/Filters/Summon"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Throwing, "DragonLens/Assets/Filters/Throwing"));

			filters.AddSeperator("Misc filters");
			filters.AddFilter(new Filter("DragonLens/Assets/GUI/NoBox", "Accessory", "Any item that can be equipped as an accessory", n => !(n is ItemButton && (n as ItemButton).item.accessory)));
			filters.AddFilter(new Filter("DragonLens/Assets/GUI/NoBox", "Armor", "Any item that can be equipped as armor", n => !(n is ItemButton && (n as ItemButton).item.defense > 0)));
		}
	}

	internal class ItemButton : BrowserButton
	{
		public Item item;

		public override string Identifier => item.Name;

		public ItemButton(Item item, Browser browser) : base(browser)
		{
			this.item = item;
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.inventoryScale = 36 / 52f * iconBox.Width / 36f;
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

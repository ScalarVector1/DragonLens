using DragonLens.Content.Filters;
using DragonLens.Content.Filters.ItemFilters;
using DragonLens.Content.GUI;
using DragonLens.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class ItemSpawner : BrowserTool<ItemBrowser>
	{
		public override string IconKey => "ItemSpawner";

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.ItemSpawner.{key}", args);
		}
	}

	internal class ItemBrowser : Browser
	{
		public override string Name => ItemSpawner.GetText("DisplayName");

		public override string IconTexture => "ItemSpawner";

		public override string HelpLink => "https://github.com/ScalarVector1/DragonLens/wiki/Item-spawner";

		public override Vector2 DefaultPosition => new(0.3f, 0.4f);

		public override void PopulateGrid(UIGrid grid)
		{
			// Clear deprecated items set during loading to allow them to get
			// their defaults set correctly + allow them to be accessed through
			// the spawner. The set is stored and restored after the grid is
			// populated.
			bool[] deprecated = ItemID.Sets.Deprecated;
			ItemID.Sets.Deprecated = ItemID.Sets.Factory.CreateBoolSet();

			var buttons = new List<ItemButton>();
			// `0` corresponds to ItemID.None - that is, no item.
			for (int k = 1; k < ItemLoader.ItemCount; k++)
			{
				var item = new Item();
				item.SetDefaults(k);

				buttons.Add(new ItemButton(item, this));
			}

			grid.AddRange(buttons);

			ItemID.Sets.Deprecated = deprecated;
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanilla", "Tools.ItemSpawner.Filters.Vanilla", n => !(n is ItemButton && (n as ItemButton).item.ModItem is null)));

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModItem>().Count() > 0))
			{
				filters.AddFilter(new ItemModFilter(mod));
			}

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Damage");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Unknown", "Tools.ItemSpawner.Filters.AnyDamage", n => !(n is ItemButton && (n as ItemButton).item.damage > 0)));
			filters.AddFilter(new DamageClassFilter(DamageClass.Melee, "DragonLens/Assets/Filters/Melee"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Ranged, "DragonLens/Assets/Filters/Ranged"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Magic, "DragonLens/Assets/Filters/Magic"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Summon, "DragonLens/Assets/Filters/Summon"));
			filters.AddFilter(new DamageClassFilter(DamageClass.Throwing, "DragonLens/Assets/Filters/Throwing"));

			filters.AddSeperator("Tools.ItemSpawner.FilterCategories.Misc");

			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Accessory", "Tools.ItemSpawner.Filters.Accessory", n => !(n is ItemButton && (n as ItemButton).item.accessory)));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Defense", "Tools.ItemSpawner.Filters.Armor", n => !(n is ItemButton && (n as ItemButton).item.defense > 0)));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Placeable", "Tools.ItemSpawner.Filters.Placeable", n => !(n is ItemButton && (n as ItemButton).item.createTile >= TileID.Dirt || (n as ItemButton).item.createWall >= 0)));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Unknown", "Tools.ItemSpawner.Filters.Deprecated", n => n is ItemButton ib && !ItemID.Sets.Deprecated[ib.item.type]));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Consumables", "Tools.ItemSpawner.Filters.Consumables", n => n is ItemButton ib && (!ib.item.consumable || ib.item.createTile >= TileID.Dirt || ib.item.createWall >= 0)));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Pickaxe", "Tools.ItemSpawner.Filters.Pickaxe", n => n is ItemButton ib && ib.item.pick == 0));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Axe", "Tools.ItemSpawner.Filters.Axe", n => n is ItemButton ib && ib.item.axe == 0));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Hammer", "Tools.ItemSpawner.Filters.Hammer", n => n is ItemButton ib && ib.item.hammer == 0));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Ammo", "Tools.ItemSpawner.Filters.Ammo", n => n is ItemButton ib && ib.item.ammo == AmmoID.None));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Expert", "Tools.ItemSpawner.Filters.Expert", n => n is ItemButton ib && !ib.item.expert));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Master", "Tools.ItemSpawner.Filters.Master", n => n is ItemButton ib && !ib.item.master));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanity", "Tools.ItemSpawner.Filters.Vanity", n => n is ItemButton ib && !ib.item.vanity));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Fishing", "Tools.ItemSpawner.Filters.Fishing", n => n is ItemButton ib && ib.item.fishingPole == 0 && ib.item.bait == 0 && !ib.item.questItem));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/MakeNPC", "Tools.ItemSpawner.Filters.MakeNPC", n => n is ItemButton ib && ib.item.makeNPC == 0));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Mounts", "Tools.ItemSpawner.Filters.Mounts", n => n is ItemButton ib && ib.item.mountType == -1));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Pets", "Tools.ItemSpawner.Filters.Pets", n => n is ItemButton ib && !(Main.vanityPet[ib.item.buffType] || Main.lightPet[ib.item.buffType])));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Wings", "Tools.ItemSpawner.Filters.Wings", n => n is ItemButton ib && ib.item.wingSlot == -1));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Hooks", "Tools.ItemSpawner.Filters.Hooks", n => n is ItemButton ib && !Main.projHook[ib.item.shoot]));
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Material", "Tools.ItemSpawner.Filters.Material", n => n is ItemButton ib && !ItemID.Sets.IsAMaterial[ib.item.type])); // Alternatively: ib.item.material
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Journey", "Tools.ItemSpawner.Filters.Unresearched", n => n is ItemButton ib && (!Terraria.GameContent.Creative.CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId.ContainsKey(ib.item.type) || Main.LocalPlayer.creativeTracker.ItemSacrifices.TryGetSacrificeNumbers(ib.item.type, out int amountWeHave, out int amountNeededTotal) && amountWeHave >= amountNeededTotal))); // Don't display if the item can't be research or already has been researched.
		}
	}

	internal class ItemButton : BrowserButton
	{
		public Item item;
		public int stackDelay = 0;

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

		public override void SafeClick(UIMouseEvent evt)
		{
			if (Main.keyState.PressingShift())
			{
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), item.type, item.maxStack);
			}
			else if (Main.mouseItem.IsAir)
			{
				if (!Main.playerInventory)
					Main.playerInventory = true;

				Main.mouseItem = item.Clone();

				if (!ItemID.Sets.Deprecated[item.type])
					Main.mouseItem.SetDefaults(item.type);

				Main.mouseItem.stack = Main.mouseItem.maxStack;
			}
		}

		public override void SafeRightMouseDown(UIMouseEvent evt)
		{
			if (Main.keyState.PressingShift())
			{
				Main.LocalPlayer.QuickSpawnItem(Main.LocalPlayer.GetSource_FromThis(), item.type, 1);
			}
			else if (Main.mouseItem.IsAir)
			{
				if (!Main.playerInventory)
					Main.playerInventory = true;

				Main.mouseItem = item.Clone();

				if (!ItemID.Sets.Deprecated[item.type])
					Main.mouseItem.SetDefaults(item.type);

				stackDelay = 30;
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			base.SafeUpdate(gameTime);

			// Allows for "Hold RMB to get more
			if (IsMouseHovering && Main.mouseRight && Main.mouseItem.type == item.type)
			{
				if (stackDelay > 0)
					stackDelay--;
				else if (Main.mouseItem.stack < Main.mouseItem.maxStack)
					Main.mouseItem.stack++;
			}
		}

		public override int CompareTo(object obj)
		{
			return item.type - (obj as ItemButton).item.type;
		}
	}
}
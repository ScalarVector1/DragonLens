using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Editors
{
	internal class AccessoryTray : Tool
	{
		public static List<Item> loadCache = new();

		public override string IconKey => "AccessoryTray";

		public override string DisplayName => "Accessory tray";

		public override string Description => "A virtually infinite collection of extra accessory slots!";

		public override void OnActivate()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				Main.NewText("Accessory tray is disabled in multiplayer", Color.Red);
				return;
			}

			AccessoryTrayUI state = UILoader.GetUIState<AccessoryTrayUI>();
			state.visible = !state.visible;

			if (!state.initialized)
			{
				state.InitializeScrollbar();
				state.initialized = true;
			}
		}

		public override void SaveData(TagCompound tag)
		{
			var mp = Main.LocalPlayer?.GetModPlayer<AcccessoryTrayPlayer>();

			if (mp is null)
				return;

			mp.accessories.RemoveAll(n => n.IsAir);

			tag["Items"] = mp.accessories;
			loadCache = mp.accessories;
		}

		public override void LoadData(TagCompound tag)
		{
			IList<Item> list = tag.GetList<Item>("Items");
			loadCache.Clear();

			foreach (Item item in list)
			{
				loadCache.Add(item);
			}
		}
	}

	internal class AcccessoryTrayPlayer : ModPlayer
	{
		public readonly List<Item> accessories = new() { new Item() };

		public override void OnEnterWorld(Player player)
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
				return;

			AccessoryTrayUI ui = UILoader.GetUIState<AccessoryTrayUI>();

			accessories.Clear();
			ui.slots.Clear();

			foreach (Item item in AccessoryTray.loadCache)
			{
				accessories.Add(item);
				ui.slots.Add(new AccessoryTraySlot(ui, item));
			}

			var next = new Item();
			accessories.Add(next);
			ui.slots.Add(new AccessoryTraySlot(ui, next));
		}

		public override void UpdateEquips()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
				return;

			accessories.ForEach(n =>
			{
				Player.VanillaUpdateEquip(n);
				Player.ApplyEquipFunctional(n, false);
			});
		}
	}

	internal class AccessoryTrayUI : DraggableUIState
	{
		public UIGrid slots;

		public FixedUIScrollbar slotsScroll;

		public bool initialized = false;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 32);

		public override Vector2 DefaultPosition => new(0.6f, 0.25f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 320;
			height = 480;

			slots = new();
			slots.Width.Set(260, 0);
			slots.Height.Set(400, 0);
			Append(slots);
		}

		public void InitializeScrollbar()
		{
			slotsScroll = new(UserInterface);
			slotsScroll.Height.Set(400, 0);
			Append(slotsScroll);

			slots.SetScrollbar(slotsScroll);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
				visible = false;

			slots.Width.Set(260, 0);
			slots.Height.Set(400, 0);
			height = 480;

			slotsScroll.Left.Set((int)newPos.X + 290, 0);
			slotsScroll.Top.Set((int)newPos.Y + 60, 0);

			slots.Left.Set((int)newPos.X + 16, 0);
			slots.Top.Set((int)newPos.Y + 62, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, BoundingBox, ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 300, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("AccessoryTray");
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var bgDims = slots.GetDimensions().ToRectangle();
			bgDims.Inflate(4, 4);
			spriteBatch.Draw(background, bgDims, Color.Black * 0.25f);

			Utils.DrawBorderStringBig(spriteBatch, "Accessory tray", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}

	internal class AccessoryTraySlot : SmartUIElement
	{
		public AccessoryTrayUI parent;

		public Item item;

		public int index;

		public static int lastIndex;

		public AcccessoryTrayPlayer Mp => Main.LocalPlayer.GetModPlayer<AcccessoryTrayPlayer>();

		public AccessoryTraySlot(AccessoryTrayUI parent, Item item)
		{
			Width.Set(48, 0);
			Height.Set(48, 0);

			this.parent = parent;
			this.item = item;

			this.index = lastIndex;
			lastIndex++;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!Main.mouseItem.IsAir)
			{
				if (!Main.mouseItem.accessory)
					return;

				Item oldItem = item.Clone();

				int index = Mp.accessories.IndexOf(item);
				Mp.accessories[index] = Main.mouseItem.Clone();
				item = Mp.accessories[index];

				if (oldItem.IsAir)
				{
					Main.mouseItem.TurnToAir();

					var next = new Item();
					Mp.accessories.Add(next);
					parent.slots.Add(new AccessoryTraySlot(parent, next));
				}
				else
				{
					Main.mouseItem = oldItem;
				}
			}
			else if (!item.IsAir)
			{
				if (Main.keyState.PressingShift())
					Main.LocalPlayer.QuickSpawnItem(null, item.type);
				else
					Main.mouseItem = item.Clone();

				Mp.accessories.Remove(item);

				parent.slots.Remove(this);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			CalculatedStyle dims = GetDimensions();
			var iconBox = new Rectangle((int)dims.X, (int)dims.Y, 48, 48);

			Main.inventoryScale = 36 / 52f * iconBox.Width / 36f;

			ItemSlot.Draw(spriteBatch, ref item, 21, iconBox.TopLeft());

			if (IsMouseHovering)
			{
				Main.LocalPlayer.mouseInterface = true;
				Main.HoverItem = item;
				Main.hoverItemName = "Place an accessory here!";
			}
		}

		public override int CompareTo(object obj)
		{
			if (obj is AccessoryTraySlot slot)
				return slot.index < index ? 1 : -1;

			return base.CompareTo(obj);
		}
	}
}
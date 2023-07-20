using DragonLens.Content.Filters;
using DragonLens.Content.Filters.DustFilters;
using DragonLens.Content.GUI;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class DustSpawner : BrowserTool<DustBrowser>
	{
		public override string IconKey => "DustSpawner";

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.DustSpawner.{key}", args);
		}
	}

	internal class DustBrowser : Browser
	{
		public static Dust selected;

		public static bool perfect;
		public static BoolEditor perfectEditor;

		public static float scale = 1f;
		public static FloatEditor scaleEditor;

		public static int alpha;
		public static IntEditor alphaEditor;

		public static Vector2 velocity;
		public static Vector2Editor velocityEditor;

		public static Color color = Color.White;
		public static ColorEditor colorEditor;

		public override string Name => DustSpawner.GetText("DisplayName");

		public override string IconTexture => "DustSpawner";

		public override Vector2 DefaultPosition => new(0.5f, 0.4f);

		public override void PostInitialize()
		{
			perfectEditor = new(DustSpawner.GetText("FieldEditors.NewDustPerfect"), n => perfect = n, false);
			Append(perfectEditor);

			scaleEditor = new(DustSpawner.GetText("FieldEditors.Scale"), n => scale = n, 1);
			Append(scaleEditor);

			alphaEditor = new(DustSpawner.GetText("FieldEditors.Alpha"), n => alpha = n, 0);
			Append(alphaEditor);

			velocityEditor = new(DustSpawner.GetText("FieldEditors.Velocity"), n => velocity = n, Vector2.Zero);
			Append(velocityEditor);

			colorEditor = new(DustSpawner.GetText("FieldEditors.Color"), n => color = n, Color.White);
			Append(colorEditor);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			base.AdjustPositions(newPos);

			float nextY = 0;

			perfectEditor.Left.Set(newPos.X - 160, 0);
			perfectEditor.Top.Set(newPos.Y, 0);
			nextY += perfectEditor.Height.Pixels + 4;

			scaleEditor.Left.Set(newPos.X - 160, 0);
			scaleEditor.Top.Set(newPos.Y + nextY, 0);
			nextY += scaleEditor.Height.Pixels + 4;

			alphaEditor.Left.Set(newPos.X - 160, 0);
			alphaEditor.Top.Set(newPos.Y + nextY, 0);
			nextY += alphaEditor.Height.Pixels + 4;

			velocityEditor.Left.Set(newPos.X - 160, 0);
			velocityEditor.Top.Set(newPos.Y + nextY, 0);
			nextY += velocityEditor.Height.Pixels + 4;

			colorEditor.Left.Set(newPos.X - 160, 0);
			colorEditor.Top.Set(newPos.Y + nextY, 0);
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<DustButton>();
			for (int k = 0; true; k++)
			{
				if (k > DustID.Count && DustLoader.GetDust(k) is null)
					break;

				var dust = new Dust
				{
					type = k
				};

				buttons.Add(new DustButton(dust, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.DustSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter("DragonLens/Assets/Filters/Vanilla", "Tools.DustSpawner.Filters.Vanilla", n => !(n is DustButton && (n as DustButton).dust.type <= DustID.Count)) { isModFilter = true});

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModDust>().Count() > 0))
			{
				filters.AddFilter(new DustModFilter(mod));
			}
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			base.DraggableUdpate(gameTime);

			if (selected != null)
			{
				Main.LocalPlayer.mouseInterface = true;

				if (Main.mouseLeft && !BoundingBox.Contains(Main.MouseScreen.ToPoint()) && !perfectEditor.IsMouseHovering && !scaleEditor.IsMouseHovering && !alphaEditor.IsMouseHovering && !velocityEditor.IsMouseHovering && !colorEditor.IsMouseHovering)
				{
					PlayerInput.SetZoom_World();

					if (perfect)
						Dust.NewDustPerfect(Main.MouseWorld, selected.type, velocity, alpha, color, scale);
					else
						Dust.NewDust(Main.MouseWorld, 16, 16, selected.type, velocity.X, velocity.Y, alpha, color, scale);

					PlayerInput.SetZoom_UI();
				}
			}

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll($"DragonLens: {Name}");
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			if (selected != null)
				selected = null;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (selected != null)
			{

			}

			// Set name here to receive game language selection changes in real time
			// This is a bit of a hack, but it works
			perfectEditor.name = DustSpawner.GetText("FieldEditors.NewDustPerfect");
			scaleEditor.name = DustSpawner.GetText("FieldEditors.Scale");
			alphaEditor.name = DustSpawner.GetText("FieldEditors.Alpha");
			velocityEditor.name = DustSpawner.GetText("FieldEditors.Velocity");
			colorEditor.name = DustSpawner.GetText("FieldEditors.Color");

			base.Draw(spriteBatch);
		}
	}

	internal class DustButton : BrowserButton
	{
		public Dust dust;

		public string name;

		public override string Identifier => name;

		public DustButton(Dust dust, Browser browser) : base(browser)
		{
			this.dust = dust;

			if (dust.type > DustID.Count)
			{
				name = DustLoader.GetDust(dust.type).Name;

				ModDust md = DustLoader.GetDust(dust.type);
				try
				{
					md.OnSpawn(dust);
					md.Update(dust);
				}
				catch
				{
					dust.frame = new Rectangle(0, 0, 8, 8);
				}
			}
			else
			{
				//We use reflection here to steal the name from DustID of this vanilla dust
				System.Reflection.FieldInfo[] fields = typeof(DustID).GetFields();
				name = fields.FirstOrDefault(n => (short)n.GetValue(null) == dust.type).Name;

				name = Regex.Replace(name, "([a-z])([A-Z])", "$1 $2");

				dust.frame.X = 10 * dust.type;
				dust.frame.Y = 10;
				dust.shader = null;
				dust.customData = null;
				int row = dust.type / 100;
				dust.frame.X -= 1000 * row;
				dust.frame.Y += 30 * row;
				dust.frame.Width = 8;
				dust.frame.Height = 8;
			}

			if (dust.frame.Width == 0)
				dust.frame = new Rectangle(0, 0, 8, 8);
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Texture2D tex;

			if (dust.type <= DustID.Count)
				tex = Terraria.GameContent.TextureAssets.Dust.Value;
			else
				tex = DustLoader.GetDust(dust.type).Texture2D.Value;

			Rectangle frame = dust.frame;

			float scale = iconBox.Width / 52f;
			if (frame.Width > 32 || frame.Height > 32)
				scale *= 32f / Math.Max(frame.Width, frame.Height);

			spriteBatch.Draw(tex, iconBox.Center(), frame, Color.White, 0, new Vector2(frame.Width, frame.Height) / 2, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);
				Tooltip.SetTooltip(DustSpawner.GetText("DustType", dust.type));
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			DustBrowser.selected = dust;
			Main.NewText(DustSpawner.GetText("Selected", Identifier));
		}

		public override int CompareTo(object obj)
		{
			return dust.type - (obj as DustButton).dust.type;
		}
	}
}
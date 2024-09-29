using DragonLens.Content.Filters;
using DragonLens.Content.Filters.ProjectileFilters;
using DragonLens.Content.GUI;
using DragonLens.Content.GUI.FieldEditors;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Spawners
{
	internal class ProjectileSpawner : BrowserTool<ProjectileBrowser>
	{
		public override string IconKey => "ProjectileSpawner";

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(ProjectileBrowser.selected is null ? 0 : ProjectileBrowser.selected.type);
			writer.Write(ProjectileBrowser.selected is null ? 0 : ProjectileBrowser.selected.damage);
			writer.Write(ProjectileBrowser.selected is null ? 0 : ProjectileBrowser.selected.knockBack);
			writer.WriteVector2(Main.MouseWorld);

			writer.WriteVector2(ProjectileBrowser.velocity);
			writer.Write(ProjectileBrowser.ai0);
			writer.Write(ProjectileBrowser.ai1);
			writer.Write(ProjectileBrowser.ai2);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			int type = reader.ReadInt32();
			int damage = reader.ReadInt32();
			float knockBack = reader.ReadSingle();
			Vector2 pos = reader.ReadVector2();

			Vector2 velocity = reader.ReadVector2();
			float ai0 = reader.ReadSingle();
			float ai1 = reader.ReadSingle();
			float ai2 = reader.ReadSingle();

			Projectile.NewProjectile(null, Main.MouseWorld, velocity, type, damage, knockBack, Main.myPlayer, ai0, ai1);

			if (Main.netMode == NetmodeID.Server && sender >= 0)
			{
				if (ProjectileBrowser.selected != null)
				{
					ProjectileBrowser.selected.type = type;
					ProjectileBrowser.selected.damage = damage;
					ProjectileBrowser.selected.knockBack = knockBack;
				}

				Main.mouseX = (int)pos.X;
				Main.mouseY = (int)pos.Y;

				ProjectileBrowser.velocity = velocity;
				ProjectileBrowser.ai0 = ai0;
				ProjectileBrowser.ai1 = ai1;
				ProjectileBrowser.ai2 = ai2;

				NetSend(-1, sender);
			}
		}

		public static string GetText(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.ProjectileSpawner.{key}", args);
		}
	}

	internal class ProjectileBrowser : Browser
	{
		public static Projectile selected;

		public static Vector2 velocity;
		public static Vector2Editor velocityEditor;

		public static float ai0;
		public static FloatEditor ai0Editor;

		public static float ai1;
		public static FloatEditor ai1Editor;

		public static float ai2;
		public static FloatEditor ai2Editor;

		public override string Name => ProjectileSpawner.GetText("DisplayName");

		public override string IconTexture => "ProjectileSpawner";

		public override Vector2 DefaultPosition => new(0.5f, 0.4f);

		public override void PostInitialize()
		{
			velocityEditor = new(ProjectileSpawner.GetText("FieldEditors.Velocity"), n => velocity = n, Vector2.Zero);
			Append(velocityEditor);

			ai0Editor = new("ai 0", n => ai0 = n, 0);
			Append(ai0Editor);

			ai1Editor = new("ai 1", n => ai1 = n, 0);
			Append(ai1Editor);

			ai2Editor = new("ai 2", n => ai2 = n, 0);
			Append(ai2Editor);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			base.AdjustPositions(newPos);

			float nextY = 0;

			velocityEditor.Left.Set(newPos.X - 160, 0);
			velocityEditor.Top.Set(newPos.Y + nextY, 0);
			nextY += velocityEditor.Height.Pixels + 4;

			ai0Editor.Left.Set(newPos.X - 160, 0);
			ai0Editor.Top.Set(newPos.Y + nextY, 0);
			nextY += ai0Editor.Height.Pixels + 4;

			ai1Editor.Left.Set(newPos.X - 160, 0);
			ai1Editor.Top.Set(newPos.Y + nextY, 0);
			nextY += ai1Editor.Height.Pixels + 4;

			ai2Editor.Left.Set(newPos.X - 160, 0);
			ai2Editor.Top.Set(newPos.Y + nextY, 0);
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<ProjectileButton>();
			// `0` corresponds to ProjectileID.None - that is, no projectile.
			for (int k = 1; k < ProjectileLoader.ProjectileCount; k++)
			{
				var proj = new Projectile();
				proj.SetDefaults(k);

				buttons.Add(new ProjectileButton(proj, this));
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.ProjectileSpawner.FilterCategories.Mod");
			filters.AddFilter(new Filter(Assets.Filters.Vanilla, "Tools.ProjectileSpawner.Filters.Vanilla", n => !(n is ProjectileButton && (n as ProjectileButton).proj.ModProjectile is null)) { isModFilter = true });

			foreach (Mod mod in ModLoader.Mods.Where(n => n.GetContent<ModProjectile>().Count() > 0))
			{
				filters.AddFilter(new ProjectileModFilter(mod));
			}

			filters.AddSeperator("Tools.ProjectileSpawner.FilterCategories.Hostility");
			filters.AddFilter(new Filter(Assets.Filters.Friendly, "Tools.ProjectileSpawner.Filters.Friendly", n => !(n is ProjectileButton && (n as ProjectileButton).proj.friendly)));
			filters.AddFilter(new Filter(Assets.Filters.Hostile, "Tools.ProjectileSpawner.Filters.Hostile", n => !(n is ProjectileButton && (n as ProjectileButton).proj.hostile)));
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("ID", (a, b) => (a as ProjectileButton).proj.type - (b as ProjectileButton).proj.type));
			SortModes.Add(new("Alphabetical", (a, b) => a.Identifier.CompareTo(b.Identifier)));
			SortModes.Add(new("Damage", (a, b) => -1 * ((a as ProjectileButton).proj.damage - (b as ProjectileButton).proj.damage)));

			SortFunction = SortModes.First().Function;
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			base.DraggableUdpate(gameTime);

			if (selected != null)
				Main.LocalPlayer.mouseInterface = true;

			if (BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll($"DragonLens: {Name}");
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			base.SafeClick(evt);

			if (selected != null && !BoundingBox.Contains(Main.MouseScreen.ToPoint()) && !filters.IsMouseHovering && !velocityEditor.IsMouseHovering && !ai0Editor.IsMouseHovering && !ai1Editor.IsMouseHovering && !ai2Editor.IsMouseHovering)
			{
				PlayerInput.SetZoom_World();
				Projectile.NewProjectile(null, Main.MouseWorld, velocity, selected.type, selected.damage, selected.knockBack, Main.myPlayer, ai0, ai1, ai2);
				ToolHandler.NetSend<ProjectileSpawner>();
				PlayerInput.SetZoom_UI();
			}
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
				Main.instance.LoadProjectile(selected.type);
				Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[selected.type].Value;

				float scale = 1;
				if (tex.Width > 32 || tex.Height > 32)
					scale = 32f / Math.Max(tex.Width, tex.Height);

				spriteBatch.Draw(tex, Main.MouseScreen + Vector2.One * 8 + tex.Size(), new Rectangle(0, 0, tex.Width, tex.Height), Color.White * 0.5f, 0, new Vector2(tex.Width, tex.Height) / 2, scale, 0, 0);
			}

			// Set name here to receive game language selection changes in real time
			// This is a bit of a hack, but it works
			velocityEditor.name = ProjectileSpawner.GetText("FieldEditors.Velocity");

			base.Draw(spriteBatch);
		}
	}

	internal class ProjectileButton : BrowserButton
	{
		public Projectile proj;
		public string name;

		public override string Identifier => name;
		public override string Key => (proj.ModProjectile?.Mod?.Name ?? "Terraria") + ":" + (proj.ModProjectile?.Name ?? ProjectileID.Search.GetName(proj.type));

		public ProjectileButton(Projectile proj, Browser browser) : base(browser)
		{
			this.proj = proj;

			try
			{
				name = proj.Name;
			}
			catch
			{
				Main.NewText(ProjectileSpawner.GetText("NameExceptionMessage", proj.ModProjectile.Name, proj.ModProjectile.Mod.DisplayName));
				name = ProjectileSpawner.GetText("NameException", proj.ModProjectile.Mod.DisplayName);
			}
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconBox)
		{
			Main.instance.LoadProjectile(proj.type);
			Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[proj.type].Value;

			Rectangle frame = tex.Frame(verticalFrames: Main.projFrames[proj.type]);
			if (tex.Width > tex.Height)
				frame = tex.Frame(horizontalFrames: Main.projFrames[proj.type]);

			float scale = iconBox.Width / 52f;
			if (frame.Width > 32 || frame.Height > 32)
				scale *= 32f / Math.Max(frame.Width, frame.Height);

			spriteBatch.Draw(tex, iconBox.Center(), frame, Color.White, 0, frame.Size() / 2, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(proj.Name);
				Tooltip.SetTooltip(ProjectileSpawner.GetText("ProjectileType", proj.type));
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			ProjectileBrowser.selected = proj;
			Main.NewText(ProjectileSpawner.GetText("Selected", Identifier));
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			Projectile.NewProjectile(null, Main.LocalPlayer.Center, ProjectileBrowser.velocity, proj.type, proj.damage, proj.knockBack, Main.myPlayer, ProjectileBrowser.ai0, ProjectileBrowser.ai1, ProjectileBrowser.ai2);
		}
	}
}
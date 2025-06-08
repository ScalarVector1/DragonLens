using DragonLens.Content.Filters.AssetFilters;
using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Spawners;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria.ModLoader.Assets;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;

namespace DragonLens.Content.Tools.Developer
{
	internal class AssetManager : BrowserTool<AssetBrowser>
	{
		public static ShaderCompiler compiler = new();

		public override string IconKey => "AssetManager";

		public override bool SyncOnClientJoint => false;

		public override void OnActivate()
		{
			AssetBrowser state = UILoader.GetUIState<AssetBrowser>();
			state.Refresh();
			base.OnActivate();
		}
	}

	internal class AssetBrowser : Browser
	{
		public ReloadButton reloadButton;

		public override string Name => ModContent.GetInstance<AssetManager>().DisplayName;

		public override string IconTexture => "AssetManager";

		public override Vector2 DefaultPosition => new(0.2f, 0.4f);

		public override void PostInitialize()
		{
			reloadButton = new(this);
			Append(reloadButton);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			base.AdjustPositions(newPos);

			reloadButton.Left.Set(newPos.X - 50, 0);
			reloadButton.Top.Set(newPos.Y, 0);
		}

		public override void PopulateGrid(UIGrid grid)
		{
			var buttons = new List<BrowserButton>();

			foreach (Mod mod in ModLoader.Mods)
			{
				if (mod is DragonLens)
					continue;

				AssetRepository repo = mod.Assets;

				lock (repo._requestLock)
				{
					foreach (KeyValuePair<string, IAsset> pair in repo._assets)
					{
						if (pair.Value is Asset<Texture2D> texAsset)
						{
							buttons.Add(new TextureAssetButton(texAsset, mod, this));
						}
						else if (pair.Value is Asset<Effect> shaderAsset)
						{
							buttons.Add(new ShaderAssetButton(shaderAsset, mod, this));
						}
					}
				}
			}

			grid.AddRange(buttons);
		}

		public override void SetupFilters(FilterPanel filters)
		{
			filters.AddSeperator("Tools.AssetManager.FilterCategories.Mod");

			foreach (Mod mod in ModLoader.Mods)
			{
				if (mod is DragonLens)
					continue;

				filters.AddFilter(new AssetModFilter(mod));
			}

			filters.AddSeperator("Tools.AssetManager.FilterCategories.Type");
			filters.AddFilter(new(Assets.Filters.Vanilla, "Tools.AssetManager.Filters.Texture", n => n is not TextureAssetButton));
			filters.AddFilter(new(Assets.Filters.Magic, "Tools.AssetManager.Filters.Effect", n => n is not ShaderAssetButton));
		}

		public override void SetupSorts()
		{
			SortModes.Add(new("Alphabetical", (a, b) => a.Identifier.CompareTo(b.Identifier)));

			SortFunction = SortModes.First().Function;
		}
	}

	internal class TextureAssetButton : BrowserButton
	{
		public Asset<Texture2D> asset;
		public Mod mod;

		public override string Identifier => (mod?.Name ?? "Terraria") + " - " + Path.GetFileName(asset.Name);

		public override string Key => (mod?.Name ?? "Terraria") + ":" + asset.Name;

		public TextureAssetButton(Asset<Texture2D> asset, Mod mod, Browser browser) : base(browser)
		{
			this.asset = asset;
			this.mod = mod;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			AssetRepository repo = mod.Assets;

			if (mod.RootContentSource is TModContentSource)
			{
				if (string.IsNullOrEmpty(mod.SourceFolder))
				{
					Main.NewText("Mod is not loaded from source, reloading this asset will have no effect.", Color.Red);
					return;
				}

				string path = Path.Combine(mod.SourceFolder, asset.Name);
				path = Path.ChangeExtension(path, "png");

				if (Path.Exists(path))
				{
					using Stream stream = File.OpenRead(path);
					{
						var newValue = Texture2D.FromStream(Main.graphics.GraphicsDevice, stream);
						asset.ownValue = newValue;

						Main.NewText($"Loaded new value from [c/CCCCFF:{path}]");
					}
				}
				else
				{
					Main.NewText($"Could not reload as path does not exist! Tried: [c/FFCCCC:{path}]");
				}
			}
			else
			{
				Main.NewText("You cannot reload this asset");
			}
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			string path = Path.Join(Main.SavePath, "DragonLensAssetExports", mod.Name, asset.Name);
			path = Path.ChangeExtension(path, ".png");

			if (!File.Exists(path))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(path));

				Stream stream = File.Create(path);
				asset.Value.SaveAsPng(stream, asset.Width(), asset.Height());
				stream.Close();
			}
			else
			{
				Stream stream = File.OpenWrite(path);
				asset.Value.SaveAsPng(stream, asset.Width(), asset.Height());
				stream.Close();
			}

			Main.NewText($"Exported to [c/CCCCFF:{path}]");
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea)
		{
			float scale = Math.Min(iconArea.Width / (float)asset.Width(), iconArea.Height / (float)asset.Height());
			scale = Math.Min(1, scale);

			spriteBatch.Draw(asset.Value, iconArea.Center.ToVector2(), null, Color.White, 0, asset.Size() / 2f, scale, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);

				string tip = asset.Name + "\n\n";

				if (!string.IsNullOrEmpty(mod.SourceFolder))
					tip += "Click to force reload";
				else
					tip += "[c/FFCCCC:Not loaded from source]";

				tip += "\nRight click to export";

				Tooltip.SetTooltip(tip);
			}
		}
	}

	internal class ShaderAssetButton : BrowserButton
	{
		public Asset<Effect> asset;
		public Mod mod;

		public string sourcePath;

		public override string Identifier => (mod?.Name ?? "Terraria") + " - " + Path.GetFileName(asset.Name);

		public override string Key => (mod?.Name ?? "Terraria") + ":" + asset.Name;

		public ShaderAssetButton(Asset<Effect> asset, Mod mod, Browser browser) : base(browser)
		{
			this.asset = asset;
			this.mod = mod;

			if (!string.IsNullOrEmpty(mod.SourceFolder))
			{
				string xnbPath = Path.Combine(mod.SourceFolder, asset.Name);
				xnbPath = Path.ChangeExtension(xnbPath, "xnb");

				this.sourcePath = FindMatchingFx(xnbPath, mod.SourceFolder);
			}
		}

		public string FindMatchingFx(string xnbPath, string searchRoot)
		{
			string xnbFileName = Path.GetFileNameWithoutExtension(xnbPath);

			string matchingFx = Directory
				.EnumerateFiles(searchRoot, "*.fx", SearchOption.AllDirectories)
				.FirstOrDefault(fx => Path.GetFileNameWithoutExtension(fx).Equals(xnbFileName, StringComparison.OrdinalIgnoreCase));

			return matchingFx;
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			AssetRepository repo = mod.Assets;

			if (mod.RootContentSource is TModContentSource)
			{
				if (string.IsNullOrEmpty(mod.SourceFolder))
				{
					Main.NewText("Mod is not loaded from source, reloading this asset will have no effect.", Color.Red);
					return;
				}

				if (!string.IsNullOrEmpty(sourcePath) && File.Exists(sourcePath))
					ReloadFromSource();
				else
					ReloadFromBinary();
			}
		}

		public async void ReloadFromSource()
		{
			Main.NewText($"Starting shader compilation for {asset.Name}", Color.SkyBlue);

			string xnbPath = Path.Combine(mod.SourceFolder, asset.Name);
			xnbPath = Path.ChangeExtension(xnbPath, "xnb");

			await AssetManager.compiler.StartShaderBuild(sourcePath, xnbPath);
			Main.QueueMainThreadAction(ReloadFromBinary);
		}

		public void ReloadFromBinary()
		{
			string path = Path.Combine(mod.SourceFolder, asset.Name);
			path = Path.ChangeExtension(path, "xnb");

			Stream stream = File.OpenRead(path);

			byte[] xnbHeader = new byte[4];

			stream.Read(xnbHeader, 0, xnbHeader.Length);
			if (xnbHeader[0] == 'X' &&
				xnbHeader[1] == 'N' &&
				xnbHeader[2] == 'B' &&
				ContentManager.targetPlatformIdentifiers.Contains((char)xnbHeader[3]))
			{
				using var xnbReader = new BinaryReader(stream);
				using ContentReader reader = Main.instance.Content.GetContentReaderFromXnb(asset.Name, ref stream, xnbReader, (char)xnbHeader[3], null);

				var newValue = reader.ReadAsset<Effect>() as Effect;
				asset.ownValue = newValue;

				Main.NewText($"Loaded new value from [c/CCCCFF:{path}]");
			}
		}

		public override void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea)
		{
			Texture2D tex = Assets.Filters.Magic.Value;
			spriteBatch.Draw(tex, iconArea.Center.ToVector2(), null, Color.White, 0, tex.Size() / 2f, 1f, 0, 0);

			string preview = Path.GetFileName(asset.Name);

			Color color = string.IsNullOrEmpty(sourcePath) ? Color.Gray : Color.Lerp(Main.DiscoColor, Color.White, 0.5f);

			Utils.DrawBorderString(spriteBatch, preview[..Math.Min(preview.Length, 4)], iconArea.Center(), color, 0.7f * (iconArea.Width / 32f), 0.5f, 0.1f);

			if (IsMouseHovering)
			{
				Tooltip.SetName(Identifier);

				string tip = asset.Name + "\n\n";

				if (!string.IsNullOrEmpty(mod.SourceFolder))
					tip += "Click to force reload";
				else
					tip += "[c/FFCCCC:Not loaded from source]";

				if (sourcePath != null)
					tip += $"\n\nSource code found at [c/CCCCFF:{sourcePath}]";
				else
					tip += "\n\n[c/FFAAAA:Could not find source code...]";

				Tooltip.SetTooltip(tip);
			}
		}
	}

	internal class ReloadButton : SmartUIElement
	{
		public AssetBrowser parent;

		public ReloadButton(AssetBrowser parent)
		{
			this.parent = parent;
			Width.Set(42, 0);
			Height.Set(42, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			parent.options.Clear();
			parent.PopulateGrid(parent.options);
			parent.SortGrid();
			parent.Recalculate();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ThemeHandler.ButtonColor);
			spriteBatch.Draw(Assets.GUI.Refresh.Value, GetDimensions().Center(), null, Color.White, 0, Assets.GUI.Refresh.Size() / 2f, 1, 0, 0);

			if (IsMouseHovering)
			{
				Tooltip.SetName("Re-scan");
				Tooltip.SetTooltip("re-scans for modded assets. This may be needed if mods lazy load assets.");
			}
		}
	}
}
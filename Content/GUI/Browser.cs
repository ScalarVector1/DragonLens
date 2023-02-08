using DragonLens.Configs;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.UI.Elements;
using Terraria.UI;
using FixedUIScrollbar = Terraria.GameContent.UI.Elements.FixedUIScrollbar;

namespace DragonLens.Content.GUI
{
	internal abstract class Browser : DraggableUIState
	{
		private UIGrid options;
		private FixedUIScrollbar scrollBar;
		internal SearchBar searchBar;

		private ToggleButton listButton;

		public bool initialized;
		public bool listMode;

		public abstract string Name { get; }

		public virtual string IconTexture => "DragonLens/Assets/Tools/TestTool";

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 500, 64);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public abstract void PopulateGrid(UIGrid grid);

		public void AddButton(BrowserButton button)
		{
			//Might need to add positioning code here?
			options.Append(button);
		}

		public void SortGrid()
		{
			options.UpdateOrder();
		}

		public void FilterGrid(Func<BrowserButton, bool> filter)
		{
			PopulateGrid(options);
			options.Children.ToList().RemoveAll(n => !filter((BrowserButton)n));
		}

		public virtual void PostInitialize() { }

		public sealed override void SafeOnInitialize()
		{
			width = 500;
			height = 600;

			scrollBar = new(UserInterface);
			scrollBar.Width.Set(24, 0);
			scrollBar.Height.Set(480, 0);
			Append(scrollBar);

			options = new();
			options.Width.Set(460, 0);
			options.Height.Set(480, 0);
			options.SetScrollbar(scrollBar);
			options.ListPadding = 0;
			Append(options);

			searchBar = new();
			searchBar.Width.Set(200, 0);
			searchBar.Height.Set(32, 0);
			Append(searchBar);

			listButton = new("DragonLens/Assets/GUI/Play", () => listMode);
			listButton.OnClick += (n, k) => listMode = !listMode;
			Append(listButton);

			PostInitialize();
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			scrollBar.Left.Set(newPos.X + 464, 0);
			scrollBar.Top.Set(newPos.Y + 110, 0);

			options.Left.Set(newPos.X + 10, 0);
			options.Top.Set(newPos.Y + 110, 0);

			searchBar.Left.Set(newPos.X + 10, 0);
			searchBar.Top.Set(newPos.Y + 66, 0);

			listButton.Left.Set(newPos.X + 220, 0);
			listButton.Top.Set(newPos.Y + 66, 0);
		}

		public void Refresh()
		{
			RemoveAllChildren();
			OnInitialize();
			PopulateGrid(options);
			SortGrid();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var target = new Rectangle((int)basePos.X, (int)basePos.Y, 500, 600);

			GUIHelper.DrawBox(spriteBatch, target, ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 48);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D gridBack = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			var gridBackTarget = options.GetDimensions().ToRectangle();
			gridBackTarget.Inflate(4, 4);
			spriteBatch.Draw(gridBack, gridBackTarget, Color.Black * 0.25f);

			Texture2D icon = ModContent.Request<Texture2D>(IconTexture).Value;
			spriteBatch.Draw(icon, basePos + Vector2.One * 16, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, Name, basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.6f);

			base.Draw(spriteBatch);
		}

		public override void Click(UIMouseEvent evt)
		{
			//stop searching if you click outside the browser
			if (!BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				searchBar.typing = false;
		}
	}

	internal abstract class BrowserButton : UIElement
	{
		public Browser parent;

		public abstract string Identifier { get; }

		public BrowserButton(Browser parent)
		{
			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);

			Width.Set(size, 0);
			Height.Set(size, 0);

			this.parent = parent;
		}

		public override void Update(GameTime gameTime)
		{
			//Will likely need a better solution to optimize when not constantly searching
			if (!Identifier.ToLower().Contains(parent.searchBar.searchingFor.ToLower()))
			{
				Width.Set(0, 0);
				Height.Set(0, 0);

				MarginLeft = 0;
				MarginRight = 0;
				MarginTop = 0;
				MarginBottom = 0;
				return;
			}

			if (parent.listMode)
				UpdateAsList();
			else
				UpdateAsGrid();

			base.Update(gameTime);
		}

		private void UpdateAsGrid()
		{
			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);

			if (GetDimensions().Width != size || MarginLeft == 0)
			{
				Width.Set(size, 0);
				Height.Set(size, 0);

				MarginLeft = 2;
				MarginRight = 2;
				MarginTop = 2;
				MarginBottom = 2;
			}
		}

		private void UpdateAsList()
		{
			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);

			Width.Set(Parent.GetDimensions().Width - 24, 0);
			Height.Set(size, 0);

			MarginLeft = 2;
			MarginRight = 2;
			MarginTop = 2;
			MarginBottom = 2;
		}

		public virtual void SafeDraw(SpriteBatch spriteBatch, Rectangle iconArea) { }

		public sealed override void Draw(SpriteBatch spriteBatch)
		{
			if (GetDimensions().Width <= 0)
				return;

			int size = (int)MathHelper.Clamp(ModContent.GetInstance<GUIConfig>().browserButtonSize, 36, 108);

			var drawBox = GetDimensions().ToRectangle();

			if (parent.listMode)
				drawBox.Width = size;

			if (parent.listMode)
			{
				GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().backgroundColor);
				Utils.DrawBorderStringBig(spriteBatch, Identifier, GetDimensions().Position() + new Vector2(size + 10, size / 2f + 4), Color.White, size / 36f / 3f, 0, 0.5f);
			}

			GUIHelper.DrawBox(spriteBatch, drawBox, ModContent.GetInstance<GUIConfig>().buttonColor);
			SafeDraw(spriteBatch, drawBox);

			base.Draw(spriteBatch);
		}
	}

	internal class SearchBar : UIElement
	{
		public bool typing;

		public string searchingFor = "";

		public override void Click(UIMouseEvent evt)
		{
			typing = true;
		}

		public override void Update(GameTime gameTime)
		{
			if (typing)
			{
				if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
					typing = false;

				PlayerInput.WritingText = true;
				Main.instance.HandleIME();

				string newText = Main.GetInputText(searchingFor);

				if (newText != searchingFor)
				{
					searchingFor = newText;
					(Parent as Browser)?.SortGrid();
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor);

			if (typing)
			{
				GUIHelper.DrawOutline(spriteBatch, GetDimensions().ToRectangle(), ModContent.GetInstance<GUIConfig>().buttonColor.InvertColor());
			}

			Vector2 pos = GetDimensions().Position() + Vector2.One * 4;

			string displayed = searchingFor;

			if (typing && Main.GameUpdateCount % 20 < 10)
				displayed += "|";

			Utils.DrawBorderString(spriteBatch, displayed, pos, Color.White);
		}
	}
}

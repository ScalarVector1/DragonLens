using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using StructureHelper.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI.Elements;
using Terraria.Social.Steam;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Paint : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/Paint";

		public override string DisplayName => "Tile painter";

		public override string Description => "Copy/paste regions of the world";

		public override void OnActivate()
		{
			PaintWindow state = UILoader.GetUIState<PaintWindow>();
			state.visible = !state.visible;

			if (!state.firstSet)
			{
				state.RemoveAllChildren();
				state.OnInitialize();
				state.firstSet = true;
			}
		}
	}

	internal class PaintWindow : DraggableUIState
	{
		public bool firstSet;

		public UIGrid structureButtons;
		public FixedUIScrollbar structureScroll;
		public ToggleButton sampleButton;
		public AdPanel adPanel;

		private Rectangle target;
		private bool selecting;
		private bool selectingSecondPoint;

		private Point16 lastPlaced;

		public StructureButton structure;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 400, 32);

		public override Vector2 DefaultPosition => new(0.7f, 0.5f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 400;
			height = 500;

			adPanel = new();
			Append(adPanel);

			sampleButton = new("DragonLens/Assets/GUI/Picker", () => selecting, "Create structure");
			sampleButton.OnClick += (a, b) => selecting = !selecting;
			Append(sampleButton);

			structureScroll = new(UserInterface);
			structureScroll.Height.Set(400, 0);
			structureScroll.Width.Set(16, 0);
			Append(structureScroll);

			structureButtons = new();
			structureButtons.Width.Set(300, 0);
			structureButtons.Height.Set(400, 0);
			structureButtons.SetScrollbar(structureScroll);
			Append(structureButtons);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			adPanel.Left.Set(newPos.X + 410, 0);
			adPanel.Top.Set(newPos.Y, 0);

			sampleButton.Left.Set(newPos.X + 344, 0);
			sampleButton.Top.Set(newPos.Y + 80, 0);

			structureScroll.Left.Set(newPos.X + 310, 0);
			structureScroll.Top.Set(newPos.Y + 80, 0);

			structureButtons.Left.Set(newPos.X + 10, 0);
			structureButtons.Top.Set(newPos.Y + 80, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			if (selecting && !sampleButton.IsMouseHovering) //otherwise the first point is always the select button lol
			{
				if (!selectingSecondPoint)
				{
					target = new Rectangle(Player.tileTargetX, Player.tileTargetY, 0, 0);
					selectingSecondPoint = true;

					Main.NewText("First point set!");
				}
				else
				{
					var secondPoint = new Point16(Player.tileTargetX, Player.tileTargetY);

					if (secondPoint.X < target.X || secondPoint.Y < target.Y)
					{
						Point16 temp = secondPoint;
						secondPoint = target.TopLeft().ToPoint16();
						target = new Rectangle(temp.X, temp.Y, 0, 0);
					}

					target.Width = secondPoint.X - target.X;
					target.Height = secondPoint.Y - target.Y;

					structureButtons.Add(new StructureButton(this, StructureHelper.Saver.SaveStructure(target)));
					selecting = false;
					selectingSecondPoint = false;

					Main.NewText("Structure saved to paint menu!");
				}
			}
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (selecting || structure != null)
				Main.LocalPlayer.mouseInterface = true;

			if (Main.mouseLeft && structure != null)
			{
				var placeTarget = new Point16(Player.tileTargetX, Player.tileTargetY);

				//Prevents re-placing the same structure in the same place which could cause lag
				if (placeTarget != lastPlaced)
				{
					StructureHelper.Generator.Generate(structure.strucutre, placeTarget);
					lastPlaced = placeTarget;

					for (int x = -1; x <= structure.strucutre.GetInt("Width") + 1; x++)
					{
						WorldGen.SquareTileFrame(placeTarget.X + x, placeTarget.Y, true);
						WorldGen.SquareTileFrame(placeTarget.X + x, placeTarget.Y + 1, true);

						if (x <= 0 || x >= structure.strucutre.GetInt("Width"))
						{
							for (int y = -1; y <= structure.strucutre.GetInt("Height") + 1; y++)
							{
								WorldGen.SquareTileFrame(placeTarget.X + x, placeTarget.Y + y, true);
							}
						}

						WorldGen.SquareTileFrame(placeTarget.X + x, placeTarget.Y + structure.strucutre.GetInt("Height") + 1, true);
						WorldGen.SquareTileFrame(placeTarget.X + x, placeTarget.Y + structure.strucutre.GetInt("Height"), true);
					}
				}
			}
		}

		public override void RightClick(UIMouseEvent evt)
		{
			if (structure != null)
				structure = null;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, width, height), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			spriteBatch.Draw(background, structureButtons.GetDimensions().ToRectangle(), Color.Black * 0.25f);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ModContent.Request<Texture2D>("DragonLens/Assets/Tools/Paint").Value;
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, "Tile painter", basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			/*
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y + 180, 400, 80), ModContent.GetInstance<GUIConfig>().backgroundColor);

			string tips = "Try out StructureHelper for more features and the ability to save structures to external files!";
			Utils.DrawBorderString(spriteBatch, tips, basePos + new Vector2(24, 190), Color.White, 0.8f);
			*/

			if (structure != null && !selecting)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				var pos = new Point16(Player.tileTargetX, Player.tileTargetY);
				Vector2 pos2 = pos.ToVector2() * 16 - Main.screenPosition;

				GUIHelper.DrawOutline(Main.spriteBatch, new Rectangle((int)pos2.X - 4, (int)pos2.Y - 4, structure.preview.Width + 8, structure.preview.Height + 8), Color.Red);

				Main.spriteBatch.Draw(structure.preview.preview, pos2, Color.White * 0.5f);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
			}

			if (selecting && selectingSecondPoint)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				var point1 = target.TopLeft().ToPoint16();
				var point2 = new Point16(Player.tileTargetX, Player.tileTargetY);
				var sampling = new Rectangle((point1.X > point2.X ? point2.X : point1.X) * 16, (point1.Y > point2.Y ? point2.Y : point1.Y) * 16, Math.Abs(point1.X - point2.X) * 16 + 16, Math.Abs(point1.Y - point2.Y) * 16 + 16);
				sampling.Offset((-Main.screenPosition).ToPoint());

				GUIHelper.DrawOutline(Main.spriteBatch, sampling, Color.Red);

				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
			}

			base.Draw(spriteBatch);
		}
	}

	internal class StructureButton : UIElement
	{
		public PaintWindow parent;
		public TagCompound strucutre;
		public StructurePreview preview;

		public Terraria.GameContent.UI.Elements.UIImageButton closeButton;

		public StructureButton(PaintWindow parent, TagCompound tag)
		{
			this.parent = parent;
			strucutre = tag;

			preview = new("Temporary structure " + strucutre.GetHashCode(), tag);

			Width.Set(140, 0);
			Height.Set(140, 0);

			closeButton = new Terraria.GameContent.UI.Elements.UIImageButton(ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Remove"));
			closeButton.Width.Set(16, 0);
			closeButton.Height.Set(16, 0);
			closeButton.Left.Set(120, 0);
			closeButton.Top.Set(4, 0);

			closeButton.OnClick += (a, b) =>
			{
				preview.Dispose();
				preview = null;
				parent.structureButtons.Remove(this);
			};

			Append(closeButton);
		}

		public override void Click(UIMouseEvent evt)
		{
			if (!closeButton.IsMouseHovering)
				parent.structure = this;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			dims.Inflate(-4, -4);

			if (preview != null)
			{
				Texture2D tex = preview.preview;
				float scale = 1f;

				if (tex.Width > dims.Width || tex.Height > dims.Width)
					scale = tex.Width > tex.Height ? dims.Width / (float)tex.Width : dims.Height / (float)tex.Height;

				spriteBatch.Draw(tex, GetDimensions().Center(), null, Color.White, 0, tex.Size() / 2f, scale, 0, 0);
			}

			base.Draw(spriteBatch);
		}
	}

	internal class AdPanel : UIElement
	{
		public AdPanel()
		{
			Width.Set(210, 0);
			Height.Set(500, 0);

			if (!ModLoader.HasMod("StructureHelper"))
			{
				var button = new AdButton();
				button.Left.Set(25, 0);
				button.Top.Set(390, 0);
				Append(button);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().backgroundColor);

			if (!ModLoader.HasMod("StructureHelper"))
			{
				ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
				string message = GUIHelper.WrapString("For more useful features, check out the full Structure Helper mod! You'll be able to: NEWBLOCK" +
					" > Export structures to files! NEWBLOCK" +
					" > Use null tiles to generate non-square structures! NEWBLOCK" +
					" > Place chests with custom, random loot pools! NEWBLOCK" +
					" > Generate structures in your own mods from files!", 160, font, 0.8f);

				Utils.DrawBorderString(spriteBatch, message, dims.TopLeft() + Vector2.One * 8, Color.White, 0.8f);
			}

			base.Draw(spriteBatch);
		}
	}

	internal class AdButton : UIElement
	{
		public AdButton()
		{
			Width.Set(160, 0);
			Height.Set(88, 0);
		}

		public override void Click(UIMouseEvent evt)
		{
			MethodInfo method = typeof(SteamedWraps).GetMethod("Download", BindingFlags.Static | BindingFlags.NonPublic);

			var publishId = new PublishedFileId_t(2790924965); //This is StructureHelpers ID on the workshop

			method.Invoke(null, new object[] { publishId, null, false });
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/StructureHelper").Value;
			spriteBatch.Draw(tex, dims.TopLeft() + Vector2.One * 4, Color.White);

			Utils.DrawBorderString(spriteBatch, "Download\n  Now!", dims.TopLeft() + new Vector2(88, 24), Color.White, 0.8f);

			base.Draw(spriteBatch);
		}
	}
}
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Steamworks;
using StructureHelper.Util;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.ModLoader.UI.Elements;
using Terraria.Social.Steam;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Paint : Tool
	{
		public override string IconKey => "Paint";

		public override void Load()
		{
			// Load the structure queue to generate images
			new PreviewRenderQueue().Load(Mod);
		}

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

		public static string GetTextValue(string key, params object[] args)
		{
			return LocalizationHelper.GetText($"Tools.Paint.{key}", args);
		}
	}

	internal class PaintWindow : DraggableUIState
	{
		public bool firstSet;

		public UIGrid structureButtons;
		public StyledScrollbar structureScroll;
		public ToggleButton sampleButton;
		public AdPanel adPanel;

		private Rectangle target;
		private bool selecting;
		private bool selectingSecondPoint;

		private Point16 lastPlaced;

		public StructureButton structure;
		public Point16 placeOffset;

		public Point16 PlaceTarget => new Point16(Player.tileTargetX, Player.tileTargetY) + placeOffset;

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

			if (!ModLoader.HasMod("StructureHelper"))
			{
				adPanel = new();
				Append(adPanel);
			}

			sampleButton = new("DragonLens/Assets/GUI/Picker", () => selecting, Paint.GetTextValue("CreateStructure"));
			sampleButton.OnLeftClick += (a, b) => selecting = !selecting;
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
			adPanel?.Left.Set(newPos.X + 410, 0);
			adPanel?.Top.Set(newPos.Y, 0);

			sampleButton.Left.Set(newPos.X + 344, 0);
			sampleButton.Top.Set(newPos.Y + 80, 0);

			structureScroll.Left.Set(newPos.X + 310, 0);
			structureScroll.Top.Set(newPos.Y + 80, 0);

			structureButtons.Left.Set(newPos.X + 10, 0);
			structureButtons.Top.Set(newPos.Y + 80, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (selecting && !sampleButton.IsMouseHovering) //otherwise the first point is always the select button lol
			{
				if (!selectingSecondPoint)
				{
					target = new Rectangle(Player.tileTargetX, Player.tileTargetY, 0, 0);
					selectingSecondPoint = true;

					Main.NewText(Paint.GetTextValue("FirstPointSet"));
				}
				else
				{
					var secondPoint = new Point16(Player.tileTargetX, Player.tileTargetY);
					var firstPoint = target.TopLeft().ToPoint16();

					target = new Rectangle(Math.Min(secondPoint.X, target.X), Math.Min(secondPoint.Y, target.Y), 0, 0);
					secondPoint = new Point16(Math.Max(secondPoint.X, firstPoint.X), Math.Max(secondPoint.Y, firstPoint.Y));

					target.Width = secondPoint.X - target.X;
					target.Height = secondPoint.Y - target.Y;

					structureButtons.Add(new StructureButton(this, StructureHelper.Saver.SaveStructure(target)));
					selecting = false;
					selectingSecondPoint = false;

					Recalculate();
					Recalculate();

					Main.NewText(Paint.GetTextValue("StructureSaved"));
				}
			}
		}

		public override void SafeRightClick(UIMouseEvent evt)
		{
			if (structure != null)
				structure = null;
		}

		public override void SafeScrollWheel(UIScrollWheelEvent evt)
		{
			if (structure != null)
			{
				if (Main.keyState.PressingShift())
					placeOffset = new Point16(placeOffset.X + (evt.ScrollWheelValue > 1 ? 1 : -1), placeOffset.Y);
				else
					placeOffset = new Point16(placeOffset.X, placeOffset.Y + (evt.ScrollWheelValue > 1 ? 1 : -1));
			}
		}

		public override void DraggableUdpate(GameTime gameTime)
		{
			if (selecting || structure != null)
				Main.LocalPlayer.mouseInterface = true;

			if (structure != null || BoundingBox.Contains(Main.MouseScreen.ToPoint()))
				PlayerInput.LockVanillaMouseScroll("DragonLens: Paint Tool");

			if (Main.mouseLeft && structure != null && !BoundingBox.Contains(Main.MouseScreen.ToPoint()))
			{
				//Prevents re-placing the same structure in the same place which could cause lag
				if (PlaceTarget != lastPlaced)
				{
					StructureHelper.Generator.Generate(structure.strucutre, PlaceTarget);
					lastPlaced = PlaceTarget;

					for (int x = -1; x <= structure.strucutre.GetInt("Width") + 1; x++)
					{
						WorldGen.SquareTileFrame(PlaceTarget.X + x, PlaceTarget.Y, true);
						WorldGen.SquareTileFrame(PlaceTarget.X + x, PlaceTarget.Y + 1, true);

						if (x <= 0 || x >= structure.strucutre.GetInt("Width"))
						{
							for (int y = -1; y <= structure.strucutre.GetInt("Height") + 1; y++)
							{
								WorldGen.SquareTileFrame(PlaceTarget.X + x, PlaceTarget.Y + y, true);
							}
						}

						WorldGen.SquareTileFrame(PlaceTarget.X + x, PlaceTarget.Y + structure.strucutre.GetInt("Height") + 1, true);
						WorldGen.SquareTileFrame(PlaceTarget.X + x, PlaceTarget.Y + structure.strucutre.GetInt("Height"), true);
					}
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, width, height), ThemeHandler.BackgroundColor);

			Texture2D background = Terraria.GameContent.TextureAssets.MagicPixel.Value;
			spriteBatch.Draw(background, structureButtons.GetDimensions().ToRectangle(), Color.Black * 0.25f);

			Texture2D back = Assets.GUI.Gradient.Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 400, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("Paint");
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, Paint.GetTextValue("DisplayName"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			if (structure != null && !selecting)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);

				Vector2 pos2 = PlaceTarget.ToVector2() * 16 - Main.screenPosition;

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

	internal class StructureButton : SmartUIElement
	{
		public PaintWindow parent;
		public TagCompound strucutre;
		public StructurePreview preview;

		public Terraria.GameContent.UI.Elements.UIImageButton closeButton;

		public StructureButton(PaintWindow parent, TagCompound tag)
		{
			this.parent = parent;
			strucutre = tag;

			// StructurePreview constructor requires a spritebatch to be active
			Main.spriteBatch.Begin();

			preview = new(Paint.GetTextValue("TempStructure", strucutre.GetHashCode()), tag);

			// End the spritebatch after the preview is created
			Main.spriteBatch.End();

			Width.Set(140, 0);
			Height.Set(140, 0);

			closeButton = new Terraria.GameContent.UI.Elements.UIImageButton(Assets.GUI.Remove);
			closeButton.Width.Set(16, 0);
			closeButton.Height.Set(16, 0);
			closeButton.Left.Set(120, 0);
			closeButton.Top.Set(4, 0);

			closeButton.OnLeftClick += (a, b) =>
			{
				preview.Dispose();
				preview = null;
				parent.structureButtons.Remove(this);

				if (parent.structure == this)
					parent.structure = null;
			};

			Append(closeButton);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (!closeButton.IsMouseHovering)
			{
				parent.structure = this;
				parent.placeOffset = new(); //reset placement offset when a new structure is selected
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			dims.Inflate(-4, -4);

			if (preview != null && preview.preview != null && !preview.preview.IsDisposed)
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

	internal class AdPanel : SmartUIElement
	{
		public AdPanel()
		{
			Width.Set(210, 0);
			Height.Set(500, 0);

			var button = new AdButton();
			button.Left.Set(25, 0);
			button.Top.Set(390, 0);
			Append(button);

			var closeButton = new Terraria.GameContent.UI.Elements.UIImageButton(Assets.GUI.Remove);
			closeButton.Width.Set(16, 0);
			closeButton.Height.Set(16, 0);
			closeButton.Left.Set(186, 0);
			closeButton.Top.Set(8, 0);
			closeButton.OnLeftClick += (a, b) => Remove();
			Append(closeButton);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.BackgroundColor);

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
			string message = GUIHelper.WrapString(Paint.GetTextValue("Advertise.Content"), 160, font, 0.8f);

			Utils.DrawBorderString(spriteBatch, message, dims.TopLeft() + Vector2.One * 8, Color.White, 0.8f);

			base.Draw(spriteBatch);
		}
	}

	internal class AdButton : SmartUIElement
	{
		public AdButton()
		{
			Width.Set(160, 0);
			Height.Set(88, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			MethodInfo method = typeof(SteamedWraps).GetMethod("Download", BindingFlags.Static | BindingFlags.NonPublic);

			var publishId = new PublishedFileId_t(2790924965); //This is StructureHelpers ID on the workshop

			method.Invoke(null, new object[] { publishId, null, false });

			Main.NewText(Paint.GetTextValue("Advertise.Downloaded"));
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ThemeHandler.ButtonColor);

			Texture2D tex = Assets.GUI.StructureHelper.Value;
			spriteBatch.Draw(tex, dims.TopLeft() + Vector2.One * 4, Color.White);

			Utils.DrawBorderString(spriteBatch, Paint.GetTextValue("Advertise.Title"), dims.TopLeft() + new Vector2(88, 24), Color.White, 0.8f);

			base.Draw(spriteBatch);
		}
	}
}
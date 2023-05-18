using DragonLens.Configs;
using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using System.Collections.Generic;
using System.IO;
using Terraria.ID;
using Terraria.ModLoader.IO;
using Terraria.UI;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class SpawnTool : Tool
	{
		public override string IconKey => "SpawnTool";

		public override void OnActivate()
		{
			SpawnWindow state = UILoader.GetUIState<SpawnWindow>();
			state.visible = !state.visible;
		}

		public override void SaveData(TagCompound tag)
		{
			tag["spawnRateModifier"] = SpawnSystem.spawnRateModifier;
		}

		public override void LoadData(TagCompound tag)
		{
			SpawnSystem.spawnRateModifier = tag.GetFloat("spawnRateModifier");
		}

		public override void SendPacket(BinaryWriter writer)
		{
			writer.Write(SpawnSystem.spawnRateModifier);
		}

		public override void RecievePacket(BinaryReader reader, int sender)
		{
			SpawnSystem.spawnRateModifier = reader.ReadSingle();

			if (Main.netMode == NetmodeID.Server)
				NetSend(-1, sender);
		}
	}

	internal class SpawnSystem : GlobalNPC
	{
		public static float spawnRateModifier = 1;

		public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
		{
			if (spawnRate == 0)
			{
				spawnRate = int.MaxValue;
				maxSpawns = 0;
				return;
			}

			spawnRate = (int)(spawnRate / spawnRateModifier);
			maxSpawns = (int)(maxSpawns * spawnRateModifier);
		}
	}

	internal class SpawnWindow : DraggableUIState
	{
		public SpawnSlider slider;

		public override Rectangle DragBox => new((int)basePos.X, (int)basePos.Y, 360, 32);

		public override Vector2 DefaultPosition => new(0.5f, 0.7f);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void SafeOnInitialize()
		{
			width = 360;
			height = 120;

			slider = new();
			Append(slider);
		}

		public override void AdjustPositions(Vector2 newPos)
		{
			slider.Left.Set(basePos.X + 25, 0);
			slider.Top.Set(basePos.Y + 75, 0);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			GUIHelper.DrawBox(spriteBatch, new Rectangle((int)basePos.X, (int)basePos.Y, 360, 120), ModContent.GetInstance<GUIConfig>().backgroundColor);

			Texture2D back = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/Gradient").Value;
			var backTarget = new Rectangle((int)basePos.X + 8, (int)basePos.Y + 8, 360, 40);
			spriteBatch.Draw(back, backTarget, Color.Black * 0.5f);

			Texture2D icon = ThemeHandler.GetIcon("SpawnTool");
			spriteBatch.Draw(icon, basePos + Vector2.One * 12, Color.White);

			Utils.DrawBorderStringBig(spriteBatch, LocalizationHelper.GetToolText("SpawnTool.EnemySpawnRate"), basePos + new Vector2(icon.Width + 24, 16), Color.White, 0.45f);

			base.Draw(spriteBatch);
		}
	}

	internal class SpawnSlider : SmartUIElement
	{
		public bool dragging;
		public float progress;

		public SpawnSlider()
		{
			Width.Set(300, 0);
			Height.Set(16, 0);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (dragging)
			{
				progress = MathHelper.Clamp((Main.MouseScreen.X - GetDimensions().Position().X) / GetDimensions().Width, 0, 1);

				SpawnSystem.spawnRateModifier = progress * 10;

				if (!Main.mouseLeft)
				{
					dragging = false;
					ToolHandler.NetSend<SpawnTool>();
				}
			}
			else
			{
				progress = SpawnSystem.spawnRateModifier * 0.1f;
			}
		}

		public override void SafeMouseDown(UIMouseEvent evt)
		{
			dragging = true;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			GUIHelper.DrawBox(spriteBatch, dims, ModContent.GetInstance<GUIConfig>().buttonColor);

			Texture2D tex = ModContent.Request<Texture2D>("DragonLens/Assets/GUI/CloudScale").Value;
			dims.Inflate(-4, -4);
			spriteBatch.Draw(tex, dims, Color.White);

			var tickTarget = new Rectangle(dims.X + 0 - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.Cyan);

			tickTarget = new Rectangle(dims.X + 30 - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.LimeGreen);

			tickTarget = new Rectangle(dims.X + 150 - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.Yellow);

			tickTarget = new Rectangle(dims.X + 294 - 5, dims.Y - 6, 10, 20);
			GUIHelper.DrawBox(spriteBatch, tickTarget, Color.Red);

			var draggerTarget = new Rectangle(dims.X + (int)(progress * dims.Width) - 6, dims.Y - 8, 12, 24);
			GUIHelper.DrawBox(spriteBatch, draggerTarget, ModContent.GetInstance<GUIConfig>().buttonColor);

			Utils.DrawBorderString(spriteBatch, LocalizationHelper.GetToolText("SpawnTool.SpawnRate", System.Math.Round(SpawnSystem.spawnRateModifier, 2)), dims.TopLeft() + new Vector2(0, -24), Color.White, 0.8f);

			Utils.DrawBorderString(spriteBatch, "0x", dims.TopLeft() + new Vector2(0, 14), Color.White, 0.8f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "1x", dims.TopLeft() + new Vector2(30, 14), Color.White, 0.8f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "5x", dims.TopLeft() + new Vector2(150, 14), Color.White, 0.8f, 0.5f);
			Utils.DrawBorderString(spriteBatch, "10x", dims.TopLeft() + new Vector2(294, 14), Color.White, 0.8f, 0.5f);
		}
	}
}
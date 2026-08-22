using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Terraria.ID;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class FastForward : Tool
	{
		public static int speedup = 0;

		public override string IconKey => "FastForward";

		public override bool HasRightClick => true;

		public override bool IsHighlighted => speedup > 0;

		public override void ResetForNonAdmin(Player player)
		{
			speedup = 0;
		}

		public override void OnActivate()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				Main.NewText(LocalizationHelper.GetToolText("FastForward.MultiplayerDisabled"), Color.Red);
				speedup = 0;
				return;
			}

			if (speedup < 4)
				speedup++;
			else
				speedup = 0;
		}

		public override void OnRightClick()
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				Main.NewText(LocalizationHelper.GetToolText("FastForward.MultiplayerDisabled"), Color.Red);
				speedup = 0;
				return;
			}

			if (speedup > 0)
				speedup--;
			else
				speedup = 4;
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (speedup > 0)
			{
				Texture2D tex = Assets.Misc.GlowAlpha.Value;
				Color color = new Color(150, 255, 170) * (speedup / 4f);
				color.A = 0;
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}

		public override void SaveData(TagCompound tag)
		{
			tag["speedup"] = speedup;
		}

		public override void LoadData(TagCompound tag)
		{
			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				speedup = 0;
				return;
			}

			speedup = tag.GetInt("speedup");
		}
	}

	internal class FastForwardSystem : ModSystem
	{
		public override void Load()
		{
			if (Main.dedServ)
			{
				return;
			}

			Terraria.On_Main.DoUpdate += UpdateExtraTimes;
		}

		public override void Unload()
		{
			Terraria.On_Main.DoUpdate -= UpdateExtraTimes;
		}

		private void UpdateExtraTimes(Terraria.On_Main.orig_DoUpdate orig, Main self, ref GameTime gameTime)
		{
			orig(self, ref gameTime);

			if (Main.netMode != NetmodeID.SinglePlayer)
			{
				return;
			}

			int extra = FastForward.speedup;
			if (extra <= 0)
			{
				return;
			}

			for (int k = 0; k < extra; k++)
			{
				orig(self, ref gameTime);
			}
		}
	}
}
using DragonLens.Core.Systems.ThemeSystem;
using DragonLens.Core.Systems.ToolSystem;
using DragonLens.Helpers;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria.ID;

namespace DragonLens.Content.Tools.Developer
{
	internal class Pause : Tool
	{
		public override string IconKey => "Pause";

		public override bool HasRightClick => true;

		public override void OnActivate()
		{
			if (Main.netMode == NetmodeID.SinglePlayer)
				FrameAdvanceSystem.paused = !FrameAdvanceSystem.paused;
			else
				Main.NewText("Pause is disabled in multiplayer", Color.Red);
		}

		public override void OnRightClick()
		{
			if (Main.netMode == NetmodeID.SinglePlayer && FrameAdvanceSystem.paused)
				FrameAdvanceSystem.stepReady = true;
			else
				Main.NewText("Cannot step while not paused", Color.Orange);
		}

		public override void DrawIcon(SpriteBatch spriteBatch, Rectangle position)
		{
			base.DrawIcon(spriteBatch, position);

			if (FrameAdvanceSystem.paused)
			{
				GUIHelper.DrawOutline(spriteBatch, new Rectangle(position.X - 4, position.Y - 4, 46, 46), ThemeHandler.ButtonColor.InvertColor());

				Texture2D tex = Assets.Misc.GlowAlpha.Value;
				Color color = Color.White;
				color.A = 0;
				var target = new Rectangle(position.X, position.Y, 38, 38);

				spriteBatch.Draw(tex, target, color);
			}
		}
	}

	internal class FrameAdvanceSystem : ModSystem
	{
		public static bool paused;
		public static bool stepReady;

		public override void Load()
		{
			IL_Main.DoUpdate += FrameAdvanceIL;
		}

		private void FrameAdvanceIL(ILContext il)
		{
			ILCursor c = new(il);

			c.TryGotoNext(n => n.MatchCall<Main>("DoUpdate_Enter_ToggleChat"));
			c.Index += 1;

			ILLabel skipLabel = il.DefineLabel(c.Next);

			c.EmitDelegate(Decide);

			c.Emit(OpCodes.Brtrue, skipLabel);

			c.Emit(OpCodes.Ret);
		}

		private bool Decide()
		{
			if (paused)
			{
				if (stepReady)
				{
					stepReady = false;
					return true;
				}

				return false;
			}

			return true;
		}
	}
}

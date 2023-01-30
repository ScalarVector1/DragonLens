using DragonLens.Core.Systems.ToolSystem;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace DragonLens.Content.Tools.Gameplay
{
	internal class Difficulty : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/Difficulty";

		public override string DisplayName => "Difficulty switcher";

		public override string Description => "Cycle through the 3 main game difficulties";

		public override void OnActivate()
		{
			if (!Main.expertMode)
			{
				Main.GameMode = GameModeID.Expert;
				Main.NewText("The game is now in expert mode.", new Color(255, 150, 0));
			}
			else if (!Main.masterMode)
			{
				Main.GameMode = GameModeID.Master;
				Main.NewText("The game is now in master mode.", new Color(255, 0, 0));
			}
			else
			{
				Main.GameMode = GameModeID.Normal;
				Main.NewText("The game is now in normal mode.", new Color(180, 180, 255));
			}
		}
	}
}

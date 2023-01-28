using DragonLens.Core.Systems.ToolSystem;
using Terraria;

namespace DragonLens.Content.Tools
{
	internal class TestTool : Tool
	{
		public override string Texture => "DragonLens/Assets/Tools/TestTool";

		public override string Name => "Test Tool";

		public override string Description => "Displays a message to test the functionality of DragonLens";

		public override void OnActivate()
		{
			Main.NewText("Test tool used!");
		}
	}
}

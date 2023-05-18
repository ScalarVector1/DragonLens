using DragonLens.Content.GUI;

namespace DragonLens.Core.Systems
{
	/// <summary>
	/// Drives the global cooldown on drawing browser buttons. This is used to ensure that browser buttons never try to draw all at once causing intense lag.
	/// </summary>
	internal class BrowserOptimizationHack : ModSystem
	{
		public override void PostUpdateEverything()
		{
			if (BrowserButton.drawDelayTimer > 0)
				BrowserButton.drawDelayTimer--;
		}
	}
}
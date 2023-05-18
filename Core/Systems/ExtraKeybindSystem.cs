using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolbarSystem;
using Microsoft.Xna.Framework.Input;
using System.Linq;
using Terraria.GameInput;

namespace DragonLens.Core.Systems
{
	internal class ExtraKeybindSystem : ModPlayer
	{
		public static ModKeybind collapseAll;

		public override void Load()
		{
			collapseAll = KeybindLoader.RegisterKeybind(Mod, "CollapseAll", Keys.None);
		}

		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (collapseAll.JustPressed)
			{
				if (ToolbarHandler.activeToolbars.Any(n => !n.collapsed))
				{
					foreach (Toolbar bar in ToolbarHandler.activeToolbars)
					{
						bar.collapsed = true;
						UILoader.GetUIState<Content.GUI.ToolbarState>().UpdateCollapse();
					}
				}
				else
				{
					foreach (Toolbar bar in ToolbarHandler.activeToolbars)
					{
						bar.collapsed = false;
						UILoader.GetUIState<Content.GUI.ToolbarState>().UpdateCollapse();
					}
				}
			}
		}
	}
}

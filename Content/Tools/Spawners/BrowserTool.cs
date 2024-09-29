using DragonLens.Content.GUI;
using DragonLens.Core.Loaders.UILoading;
using DragonLens.Core.Systems.ToolSystem;
using System.Collections.Generic;
using Terraria.ModLoader.IO;

namespace DragonLens.Content.Tools.Spawners
{
	internal abstract class BrowserTool : Tool
	{
		public List<string> Favorites = new();
	}

	internal abstract class BrowserTool<T> : BrowserTool where T : Browser
	{
		public override void OnActivate()
		{
			T state = UILoader.GetUIState<T>();
			state.visible = !state.visible;

			BrowserButton.drawDelayTimer = 2;

			if (!state.initialized)
			{
				UILoader.GetUIState<T>().Refresh();
				state.initialized = true;
			}
		}

		public override void SaveData(TagCompound tag)
		{
			T state = UILoader.GetUIState<T>();
			tag["list"] = state.listMode;
			tag["filtersVisible"] = state.filtersVisible;
			tag["buttonSize"] = state.buttonSize;
			tag["Favorites"] = Favorites;
		}

		public override void LoadData(TagCompound tag)
		{
			T state = UILoader.GetUIState<T>();
			state.listMode = tag.GetBool("list");
			state.filtersVisible = tag.GetBool("filtersVisible");
			state.buttonSize = tag.GetInt("buttonSize");
			Favorites = (List<string>)tag.GetList<string>("Favorites");

			state.tool = this;

			if (state.filtersVisible)
				state.filters.Width.Set(220, 0);
			else
				state.filters.Width.Set(0, 0);
		}
	}
}
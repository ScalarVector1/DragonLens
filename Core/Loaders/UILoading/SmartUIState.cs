using System.Collections.Generic;
using Terraria.UI;

namespace DragonLens.Core.Loaders.UILoading
{
	public abstract class SmartUIState : UIState
	{
		protected internal virtual UserInterface UserInterface { get; set; }
		
		public abstract int InsertionIndex(List<GameInterfaceLayer> layers);

		public virtual bool Visible { get; set; } = false;

		public virtual InterfaceScaleType Scale { get; set; } = InterfaceScaleType.UI;

		public virtual void Unload() { }
	}
}

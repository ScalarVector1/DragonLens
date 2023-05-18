using System;
using System.Linq;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class PlayerEditor : EntityEditor<Player>
	{
		public PlayerEditor(string name, Action<Player> onValueChanged, Player initialValue, Func<Player> listenForUpdates = null, string description = "") : base(name, onValueChanged, listenForUpdates, initialValue, description)
		{
		}

		public override Player OnSelect()
		{
			//TODO: Adjust for zoom/uiscale
			return Main.player.FirstOrDefault(n => n.active && n.Hitbox.Contains(Main.MouseWorld.ToPoint()));
		}
	}
}
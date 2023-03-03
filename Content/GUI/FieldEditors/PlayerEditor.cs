using System;
using System.Linq;
using Terraria;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class PlayerEditor : EntityEditor<Player>
	{
		public PlayerEditor(string name, Action<Entity> onValueChanged, Player initialValue = null, string description = "") : base(name, onValueChanged, initialValue, description)
		{
		}

		public override Player OnSelect()
		{
			//TODO: Adjust for zoom/uiscale
			return Main.player.FirstOrDefault(n => n.active && n.Hitbox.Contains(Main.MouseWorld.ToPoint()));
		}
	}
}

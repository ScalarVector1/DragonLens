using System;
using System.Linq;
using Terraria;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class NPCEditor : EntityEditor<NPC>
	{
		public NPCEditor(string name, Action<NPC> onValueChanged, NPC initialValue, Func<NPC> listenForUpdates = null, string description = "") : base(name, onValueChanged, listenForUpdates, initialValue, description)
		{
		}

		public override NPC OnSelect()
		{
			//TODO: Adjust for zoom/uiscale
			return Main.npc.FirstOrDefault(n => n.active && n.Hitbox.Contains(Main.MouseWorld.ToPoint()));
		}
	}
}
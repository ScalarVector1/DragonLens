using System;
using System.Linq;
using Terraria;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class ProjectileEditor : EntityEditor<Projectile>
	{
		public ProjectileEditor(string name, Action<Entity> onValueChanged, Projectile initialValue = null, string description = "") : base(name, onValueChanged, initialValue, description)
		{
		}

		public override Projectile OnSelect()
		{
			//TODO: Adjust for zoom/uiscale
			return Main.projectile.FirstOrDefault(n => n.active && n.Hitbox.Contains(Main.MouseWorld.ToPoint()));
		}
	}
}

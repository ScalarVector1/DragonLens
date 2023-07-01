using System;
using System.Linq;

namespace DragonLens.Content.GUI.FieldEditors
{
	internal class ProjectileEditor : EntityEditor<Projectile>
	{
		public ProjectileEditor(string name, Action<Projectile> onValueChanged, Projectile initialValue, Func<Projectile> listenForUpdates = null, string description = "") : base(name, onValueChanged, listenForUpdates, initialValue, description)
		{
		}

		public override Projectile OnSelect()
		{
			//TODO: Adjust for zoom/uiscale
			return Main.projectile.FirstOrDefault(n => n.active && n.Hitbox.Contains(Main.MouseWorld.ToPoint()));
		}
	}
}
using DragonLens.Content.GUI;
using DragonLens.Content.Tools.Spawners;
using ReLogic.Content;

namespace DragonLens.Content.Filters.ItemFilters
{
	internal class DamageClassFilter : Filter
	{
		public DamageClass damageClass;

		public DamageClassFilter(DamageClass damageClass, Asset<Texture2D> texture) : base(texture, "", n => FilterByDamageClass(n, damageClass))
		{
			this.damageClass = damageClass;
		}

		public override string Name => damageClass.DisplayName.Value.Trim();

		public override string Description => ItemSpawner.GetText("Filters.DamageClass.Description", damageClass.DisplayName.Value.Trim());

		public static bool FilterByDamageClass(BrowserButton button, DamageClass damageClass)
		{
			if (button is ItemButton)
			{
				var ib = button as ItemButton;

				if (ib.item.damage > 0 && ib.item.DamageType.CountsAsClass(damageClass))
					return false;
			}

			return true;
		}
	}
}
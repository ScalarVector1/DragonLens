using Terraria.DataStructures;

namespace DragonLens.Content.Tools.Multiplayer.Drawers
{
	public class ModifyPlayerDrawInfo : ModPlayer
	{
		public static bool ForceFullBrightOnce;

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if (!ForceFullBrightOnce)
				return;

			Player p = drawInfo.drawPlayer;

			drawInfo.colorEyeWhites = Color.White;
			drawInfo.colorEyes = p.eyeColor;
			drawInfo.colorHair = p.GetHairColor(useLighting: false);
			drawInfo.colorHead = p.skinColor;
			drawInfo.colorBodySkin = p.skinColor;
			drawInfo.colorLegs = p.skinColor;

			drawInfo.colorShirt = p.shirtColor;
			drawInfo.colorUnderShirt = p.underShirtColor;
			drawInfo.colorPants = p.pantsColor;
			drawInfo.colorShoes = p.shoeColor;

			drawInfo.colorArmorHead = Color.White;
			drawInfo.colorArmorBody = Color.White;
			drawInfo.colorArmorLegs = Color.White;
			drawInfo.colorMount = Color.White;
		}
	}
}
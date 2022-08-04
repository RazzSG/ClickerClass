using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ClickerClass.Items.Placeable.Mice
{
	public class MiceBrickWall : ModItem
	{
		public override void SetStaticDefaults()
		{
			SacrificeTotal = 400;
		}

		public override void SetDefaults()
		{
			Item.DefaultToPlacableWall((ushort)ModContent.WallType<Walls.MiceBrickWall>());
		}

		public override void AddRecipes()
		{
			CreateRecipe(4).AddIngredient(ModContent.ItemType<MiceBrick>(), 1).AddTile(TileID.WorkBenches).Register();
		}
	}
}
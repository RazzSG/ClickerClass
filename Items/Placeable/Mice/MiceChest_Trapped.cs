using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ClickerClass.Items.Placeable.Mice
{
	public class MiceChest_Trapped : ModItem
	{
		public override void SetStaticDefaults()
		{
			ItemID.Sets.TrapSigned[Item.type] = true;
			SacrificeTotal = 1;
		}

		public override string Texture => (GetType().Namespace + ".MiceChest").Replace('.', '/');

		public override void SetDefaults()
		{
			Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Mice.TrappedMiceChest>());
			Item.width = 26;
			Item.height = 22;
			Item.maxStack = 99;
			Item.value = Item.sellPrice(0, 0, 1, 0);
			Item.rare = ItemRarityID.White;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ModContent.ItemType<MiceChest>(), 1).AddIngredient(ItemID.Wire, 10).AddTile(TileID.HeavyWorkBench).Register();
		}
	}
}
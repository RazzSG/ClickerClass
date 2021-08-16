using ClickerClass.DrawLayers;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.Creative;
using Terraria.ID;
using Terraria.ModLoader;

namespace ClickerClass.Items.Tools
{
	public class MicePickaxe : ModItem
	{
		public override void SetStaticDefaults()
		{
			if (!Main.dedServ)
			{
				HeldItemLayer.RegisterData(Item.type, new DrawLayerData()
				{
					Texture = ModContent.Request<Texture2D>(Texture + "_Glow")
				});
			}

			CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 20;
			Item.useTime = 6;
			Item.useAnimation = 12;
			Item.damage = 80;
			Item.DamageType = DamageClass.Melee;
			Item.pick = 225;
			Item.useStyle = 1;
			Item.knockBack = 5.5f;
			Item.tileBoost = 4;
			Item.value = Item.sellPrice(0, 5, 0, 0);
			Item.rare = 10;
			Item.UseSound = SoundID.Item1;
			Item.autoReuse = true;
			Item.useTurn = true;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ModContent.ItemType<MiceFragment>(), 12).AddIngredient(ItemID.LunarBar, 10).AddTile(TileID.LunarCraftingStation).Register();
		}
	}
}

using ClickerClass.Projectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;

namespace ClickerClass.Items.Weapons.Clickers
{
	public class CrystalClicker : ClickerWeapon
	{
		public override void SetStaticDefaults()
		{
			base.SetStaticDefaults();

			ClickEffect.Dazzle = ClickerSystem.RegisterClickEffect(Mod, "Dazzle", null, null, 8, new Color(200, 50, 255), delegate (Player player, ProjectileSource_Item_WithAmmo source, Vector2 position, int type, int damage, float knockBack)
			{
				Projectile.NewProjectile(source, Main.MouseWorld, Vector2.Zero, ModContent.ProjectileType<CrystalClickerPro>(), 0, knockBack, player.whoAmI);
			});
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			SetRadius(Item, 3.1f);
			SetColor(Item, new Color(200, 50, 255));
			SetDust(Item, 86);
			AddEffect(Item, ClickEffect.Dazzle);

			Item.damage = 24;
			Item.width = 30;
			Item.height = 30;
			Item.knockBack = 2f;
			Item.value = 90000;
			Item.rare = 4;
		}

		public override void AddRecipes()
		{
			CreateRecipe(1).AddIngredient(ItemID.CrystalShard, 8).AddTile(TileID.MythrilAnvil).Register();
		}
	}
}

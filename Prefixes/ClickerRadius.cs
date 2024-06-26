﻿using System.Collections.Generic;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace ClickerClass.Prefixes
{
	public class ClickerRadius : ModPrefix
	{
		public static readonly int RadiusIncrease = 15;

		public static LocalizedText TooltipText { get; private set; }

		public override void SetStaticDefaults()
		{
			TooltipText = this.GetLocalization("Tooltip");
		}

		public override PrefixCategory Category => PrefixCategory.Accessory;

		public override void ModifyValue(ref float valueMult) => valueMult *= 1.05f;

		public override IEnumerable<TooltipLine> GetTooltipLines(Item item)
		{
			yield return new TooltipLine(Mod, $"{Name}Tooltip", TooltipText.Format(RadiusIncrease))
			{
				IsModifier = true
			};
		}
	}
}

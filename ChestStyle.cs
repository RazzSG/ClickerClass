﻿namespace ClickerClass
{
	public enum ChestStyle : byte
	{
		Wooden,
		Gold,
		LockedGold,
		Shadow,
		LockedShadow,
		Barrel,
		TrashCan,
		Ebonwood,
		RichMahogany,
		Pearlwood,
		Ivy,
		Ice,
		LivingWood,
		Skyware,
		Shadewood,
		WebCovered,
		Lihzahrd,
		Water,
		Jungle,
		Corruption,
		Crimson,
		Hallowed,
		Frozen,
		LockedJungle,
		LockedCorruption,
		LockedCrimson,
		LockedHallowed,
		LockedFrozen,
		Dynasty,
		Honey,
		Steampunk,
		PalmWood,
		Mushroom,
		BorealWood,
		Slime,
		GreenDungeon,
		LockedGreenDungeon,
		PinkDungeon,
		LockedPinkDungeon,
		BlueDungeon,
		LockedBlueDungeon,
		Bone,
		Cactus,
		Flesh,
		Obsidian,
		Pumpkin,
		Spooky,
		Glass,
		Martian,
		Meteorite,
		Granite,
		Marble,
		Crystal,
		Golden = 53,
		Containers2Offset = 54, //This is the index of the first chest from TileID.Containers2. Things checking for the frameX need to add to it if the tile type matches
		CrystalReal = Containers2Offset + 0,
		GoldenReal = Containers2Offset + 1,
		Spider = Containers2Offset + 2,
		Lesion = Containers2Offset + 3,
		DeadMans = Containers2Offset + 4,
		Solar = Containers2Offset + 5,
		Vortex = Containers2Offset + 6,
		Nebula = Containers2Offset + 7,
		Stardust = Containers2Offset + 8,
		Golf = Containers2Offset + 9,
		Sandstone = Containers2Offset + 10,
		Bamboo = Containers2Offset + 11,
		DesertDungeon = Containers2Offset + 12,
		LockedDesertDungeon = Containers2Offset + 13,
	}
}

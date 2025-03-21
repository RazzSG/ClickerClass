﻿using ClickerClass.Buffs;
using ClickerClass.Dusts;
using ClickerClass.Items.Armors;
using ClickerClass.Prefixes.ClickerPrefixes;
using ClickerClass.Projectiles;
using ClickerClass.Utilities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace ClickerClass.Items
{
	/// <summary>
	/// The class responsible for any clicker item related logic, only applies to items registered via <see cref="ClickerSystem.RegisterClickerItem(ModItem)"/>
	/// </summary>
	public class ClickerItemCore : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public override bool AppliesToEntity(Item entity, bool lateInstantiation)
		{
			return ClickerSystem.IsClickerItem(entity.type);
		}

		/// <summary>
		/// A clickers color used for the radius
		/// </summary>
		public Color clickerRadiusColor = Color.White;

		/// <summary>
		/// The clickers effects
		/// </summary>
		public List<string> itemClickEffects = new List<string>();

		/// <summary>
		/// The clickers dust that is spawned on use
		/// </summary>
		public int clickerDustColor = 0;

		/// <summary>
		/// Makes it so that this item cannot be equiped when other items of the same type are already equipped
		/// </summary>
		public ClickerAccessoryType accessoryType = ClickerAccessoryType.None;

		/// <summary>
		/// Displays total clicks in the tooltip
		/// </summary>
		public bool isClickerDisplayTotal = false;

		/// <summary>
		/// Displays total money generated by given item
		/// </summary>
		public bool isClickerDisplayMoneyGenerated = false;

		/// <summary>
		/// Additional range for this clicker (1f = 100 pixel, 1f by default from the player)
		/// </summary>
		public float radiusBoost = 0f;

		internal float radiusBoostPrefix = 0f;
		internal int clickBoostPrefix = 0;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			ClickerItemCore myClone = (ClickerItemCore)base.Clone(item, itemClone);
			myClone.clickerRadiusColor = clickerRadiusColor;
			myClone.itemClickEffects = new List<string>(itemClickEffects);
			myClone.clickerDustColor = clickerDustColor;
			myClone.clickBoostPrefix = clickBoostPrefix;
			myClone.accessoryType = accessoryType;
			myClone.isClickerDisplayTotal = isClickerDisplayTotal;
			myClone.isClickerDisplayMoneyGenerated = isClickerDisplayMoneyGenerated;
			myClone.radiusBoost = radiusBoost;
			myClone.radiusBoostPrefix = radiusBoostPrefix;
			return myClone;
		}

		public override void UpdateInventory(Item item, Player player)
		{
			if (ClickerSystem.IsSFXButton(item.type, out var playSoundAction))
			{
				player.GetModPlayer<ClickerPlayer>().AddSFXButtonStack(item);
			}
		}

		public override float UseTimeMultiplier(Item item, Player player)
		{
			if (ClickerSystem.IsClickerWeapon(item))
			{
				//Current value calculation:
				//Use time 2
				//60 ticks per second
				//
				//Examples (SpeedFactor on the left):
				//SpeedFactor of below 2 is not possible due to limitation
				//2f => 1 * 2 = 2 => 60 / 2 = 30 cps
				//6f = 1 * 6 = 6 => 60 / 6 = 10 cps

				//Disclaimer: the use time on the wiki is halved, it's listed as use time 1 (whereas code is 2 due to limitation of the game),
				//So the multiplier is halved here aswell, it makes it easier to conceptualize when starting at 1

				ClickerPlayer clickerPlayer = player.GetModPlayer<ClickerPlayer>();
				var activeAutoReuseEffect = clickerPlayer.ActiveAutoReuseEffect;

				if (!ClickerConfigClient.Instance.ToggleAutoreuseLimiter)
				{
					//useTime set to 2 -> 30 per click maximum possible
					return 30f / ClickerConfigClient.Instance.ToggleAutoreuseLimiterValue;
				}
				else
				{
					if (player.CanAutoReuseItem(item))
					{
						if (activeAutoReuseEffect != default)
						{
							return Math.Max(1, activeAutoReuseEffect.SpeedFactor / 2);
						}
						else
						{
							return 10f; //non-clicker induced autoswing (just a fallback, shouldn't ever happen with CanAutoReuseItem returning false)
						}
					}
					else
					{
						return 1f; //No change when not autoswinging
					}
				}
			}

			return base.UseTimeMultiplier(item, player);
		}

		public override bool? CanAutoReuseItem(Item item, Player player)
		{
			if (ClickerSystem.IsClickerWeapon(item))
			{
				if (!ClickerConfigClient.Instance.ToggleAutoreuseLimiter)
				{
					return base.CanAutoReuseItem(item, player);
				}
				else
				{
					ClickerPlayer clickerPlayer = player.GetModPlayer<ClickerPlayer>();
					return clickerPlayer.ActiveAutoReuseEffect != default;
				}
			}

			return base.CanAutoReuseItem(item, player);
		}

		public override bool CanUseItem(Item item, Player player)
		{
			if (ClickerSystem.IsClickerWeapon(item))
			{
				ClickerPlayer clickerPlayer = player.GetModPlayer<ClickerPlayer>();

				if (!clickerPlayer.HasClickEffect(ClickEffect.PhaseReach))
				{
					clickerPlayer.CheckPositionInRange(Main.MouseWorld, out bool inRange, out bool inRangeMotherboard);
					if (inRange || (inRangeMotherboard && player.altFunctionUse != 2))
					{
						return true;
					}
					else
					{
						return false;
					}
				}
			}
			return base.CanUseItem(item, player);
		}

		private bool HasAltFunctionUse(Item item, Player player)
		{
			if (ClickerSystem.IsClickerWeapon(item))
			{
				ClickerPlayer clickerPlayer = player.GetModPlayer<ClickerPlayer>();
				return clickerPlayer.setMice || clickerPlayer.setMotherboard;
			}
			return false;
		}

		public override bool AltFunctionUse(Item item, Player player)
		{
			return HasAltFunctionUse(item, player);
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			Player player = Main.LocalPlayer;
			if (!player.TryGetModPlayer(out ClickerPlayer clickerPlayer))
			{
				//Avoid incompatibility with TRAI calling ModifyTooltips during mod load when no players exist
				return;
			}

			int index;

			float alpha = Main.mouseTextColor / 255f;

			if (ClickerConfigClient.Instance.ShowClassTags)
			{
				index = tooltips.FindIndex(tt => tt.Mod.Equals("Terraria") && tt.Name.Equals("ItemName"));
				if (index != -1)
				{
					tooltips.Insert(index + 1, new TooltipLine(Mod, "ClickerTag", $"-{LangHelper.GetText("Tooltip.ClickerTag")}-")
					{
						OverrideColor = Main.DiscoColor
					});
				}
			}

			if (isClickerDisplayTotal)
			{
				index = tooltips.FindLastIndex(tt => tt.Mod.Equals("Terraria") && tt.Name.StartsWith("Tooltip"));

				if (index != -1)
				{
					string color = (new Color(252, 210, 44) * alpha).Hex3();
					tooltips.Insert(index + 1, new TooltipLine(Mod, "TotalClicks", LangHelper.GetLocalization("Tooltip.TotalClicks").Format(color, clickerPlayer.clickerTotal)));
				}
			}

			if (isClickerDisplayMoneyGenerated)
			{
				index = tooltips.FindLastIndex(tt => tt.Mod.Equals("Terraria") && tt.Name.StartsWith("Tooltip"));

				if (index != -1)
				{
					int currentValue = clickerPlayer.clickerMoneyGenerated;
					string displayValue = currentValue != 0 ? PopupText.ValueToName(currentValue) : "0";
					string color = (new Color(252, 210, 44) * alpha).Hex3();
					tooltips.Insert(index + 1, new TooltipLine(Mod, "MoneyGenerated", LangHelper.GetLocalization("Tooltip.MoneyGenerated").Format(color, displayValue)));
				}
			}

			if (ClickerSystem.IsClickerWeapon(item))
			{
				//Show the clicker's effects
				//Then show ones missing through the players enabled effects (respecting overlap, ignoring the currently held clickers effect if its not the same type)
				List<string> effects = new List<string>(itemClickEffects);
				foreach (var name in ClickerSystem.GetAllEffectNames())
				{
					if (clickerPlayer.HasClickEffect(name, out ClickEffect effect) && !effects.Contains(name))
					{
						if (!(player.HeldItem.type != item.type && player.HeldItem.type != ItemID.None && player.HeldItem.TryGetGlobalItem<ClickerItemCore>(out var clickerItem) && clickerItem.itemClickEffects.Contains(name)))
						{
							effects.Add(name);
						}
					}
				}

				if (effects.Count > 0)
				{
					index = tooltips.FindIndex(tt => tt.Mod.Equals("Terraria") && tt.Name.Equals("Knockback"));

					if (index != -1)
					{
						//If has a key, but not pressing it, show the ForMoreInfo text
						//Otherwise, list all effects
						string keybind = PlayerInput.GenerateInputTag_ForCurrentGamemode(tagForGameplay: false, TriggerNames.SmartSelect);
						string keyname = Language.GetTextValue("LegacyMenu.160");
						bool showDesc;

						if (keybind == string.Empty)
						{
							showDesc = true;
							keybind = Language.GetTextValue("LegacyMenu.195"); //<unbound>
						}
						else
						{
							//No tml hooks between controlTorch getting set, and then reset again in SmartSelectLookup, so we have to use the raw data from PlayerInput
							showDesc = PlayerInput.Triggers.Current.SmartSelect;
						}

						foreach (var name in effects)
						{
							if (ClickerSystem.IsClickEffect(name, out ClickEffect effect))
							{
								tooltips.Insert(++index, effect.ToTooltip(clickerPlayer.GetClickAmountTotal(this, name), alpha, showDesc));
							}
						}

						if (!showDesc && ClickerConfigClient.Instance.ShowEffectSuggestion)
						{
							//Add ForMoreInfo as the last line
							index = tooltips.FindLastIndex(tt => tt.Mod.Equals("Terraria") && tt.Name.StartsWith("Tooltip"));
							var ttl = new TooltipLine(Mod, "ForMoreInfo", LangHelper.GetText("Tooltip.ForMoreInfo", keyname, keybind))
							{
								OverrideColor = Color.Gray
							};

							if (index != -1)
							{
								tooltips.Insert(++index, ttl);
							}
							else
							{
								tooltips.Add(ttl);
							}
						}
					}
				}
			}

			if (item.prefix < PrefixID.Count || !ClickerPrefix.ClickerPrefixes.Contains(item.prefix))
			{
				return;
			}

			int ttindex = tooltips.FindLastIndex(t => (t.Mod == "Terraria" || t.Mod == Mod.Name) && (t.IsModifier || t.Name.StartsWith("Tooltip") || t.Name.Equals("Material")));
			if (ttindex != -1)
			{
				if (radiusBoostPrefix != 0)
				{
					TooltipLine tt = new TooltipLine(Mod, "PrefixClickerRadius", (radiusBoostPrefix > 0 ? "+" : "") + LangHelper.GetText("Prefixes.Common.RadiusBoost", (int)((radiusBoostPrefix / 2) * 100)))
					{
						IsModifier = true,
						IsModifierBad = radiusBoostPrefix < 0
					};
					tooltips.Insert(++ttindex, tt);
				}
				if (clickBoostPrefix != 0)
				{
					TooltipLine tt = new TooltipLine(Mod, "PrefixClickBoost", (clickBoostPrefix < 0 ? "" : "+") + LangHelper.GetText("Prefixes.Common.ClickBoost", clickBoostPrefix))
					{
						IsModifier = true,
						IsModifierBad = clickBoostPrefix > 0
					};
					tooltips.Insert(++ttindex, tt);
				}
			}
		}

		public override bool CanAccessoryBeEquippedWith(Item equippedItem, Item incomingItem, Player player)
		{
			if (equippedItem.TryGetGlobalItem(out ClickerItemCore equippedClickerItem) &&
				incomingItem.TryGetGlobalItem(out ClickerItemCore incomingClickerItem))
			{
				if (equippedClickerItem.accessoryType != ClickerAccessoryType.None &&
					incomingClickerItem.accessoryType != ClickerAccessoryType.None)
				{
					//If accessory types match, return false. This prevents the player from equipping two of the same accessory type, and it will instead swap them (if applicable)
					return equippedClickerItem.accessoryType != incomingClickerItem.accessoryType;
				}
			}

			return base.CanAccessoryBeEquippedWith(equippedItem, incomingItem, player);
		}


		public override int ChoosePrefix(Item item, UnifiedRandom rand)
		{
			if (ClickerPrefix.DoConditionsApply(item))
			{
				return rand.Next(ClickerPrefix.ClickerPrefixes);
			}
			return base.ChoosePrefix(item, rand);
		}

		public override bool? UseItem(Item item, Player player)
		{
			if (player.altFunctionUse == 2 && HasAltFunctionUse(item, player))
			{
				//Right click 
				var clickerPlayer = player.GetModPlayer<ClickerPlayer>();
				if (clickerPlayer.setAbilityDelayTimer <= 0)
				{
					//Mice armor 
					if (clickerPlayer.setMice)
					{
						bool canTeleport = false;
						if (!clickerPlayer.HasClickEffect(ClickEffect.PhaseReach))
						{
							//Collision
							clickerPlayer.CheckPositionInRange(Main.MouseWorld, out bool inRange, out bool _, false);
							if (inRange)
							{
								canTeleport = true;
							}
						}
						else
						{
							canTeleport = true;
						}

						if (canTeleport)
						{
							Vector2 teleportPos = Main.MouseWorld;
							SoundEngine.PlaySound(SoundID.Item115, teleportPos);

							player.ClickerTeleport(teleportPos);

							NetMessage.SendData(MessageID.PlayerControls, number: player.whoAmI);
							clickerPlayer.setAbilityDelayTimer = 60;

							float num102 = 50f;
							int num103 = 0;
							while ((float)num103 < num102)
							{
								Vector2 vector12 = Vector2.UnitX * 0f;
								vector12 += -Vector2.UnitY.RotatedBy((double)((float)num103 * (MathHelper.TwoPi / num102)), default(Vector2)) * new Vector2(2f, 2f);
								vector12 = vector12.RotatedBy((double)Vector2.Zero.ToRotation(), default(Vector2));
								int num104 = Dust.NewDust(teleportPos, 0, 0, ModContent.DustType<MiceDust>(), 0f, 0f, 0, default(Color), 2f);
								Main.dust[num104].noGravity = true;
								Main.dust[num104].position = teleportPos + vector12;
								Main.dust[num104].velocity = Vector2.Zero * 0f + vector12.SafeNormalize(Vector2.UnitY) * 4f;
								int num = num103;
								num103 = num + 1;
							}
						}
					}
					else if (clickerPlayer.setMotherboard)
					{
						Vector2 motherboardPos = Main.MouseWorld;
						SoundEngine.PlaySound(SoundID.Camera, motherboardPos);

						Vector2 sensorLocation = player.Center + clickerPlayer.CalculateMotherboardPosition(clickerPlayer.ClickerRadiusReal);

						if (sensorLocation.DistanceSQ(motherboardPos) < 20 * 20)
						{
							//Clicked onto the sensor
							clickerPlayer.ResetMotherboardPosition();
						}
						else
						{
							clickerPlayer.SetMotherboardRelativePosition(motherboardPos);
						}

						clickerPlayer.setAbilityDelayTimer = 60;
					}
				}
				return false;
			}

			return base.UseItem(item, player);
		}

		public override bool CanShoot(Item item, Player player)
		{
			if (player.altFunctionUse == 2 && HasAltFunctionUse(item, player))
			{
				return false;
			}
			return base.CanShoot(item, player);
		}

		public override void ModifyShootStats(Item item, Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
		{
			if (ClickerSystem.IsClickerWeapon(item))
			{
				var clickerPlayer = player.GetModPlayer<ClickerPlayer>();
				position = clickerPlayer.clickerPosition;
			}
		}

		public override bool Shoot(Item item, Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
		{
			if (ClickerSystem.IsClickerWeapon(item))
			{
				var clickerPlayer = player.GetModPlayer<ClickerPlayer>();

				HandleClickSFX(clickerPlayer);

				//Base
				//This shouldn't be here, but some mods (DormantDawnMOD) override projectile position, so we set it again like in ModifyShootStats
				position = clickerPlayer.clickerPosition;

				clickerPlayer.AddClick();

				bool preventsClickEffects = player.CanAutoReuseItem(item) && clickerPlayer.ActiveAutoReuseEffect.PreventsClickEffects;
				if (!preventsClickEffects)
				{
					clickerPlayer.AddClickAmount();
				}

				//TODO dire: maybe "PreShoot" hook wrapping around the next NewProjectile

				//Spawn normal click damage
				int projClick = Projectile.NewProjectile(source, position, Vector2.Zero, type, damage, knockback, player.whoAmI);

				if (clickerPlayer.accEnlarge)
				{
					Projectile extendedPro = Main.projectile[projClick];
					extendedPro.height = extendedPro.height * 2;
					extendedPro.width = extendedPro.width * 2;
					extendedPro.position -= new Vector2(extendedPro.width / 4, extendedPro.height / 4);

					Vector2 vec = position;
					float num102 = 30f;
					int num103 = 0;
					while ((float)num103 < num102)
					{
						Vector2 vector12 = Vector2.UnitX * 0f;
						vector12 += -Vector2.UnitY.RotatedBy((double)((float)num103 * (6.28318548f / num102)), default(Vector2)) * new Vector2(30f, 30f);
						vector12 = vector12.RotatedBy((double)player.velocity.ToRotation(), default(Vector2));
						int num104 = Dust.NewDust(vec, 0, 0, 205, 0f, 0f, 0, default(Color), 1f);
						Main.dust[num104].noGravity = true;
						Main.dust[num104].position = vec + vector12;
						Main.dust[num104].velocity = player.velocity * 0f + vector12.SafeNormalize(Vector2.UnitY) * 1f;
						int num = num103;
						num103 = num + 1;
					}

					// Resonance Scepter sparkles
					ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.PrincessWeapon, new ParticleOrchestraSettings
					{
						PositionInWorld = position,
						MovementVector = Main.rand.NextVector2Circular(2f, 2f)
					}, player.whoAmI);
				}

				//Portable Particle Accelerator acc
				if (clickerPlayer.IsPortableParticleAcceleratorActive)
				{
					Vector2 vec = position;
					float num102 = 25f;
					int num103 = 0;
					while ((float)num103 < num102)
					{
						Vector2 vector12 = Vector2.UnitX * 0f;
						vector12 += -Vector2.UnitY.RotatedBy((double)((float)num103 * (6.28318548f / num102)), default(Vector2)) * new Vector2(4f, 4f);
						vector12 = vector12.RotatedBy((double)player.velocity.ToRotation(), default(Vector2));
						int num104 = Dust.NewDust(vec, 0, 0, 229, 0f, 0f, 0, default(Color), 1f);
						Main.dust[num104].noGravity = true;
						Main.dust[num104].position = vec + vector12;
						Main.dust[num104].velocity = player.velocity * 0f + vector12.SafeNormalize(Vector2.UnitY) * 1f;
						int num = num103;
						num103 = num + 1;
					}
				}

				//Mouse Trap
				if (clickerPlayer.accMouseTrap)
				{
					if (Main.rand.NextBool(50))
					{
						SoundEngine.PlaySound(SoundID.Item153, player.Center);
						player.AddBuff(BuffID.Cursed, 60 * 3, false);
					}
				}

				//Hot Keychain 
				if (clickerPlayer.accHotKeychain2)
				{
					int damageAmount = Math.Max(1, (int)(damage * 0.25f));
					Projectile.NewProjectile(source, position, Vector2.Zero, ModContent.ProjectileType<HotKeychainPro>(), damageAmount, knockback, player.whoAmI);
				}

				int overclockType = ModContent.BuffType<OverclockBuff>();
				//Overclock armor set bonus
				if (clickerPlayer.clickAmount % OverclockHelmet.ClickAmount == 0 && clickerPlayer.setOverclock)
				{
					SoundEngine.PlaySound(SoundID.Item94, position);
					player.AddBuff(overclockType, 180, false);
					for (int i = 0; i < 25; i++)
					{
						int num6 = Dust.NewDust(player.position, 20, 20, 90, 0f, 0f, 150, default(Color), 1.35f);
						Main.dust[num6].noGravity = true;
						Main.dust[num6].velocity *= 0.75f;
						int num7 = Main.rand.Next(-50, 51);
						int num8 = Main.rand.Next(-50, 51);
						Dust dust = Main.dust[num6];
						dust.position.X = dust.position.X + (float)num7;
						Dust dust2 = Main.dust[num6];
						dust2.position.Y = dust2.position.Y + (float)num8;
						Main.dust[num6].velocity.X = -(float)num7 * 0.075f;
						Main.dust[num6].velocity.Y = -(float)num8 * 0.075f;
					}
				}

				bool overclock = player.HasBuff(overclockType);

				if (!preventsClickEffects)
				{
					foreach (var name in ClickerSystem.GetAllEffectNames())
					{
						if (clickerPlayer.HasClickEffect(name, out ClickEffect effect))
						{
							//Find click amount
							int clickAmountTotal = clickerPlayer.GetClickAmountTotal(this, name);
							bool reachedAmount = clickerPlayer.clickAmount % clickAmountTotal == 0;

							if (reachedAmount || (clickerPlayer.accTriggerFinger && clickerPlayer.OutOfCombat))
							{
								effect.Action?.Invoke(player, source, position, type, damage, knockback);

								if (clickerPlayer.accTriggerFinger)
								{
									//TODO looks like a hack
									clickerPlayer.outOfCombatTimer = ClickerPlayer.OutOfCombatTimeMax;
								}
							}
						}
					}
				}

				return false;
			}
			return base.Shoot(item, player, source, position, velocity, type, damage, knockback);
		}

		private static void HandleClickSFX(ClickerPlayer clickerPlayer)
		{
			var list = new List<(Action<int> playSound, int stack)>();
			foreach (var pair in clickerPlayer.GetAllSFXButtonStacks())
			{
				if (ClickerSystem.IsSFXButton(pair.Key, out var playSoundAction))
				{
					list.Add((playSoundAction, pair.Value));
				}
			}

			bool sfxDefault = list.Count == 0;
			if (sfxDefault)
			{
				// Default click
				SoundEngine.PlaySound(SoundID.MenuTick);
				return;
			}

			var sfxOption = Main.rand.Next(list);
			sfxOption.playSound.Invoke(sfxOption.stack);
		}
	}
}

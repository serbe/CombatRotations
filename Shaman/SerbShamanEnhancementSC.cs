
//  NEED UPDATE FROM CS

using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Shaman Enhancement SC", "ReBot", WoWClass.Shaman, Specialization.ShamanEnhancement, 5, 25)]

	public class SerbShamanEnhancementSc : SerbShaman
	{
		public SerbShamanEnhancementSc ()
		{
			PullSpells = new[] {
				"Lightning Bolt"
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_agility_flask
			//	actions.precombat+=/food,type=buttered_sturgeon
			//	actions.precombat+=/lightning_shield,if=!buff.lightning_shield.up
			if (LightningShield ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_agility
				
			return false;
		}

		public override void Combat ()
		{
			//	actions=wind_shear
			//	# Bloodlust casting behavior mirrors the simulator settings for proxy bloodlust. See options 'bloodlust_percent', and 'bloodlust_time'. 
			//	actions+=/bloodlust,if=target.health.pct<25|time>0.500
			if ((IsBoss (Target) && Health (Target) < 0.25) || (IsPlayer () && Health (Target) < 0.6))
				Bloodlust ();
			//	actions+=/auto_attack
			//	actions+=/use_item,name=beating_heart_of_the_mountain
			//	# In-combat potion is preferentially linked to the Fire or Storm Elemental, depending on talents, unless combat will end shortly
			//	actions+=/potion,name=draenic_agility,if=(talent.storm_elemental_totem.enabled&(pet.storm_elemental_totem.remains>=25|(cooldown.storm_elemental_totem.remains>target.time_to_die&pet.fire_elemental_totem.remains>=25)))|(!talent.storm_elemental_totem.enabled&pet.fire_elemental_totem.remains>=25)|target.time_to_die<=30
			//	actions+=/blood_fury
			BloodFury ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/berserking
			Berserking ();
			//	actions+=/elemental_mastery
			ElementalMastery ();
			//	actions+=/storm_elemental_totem
			if (StormElementalTotem ())
				return;
			//	actions+=/fire_elemental_totem
			if (FireElementalTotem ())
				return;
			//	actions+=/feral_spirit
			if (FeralSpirit ())
				return;
			//	actions+=/liquid_magma,if=pet.searing_totem.remains>10|pet.magma_totem.remains>10|pet.fire_elemental_totem.remains>10
			//	actions+=/ancestral_swiftness
			AncestralSwiftness ();
			//	actions+=/ascendance
			Ascendance ();
			//	# If only one enemy, priority follows the 'single' action list.
			//	actions+=/call_action_list,name=single,if=active_enemies=1
			if (ActiveEnemies (8) == 1) {
				if (Single ())
					return;
			}
			//	# On multiple enemies, the priority follows the 'aoe' action list.
			//	actions+=/call_action_list,name=aoe,if=active_enemies>1
			if (ActiveEnemies (8) > 1) {
				if (Aoe ())
					return;
			}
		}

		public bool Single ()
		{
			//	actions.single=searing_totem,if=!totem.fire.active
			if (!Me.TotemExist (TotemType.Fire_M1_DeathKnightGhoul)) {
				if (SearingTotem ())
					return true;
			}
			//	actions.single+=/unleash_elements,if=(talent.unleashed_fury.enabled|set_bonus.tier16_2pc_melee=1)
			if (HasSpell ("Unleashed Fury") || HasSpell (144962)) {
				if (UnleashElements ())
					return true;
			}
			//	actions.single+=/elemental_blast,if=buff.maelstrom_weapon.react=5
			if (Me.GetAura ("Maelstrom Weapon").StackCount == 5) {
				if (ElementalBlast ())
					return true;
			}
			//	actions.single+=/windstrike,if=!talent.echo_of_the_elements.enabled|(talent.echo_of_the_elements.enabled&(charges=2|(action.windstrike.charges_fractional>1.75)|(charges=1&buff.ascendance.remains<1.5)))
//			if (!HasSpell ("Echo of the Elements") || (HasSpell ("Echo of the Elements") && (SpellCharges ("Windstrike") == 2 || (WindstrikeFrac > 1.75) || (SpellCharges ("Windstrike") == 1 && Me.AuraTimeRemaining ("Ascendance") < 1.5)))) {
//				if (Windstrike ())
//					return true;
//			}
			//	actions.single+=/lightning_bolt,if=buff.maelstrom_weapon.react=5
			if (Me.GetAura ("Maelstrom Weapon").StackCount == 5) {
				if (LightningBolt ())
					return true;
			}
			//	actions.single+=/stormstrike,if=!talent.echo_of_the_elements.enabled|(talent.echo_of_the_elements.enabled&(charges=2|(action.stormstrike.charges_fractional>1.75)|target.time_to_die<6))
			if (!HasSpell ("Echo of the Elements") || (HasSpell ("Echo of the Elements") && (SpellCharges ("Stormstrike") == 2 || (Frac ("Stormstrike") > 1.75) || TimeToDie () < 6))) {
				if (Stormstrike ())
					return true;
			}
			//	actions.single+=/lava_lash,if=!talent.echo_of_the_elements.enabled|(talent.echo_of_the_elements.enabled&(charges=2|(action.lava_lash.charges_fractional>1.8)|target.time_to_die<8))
			if (!HasSpell ("Echo of the Elements") || (HasSpell ("Echo of the Elements") && (SpellCharges ("Lava Lash") == 2 || (Frac ("Lava Lash") > 1.75) || TimeToDie () < 8))) {
				if (LavaLash ())
					return true;
			}
			//	actions.single+=/flame_shock,if=(talent.elemental_fusion.enabled&buff.elemental_fusion.stack=2&buff.unleash_flame.up&dot.flame_shock.remains<16)|(!talent.elemental_fusion.enabled&buff.unleash_flame.up&dot.flame_shock.remains<=9)|!ticking
			if ((HasSpell ("Elemental Fusion") && Me.GetAura ("Elemental Fusion").StackCount == 2 && Me.HasAura ("Unleash Flame") && Target.AuraTimeRemaining ("Flame Shock", true) < 16) || (!HasSpell ("Elemental Fusion") && Me.HasAura ("Unleash Flame") && Target.AuraTimeRemaining ("Flame Shock", true) <= 9) || !Target.HasAura ("Flame Shock", true)) {
				if (FlameShock ())
					return true;
			}
			//	actions.single+=/unleash_elements
			if (UnleashElements ())
				return true;
			//	actions.single+=/windstrike,if=talent.echo_of_the_elements.enabled
			//
			//	actions.single+=/elemental_blast,if=buff.maelstrom_weapon.react>=3|buff.ancestral_swiftness.up
			if (Me.GetAura ("Maelstrom Weapon").StackCount >= 3 || Me.HasAura ("Ancestral Swiftness")) {
				if (ElementalBlast ())
					return true;
			}
			//	actions.single+=/lightning_bolt,if=(buff.maelstrom_weapon.react>=3&!buff.ascendance.up)|buff.ancestral_swiftness.up
			if ((Me.GetAura ("Maelstrom Weapon").StackCount >= 3 && !Me.HasAura ("Ascendance")) || Me.HasAura ("Ancestral Swiftness")) {
				if (LightningBolt ())
					return true;
			}
			//	actions.single+=/lava_lash,if=talent.echo_of_the_elements.enabled
			if (HasSpell ("Echo of the Elements")) {
				if (LavaLash ())
					return true;
			}
			//	actions.single+=/frost_shock,if=(talent.elemental_fusion.enabled&dot.flame_shock.remains>=16)|!talent.elemental_fusion.enabled
			if ((HasSpell ("Elemental Fusion") && Target.AuraTimeRemaining ("Flame Shock", true) >= 16) || !HasSpell ("Elemental Fusion")) {
				if (FrostShock ())
					return true;
			}
			//	actions.single+=/elemental_blast,if=buff.maelstrom_weapon.react>=1
			if (Me.GetAura ("Maelstrom Weapon").StackCount >= 1) {
				if (ElementalBlast ())
					return true;
			}
			//	actions.single+=/lightning_bolt,if=talent.echo_of_the_elements.enabled&((buff.maelstrom_weapon.react>=2&!buff.ascendance.up)|buff.ancestral_swiftness.up)
			if (HasSpell ("Echo of the Elements") && ((Me.GetAura ("Maelstrom Weapon").StackCount >= 2 && !Me.HasAura ("Ascendance")) || Me.HasAura ("Ancestral Swiftness"))) {
				if (LightningBolt ())
					return true;
			}
			//	actions.single+=/stormstrike,if=talent.echo_of_the_elements.enabled
			if (HasSpell ("Echo of the Elements")) {
				if (Stormstrike ())
					return true;
			}
			//	actions.single+=/lightning_bolt,if=(buff.maelstrom_weapon.react>=1&!buff.ascendance.up)|buff.ancestral_swiftness.up
			if ((Me.GetAura ("Maelstrom Weapon").StackCount >= 1 && !Me.HasAura ("Ascendance")) || Me.HasAura ("Ancestral Swiftness")) {
				if (LightningBolt ())
					return true;
			}
			//	actions.single+=/searing_totem,if=pet.searing_totem.remains<=20&!pet.fire_elemental_totem.active&!buff.liquid_magma.up

			return false;
		}

		public bool Aoe ()
		{
			//	actions.aoe=unleash_elements,if=active_enemies>=4&dot.flame_shock.ticking&(cooldown.shock.remains>cooldown.fire_nova.remains|cooldown.fire_nova.remains=0)
			//	actions.aoe+=/fire_nova,if=active_dot.flame_shock>=3
			if (Target.HasAura ("Flame Shock", true)) {
				if (FireNova ())
					return true;
			}
			//	actions.aoe+=/wait,sec=cooldown.fire_nova.remains,if=!talent.echo_of_the_elements.enabled&active_dot.flame_shock>=4&cooldown.fire_nova.remains<=action.fire_nova.gcd%2
			//	actions.aoe+=/magma_totem,if=!totem.fire.active
			//	actions.aoe+=/lava_lash,if=dot.flame_shock.ticking&active_dot.flame_shock<active_enemies
			//	actions.aoe+=/elemental_blast,if=!buff.unleash_flame.up&(buff.maelstrom_weapon.react>=4|buff.ancestral_swiftness.up)
			//	actions.aoe+=/chain_lightning,if=buff.maelstrom_weapon.react=5&((glyph.chain_lightning.enabled&active_enemies>=3)|(!glyph.chain_lightning.enabled&active_enemies>=2))
			//	actions.aoe+=/unleash_elements,if=active_enemies<4
			//	actions.aoe+=/flame_shock,if=dot.flame_shock.remains<=9|!ticking
			//	actions.aoe+=/windstrike,target=1,if=!debuff.stormstrike.up
			//	actions.aoe+=/windstrike,target=2,if=!debuff.stormstrike.up
			//	actions.aoe+=/windstrike,target=3,if=!debuff.stormstrike.up
			//	actions.aoe+=/windstrike
			//	actions.aoe+=/elemental_blast,if=!buff.unleash_flame.up&buff.maelstrom_weapon.react>=3
			//	actions.aoe+=/chain_lightning,if=(buff.maelstrom_weapon.react>=3|buff.ancestral_swiftness.up)&((glyph.chain_lightning.enabled&active_enemies>=4)|(!glyph.chain_lightning.enabled&active_enemies>=3))
			//	actions.aoe+=/magma_totem,if=pet.magma_totem.remains<=20&!pet.fire_elemental_totem.active&!buff.liquid_magma.up
			//	actions.aoe+=/lightning_bolt,if=buff.maelstrom_weapon.react=5&glyph.chain_lightning.enabled&active_enemies<3
			//	actions.aoe+=/stormstrike,target=1,if=!debuff.stormstrike.up
			//	actions.aoe+=/stormstrike,target=2,if=!debuff.stormstrike.up
			//	actions.aoe+=/stormstrike,target=3,if=!debuff.stormstrike.up
			//	actions.aoe+=/stormstrike
			if (Stormstrike ())
				return true;
			//	actions.aoe+=/lava_lash
			if (LavaLash ())
				return true;
			//	actions.aoe+=/fire_nova,if=active_dot.flame_shock>=2
			//	actions.aoe+=/elemental_blast,if=!buff.unleash_flame.up&buff.maelstrom_weapon.react>=1
			//	actions.aoe+=/chain_lightning,if=(buff.maelstrom_weapon.react>=1|buff.ancestral_swiftness.up)&((glyph.chain_lightning.enabled&active_enemies>=3)|(!glyph.chain_lightning.enabled&active_enemies>=2))
			//	actions.aoe+=/lightning_bolt,if=(buff.maelstrom_weapon.react>=1|buff.ancestral_swiftness.up)&glyph.chain_lightning.enabled&active_enemies<3
			//	actions.aoe+=/fire_nova,if=active_dot.flame_shock>=1

			return false;
		}

		public override bool AfterCombat ()
		{
			if (CastSelf ("Totemic Recall", () => Me.TotemExist (TotemType.Air, TotemType.Earth_M2, TotemType.Fire_M1_DeathKnightGhoul, TotemType.Water_M3)))
				return true;
			return false;
		}
	}
}

//	shaman="Shaman_Elemental_T17N"
//	level=100
//	race=dwarf
//	role=spell
//	position=back
//	professions=blacksmithing=600/engineering=600
//	talents=0003011
//	glyphs=chain_lightning
//	spec=elemental

using System;
using System.Linq;
using ReBot.API;

namespace ReBot
{
	[Rotation ("Serb Shaman Elemental", "ReBot", WoWClass.Shaman, Specialization.ShamanElemental, 40, 25)]

	public class SerbShamanElemental : SerbShaman
	{
		public SerbShamanElemental ()
		{
			PullSpells = new[] {
				"Lightning Bolt"
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_intellect_flask
			//	actions.precombat+=/food,type=salty_squid_roll
			//	actions.precombat+=/lightning_shield,if=!buff.lightning_shield.up
			if (LightningShield ())
				return true;
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_intellect

			if (Health (Me) <= 0.8 && !Me.IsMoving) {
				if (HealingSurge (Me))
					return true;
			}

			if (CleanCurse ())
				return true;

			if (Me.MovementSpeed != 0 && !Me.IsSwimming && Me.DistanceTo (API.GetNaviTarget ()) > 20) {
				if (GhostWolf ())
					return true;
			}


			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (InCombat) {
				InCombat = false;
				return true;
			}

			return false;
		}

		public override void Combat ()
		{
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			if (MeIsBusy)
				return;


			//Heal - Support
			if (Health (Me) < 0.9) {
				if (Heal ())
					return;
			}

//			if (CastSelf ("Cleanse Spirit", () => Me.Auras.Any (x => x.IsDebuff && "Curse".Contains (x.DebuffType))))
//				return;
//			if (Cast ("Earth Elemental Totem", () => (Target.IsElite () || Adds.Count >= 3) && Me.HealthFraction < 0.7 && Target.Target == Me && Target.HealthFraction >= 0.3))
//				return;
//			if (Cast ("Earth Elemental Totem", () => (Target.IsElite () || Adds.Count >= 3) && Me.HealthFraction < 0.7 && Target.Target == Me && Target.HealthFraction >= 0.3))
//				return;
//			if (Cast ("Grounding Totem", () => Target.IsCasting && Target.Target == Me))
//				return;



			//	actions=wind_shear
			if (Interrupt ())
				return;
			//	# Bloodlust casting behavior mirrors the simulator settings for proxy bloodlust. See options 'bloodlust_percent', and 'bloodlust_time'. 
			//	actions+=/bloodlust,if=target.health.pct<25|time>0.500
			if (Health () < 0.25 || Time > 0.5)
				Bloodlust ();
			//	# In-combat potion is preferentially linked to Ascendance, unless combat will end shortly
			//	actions+=/potion,name=draenic_intellect,if=buff.ascendance.up|target.time_to_die<=30
			if (InRaid && (Me.HasAura ("Ascendance") || TimeToDie () <= 30)) {
				if (DraenicIntellect ())
					return;
			}
			//	actions+=/berserking,if=!buff.bloodlust.up&!buff.elemental_mastery.up&(set_bonus.tier15_4pc_caster=1|(buff.ascendance.cooldown_remains=0&(dot.flame_shock.remains>buff.ascendance.duration|level<87)))
			if (!Me.HasAura ("Bloodlust") && !Me.HasAura ("Elemental Mastery") && (HasSpell (138144) || (Cooldown ("Ascendance") == 0 && (Target.AuraTimeRemaining ("Flame Shock") > 15 || Me.Level < 87))))
				Berserking ();
			//	actions+=/blood_fury,if=buff.bloodlust.up|buff.ascendance.up|((cooldown.ascendance.remains>10|level<87)&cooldown.fire_elemental_totem.remains>10)
			if (Me.HasAura ("Bloodlust") || Me.HasAura ("Ascendance") || ((Cooldown ("Ascendance") > 10 || Me.Level < 87) && Cooldown ("Fire Elemental Totem") > 10))
				BloodFury ();
			//	actions+=/arcane_torrent
			ArcaneTorrent ();
			//	actions+=/elemental_mastery,if=action.lava_burst.cast_time>=1.2
			if (CastTime (51505) > 1.2)
				ElementalMastery ();
			//	actions+=/ancestral_swiftness,if=!buff.ascendance.up
			if (!Me.HasAura ("Ascendance"))
				AncestralSwiftness ();
			//	actions+=/storm_elemental_totem
			if (StormElementalTotem ())
				return;
			//	actions+=/fire_elemental_totem,if=!active
			if (!HasActiveFireElementalTotem) {
				if (FireElementalTotem ())
					return;
			}
			//	actions+=/ascendance,if=active_enemies>1|(dot.flame_shock.remains>buff.ascendance.duration&(target.time_to_die<20|buff.bloodlust.up|time>=60)&cooldown.lava_burst.remains>0)
			if (ActiveEnemies (40) > 1 || (Target.AuraTimeRemaining ("Flame Shock", true) > 15 && (TimeToDie () < 20 || Me.HasAura ("Bloodlust") || Time >= 60) && Cooldown ("Lava Burst") > 0))
				Ascendance ();
			//	actions+=/liquid_magma,if=pet.searing_totem.remains>=15|pet.fire_elemental_totem.remains>=15
			if (TotemRemainTime ("Searing Totem") > 15 || TotemRemainTime ("Fire Elemental Totem") >= 15) {
				if (LiquidMagma ())
					return;
			}
			//	# If one or two enemies, priority follows the 'single' action list.
			//	actions+=/call_action_list,name=single,if=active_enemies<3
			if (ActiveEnemies (40) < 3)
				ActionSingle ();
			//	# On multiple enemies, the priority follows the 'aoe' action list.
			//	actions+=/call_action_list,name=aoe,if=active_enemies>2
			if (ActiveEnemies (40) > 2)
				ActionAoe ();

		}

		void ActionSingle ()
		{
			//	actions.single=unleash_flame,moving=1
			if (Me.IsMoving) {
				if (UnleashFlame ())
					return;
			}
			//	actions.single+=/spiritwalkers_grace,moving=1,if=buff.ascendance.up
			if (Me.IsMoving && Me.HasAura ("Ascendance"))
				SpiritwalkersGrace ();
			//	actions.single+=/earth_shock,if=buff.lightning_shield.react=buff.lightning_shield.max_stack
			if (GetAuraStack ("Lightning Shield", Me) == MaxLightningShieldCharges) {
				if (EarthShock ())
					return;
			}
			//	actions.single+=/flame_shock,if=(talent.elemental_fusion.enabled&buff.elemental_fusion.stack=2&buff.unleash_flame.up&dot.flame_shock.remains<(dot.flame_shock.duration*0.3))
			if (HasSpell ("Elemental Fusion") && AuraStackCount ("Elemental Fusion") == 2 && Me.HasAura ("Unleash Flame") && Target.AuraTimeRemaining ("Flame Shock", true) <= 1.8) {
				if (FlameShock ())
					return;
			}
			//	actions.single+=/lava_burst,if=dot.flame_shock.remains>cast_time&(buff.ascendance.up|cooldown_react)
			if (Target.AuraTimeRemaining ("Flame Shock") > CastTime (51505) && (Me.HasAura ("Ascendance") || Cooldown ("Lava Burst") == 0)) {
				if (LavaBurst ()) {
					return;
				}
			}
			//	actions.single+=/earth_shock,if=(set_bonus.tier17_4pc&buff.lightning_shield.react>=12&!buff.lava_surge.up)|(set_bonus.tier18_4pc&buff.lightning_shield.react>=14)|(!set_bonus.tier17_4pc&!set_bonus.tier18_4pc&buff.lightning_shield.react>15)
			if ((HasSpell (165580) && GetAuraStack ("Lightning Shield", Me) >= 12 && !Me.HasAura ("Lava Surge")) || (HasSpell (165580) && GetAuraStack ("Lightning Shield", Me) >= 14) || (!HasSpell (165580) && !HasSpell (165580) && GetAuraStack ("Lightning Shield", Me) > 15)) {
				if (EarthShock ())
					return;
			}
			//	actions.single+=/flame_shock,if=dot.flame_shock.remains<=9
			if (Target.AuraTimeRemaining ("Flame Shock") <= 9) {
				if (FlameShock ())
					return;
			}
			//	actions.single+=/elemental_blast
			if (ElementalBlast ())
				return;
			//	# After the initial Ascendance, use Flame Shock pre-emptively just before Ascendance to guarantee Flame Shock staying up for the full duration of the Ascendance buff
			//	actions.single+=/flame_shock,if=time>60&remains<=buff.ascendance.duration&cooldown.ascendance.remains+buff.ascendance.duration<duration
			if (Time > 60 && Target.AuraTimeRemaining ("Flame Shock") <= 15 && Cooldown ("Ascendance") + 15 < 30) {
				if (FlameShock ())
					return;
			}
			//	# Keep Searing Totem up, unless Fire Elemental Totem is coming off cooldown in the next 20 seconds
			//	actions.single+=/searing_totem,if=(!talent.liquid_magma.enabled&(!totem.fire.active|(pet.searing_totem.remains<=10&!pet.fire_elemental_totem.active&talent.unleashed_fury.enabled)))|(talent.liquid_magma.enabled&pet.searing_totem.remains<=20&!pet.fire_elemental_totem.active&!buff.liquid_magma.up)
			if ((!HasSpell ("Liquid Magma") && (!(HasActiveFireElementalTotem || HasActiveSearingTotem) || (TotemRemainTime ("Searing Totem") <= 10 && !HasActiveFireElementalTotem && HasSpell ("Unleashed Fury")))) || (HasSpell ("Liquid Magma") && TotemRemainTime ("Searing Totem") <= 20 && !HasActiveFireElementalTotem && !Me.HasAura ("Liquid Magma"))) {
				if (SearingTotem ())
					return;
			}
			//	actions.single+=/unleash_flame,if=talent.unleashed_fury.enabled&!buff.ascendance.up|(talent.elemental_fusion.enabled&buff.elemental_fusion.stack=2&(dot.flame_shock.remains)<(dot.flame_shock.duration*0.3)&cooldown.flame_shock.remains=0)
			if (HasSpell ("Unleashed Fury") && !Me.HasAura ("Ascendance") || (HasSpell ("Elemental Fusion") && AuraStackCount ("Elemental Fusion") == 2 && (Target.AuraTimeRemaining ("Flame Shock", true) <= 1.8) && Cooldown ("Flame Shock") == 0)) {
				if (UnleashFlame ())
					return;
			}
			//	actions.single+=/spiritwalkers_grace,moving=1,if=((talent.elemental_blast.enabled&cooldown.elemental_blast.remains=0)|(cooldown.lava_burst.remains=0&!buff.lava_surge.react))
			if (Me.IsMoving && ((HasSpell ("Elemental Blast") && Cooldown ("Elemental Blast") == 0) || (Cooldown ("Lava Burst") == 0 && !Me.HasAura ("Lava Surge"))))
				SpiritwalkersGrace ();
			//	actions.single+=/lightning_bolt
			if (LightningBolt ())
				return;
		}

		void ActionAoe ()
		{
			//	actions.aoe=earthquake,cycle_targets=1,if=!ticking&(buff.enhanced_chain_lightning.up|level<=90)&active_enemies>=2
			if ((Me.HasAura ("Enhanced Chain Lightning") || Me.Level <= 90) && ActiveEnemies (40) >= 2) {
				var Unit = Enemy.Where (u => !u.HasAura ("Earthquake") && !u.IsMoving && Range (35, u)).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Earthquake (Unit))
					return;
			}
			//	actions.aoe+=/lava_beam
			if (LavaBeam ())
				return;
			//	actions.aoe+=/earth_shock,if=buff.lightning_shield.react=buff.lightning_shield.max_stack
			if (GetAuraStack ("Lightning Shield", Me) == MaxLightningShieldCharges) {
				if (EarthShock ())
					return;
			}
			//	actions.aoe+=/thunderstorm,if=active_enemies>=10
			if (ActiveEnemies (10) >= 10) {
				if (Thunderstorm ())
					return;
			}
			//	actions.aoe+=/searing_totem,if=(!talent.liquid_magma.enabled&!totem.fire.active)|(talent.liquid_magma.enabled&pet.searing_totem.remains<=20&!pet.fire_elemental_totem.active&!buff.liquid_magma.up)
			if ((!HasSpell ("Liquid Magma") && !Me.TotemExist (TotemType.Fire_M1_DeathKnightGhoul)) || (HasSpell ("Liquid Magma") && TotemRemainTime ("Searing Totem") <= 20 && !HasActiveFireElementalTotem && !Me.HasAura ("Liquid Magma"))) {
				if (SearingTotem ())
					return;
			}
			//	actions.aoe+=/chain_lightning,if=active_enemies>=2
			if (ActiveEnemies (30) >= 2) {
				if (ChainLightning ())
					return;
			}
			//	actions.aoe+=/lightning_bolt
			if (LightningBolt ())
				return;
		}

		public bool Heal ()
		{
			if (Health (Me) <= 0.75) { 
				GiftoftheNaaru (Me);
			}
			if (Health (Me) <= 0.5) { 
				if (Healthstone ())
					return true;
			}
			if (Health (Me) <= 0.6 && Target.MaxHealth > Me.MaxHealth && Danger (Target, 40)) {
				if (AncestralGuidance ())
					return true;
			}
			if (Health (Me) <= 0.5 && Target.HpGreaterThanOrElite (0.3) && Me.InCombat && Target.Target == Me && !HasAura ("Shamanistic Rage")) {
				AstralShift ();
			}
			if (Health (Me) < 0.7 && Me.InCombat && !Me.HasAura ("Astral Shift") && Target.HpGreaterThanOrElite (0.3) && Target.Target == Me) {
				ShamanisticRage ();
			}

			if (!(InRaid || InInstance)) {
				if (Health (Me) < 0.85) {
					if (HealingRain (Me))
						return true;
				}
				if (Health (Me) <= 0.4 && HasAura ("Maelstrom Weapon", true, 5)) {
					if (HealingSurge (Me))
						return true;
				}
				if (Health (Me) < 0.7) {
					if (HealingStreamTotem ())
						return true;
				}
			}

			return false;
		}
	}
}

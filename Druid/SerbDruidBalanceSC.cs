using System;
using System.Linq;
using ReBot.API;
using Newtonsoft.Json;

namespace ReBot
{
	[Rotation ("Serb Balance Druid SC", "Serb", WoWClass.Druid, Specialization.DruidBalance, 40)]

	public class SerbDruidBalanceSc : SerbDruid
	{
		[JsonProperty ("Use StarFall")]
		public bool UseStarFall;

		public SerbDruidBalanceSc ()
		{
			GroupBuffs = new [] {
				"Mark of the Wild"
			};
			PullSpells = new [] {
				"Moonfire"
			};

			DismountSpell = "Moonkin Form";
		}

		public override bool OutOfCombat ()
		{
			// actions.precombat=flask,type=greater_draenic_intellect_flask
			// actions.precombat+=/food,type=sleeper_sushi
			// actions.precombat+=/mark_of_the_wild,if=!aura.str_agi_int.up
			if (MarkoftheWild (Me))
				return true;
			// actions.precombat+=/moonkin_form
			// # Snapshot raid buffed stats before combat begins and pre-potting is done.
			// actions.precombat+=/snapshot_stats
			// actions.precombat+=/potion,name=draenic_intellect
			// actions.precombat+=/incarnation
			// actions.precombat+=/starfire

			// Heal
			if (Health (Me) <= 0.75 && !Me.HasAura ("Rejuvenation")) {
				if (Rejuvenation (Me))
					return true;
			}
			if (Health (Me) <= 0.5 && !Me.IsMoving) {
				if (HealingTouch (Me))
					return true;
			}
			if (Me.Auras.Any (x => x.IsDebuff && "Curse,Poison".Contains (x.DebuffType))) {
				if (RemoveCorruption (Me))
					return true;
			}

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			if (InCombat) {
				InCombat = false;
			}

			return false;
		}

		public override void Combat ()
		{
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			if (Gcd && HasGlobalCooldown ())
				return;

			if (!Me.HasAura ("Bear Form")) {
				if (MoonkinForm ())
					return;
			}

			// Heal
			if (Health (Me) < 0.9) {
				if (Heal ())
					return;
			}

			if (Interrupt ())
				return;

			// actions=potion,name=draenic_intellect,if=buff.celestial_alignment.up
			// actions+=/blood_fury,if=buff.celestial_alignment.up
			if (Me.HasAura ("Celestial Alignment"))
				BloodFury ();
			// actions+=/berserking,if=buff.celestial_alignment.up
			if (Me.HasAura ("Celestial Alignment"))
				Berserking ();
			// actions+=/arcane_torrent,if=buff.celestial_alignment.up
			if (Me.HasAura ("Celestial Alignment"))
				ArcaneTorrent ();
			// actions+=/force_of_nature,if=trinket.stat.intellect.up|charges=3|target.time_to_die<21
			if (SpellCharges ("Force of Nature") == 3 || TimeToDie (Target) < 21) {
				if (ForceofNature ())
					return;
			}
			// actions+=/call_action_list,name=single_target,if=active_enemies=1
			if (ActiveEnemies (40) == 1) {
				if (Single ())
					return;
			}
			// actions+=/call_action_list,name=aoe,if=active_enemies>1
			if (ActiveEnemies (40) > 1) {
				if (AOEAction ())
					return;
			}
		}

		public bool Single ()
		{
			//	actions.single_target=starsurge,if=buff.lunar_empowerment.down&(eclipse_energy>20|buff.celestial_alignment.up)				
			if (!Me.HasAura ("Lunar Empowerment") && (Eclipse > 20 || Me.HasAura ("Celestial Alignment"))) {
				if (Starsurge ())
					return true;
			}
			// actions.single_target+=/starsurge,if=buff.solar_empowerment.down&eclipse_energy<-40
			if (!Me.HasAura ("Solar Empowerment") && Eclipse < -40) {
				if (Starsurge ())
					return true;
			}
			// actions.single_target+=/starsurge,if=(charges=2&recharge_time<6)|charges=3
			if ((SpellCharges ("Starsurge") == 2 && SpellCooldown ("Starsurge") < 6) || SpellCharges ("Starsurge") == 3) {
				if (Starsurge ())
					return true;
			}
			// actions.single_target+=/celestial_alignment,if=eclipse_energy>0
			if (Eclipse > 0) {
				if (CelestialAlignment ())
					return true;
			}
			// actions.single_target+=/incarnation,if=eclipse_energy>0
			if (Eclipse > 0) {
				if (IncarnationChosenofElune ())
					return true;
			}
			//	actions.single_target+=/sunfire,if=remains<7|(buff.solar_peak.up&buff.solar_peak.remains<action.wrath.cast_time&!talent.balance_of_power.enabled)
			if (Target.AuraTimeRemaining ("Sunfire", true) < 7 || (Me.HasAura ("Solar Peak") && Me.AuraTimeRemaining ("Solar Peak") < CastTime (5176) && !HasSpell ("Balance of Power"))) {
				if (Sunfire ())
					return true;
			}
			// actions.single_target+=/stellar_flare,if=remains<7
			if (Target.AuraTimeRemaining ("Stellar Flare", true) < 7) {
				if (StellarFlare ())
					return true;
			}
			//	actions.single_target+=/moonfire,if=!talent.balance_of_power.enabled&(buff.lunar_peak.up&buff.lunar_peak.remains<action.starfire.cast_time&remains<eclipse_change+20|remains<4|(buff.celestial_alignment.up&buff.celestial_alignment.remains<=2&remains<eclipse_change+20))
			if (Eclipse <= 0 && !HasSpell ("Balance of Power") && (Me.HasAura ("Lunar Peak") && Me.AuraTimeRemaining ("Lunar Peak") < CastTime (2912) && Target.AuraTimeRemaining ("Moonfire", true) < (EclipseChange + 20) || Target.AuraTimeRemaining ("Moonfire", true) < 4 || (Me.HasAura ("Celestial Alignment") && Me.AuraTimeRemaining ("Celestial Alignment") <= 2 && Target.AuraTimeRemaining ("Moonfire", true) < (EclipseChange + 20)))) {
				if (Moonfire ())
					return true;
			}
			// actions.single_target+=/moonfire,if=talent.balance_of_power.enabled&(remains<4|(buff.celestial_alignment.up&buff.celestial_alignment.remains<=2&remains<eclipse_change+20))
			if (Eclipse <= 0 && HasSpell ("Balance of Power") && (Target.AuraTimeRemaining ("Moonfire", true) < 4) || (Me.HasAura ("Celestial Alignment") && Me.AuraTimeRemaining ("Celestial Alignment") <= 2 && Target.AuraTimeRemaining ("Moonfire", true) < (EclipseChange + 20))) {
				if (Moonfire ())
					return true;
			}
			// actions.single_target+=/wrath,if=(eclipse_energy<=0&eclipse_change>cast_time)|(eclipse_energy>0&cast_time>eclipse_change)
			if ((Eclipse <= 0 && EclipseChange > 2) || (Eclipse > 0 && EclipseChange < 2)) {
				if (Wrath ())
					return true;
			}
			// actions.single_target+=/starfire,if=(eclipse_energy>=0&eclipse_change>cast_time)|(eclipse_energy<0&cast_time>eclipse_change)
			if ((Eclipse >= 0 && EclipseChange > 3) || (Eclipse < 0 && EclipseChange < 3)) {
				if (Starfire ())
					return true;
			}
			// actions.single_target+=/wrath
			if (Wrath ())
				return true;

			return false;
		}

		public bool AOEAction ()
		{
			// actions.aoe=celestial_alignment,if=lunar_max<8|target.time_to_die<20
			if ((EclipseDirection == "moon" && Eclipse < -20) || TimeToDie (Target) < 20) {
				if (Starfire ())
					return true;
			}
			// actions.aoe+=/incarnation,if=buff.celestial_alignment.up
			if (Me.HasAura ("Celestial Alignment")) {
				if (IncarnationChosenofElune ())
					return true;
			}
			// actions.aoe+=/sunfire,cycle_targets=1,if=remains<8
			if (Usable ("Sunfire") && Eclipse > 0) {
				Unit = Enemy.Where (x => Range (40, x) && x.AuraTimeRemaining ("Sunfire", true) < 8).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Sunfire (Unit))
					return true;
			}
			// actions.aoe+=/starfall,if=!buff.starfall.up&active_enemies>2
			if (UseStarFall && !Me.HasAura ("Starfall") && ActiveEnemies (40) > 2) {
				if (Starfall ())
					return true;
			}
			// actions.aoe+=/starsurge,if=(charges=2&recharge_time<6)|charges=3
			if ((SpellCharges ("Starsurge") == 2 && SpellCooldown ("Starsurge") < 6) || SpellCharges ("Starsurge") == 3) {
				if (Starsurge ())
					return true;
			}
			// actions.aoe+=/moonfire,cycle_targets=1,if=remains<12
			if (Usable ("Moonfire") && Eclipse <= 0) {
				Unit = Enemy.Where (x => Range (40, x) && x.AuraTimeRemaining ("Moonfire", true) < 12).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && Moonfire ())
					return true;
			}
			// actions.aoe+=/stellar_flare,cycle_targets=1,if=remains<7
			if (Usable ("Stellar Flare")) {
				Unit = Enemy.Where (x => Range (40, x) && x.AuraTimeRemaining ("Stellar Flare", true) < 7).DefaultIfEmpty (null).FirstOrDefault ();
				if (Unit != null && StellarFlare ())
					return true;
			}
			// actions.aoe+=/starsurge,if=buff.lunar_empowerment.down&eclipse_energy>20&active_enemies=2
			if (!Me.HasAura ("Lunar Empowerment") && Eclipse > 20 && ActiveEnemies (40) == 2) {
				if (Starsurge ())
					return true;
			}
			// actions.aoe+=/starsurge,if=buff.solar_empowerment.down&eclipse_energy<-40&active_enemies=2
			if (!Me.HasAura ("Lunar Empowerment") && Eclipse < -40 && ActiveEnemies (40) == 2) {
				if (Starsurge ())
					return true;
			}
			// actions.aoe+=/wrath,if=(eclipse_energy<=0&eclipse_change>cast_time)|(eclipse_energy>0&cast_time>eclipse_change)
			if ((Eclipse <= 0 && EclipseChange > 2) || (Eclipse > 0 && EclipseChange < 2)) {
				if (Wrath ())
					return true;
			}
			// actions.aoe+=/starfire,if=(eclipse_energy>=0&eclipse_change>cast_time)|(eclipse_energy<0&cast_time>eclipse_change)
			if ((Eclipse >= 0 && EclipseChange > 3) || (Eclipse < 0 && EclipseChange < 3)) {
				if (Starfire ())
					return true;
			}
			// actions.aoe+=/wrath
			if (Wrath ())
				return true;

			return false;
		}

		public bool RunToEnemy ()
		{
			return false;
		}
	}
}

using System;
using ReBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Linq;

namespace ReBot
{
	[Rotation ("Serb Warrior Arms SC", "Serb", WoWClass.Warrior, Specialization.WarriorArms, 5, 25)]

	public class SerbWarriorArms : SerbWarrior
	{
		[JsonProperty ("War cry"), JsonConverter (typeof(StringEnumConverter))]							
		public WarCry Shout = WarCry.BattleShout;
		[JsonProperty ("Use Movement")]
		public bool Move = false;

		public SerbWarriorArms ()
		{
			GroupBuffs = new[] {
				"Battle Shout"
			};
			PullSpells = new[] {
				"Charge",
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_strength_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/stance,choose=battle
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	# Generic on-use trinket line if needed when swapping trinkets out. 
			//	# actions+=/use_item,slot=trinket1,if=active_enemies=1&(buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up))|(active_enemies>=2&(prev_gcd.ravager|(!talent.ravager.enabled&!cooldown.bladestorm.remains&dot.rend.ticking)))
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/potion,name=draenic_strength

			if (Buff (Shout))
				return true;

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;

			return false;
		}

		public override void Combat ()
		{

			if (Interrupt ())
				return;

			if (Reflect ())
				return;

			if (Me.CanNotParticipateInCombat ())
				Freedom ();

			if (Gcd && HasGlobalCooldown ())
				return;

			if (!(InInstance || InRaid)) {
				if (Heal ())
					return;
			}

			//	actions=charge,if=debuff.charge.down
			//	actions+=/auto_attack
			//	# This is mostly to prevent cooldowns from being accidentally used during movement.
			//	actions+=/run_action_list,name=movement,if=movement.distance>5
			if (Range (40, Target, 10) && Move) {
				if (Movement ())
					return;
			}
			//	actions+=/use_item,name=vial_of_convulsive_shadows,if=(buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up))
			//	actions+=/potion,name=draenic_strength,if=(target.health.pct<20&buff.recklessness.up)|target.time_to_die<25
			//	# This incredibly long line (Due to differing talent choices) says 'Use recklessness on cooldown with colossus smash, unless the boss will die before the ability is usable again, and then use it with execute.'
			//	actions+=/recklessness,if=(((target.time_to_die>190|target.health.pct<20)&(buff.bloodbath.up|!talent.bloodbath.enabled))|target.time_to_die<=12|talent.anger_management.enabled)&((desired_targets=1&!raid_event.adds.exists)|!talent.bladestorm.enabled)
			if ((((TimeToDie () > 190 || Health () < 0.2) && (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath"))) || TimeToDie () <= 12 || HasSpell ("Anger Management")) && ((ActiveEnemies (30) == 1) || !HasSpell ("Bladestorm"))) {
				if (Recklessness ())
					return;
			}
			//	actions+=/bloodbath,if=(dot.rend.ticking&cooldown.colossus_smash.remains<5&((talent.ravager.enabled&prev_gcd.ravager)|!talent.ravager.enabled))|target.time_to_die<20
			if ((Target.HasAura ("Rend", true) && Cooldown ("Colossus Smash") < 5 && ((HasSpell ("Ravager") && PrevGcdRavager) || !HasSpell ("Ravager"))) || TimeToDie () < 20) {
				if (Bloodbath ())
					return;
			}
			//	actions+=/avatar,if=buff.recklessness.up|target.time_to_die<25
			if (Me.HasAura ("Recklessness") || TimeToDie () < 25) {
				if (Avatar ())
					return;
			}
			//	actions+=/blood_fury,if=buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up)|buff.recklessness.up
			if (Me.HasAura ("Bloodbath") || (!HasSpell ("Bloodbath") && Target.HasAura ("Colossus Smash", true)) || Me.HasAura ("Recklessness")) {
				BloodFury ();
			}
			//	actions+=/berserking,if=buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up)|buff.recklessness.up
			if (Me.HasAura ("Bloodbath") || (!HasSpell ("Bloodbath") && Target.HasAura ("Colossus Smash", true)) || Me.HasAura ("Recklessness")) {
				BerserkerRage ();
			}
			//	actions+=/arcane_torrent,if=rage<rage.max-40
			if (Rage < RageMax - 40) {
				ArcaneTorrent ();
			}
			//	actions+=/heroic_leap,if=(raid_event.movement.distance>25&raid_event.movement.in>45)|!raid_event.movement.exists
			//	actions+=/call_action_list,name=single,if=active_enemies=1
			if (ActiveEnemies (8) == 1)
				ActionSingle ();
			//	actions+=/call_action_list,name=aoe,if=active_enemies>1
			if (ActiveEnemies (8) > 1)
				ActionAoe ();

		}

		public bool Movement ()
		{
			//	actions.movement=heroic_leap
			if (HeroicLeap ())
				return true;
			//	actions.movement+=/charge,cycle_targets=1,if=debuff.charge.down
			//	# If possible, charge a target that will give us rage. Otherwise, just charge to get back in range.
			//	actions.movement+=/charge
			if (Charge ())
				return true;
			//	# May as well throw storm bolt if we can.
			//	actions.movement+=/storm_bolt
			if (StormBolt ())
				return true;
			//	actions.movement+=/heroic_throw
			if (HeroicThrow ())
				return true;

			return false;
		}

		void ActionSingle ()
		{
			//	actions.single=rend,if=target.time_to_die>4&dot.rend.remains<5.4&(target.health.pct>20|!debuff.colossus_smash.up)
			if (TimeToDie () > 4 && Target.AuraTimeRemaining ("Rend", true) < 5.4 && (Health () > 0.2 || !Target.HasAura ("Colossus Smash", true))) {
				if (Rend ())
					return;
			}
			//	actions.single+=/ravager,if=cooldown.colossus_smash.remains<4&(!raid_event.adds.exists|raid_event.adds.in>55)
			if (Cooldown ("Colossus Smash") < 4) {
				if (Ravager ()) {
					PrevRavager = DateTime.Now;
					return;
				}
			}
			//	actions.single+=/colossus_smash
			if (ColossusSmash ())
				return;
			//	actions.single+=/mortal_strike,if=target.health.pct>20
			if (Health () > 0.2) {
				if (MortalStrike ())
					return;
			}
			//	actions.single+=/bladestorm,if=(((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4))&(!raid_event.adds.exists|raid_event.adds.in>55|(talent.anger_management.enabled&raid_event.adds.in>40))
			if ((((Target.HasAura ("Colossus Smash", true) || Cooldown ("Colossus Smash") > 3) && Health () > 0.2) || (Health () < 0.2 && Rage < 30 && Cooldown ("Colossus Smash") > 4))) {
				if (Bladestorm ())
					return;
			}
			//	actions.single+=/storm_bolt,if=target.health.pct>20|(target.health.pct<20&!debuff.colossus_smash.up)
			if (Health () > 0.2 || (Health () < 0.2 && !Target.HasAura ("Colossus Smash", true))) {
				if (StormBolt ())
					return;
			}
			//	actions.single+=/siegebreaker
			if (Siegebreaker ())
				return;
			//	actions.single+=/dragon_roar,if=!debuff.colossus_smash.up&(!raid_event.adds.exists|raid_event.adds.in>55|(talent.anger_management.enabled&raid_event.adds.in>40))
			if (!Target.HasAura ("Colossus Smash", true)) {
				if (DragonRoar ())
					return;
			}
			//	actions.single+=/execute,if=buff.sudden_death.react
			if (Me.HasAura ("Sudden Death")) {
				if (Execute ())
					return;
			}
			//	actions.single+=/execute,if=!buff.sudden_death.react&(rage>72&cooldown.colossus_smash.remains>gcd)|debuff.colossus_smash.up|target.time_to_die<5
			if (!Me.HasAura ("Sudden Death") && (Rage > 72 && Cooldown ("Colossus Smash") > 1.5) || Target.HasAura ("Colossus Smash", true) || TimeToDie () < 5) {
				if (Execute ())
					return;
			}
			//	actions.single+=/impending_victory,if=rage<40&target.health.pct>20&cooldown.colossus_smash.remains>1
			if (Rage < 40 && Health () > 0.2 && Cooldown ("Colossus Smash") > 1) {
				if (ImpendingVictory ())
					return;
			}
			//	actions.single+=/slam,if=(rage>20|cooldown.colossus_smash.remains>gcd)&target.health.pct>20&cooldown.colossus_smash.remains>1
			if ((Rage > 20 || Cooldown ("Colossus Smash") > 1.5) && Health () > 0.2 && Cooldown ("Colossus Smash") > 1) {
				if (Slam ())
					return;
			}
			//	actions.single+=/thunder_clap,if=!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&glyph.resonating_power.enabled&cooldown.colossus_smash.remains>gcd
			if (!HasSpell ("Slam") && Health () > 0.2 && (Rage >= 40 || Target.HasAura ("Colossus Smash", true)) && HasGlyph (57164) && Cooldown ("Colossus Smash") > 1.5) {
				if (ThunderClap ())
					return;
			}
			//	actions.single+=/whirlwind,if=!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&cooldown.colossus_smash.remains>gcd
			if (!HasSpell ("Slam") && Health () > 0.2 && (Rage >= 40 || Target.HasAura ("Colossus Smash", true)) && Cooldown ("Colossus Smash") > 1.5) {
				if (Whirlwind ())
					return;
			}
			//	actions.single+=/shockwave
			if (Shockwave ())
				return;
		}

		void ActionAoe ()
		{
			//	actions.aoe=sweeping_strikes
			if (SweepingStrikes ())
				return;
			//	actions.aoe+=/rend,if=ticks_remain<2&target.time_to_die>4&(target.health.pct>20|!debuff.colossus_smash.up)
			if (Target.AuraTimeRemaining ("Rend", true) < 2 && TimeToDie () > 4 && (Health () > 0.2 || !Target.HasAura ("Colossus Smash", true))) {
				if (Rend ())
					return;
			}
			//	actions.aoe+=/rend,cycle_targets=1,max_cycle_targets=2,if=ticks_remain<2&target.time_to_die>8&!buff.colossus_smash_up.up&talent.taste_for_blood.enabled
			if (HasSpell ("Taste for Blood") && !Me.HasAura ("Colossus Smash")) {
				MaxCycle = Enemy.Where (u => Range (5, u) && u.AuraTimeRemaining ("Rend", true) < 2 && TimeToDie (u) > 8);
				if (MaxCycle.ToList ().Count <= 2) {
					CycleTarget = MaxCycle.DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null && Rend (CycleTarget))
						return;
				}
			}
			//	actions.aoe+=/rend,cycle_targets=1,if=ticks_remain<2&target.time_to_die-remains>18&!buff.colossus_smash_up.up&active_enemies<=8
			if (ActiveEnemies (5) <= 8 && !Me.HasAura ("Colossus Smash")) {
				CycleTarget = Enemy.Where (u => Range (5, u) && u.AuraTimeRemaining ("Rend", true) < 2 && TimeToDie (u) - u.AuraTimeRemaining ("Rend", true) > 18).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Rend (CycleTarget))
					return;
			}
			//	actions.aoe+=/ravager,if=buff.bloodbath.up|cooldown.colossus_smash.remains<4
			if (Me.HasAura ("Bloodbath") || Cooldown ("Colossus Smash") < 4) {
				if (Ravager ()) {
					PrevRavager = DateTime.Now;
					return;
				}
			}
			//	actions.aoe+=/bladestorm,if=((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4)
			if (((Target.HasAura ("Colossus Smash", true) || Cooldown ("Colossus Smash") > 3) && Health () > 0.2) || (Health () < 0.2 && Rage < 30 && Cooldown ("Colossus Smash") > 4)) {
				if (Bladestorm ())
					return;
			}
			//	actions.aoe+=/colossus_smash,if=dot.rend.ticking
			if (Target.HasAura ("Rend", true)) {
				if (ColossusSmash ())
					return;
			}
			//	actions.aoe+=/execute,cycle_targets=1,if=!buff.sudden_death.react&active_enemies<=8&((rage>72&cooldown.colossus_smash.remains>gcd)|rage>80|target.time_to_die<5|debuff.colossus_smash.up)
			if (!Me.HasAura ("Sudden Death") && ActiveEnemies (8) <= 8) {
				CycleTarget = Enemy.Where (u => Range (5, u) && ((Rage > 72 && Cooldown ("Colossus Smash") > 1.5) || Rage > 80 || TimeToDie (u) < 5 || u.HasAura ("Colossus Smash", true))).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Execute (CycleTarget))
					return;
			}
			//	actions.aoe+=/heroic_charge,cycle_targets=1,if=target.health.pct<20&rage<70&swing.mh.remains>2&debuff.charge.down
			//	# Heroic Charge is an event that makes the warrior heroic leap out of melee range for an instant
			//	#If heroic leap is not available, the warrior will simply run out of melee to charge range, and then charge back in.
			//	#This can delay autoattacks, but typically the rage gained from charging (Especially with bull rush glyphed) is more than
			//	#The amount lost from delayed autoattacks. Charge only grants rage from charging a different target than the last time.
			//	#Which means this is only worth doing on AoE, and only when you cycle your charge target.
			//	actions.aoe+=/mortal_strike,if=target.health.pct>20&active_enemies<=5
			if (Health () > 0.2 && ActiveEnemies (5) <= 5) {
				if (MortalStrike ())
					return;
			}
			//	actions.aoe+=/dragon_roar,if=!debuff.colossus_smash.up
			if (!Target.HasAura ("Colossus Smash", true)) {
				if (DragonRoar ())
					return;
			}
			//	actions.aoe+=/thunder_clap,if=(target.health.pct>20|active_enemies>=9)&glyph.resonating_power.enabled
			if ((Health () > 0.2 || ActiveEnemies (8) >= 9) && HasGlyph (57164)) {
				if (ThunderClap ())
					return;
			}
			//	actions.aoe+=/rend,cycle_targets=1,if=ticks_remain<2&target.time_to_die>8&!buff.colossus_smash_up.up&active_enemies>=9&rage<50&!talent.taste_for_blood.enabled
			if (!Me.HasAura ("Colossus Smash") && ActiveEnemies (5) >= 9 && Rage < 50 && !HasSpell ("Taste for Blood")) {
				CycleTarget = Enemy.Where (u => Range (5, u) && u.AuraTimeRemaining ("Rend", true) < 2 && TimeToDie (u) > 8).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Rend (CycleTarget))
					return;
			}
			//	actions.aoe+=/whirlwind,if=target.health.pct>20|active_enemies>=9
			if (Health () > 0.2 || ActiveEnemies (8) >= 9) {
				if (Whirlwind ())
					return;
			}
			//	actions.aoe+=/siegebreaker
			if (Siegebreaker ())
				return;
			//	actions.aoe+=/storm_bolt,if=cooldown.colossus_smash.remains>4|debuff.colossus_smash.up
			if (Cooldown ("Colossus Smash") > 4 || Target.HasAura ("Colossus Smash", true)) {
				if (StormBolt ())
					return;
			}
			//	actions.aoe+=/shockwave
			if (Shockwave ())
				return;
			//	actions.aoe+=/execute,if=buff.sudden_death.react
			if (Me.HasAura ("Sudden Death")) {
				if (Execute ())
					return;
			}
		}
	}
}


//	//Def CD

//
//	// Interrups casting or reflect
//	if (Cast("Pummel", () => Target.IsCastingAndInterruptible() && !Me.HasAura("Spell Reflect") && !Me.HasAura("Mass Spell Reflection"))) return;
//	castingAddInRange = Adds.Where(x => x.IsInCombatRangeAndLoS && x.DistanceSquared <= 5 * 5 && x.IsCastingAndInterruptible()).ToList().FirstOrDefault(x => !Me.HasAura("Spell Reflect") && !Me.HasAura("Mass Spell Reflection"));
//	if (castingAddInRange != null) {
//		if (Cast("Pummel", castingAddInRange)) return;
//	}
//	if (Cast("Storm Bolt", () => HasSpell("Storm Bolt") && Target.IsInCombatRangeAndLoS && !Me.HasAura("Spell Reflect") && !Me.HasAura("Mass Spell Reflection") && (Target.IsCasting || IsElite || IsPlayer))) return;
//	if (HasSpell("Storm Bolt")) {
//		castingAddInRange = Adds.Where(x => x.IsInCombatRangeAndLoS && x.IsCastingAndInterruptible()).ToList().FirstOrDefault(x => !Me.HasAura("Spell Reflect") && !Me.HasAura("Mass Spell Reflection"));
//		if (castingAddInRange != null) {
//			if (Cast("Storm Bolt", castingAddInRange)) return;
//		}
//	}
//	if (CastSelf("Spell Reflection", () => HasSpell("Spell Reflection") && Target.IsCasting && Target.CombatRange <= 40 && Target.Target == Me)) return;
//	if (CastSelf("Mass Spell Reflection", () => HasSpell("Mass Spell Reflection") && Target.IsCasting)) return;
//
//	// Slow Enemy Player
//	if (Cast("Hamstring", () => (IsPlayer || IsFleeing) && !Target.HasAura("Hamstring"))) return;
//
//	//Heal
//	if (Cast("Victory Rush", () => Health < 0.9 && HasAura("Victorious"))) return;
//	if (Cast("Impending Victory", () => HasSpell("Impending Victory") && Health < 0.6)) return;
//	if (CastSelf("Rallying Cry", () => Health <= 0.25)) return;
//	if (CastSelf("Enraged Regeneration", () => Health <= 0.5)) return;
//

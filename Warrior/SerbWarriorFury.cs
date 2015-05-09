using ReBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Linq;

namespace ReBot
{
	[Rotation ("Warrior Protection SC", "Serb", WoWClass.Warrior, Specialization.WarriorProtection, 5, 25)]

	public class SerbWarriorFury : SerbWarrior
	{
		[JsonProperty ("Max rage")]
		public int RageMax = 100;
		[JsonProperty ("War cry"), JsonConverter (typeof(StringEnumConverter))]							
		public WarCry Shout = WarCry.BattleShout;

		public SerbWarriorFury ()
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
			//	actions.precombat+=/food,type=pickled_eel
			//	actions.precombat+=/stance,choose=battle
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	# Generic on-use trinket line if needed when swapping trinkets out.
			//	#actions+=/use_item,slot=trinket1,if=active_enemies=1&(buff.bloodbath.up|(!talent.bloodbath.enabled&(buff.avatar.up|!talent.avatar.enabled)))|(active_enemies>=2&buff.ravager.up)
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


			//Def CD
//			if (Cast ("Die by the Sword", () => Health <= 0.4))
//				return;
//			if (Cast ("Shield Barrier", () => Rage >= 20 && !Me.HasAura ("Shield Barrier") && Health <= 0.6))
//				return;
//			if (CastSelf ("Rallying Cry", () => Health <= 0.3))
//				return;

			// Interrups casting or reflect
//			if (SpellCooldown ("Pummel") == 0) {
//				var interruptTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.CombatRange <= 6 && u.RemainingCastTime > 0 && (u.Target == (UnitObject)Me && !Me.HasAura ("Spell Reflect")) && !Me.HasAura ("Mass Spell Reflection")).OrderBy (u => u.RemainingCastTime).DefaultIfEmpty (null).FirstOrDefault ();
//				if (Cast ("Pummel", interruptTarget, () => interruptTarget != null))
//					return;
//			}
//			if (HasSpell ("Storm Bolt") && SpellCooldown ("Storm Bolt") == 0) {
//				var interruptTarget = targets.Where (u => u.IsCastingAndInterruptible () && u.CombatRange <= 30 && u.RemainingCastTime > 0 && (u.Target == (UnitObject)Me && !Me.HasAura ("Spell Reflect")) && !Me.HasAura ("Mass Spell Reflection")).OrderBy (u => u.RemainingCastTime).DefaultIfEmpty (null).FirstOrDefault ();
//				if (Cast ("Storm Bolt", interruptTarget, () => interruptTarget != null))
//					return;
//			}

//			if (HasSpell ("Spell Reflection") && SpellCooldown ("Spell Reflection") == 0 && !HasGlobalCooldown ()) {
//				var castToMe = targets.Where (u => u.IsCasting && u.RemainingCastTime > 0 && u.Target == (UnitObject)Me && !Me.HasAura ("Mass Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
//				if (Cast ("Spell Reflection", () => castToMe != null))
//					return;
//			}
//			if (HasSpell ("Mass Spell Reflection") && SpellCooldown ("Mass Spell Reflection") == 0 && !HasGlobalCooldown ()) {
//				var castToMe = targets.Where (u => u.IsCasting && u.RemainingCastTime > 0 && u.Target == (UnitObject)Me && !Me.HasAura ("Spell Reflection")).DefaultIfEmpty (null).FirstOrDefault ();
//				if (Cast ("Mass Spell Reflection", () => castToMe != null))
//					return;
//			}

			// Slow Enemy Player
//			if (Cast ("Hamstring", () => (IsPlayer && IsFleeing) && !Target.HasAura ("Hamstring")))
//				return;
//
			//Heal
//			if (Cast ("Victory Rush", () => Health < 0.9 && Me.HasAura ("Victorious")))
//				return;
//			if (Cast ("Impending Victory", () => HasSpell ("Impending Victory") && Health < 0.6))
//				return;
//			if (CastSelf ("Rallying Cry", () => Health <= 0.25))
//				return;
//			if (CastSelf ("Enraged Regeneration", () => Health <= 0.5))
//				return;

			//CD
			// if (CastSelf("Recklessness",	() => Target.IsElite() && RecklessnessCD)) return;

			// # Executed every time the actor is available.


			//	actions=charge,if=debuff.charge.down
			//	actions+=/auto_attack
			//	# This is mostly to prevent cooldowns from being accidentally used during movement.
			//	actions+=/run_action_list,name=movement,if=movement.distance>5
			if (Range (40, Target, 10)) {
				if (Movement ())
					return;
			}
			//	actions+=/berserker_rage,if=buff.enrage.down|(prev_gcd.bloodthirst&buff.raging_blow.stack<2)
			if (!Me.HasAura ("Enrage") || (PrevGcdBloodthirst && AuraStackCount ("Raging Blow") < 2)) {
				if (BerserkerRage ())
					return;
			}
			//	actions+=/heroic_leap,if=(raid_event.movement.distance>25&raid_event.movement.in>45)|!raid_event.movement.exists
			//	actions+=/use_item,name=vial_of_convulsive_shadows,if=(active_enemies>1|!raid_event.adds.exists)&((talent.bladestorm.enabled&cooldown.bladestorm.remains=0)|buff.recklessness.up|target.time_to_die<25|!talent.anger_management.enabled)
			//	actions+=/potion,name=draenic_strength,if=(target.health.pct<20&buff.recklessness.up)|target.time_to_die<=25
			//	# Skip cooldown usage if we can line them up with bladestorm on a large set of adds, or if movement is coming soon.
			//	actions+=/run_action_list,name=single_target,if=(raid_event.adds.cooldown<60&raid_event.adds.count>2&active_enemies=1)|raid_event.movement.cooldown<5
			//	# This incredibly long line (Due to differing talent choices) says 'Use recklessness on cooldown, unless the boss will die before the ability is usable again, and then use it with execute.'
			//	actions+=/recklessness,if=(((target.time_to_die>190|target.health.pct<20)&(buff.bloodbath.up|!talent.bloodbath.enabled))|target.time_to_die<=12|talent.anger_management.enabled)&((talent.bladestorm.enabled&(!raid_event.adds.exists|enemies=1))|!talent.bladestorm.enabled)
			if ((((TimeToDie () > 190 || Health () < 0.2) && (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath"))) || TimeToDie () <= 12 || HasSpell ("Anger Management"))) {
				if (Recklessness ())
					return;
			}
			//	actions+=/avatar,if=buff.recklessness.up|cooldown.recklessness.remains>60|target.time_to_die<30
			if (Me.HasAura ("Recklessness") || Cooldown ("Recklessness") > 60 || TimeToDie () < 30) {
				if (Avatar ())
					return;
			}
			//	actions+=/blood_fury,if=buff.bloodbath.up|!talent.bloodbath.enabled|buff.recklessness.up
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath") || HasAura ("Recklessness"))
				BloodFury ();
			//	actions+=/berserking,if=buff.bloodbath.up|!talent.bloodbath.enabled|buff.recklessness.up
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath") || HasAura ("Recklessness"))
				Berserking ();
			//	actions+=/arcane_torrent,if=rage<rage.max-40
			if (Rage < RageMax - 40)
				ArcaneTorrent ();
			//	actions+=/call_action_list,name=single_target,if=active_enemies=1
			if (ActiveEnemies (10) == 1) {
				if (SingleTarget ())
					return;
			}
			//	actions+=/call_action_list,name=two_targets,if=active_enemies=2
			if (ActiveEnemies (10) == 2) {
				if (two_targets ())
					return;
			}
			//	actions+=/call_action_list,name=three_targets,if=active_enemies=3
			if (ActiveEnemies (10) == 3) {
				if (three_targets ())
					return;
			}
			//	actions+=/call_action_list,name=aoe,if=active_enemies>3
			if (ActiveEnemies (10) > 3) {
				if (aoe ())
					return;
			}

		}

		public bool SingleTarget ()
		{
			//	actions.single_target=bloodbath
			if (Bloodbath ())
				return true;
			//	actions.single_target+=/recklessness,if=target.health.pct<20&raid_event.adds.exists
			if (Health () < 0.2 && ActiveEnemies (40) > 1) {
				if (Recklessness ())
					return true;
			}
			//	actions.single_target+=/wild_strike,if=(rage>rage.max-20)&target.health.pct>20
			if ((Rage > RageMax - 20) && Health () > 0.2) {
				if (WildStrike ())
					return true;
			}
			//	actions.single_target+=/bloodthirst,if=(!talent.unquenchable_thirst.enabled&(rage<rage.max-40))|buff.enrage.down|buff.raging_blow.stack<2
			if ((!HasSpell ("Unquenchable Thirst") && (Rage < RageMax - 40)) || !Me.HasAura ("Enrage") || AuraStackCount ("Raging Blow") < 2) {
				if (Bloodthirst ()) {
					PrevBloodthirst = DateTime.Now;
					return true;
				}
			}
			//	actions.single_target+=/ravager,if=buff.bloodbath.up|(!talent.bloodbath.enabled&(!raid_event.adds.exists|raid_event.adds.in>60|target.time_to_die<40))
			if (Me.HasAura ("Bloodbath") || (!HasSpell ("Bloodbath") && TimeToDie () < 40)) {
				if (Ravager ())
					return true;
			}
			//	actions.single_target+=/siegebreaker
			if (Siegebreaker ())
				return true;
			//	actions.single_target+=/execute,if=buff.sudden_death.react
			if (Me.HasAura ("Sudden Death")) {
				if (Execute ())
					return true;
			}
			//	actions.single_target+=/storm_bolt
			if (StormBolt ())
				return true;
			//	actions.single_target+=/wild_strike,if=buff.bloodsurge.up
			if (Me.HasAura ("Bloodsurge")) {
				if (WildStrike ())
					return true;
			}
			//	actions.single_target+=/execute,if=buff.enrage.up|target.time_to_die<12
			if (Me.HasAura ("Enrage") || TimeToDie () < 12) {
				if (Execute ())
					return true;
			}
			//	actions.single_target+=/dragon_roar,if=buff.bloodbath.up|!talent.bloodbath.enabled
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath")) {
				if (DragonRoar ())
					return true;
			}
			//	actions.single_target+=/raging_blow
			if (RagingBlow ())
				return true;
			//	actions.single_target+=/wait,sec=cooldown.bloodthirst.remains,if=cooldown.bloodthirst.remains<0.5&rage<50
//	if (Cast ("Wild Strike", () => Rage >= 45 && Me.HasAura ("Enrage") && TargetHealth > 0.2))
//		return;
			//	actions.single_target+=/wild_strike,if=buff.enrage.up&target.health.pct>20
			if (Me.HasAura ("Enrage") && Health () > 0.2) {
				if (WildStrike ())
					return true;
			}
			//	actions.single_target+=/bladestorm,if=!raid_event.adds.exists
			if (Bladestorm ())
				return true;
			//	actions.single_target+=/shockwave,if=!talent.unquenchable_thirst.enabled
			if (!HasSpell ("Unquenchable Thirst")) {
				if (Shockwave ())
					return true;
			}
			//	actions.single_target+=/impending_victory,if=!talent.unquenchable_thirst.enabled&target.health.pct>20
			if (!HasSpell ("Unquenchable Thirst") && Health () > 0.2) {
				if (ImpendingVictory ())
					return true;
			}
			//	actions.single_target+=/bloodthirst
			if (Bloodthirst ())
				return true;

			return false;
		}

		public bool two_targets ()
		{
			//	actions.two_targets=bloodbath
			if (Bloodbath ())
				return true;
			//	actions.two_targets+=/ravager,if=buff.bloodbath.up|!talent.bloodbath.enabled
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath")) {
				if (Ravager ())
					return true;
			}
			//	actions.two_targets+=/dragon_roar,if=buff.bloodbath.up|!talent.bloodbath.enabled
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath")) {
				if (DragonRoar ())
					return true;
			}
			//	actions.two_targets+=/call_action_list,name=bladestorm
			if (Action_bladestorm ())
				return true;
			//	actions.two_targets+=/bloodthirst,if=buff.enrage.down|rage<40|buff.raging_blow.down
			if (!Me.HasAura ("Enrage") || Rage < 40 || !Me.HasAura (("Raging Blow"))) {
				if (Bloodthirst ())
					return true;
			}
			//	actions.two_targets+=/siegebreaker
			if (Siegebreaker ())
				return true;
			//	actions.two_targets+=/execute,cycle_targets=1
			if (HasRage (30) || Me.HasAura ("Sudden Death")) {
				CycleTarget = Enemy.Where (u => Range (5, u) && (Health (u) < 0.2 || Me.HasAura ("Sudden Death"))).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Execute (CycleTarget))
					return true; 
			}
			//	actions.two_targets+=/raging_blow,if=buff.meat_cleaver.up|target.health.pct<20
			if (Me.HasAura ("Meat Cleaver") || Health () < 0.2) {
				if (RagingBlow ())
					return true;
			}
			//	actions.two_targets+=/whirlwind,if=!buff.meat_cleaver.up&target.health.pct>20
			if (!Me.HasAura ("Meat Cleaver") && Health () > 0.2) {
				if (Whirlwind ())
					return true;
			}
			//	actions.two_targets+=/wild_strike,if=buff.bloodsurge.up
			if (Me.HasAura ("Bloodsurge")) {
				if (WildStrike ())
					return true;
			}
			//	actions.two_targets+=/bloodthirst
			if (Bloodthirst ())
				return true;
			//	actions.two_targets+=/whirlwind
			if (Whirlwind ())
				return true;

			return false;
		}

		public bool three_targets ()
		{
			//	actions.three_targets=bloodbath
			if (Bloodbath ())
				return true;
			//	actions.three_targets+=/ravager,if=buff.bloodbath.up|!talent.bloodbath.enabled
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath")) {
				if (Ravager ())
					return true;
			}
			//	actions.three_targets+=/call_action_list,name=bladestorm
			if (Action_bladestorm ())
				return true;
			//	actions.three_targets+=/bloodthirst,if=buff.enrage.down|rage<50|buff.raging_blow.down
			if (!Me.HasAura ("Enrage") || Rage < 50 || !Me.HasAura (("Raging Blow"))) {
				if (Bloodthirst ())
					return true;
			}
			//	actions.three_targets+=/raging_blow,if=buff.meat_cleaver.stack>=2
			if (AuraStackCount ("Meat Cleaver") >= 2) {
				if (RagingBlow ())
					return true;
			}
			//	actions.three_targets+=/siegebreaker
			if (Siegebreaker ())
				return true;
			//	actions.three_targets+=/execute,cycle_targets=1
			if (HasRage (30) || Me.HasAura ("Sudden Death")) {
				CycleTarget = Enemy.Where (u => Range (5, u) && (Health (u) < 0.2 || Me.HasAura ("Sudden Death"))).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && Execute (CycleTarget))
					return true; 
			}
			//	actions.three_targets+=/dragon_roar,if=buff.bloodbath.up|!talent.bloodbath.enabled
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath")) {
				if (DragonRoar ())
					return true;
			}
			//	actions.three_targets+=/whirlwind,if=target.health.pct>20
			if (Health () >= 0.2) {
				if (Whirlwind ())
					return true;
			}
			//	actions.three_targets+=/bloodthirst
			if (Bloodthirst ())
				return true;
			//	actions.three_targets+=/wild_strike,if=buff.bloodsurge.up
			if (Me.HasAura ("Bloodsurge")) {
				if (WildStrike ())
					return true;
			}
			//	actions.three_targets+=/raging_blow
			if (RagingBlow ())
				return true;

			return false;
		}

		public bool aoe ()
		{
			//	actions.aoe=bloodbath
			if (Bloodbath ())
				return true;
			//	actions.aoe+=/ravager,if=buff.bloodbath.up|!talent.bloodbath.enabled
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath")) {
				if (Ravager ())
					return true;
			}
			//	actions.aoe+=/raging_blow,if=buff.meat_cleaver.stack>=3&buff.enrage.up
			if (AuraStackCount ("Meat Cleaver") >= 3 && Me.HasAura ("Enrage")) {
				if (RagingBlow ())
					return true;
			}
			//	actions.aoe+=/bloodthirst,if=buff.enrage.down|rage<50|buff.raging_blow.down
			if (!Me.HasAura ("Enrage") || Rage < 50 || !Me.HasAura (("Raging Blow"))) {
				if (Bloodthirst ())
					return true;
			}
			//	actions.aoe+=/raging_blow,if=buff.meat_cleaver.stack>=3
			if (AuraStackCount ("Meat Cleaver") >= 3) {
				if (RagingBlow ())
					return true;
			}
			//	actions.aoe+=/call_action_list,name=bladestorm
			if (Action_bladestorm ())
				return true;
			//	actions.aoe+=/whirlwind
			if (WildStrike ())
				return true;
			//	actions.aoe+=/siegebreaker
			if (Siegebreaker ())
				return true;
			//	actions.aoe+=/execute,if=buff.sudden_death.react
			if (HasAura ("Sudden Death")) {
				if (Execute ())
					return true;
			}
			//	actions.aoe+=/dragon_roar,if=buff.bloodbath.up|!talent.bloodbath.enabled
			if (Me.HasAura ("Bloodbath") || !HasSpell ("Bloodbath")) {
				if (DragonRoar ())
					return true;
			}
			//	actions.aoe+=/bloodthirst
			if (Bloodthirst ())
				return true;
			//	actions.aoe+=/wild_strike,if=buff.bloodsurge.up
			if (Me.HasAura ("Bloodsurge")) {
				if (WildStrike ())
					return true;
			}

			return false;
		}

		public bool Movement ()
		{

			//	actions.movement=heroic_leap
			if (CastOnTerrain ("Heroic Leap", Target.Position, () => Range >= 8 && Range <= 40 && RunToEnemy))
				return true;
			//	actions.movement+=/charge,cycle_targets=1,if=debuff.charge.down
			//	# If possible, charge a target that will give rage. Otherwise, just charge to get back in range.
			//	actions.movement+=/charge
			//	# May as well throw storm bolt if we can.
			//	actions.movement+=/storm_bolt
			if (Cast ("Storm Bolt", () => HasSpell ("Storm Bolt")))
				return true;
			//	actions.movement+=/heroic_throw
			if (Cast ("Heroic Throw", () => Range >= 8 && Range <= 40))
				return true;

			return false;
		}

		public bool Action_bladestorm ()
		{
			//	actions.bladestorm=recklessness,sync=bladestorm,if=buff.enrage.remains>6&((talent.anger_management.enabled&raid_event.adds.in>45)|(!talent.anger_management.enabled&raid_event.adds.in>60)|!raid_event.adds.exists|active_enemies>desired_targets)
			//	actions.bladestorm+=/bladestorm,if=buff.enrage.remains>6&((talent.anger_management.enabled&raid_event.adds.in>45)|(!talent.anger_management.enabled&raid_event.adds.in>60)|!raid_event.adds.exists|active_enemies>desired_targets)
		
			return false;
		}
	}
}


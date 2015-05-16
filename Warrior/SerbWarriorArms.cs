using System;
using ReBot.API;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReBot
{
	[Rotation ("Serb Warrior Arms SC", "Serb", WoWClass.Warrior, Specialization.WarriorArms, 5, 25)]

	public class SerbWarriorArms : SerbWarrior
	{
		[JsonProperty ("Max rage")]
		public int RageMax = 100;
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
			if (Me.HasAura ("Bloodbath") || (!HasSpell ("Bloodbath") && Target.HasAura ("Colossus Smash")) || Me.HasAura ("Recklessness")) {
				BloodFury ();
			}
			//	actions+=/berserking,if=buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up)|buff.recklessness.up
			if (Me.HasAura ("Bloodbath") || (!HasSpell ("Bloodbath") && Target.HasAura ("Colossus Smash")) || Me.HasAura ("Recklessness")) {
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


//			// actions+=/use_item,name=bonemaws_big_toe,if=(buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up))
//			if (Me.HasAura("Bloodbath") || (!HasSpell("Bloodbath") && Target.HasAura("Colossus Smash"))) {
//				API.UseItem(110012);
//				return;
//			}
//			// actions+=/use_item,name=turbulent_emblem,if=(buff.bloodbath.up|(!talent.bloodbath.enabled&debuff.colossus_smash.up))
//			if (Me.HasAura("Bloodbath") || (!HasSpell("Bloodbath") && Target.HasAura("Colossus Smash"))) {
//				API.UseItem(114491);
//				return;
//			}

		}

		public bool Movement ()
		{
			//	actions.movement=heroic_leap
			if (HeroicLeap ())
				return true;
			//	actions.movement+=/charge,cycle_targets=1,if=debuff.charge.down
			//	# If possible, charge a target that will give us rage. Otherwise, just charge to get back in range.
			//	actions.movement+=/charge
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
			if (SpellCooldown ("Colossus Smash") < 4) {
				if (Ravager ())
					return;
			}
			//	actions.single+=/colossus_smash
			if (ColossusSmash ())
				return;
			//	actions.single+=/mortal_strike,if=target.health.pct>20
			// actions.single+=/mortal_strike,if=target.health.pct>20
			if (Health () > 0.2) {
				if (MortalStrike ())
					return;
			}
			//	actions.single+=/bladestorm,if=(((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4))&(!raid_event.adds.exists|raid_event.adds.in>55|(talent.anger_management.enabled&raid_event.adds.in>40))
			// actions.single+=/bladestorm,if=(((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4))&(!raid_event.adds.exists|raid_event.adds.in>55|(talent.anger_management.enabled&raid_event.adds.in>40))
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
			//	actions.single+=/execute,if=buff.sudden_death.react
			//	actions.single+=/execute,if=!buff.sudden_death.react&(rage>72&cooldown.colossus_smash.remains>gcd)|debuff.colossus_smash.up|target.time_to_die<5
			//	actions.single+=/impending_victory,if=rage<40&target.health.pct>20&cooldown.colossus_smash.remains>1
			//	actions.single+=/slam,if=(rage>20|cooldown.colossus_smash.remains>gcd)&target.health.pct>20&cooldown.colossus_smash.remains>1
			//	actions.single+=/thunder_clap,if=!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&glyph.resonating_power.enabled&cooldown.colossus_smash.remains>gcd
			//	actions.single+=/whirlwind,if=!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&cooldown.colossus_smash.remains>gcd
			//	actions.single+=/shockwave


			// actions.single+=/dragon_roar,if=!debuff.colossus_smash.up&(!raid_event.adds.exists|raid_event.adds.in>55|(talent.anger_management.enabled&raid_event.adds.in>40))
			if (Cast ("Dragon Roar",	() => HasSpell ("Dragon Roar") && !Target.HasAura ("Colossus Smash")))
				return;
			// actions.single+=/rend,if=!debuff.colossus_smash.up&target.time_to_die>4&remains<5.4
			if (Cast ("Rend", () => !Target.HasAura ("Colossus Smash") && TimeToDie (Target) > 4 && Target.AuraTimeRemaining ("Rend") < 5.4))
				return;
			// actions.single+=/execute,if=buff.sudden_death.react
			if (Cast ("Execute",	() => Me.HasAura ("Sudden Death")))
				return;
			// actions.single+=/execute,if=!buff.sudden_death.react&(rage>72&cooldown.colossus_smash.remains>gcd)|debuff.colossus_smash.up|target.time_to_die<5
			if (Cast ("Execute",	() => !Me.HasAura ("Sudden Death") && (Rage > 72 && SpellCooldown ("Colossus Smash") > 1.5) || Target.HasAura ("Colossus Smash") || TimeToDie (Target) < 5))
				return;
			// actions.single+=/impending_victory,if=rage<40&target.health.pct>20&cooldown.colossus_smash.remains>1
			if (Cast ("Impending Victory", () => Rage < 40 && TargetHealth > 0.2 && SpellCooldown ("Colossus Smash") > 1))
				return;
			// actions.single+=/slam,if=(rage>20|cooldown.colossus_smash.remains>gcd)&target.health.pct>20&cooldown.colossus_smash.remains>1
			if (Cast ("Slam", () => (Rage > 20 || SpellCooldown ("Colossus Smash") > 1.5) && TargetHealth > 0.2 && SpellCooldown ("Colossus Smash") > 1))
				return;
			// actions.single+=/thunder_clap,if=!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&glyph.resonating_power.enabled&cooldown.colossus_smash.remains>gcd
			if (Cast ("Thunder Clap", () => !HasSpell ("Slam") && TargetHealth > 0.2 && (Rage >= 40 || Target.HasAura ("Colossus Smash")) && HasGlyph (57164) && SpellCooldown ("Colossus Smash") > 1.5))
				return;
			// actions.single+=/whirlwind,if=!talent.slam.enabled&target.health.pct>20&(rage>=40|debuff.colossus_smash.up)&cooldown.colossus_smash.remains>gcd
			if (Cast ("Whirlwind", () => !HasSpell ("Slam") && TargetHealth > 0.2 && (Rage >= 40 || Target.HasAura ("Colossus Smash")) && SpellCooldown ("Colossus Smash") > 1.5))
				return;
			// actions.single+=/shockwave
			if (Cast ("Shockwave", () => HasSpell ("Shockwave")))
				return;

		}

		void ActionAoe ()
		{
			//	actions.aoe=sweeping_strikes
			//	actions.aoe+=/rend,if=ticks_remain<2&target.time_to_die>4&(target.health.pct>20|!debuff.colossus_smash.up)
			//	actions.aoe+=/rend,cycle_targets=1,max_cycle_targets=2,if=ticks_remain<2&target.time_to_die>8&!buff.colossus_smash_up.up&talent.taste_for_blood.enabled
			//	actions.aoe+=/rend,cycle_targets=1,if=ticks_remain<2&target.time_to_die-remains>18&!buff.colossus_smash_up.up&active_enemies<=8
			//	actions.aoe+=/ravager,if=buff.bloodbath.up|cooldown.colossus_smash.remains<4
			//	actions.aoe+=/bladestorm,if=((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4)
			//	actions.aoe+=/colossus_smash,if=dot.rend.ticking
			//	actions.aoe+=/execute,cycle_targets=1,if=!buff.sudden_death.react&active_enemies<=8&((rage>72&cooldown.colossus_smash.remains>gcd)|rage>80|target.time_to_die<5|debuff.colossus_smash.up)
			//	actions.aoe+=/heroic_charge,cycle_targets=1,if=target.health.pct<20&rage<70&swing.mh.remains>2&debuff.charge.down
			//	# Heroic Charge is an event that makes the warrior heroic leap out of melee range for an instant
			//	#If heroic leap is not available, the warrior will simply run out of melee to charge range, and then charge back in.
			//	#This can delay autoattacks, but typically the rage gained from charging (Especially with bull rush glyphed) is more than
			//	#The amount lost from delayed autoattacks. Charge only grants rage from charging a different target than the last time.
			//	#Which means this is only worth doing on AoE, and only when you cycle your charge target.
			//	actions.aoe+=/mortal_strike,if=target.health.pct>20&active_enemies<=5
			//	actions.aoe+=/dragon_roar,if=!debuff.colossus_smash.up
			//	actions.aoe+=/thunder_clap,if=(target.health.pct>20|active_enemies>=9)&glyph.resonating_power.enabled
			//	actions.aoe+=/rend,cycle_targets=1,if=ticks_remain<2&target.time_to_die>8&!buff.colossus_smash_up.up&active_enemies>=9&rage<50&!talent.taste_for_blood.enabled
			//	actions.aoe+=/whirlwind,if=target.health.pct>20|active_enemies>=9
			//	actions.aoe+=/siegebreaker
			//	actions.aoe+=/storm_bolt,if=cooldown.colossus_smash.remains>4|debuff.colossus_smash.up
			//	actions.aoe+=/shockwave
			//	actions.aoe+=/execute,if=buff.sudden_death.react


			// actions.aoe=sweeping_strikes
			if (CastSelf ("Sweeping Strikes", () => !Me.HasAura ("Sweeping Strikes")))
				return;
			// actions.aoe+=/rend,if=ticks_remain<2&target.time_to_die>4&(target.health.pct>20|!debuff.colossus_smash.up)
			if (Cast ("Rend", () => Target.AuraTimeRemaining ("Rend") < 2 && TimeToDie (Target) > 4 && (TargetHealth > 0.2 || !Target.HasAura ("Colossus Smash"))))
				return;
			// actions.aoe+=/rend,cycle_targets=1,max_cycle_targets=2,if=ticks_remain<2&target.time_to_die>8&!buff.colossus_smash_up.up&talent.taste_for_blood.enabled
			if (HasSpell ("Taste for Blood") && !Me.HasAura ("Colossus Smash")) {
				castingAddInRange = Adds.Where (x => x.DistanceSquared <= 5 * 5).ToList ().FirstOrDefault (x => x.AuraTimeRemaining ("Rend") < 2 && TimeToDie (x) > 8);
				if (castingAddInRange != null)
				if (Cast ("Rend", castingAddInRange))
					return;
			}
			// actions.aoe+=/rend,cycle_targets=1,if=ticks_remain<2&target.time_to_die-remains>18&!buff.colossus_smash_up.up&active_enemies<=8
			if (nearbyAdds <= 8 && !Me.HasAura ("Colossus Smash")) {
				castingAddInRange = Adds.Where (x => x.DistanceSquared <= 5 * 5).ToList ().FirstOrDefault (x => x.AuraTimeRemaining ("Rend") < 2 && TimeToDie (x) - x.AuraTimeRemaining ("Rend") > 18);
				if (castingAddInRange != null)
				if (Cast ("Rend", castingAddInRange))
					return;
			}
			// actions.aoe+=/ravager,if=buff.bloodbath.up|cooldown.colossus_smash.remains<4
			if (Cast ("Ravager",	() => HasSpell ("Ravager") && (HasAura ("Bloodbath") || SpellCooldown ("Colossus Smash") < 4)))
				return;
			// actions.aoe+=/bladestorm,if=((debuff.colossus_smash.up|cooldown.colossus_smash.remains>3)&target.health.pct>20)|(target.health.pct<20&rage<30&cooldown.colossus_smash.remains>4)
			if (Cast ("Bladestorm", () => HasSpell ("Bladestorm") && ((Target.HasAura ("Colossus Smash") || SpellCooldown ("Colossus Smash") > 3) && TargetHealth > 0.2) || (TargetHealth < 0.2 && Rage < 30 && SpellCooldown ("Colossus Smash") > 4)))
				return;
			// actions.aoe+=/colossus_smash,if=dot.rend.ticking
			if (Cast ("Colossus Smash", () => Target.HasAura ("Rend")))
				return;
			// actions.aoe+=/execute,cycle_targets=1,if=!buff.sudden_death.react&active_enemies<=8&((rage>72&cooldown.colossus_smash.remains>gcd)|rage>80|target.time_to_die<5|debuff.colossus_smash.up)
			if (!Me.HasAura ("Sudden Death") && nearbyAdds <= 8) {
				castingAddInRange = Adds.Where (x => x.DistanceSquared <= 5 * 5).ToList ().FirstOrDefault (x => (Rage > 72 && SpellCooldown ("Colossus Smash") > 1.5) || Rage > 80 || TimeToDie (x) < 5 || x.HasAura ("Colossus Smash"));
				if (castingAddInRange != null)
				if (Cast ("Execute", castingAddInRange))
					return;
			}
			// actions.aoe+=/mortal_strike,if=target.health.pct>20&active_enemies<=5
			if (Cast ("Mortal Strike", () => TargetHealth > 0.2 && nearbyAdds <= 5))
				return;
			// actions.aoe+=/dragon_roar,if=!debuff.colossus_smash.up
			if (Cast ("Dragon Roar",	() => HasSpell ("Dragon Roar") && !Target.HasAura ("Colossus Smash")))
				return;
			// actions.aoe+=/thunder_clap,if=(target.health.pct>20|active_enemies>=9)&glyph.resonating_power.enabled
			if (Cast ("Thunder Clap", () => (TargetHealth > 0.2 || nearbyAdds >= 9) && HasGlyph (57164)))
				return;
			// actions.aoe+=/rend,cycle_targets=1,if=ticks_remain<2&target.time_to_die>8&!buff.colossus_smash_up.up&active_enemies>=9&rage<50&!talent.taste_for_blood.enabled
			if (!Me.HasAura ("Colossus Smash") && nearbyAdds >= 9 && Rage < 50 && !HasSpell ("Taste for Blood")) {
				castingAddInRange = Adds.Where (x => x.DistanceSquared <= 5 * 5).ToList ().FirstOrDefault (x => x.AuraTimeRemaining ("Rend") < 2 && TimeToDie (x) > 8);
				if (castingAddInRange != null)
				if (Cast ("Rend", castingAddInRange))
					return;
			}
			// actions.aoe+=/whirlwind,if=target.health.pct>20|active_enemies>=9
			if (Cast ("Whirlwind", () => TargetHealth > 0.2 || nearbyAdds >= 9))
				return;
			// actions.aoe+=/siegebreaker
			if (Cast ("Siegebreaker", () => HasSpell ("Siegebreaker")))
				return;
			// actions.aoe+=/storm_bolt,if=cooldown.colossus_smash.remains>4|debuff.colossus_smash.up
			if (Cast ("Storm Bolt", () => HasSpell ("Storm Bolt") && SpellCooldown ("Colossus Smash") > 4 || Target.HasAura ("Colossus Smash")))
				return;
			// actions.aoe+=/shockwave
			if (Cast ("Shockwave", () => HasSpell ("Shockwave")))
				return;
			// actions.aoe+=/execute,if=buff.sudden_death.react
			if (Cast ("Execute",	() => Me.HasAura ("Sudden Death")))
				return;
		}
	}
}


//	//Def CD
//	// if (Cast("Die by the Sword",		() => MyHealth <= DbtSwordHP)) return;
//	// if (CastSelf("Shield Wall",			() => MyHealth <= ShieldWallHP)) return;
//	// if (Cast("Shield Block",			() => SpellCharges("Shield Block") == 2 && MyRage >= 60 && MyHealth <= ShieldBlockHP && !Me.HasAura("Shield Block"))) return;
//	// if (Cast("Shield Barrier",			() => MyRage >= 20 && !Me.HasAura("Shield Barrier") && MyHealth <= ShieldBarrHP)) return;
//	// if (CastSelf("Rallying Cry",		() => MyHealth <= RallyingCryHP)) return;
//	// if (CastSelf("Demoralizing Shout",	() => MyHealth <= DemoralShoutHP)) return;
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
//	//CD
//	// if (CastSelf("Recklessness",	() => Target.IsElite() && RecklessnessCD)) return;



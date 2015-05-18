using ReBot.API;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReBot
{
	[Rotation ("Warrior Protection SC", "Serb", WoWClass.Warrior, Specialization.WarriorProtection, 5, 25)]

	public class SerbWarriorProtectionSC : SerbWarrior
	{
		[JsonProperty ("War cry"), JsonConverter (typeof(StringEnumConverter))]							
		public WarCry Shout = WarCry.BattleShout;
		[JsonProperty ("Use Movement")]
		public bool Move = false;
		[JsonProperty ("Use Charge")]
		public bool UseCharge = false;


		public 	SerbWarriorProtectionSC ()
		{
			GroupBuffs = new[] {
				"Battle Shout"
			};
			PullSpells = new[] {
				"Devastate"
			};
		}

		public override bool OutOfCombat ()
		{
			//	actions.precombat=flask,type=greater_draenic_stamina_flask
			//	actions.precombat+=/food,type=sleeper_sushi
			//	actions.precombat+=/stance,choose=defensive
			//	# Snapshot raid buffed stats before combat begins and pre-potting is done.
			//	# Generic on-use trinket line if needed when swapping trinkets out. 
			//	#actions+=/use_item,slot=trinket1,if=buff.bloodbath.up|buff.avatar.up|target.time_to_die<10
			//	actions.precombat+=/snapshot_stats
			//	actions.precombat+=/shield_wall
			//	actions.precombat+=/potion,name=draenic_armor
			if (InCombat) {
				InCombat = false;
			}

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
			if (!InCombat) {
				InCombat = true;
				StartBattle = DateTime.Now;
			}

			if (Interrupt ())
				return;

			if (Reflect ())
				return;

			if (Me.CanNotParticipateInCombat ())
				Freedom ();

			if (Gcd && HasGlobalCooldown ())
				return;

			if (Health (Me) < 0.9 && Me.HasAura ("Victorious")) {
				if (VictoryRush ())
					return;
			}
			if (Health (Me) < 0.9 && HasAura ("Victorious")) {
				if (ImpendingVictory ())
					return;
			}

			if (!(InInstance || InRaid) || Health (Me) < 0.4) {
				if (Heal ())
					return;
			}

//			if (UseStance && !(Me.HasAura ("Defensive Stance") || Me.HasAura ("Improved Defensive Stance")) && (CombatRole == CombatRole.Tank || Health () < 0.4)) {
//				if (DefensiveStance ())
//					return;
//			}
//
//			if (UseStance && !Me.HasAura ("Battle Stance") && (CombatRole == CombatRole.DPS || Health () >= 0.4)) {
//				if (BattleStance ())
//					return;
//			}

//			if (Me.HasAura ("Defensive Stance") || Me.HasAura ("Improved Defensive Stance")) {
			if (Protection ())
				return;
//			}

//			if (Me.HasAura ("Battle Stance")) {
//				if (Gladiator ())
//					return;
//			}

		}

		public bool Protection ()
		{
			//	actions=charge
//			if (Range (40, Target, 10)) {
//				if (Charge ())
//					return true;
//			}
			//	actions+=/auto_attack
			//	actions+=/use_item,name=tablet_of_turnbuckle_teamwork,if=active_enemies=1&(buff.bloodbath.up|!talent.bloodbath.enabled)|(active_enemies>=2&buff.ravager_protection.up)
			//	actions+=/blood_fury,if=buff.bloodbath.up|buff.avatar.up
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				BloodFury ();
			//	actions+=/berserking,if=buff.bloodbath.up|buff.avatar.up
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				Berserking ();
			//	actions+=/arcane_torrent,if=buff.bloodbath.up|buff.avatar.up
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				ArcaneTorrent ();
			//	actions+=/berserker_rage,if=buff.enrage.down
			if (!Me.HasAura ("Enrage"))
				BerserkerRage ();
			//	actions+=/call_action_list,name=prot
			if (Prot ())
				return true;
			// --- My 
//			if (Me.Level >= 32 & !Target.HasAura ("Deep Wounds", true)) {
//				if (ThunderClap ())
//					return true;
//			}
			if (Me.Level < 100) {
				if (Rage >= RageMax - 10) {
					if (Execute ())
						return true;
				}
				if (Rage >= RageMax - 40) {
					if (HeroicStrike ())
						return true;
				}
			}

			return false;
		}

		public bool Prot ()
		{
			//	actions.prot=shield_block,if=!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up)
			if (Health (Me) < 0.9 && UseShieldBlock && !(Target.HasAura ("Demoralizing Shout") || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block")))
				ShieldBlock ();
			//	actions.prot+=/shield_barrier,if=buff.shield_barrier.down&((buff.shield_block.down&action.shield_block.charges_fractional<0.75)|rage>=85)
			if (Health (Me) < 0.9 && !Me.HasAura ("Shield Barrier") && ((!Me.HasAura ("Shield Block") && Frac ("Shield Block") < 0.75) || Rage >= 85))
				ShieldBarrier (); // 5
			//	actions.prot+=/demoralizing_shout,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion")))
				DemoralizingShout ();
			//	actions.prot+=/enraged_regeneration,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			if (Health (Me) < 0.7 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion")))
				EnragedRegeneration ();
			//	actions.prot+=/shield_wall,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion")))
				ShieldWall ();
			//	actions.prot+=/last_stand,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion")))
				LastStand ();
			//	actions.prot+=/potion,name=draenic_armor,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)|target.time_to_die<=25
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion")) || TimeToDie () <= 25) {
				if (DraenicArmor ())
					return true;
			}
			//	actions.prot+=/stoneform,if=incoming_damage_2500ms>health.max*0.1&!(debuff.demoralizing_shout.up|buff.ravager_protection.up|buff.shield_wall.up|buff.last_stand.up|buff.enraged_regeneration.up|buff.shield_block.up|buff.potion.up)
			if (Health (Me) < 0.4 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion")))
				Stoneform ();
			//	actions.prot+=/call_action_list,name=prot_aoe,if=active_enemies>3
			if (ActiveEnemies (8) > 3) {
				if (ProtAoe ())
					return true;
			}
			//	actions.prot+=/heroic_strike,if=buff.ultimatum.up|(talent.unyielding_strikes.enabled&buff.unyielding_strikes.stack>=6)
			if (Me.HasAura ("Ultimatum") || (HasSpell ("Unyielding Strikes") && AuraStackCount ("Unyielding Strikes") >= 6)) {
				if (HeroicStrike ())
					return true;
			}
			//	actions.prot+=/bloodbath,if=talent.bloodbath.enabled&((cooldown.dragon_roar.remains=0&talent.dragon_roar.enabled)|(cooldown.storm_bolt.remains=0&talent.storm_bolt.enabled)|talent.shockwave.enabled)
			if (HasSpell ("Bloodbath") && ((Cooldown ("Dragon Roar") == 0 && HasSpell ("Dragon Roar")) || (Cooldown ("Storm Bolt") == 0 && HasSpell ("Storm Bolt")) || HasSpell ("Shockwave")))
				Bloodbath ();
			//	actions.prot+=/avatar,if=talent.avatar.enabled&((cooldown.ravager.remains=0&talent.ravager.enabled)|(cooldown.dragon_roar.remains=0&talent.dragon_roar.enabled)|(talent.storm_bolt.enabled&cooldown.storm_bolt.remains=0)|(!(talent.dragon_roar.enabled|talent.ravager.enabled|talent.storm_bolt.enabled)))
			if (HasSpell ("Avatar") && ((Cooldown ("Ravager") == 0 && HasSpell ("Ravager")) || (Cooldown ("Dragon Roar") == 0 && HasSpell ("Dragon Roar")) || (Cooldown ("Storm Bolt") == 0 && HasSpell ("Storm Bolt")) || (!(HasSpell ("Dragon Roar") || HasSpell ("Ravager") || HasSpell ("Storm Bolt")))))
				Avatar ();
			//	actions.prot+=/shield_slam
			if (ShieldSlam ())
				return true;
			//	actions.prot+=/revenge
			if (Revenge ())
				return true;
			//	actions.prot+=/ravager
			if (Ravager ())
				return true;
			//	actions.prot+=/storm_bolt
			if (StormBolt ())
				return true;
			//	actions.prot+=/dragon_roar
			DragonRoar ();
			//	actions.prot+=/impending_victory,if=talent.impending_victory.enabled&cooldown.shield_slam.remains<=execute_time
			if (HasSpell ("Impending Victory") && Cooldown ("Shield Slam") <= 1.5) {
				if (ImpendingVictory ())
					return true;
			}
			//	actions.prot+=/victory_rush,if=!talent.impending_victory.enabled&cooldown.shield_slam.remains<=execute_time
			if (!HasSpell ("Impending Victory") && Cooldown ("Shield Slam") <= 1.5) {
				if (VictoryRush ())
					return true;
			}
			//	actions.prot+=/execute,if=buff.sudden_death.react
			if (Me.HasAura ("Sudden Death")) {
				if (Execute ())
					return true;
			}
			//	actions.prot+=/devastate
			if (Devastate ())
				return true;

			return false;
		}

		public bool ProtAoe ()
		{
			//	actions.prot_aoe=bloodbath
			Bloodbath ();
			//	actions.prot_aoe+=/avatar
			Avatar ();
			//	actions.prot_aoe+=/thunder_clap,if=!dot.deep_wounds.ticking
			if (Me.Level >= 32 && Usable ("Thunder Clap")) {
				CycleTarget = Enemy.Where (u => Range (8, u) && !u.HasAura ("Deep Wounds", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && ThunderClap ())
					return true;
			}
			//	actions.prot_aoe+=/heroic_strike,if=buff.ultimatum.up|rage>110|(talent.unyielding_strikes.enabled&buff.unyielding_strikes.stack>=6)
			if (Me.HasAura ("Ultimatum") || Rage >= RageMax - 20 || (HasSpell ("Unyielding Strikes") && Me.GetAura ("Unyielding Strikes").StackCount >= 6)) {
				if (HeroicStrike ())
					return true;
			}
			//	actions.prot_aoe+=/heroic_leap,if=(raid_event.movement.distance>25&raid_event.movement.in>45)|!raid_event.movement.exists
			//
			//	actions.prot_aoe+=/shield_slam,if=buff.shield_block.up
			if (Me.HasAura ("Shield Block")) {
				if (ShieldSlam ())
					return true;
			}
			//	actions.prot_aoe+=/ravager,if=(buff.avatar.up|cooldown.avatar.remains>10)|!talent.avatar.enabled
			if ((Me.HasAura ("Avatar") || Cooldown ("Avatar") > 10) || !HasSpell ("Avatar")) {
				if (Ravager ())
					return true;
			}
			//	actions.prot_aoe+=/dragon_roar,if=(buff.bloodbath.up|cooldown.bloodbath.remains>10)|!talent.bloodbath.enabled
			if ((Me.HasAura ("Bloodbath") || Cooldown ("Bloodbath") > 10) || !HasSpell ("Bloodbath"))
				DragonRoar ();
			//	actions.prot_aoe+=/shockwave
			if (Shockwave ())
				return true;
			//	actions.prot_aoe+=/revenge
			if (InInstance) {
				CycleTarget = Enemy.Where (u => Range (5, u) && u != Target).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Revenge (CycleTarget))
						return true;
				} else {
					if (Revenge ())
						return true;
				}
			} else {
				if (Revenge ())
					return true;
			}
			//	actions.prot_aoe+=/thunder_clap
			if (ThunderClap ())
				return true;
			//	actions.prot_aoe+=/bladestorm
			if (Bladestorm ())
				return true;
			//	actions.prot_aoe+=/shield_slam
			if (ShieldSlam ())
				return true;
			//	actions.prot_aoe+=/storm_bolt
			if (StormBolt ())
				return true;
			//	actions.prot_aoe+=/shield_slam
			if (ShieldSlam ())
				return true;
			//	actions.prot_aoe+=/execute,if=buff.sudden_death.react
			if (Me.HasAura ("Sudden Death")) {
				if (InInstance) {
					CycleTarget = Enemy.Where (u => Range (5, u) && u != Target).DefaultIfEmpty (null).FirstOrDefault ();
					if (CycleTarget != null) {
						if (Execute (CycleTarget))
							return true;
					} else {
						if (Execute ())
							return true;
					}
				} else {
					if (Execute ())
						return true;
				}
			}
			//	actions.prot_aoe+=/devastate
			if (InInstance) {
				CycleTarget = Enemy.Where (u => Range (5, u) && u != Target).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (Devastate (CycleTarget))
						return true;
				} else {
					if (Devastate ())
						return true;
				}
			} else {
				if (Devastate ())
					return true;
			}
			
			return false;
		}

		public bool Gladiator ()
		{
			//	actions=charge
			if (Range (10) && Move) {
				if (Charge ())
					return true;
			}
			//	actions+=/auto_attack
			//	# This is mostly to prevent cooldowns from being accidentally used during movement.
			//	actions+=/call_action_list,name=movement,if=movement.distance>5
			if (Range (5)) {
				if (Movement ())
					return true;
			}
			//	actions+=/avatar
			Avatar ();
			//	actions+=/bloodbath
			Bloodbath ();
			//	actions+=/blood_fury,if=buff.bloodbath.up|buff.avatar.up|buff.shield_charge.up|target.time_to_die<10
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar") || Me.HasAura ("Shield Charge") || TimeToDie () < 10)
				BloodFury ();
			//	actions+=/berserking,if=buff.bloodbath.up|buff.avatar.up|buff.shield_charge.up|target.time_to_die<10
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar") || Me.HasAura ("Shield Charge") || TimeToDie () < 10)
				Berserking ();
			//	actions+=/arcane_torrent,if=rage<rage.max-40
			if (Rage < RageMax - 40)
				ArcaneTorrent ();
			//	actions+=/potion,name=draenic_armor,if=buff.bloodbath.up|buff.avatar.up|buff.shield_charge.up
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar") || Me.HasAura ("Shield Charge"))
				DraenicArmor ();
			//	actions+=/shield_charge,if=(!buff.shield_charge.up&!cooldown.shield_slam.remains)|charges=2
			if ((!Me.HasAura ("Shield Charge") && Cooldown ("Shield Slam") > 0) || SpellCharges ("Shield Charge") == 2) {
				if (ShieldCharge ())
					return true;
			}
			//	actions+=/berserker_rage,if=buff.enrage.down
			if (!Me.HasAura ("Enrage"))
				BerserkerRage ();
			//	actions+=/heroic_leap,if=(raid_event.movement.distance>25&raid_event.movement.in>45)|!raid_event.movement.exists
			//	actions+=/heroic_strike,if=(buff.shield_charge.up|(buff.unyielding_strikes.up&rage>=80-buff.unyielding_strikes.stack*10))&target.health.pct>20
			if ((Me.HasAura ("Shield Charge") || (Me.HasAura ("Unyielding Strikes") && Rage >= 80 - AuraStackCount ("Unyielding Strikes") * 10)) && Health (Target) > 20) {
				if (HeroicStrike ())
					return true;
			}
			//	actions+=/heroic_strike,if=buff.ultimatum.up|rage>=rage.max-20|buff.unyielding_strikes.stack>4|target.time_to_die<10
			if (Me.HasAura ("Ultimatum") || Rage >= RageMax - 20 || Me.GetAura ("Unyielding Strikes").StackCount > 4 || TimeToDie () < 10) {
				if (HeroicStrike ())
					return true;
			}
			//	actions+=/call_action_list,name=single,if=active_enemies=1
			if (ActiveEnemies (8) == 1) {
				if (GladSingle ())
					return true;
			}
			//	actions+=/call_action_list,name=aoe,if=active_enemies>=2
			if (ActiveEnemies (8) >= 2) {
				if (GladAoe ())
					return true;
			}

			return false;
		}

		public bool Movement ()
		{
			//	actions.movement=heroic_leap
			HeroicLeap ();
			//	actions.movement+=/shield_charge
			ShieldCharge ();
			//	# May as well throw storm bolt if we can.
			//	actions.movement+=/storm_bolt
			if (StormBolt ())
				return true;
			//	actions.movement+=/heroic_throw
			if (HeroicThrow ())
				return true;

			return false;
		}

		public bool GladSingle ()
		{
			//	actions.single=devastate,if=buff.unyielding_strikes.stack>0&buff.unyielding_strikes.stack<6&buff.unyielding_strikes.remains<1.5
			if (Me.GetAura ("Unyielding Strikes").StackCount > 0 && Me.GetAura ("Unyielding Strikes").StackCount < 6 && Me.AuraTimeRemaining ("Unyielding Strikes") < 1.5) {
				if (Devastate ())
					return true;
			}
			//	actions.single+=/shield_slam
			if (ShieldSlam ())
				return true;
			//	actions.single+=/revenge
			if (Revenge ())
				return true;
			//	actions.single+=/execute,if=buff.sudden_death.react
			if (Me.HasAura ("Sudden Death")) {
				if (Execute ())
					return true;
			}
			//	actions.single+=/storm_bolt
			if (StormBolt ())
				return true;
			//	actions.single+=/dragon_roar,if=buff.unyielding_strikes.stack>=4&buff.unyielding_strikes.stack<6
			if (AuraStackCount ("Unyielding Strikes") >= 4 && Me.GetAura ("Unyielding Strikes").StackCount < 6)
				DragonRoar ();
			//	actions.single+=/execute,if=rage>60&target.health.pct<20
			if (Execute ())
				return true;
			//	actions.single+=/devastate
			if (Devastate ())
				return true;

			return false;
		}

		public bool GladAoe ()
		{
			//	actions.aoe=revenge
			if (Revenge ())
				return true;
			//	actions.aoe+=/shield_slam
			if (ShieldSlam ())
				return true;
			//	actions.aoe+=/dragon_roar,if=(buff.bloodbath.up|cooldown.bloodbath.remains>10)|!talent.bloodbath.enabled
			if ((Me.HasAura ("Bloodbath") || Cooldown ("Bloodbath") > 10) || !HasSpell ("Bloodbath"))
				DragonRoar ();
			//	actions.aoe+=/storm_bolt,if=(buff.bloodbath.up|cooldown.bloodbath.remains>7)|!talent.bloodbath.enabled
			if ((Me.HasAura ("Bloodbath") || Cooldown ("Bloodbath") > 7) || !HasSpell ("Bloodbath")) {
				if (StormBolt ())
					return true;
			}
			//	actions.aoe+=/thunder_clap,cycle_targets=1,if=dot.deep_wounds.remains<3&active_enemies>4
			if (ActiveEnemies (8) > 4) {
				CycleTarget = Enemy.Where (u => Me.Level >= 32 & u.AuraTimeRemaining ("Deep Wounds", true) < 3).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null && ThunderClap ())
					return true;
			}
			//	actions.aoe+=/bladestorm,if=buff.shield_charge.down
			if (!Me.HasAura ("Shield Charge")) {
				if (Bladestorm ())
					return true;
			}
			//	actions.aoe+=/execute,if=buff.sudden_death.react
			if (Me.HasAura ("Sudden Death")) {
				if (Execute ())
					return true;
			}
			//	actions.aoe+=/thunder_clap,if=active_enemies>6
			if (ActiveEnemies (8) > 6) {
				if (ThunderClap ())
					return true;
			}
			//	actions.aoe+=/devastate,cycle_targets=1,if=dot.deep_wounds.remains<5&cooldown.shield_slam.remains>execute_time*0.4
			CycleTarget = Enemy.Where (u => Me.Level >= 32 && u.AuraTimeRemaining ("Deep Wounds", true) < 5 && Cooldown ("Shield Slam") > 1.5 * 0.4).DefaultIfEmpty (null).FirstOrDefault ();
			if (CycleTarget != null) {
				if (Devastate (CycleTarget))
					return true;
			}
			//	actions.aoe+=/devastate,if=cooldown.shield_slam.remains>execute_time*0.4
			if (Cooldown ("Shield Slam") > 1.5 * 0.4) {
				if (Devastate ())
					return true;
			}

			return false;
		}
	}
}
	
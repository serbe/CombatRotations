
using ReBot.API;
using System.Linq;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ReBot
{
	[Rotation ("Warrior Protection IV", "Serb", WoWClass.Warrior, Specialization.WarriorProtection, 5, 25)]

	public class SerbWarriorProtectionIV : SerbWarrior
	{
		[JsonProperty ("Max rage")]
		public int RageMax = 100;
		[JsonProperty ("Select Shout"), JsonConverter (typeof(StringEnumConverter))]
		public WarCry Shout = WarCry.BattleShout;

		public 	SerbWarriorProtectionIV ()
		{
//			GroupBuffs = new[] {
//				"Mark of the Wild"
//			};
			PullSpells = new[] {
				"Charge"
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

			if (Buff (Shout)) {
				return true;
			}

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


			if (Health () < 0.9) {
				if (Heal ())
					return;
			}


			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				BloodFury ();
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				Berserking ();
			if (Me.HasAura ("Bloodbath") || Me.HasAura ("Avatar"))
				ArcaneTorrent ();
			if (!Me.HasAura ("Enrage"))
				BerserkerRage ();

			if (Gcd && HasGlobalCooldown ())
				return;
			

			if (ActiveEnemies (10) > 2) {
				if (AOETarget ()) {
					return;
				}
			}

			if (Usable ("Heroic Throw")) {
				CycleTarget = Enemy.Where (u => Range (30, u, 8) && u.Target != Me && u.InCombat).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {
					if (HeroicThrow (CycleTarget))
						;
				}
			}

			if (SingleTarget ()) {
				return;
			}

			if (HasSpell ("Bloodbath") && ((Cooldown ("Dragon Roar") == 0 && HasSpell ("Dragon Roar")) || (Cooldown ("Storm Bolt") == 0 && HasSpell ("Storm Bolt")) || HasSpell ("Shockwave")))
				Bloodbath ();
			//	actions.prot+=/avatar,if=talent.avatar.enabled&((cooldown.ravager.remains=0&talent.ravager.enabled)|(cooldown.dragon_roar.remains=0&talent.dragon_roar.enabled)|(talent.storm_bolt.enabled&cooldown.storm_bolt.remains=0)|(!(talent.dragon_roar.enabled|talent.ravager.enabled|talent.storm_bolt.enabled)))
			if (HasSpell ("Avatar") && ((Cooldown ("Ravager") == 0 && HasSpell ("Ravager")) || (Cooldown ("Dragon Roar") == 0 && HasSpell ("Dragon Roar")) || (Cooldown ("Storm Bolt") == 0 && HasSpell ("Storm Bolt")) || (!(HasSpell ("Dragon Roar") || HasSpell ("Ravager") || HasSpell ("Storm Bolt")))))
				Avatar ();

			//	actions.prot+=/impending_victory,if=talent.impending_victory.enabled&cooldown.shield_slam.remains<=execute_time
			if (HasSpell ("Impending Victory") && Cooldown ("Shield Slam") <= 1.5) {
				if (ImpendingVictory ()) {
					return;
				}
			}
			//	actions.prot+=/victory_rush,if=!talent.impending_victory.enabled&cooldown.shield_slam.remains<=execute_time
			if (!HasSpell ("Impending Victory") && Cooldown ("Shield Slam") <= 1.5) {
				if (VictoryRush ()) {
					return;
				}
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
//			if (Protection ())
//				return;
//			}

//			if (Me.HasAura ("Battle Stance")) {
//				if (Gladiator ())
//					return;
//			}

		}

		public bool SingleTarget ()
		{
			if (DragonRoar ()) {
				return true;
			}
			if (Me.HasAura ("Sword and Board") || Me.HasAura ("Shield Block")) {
				if (ShieldSlam ())
					return true;
			}
			if (Revenge ()) {
				return true;
			}
			if (ShieldSlam ()) {
				return true;
			}
			if (Me.HasAura ("Sudden Death") || (Rage > RageMax - 10 && Health (Target) < 0.2)) {
				if (Execute ()) {
					return true;
				}
			}
			if (StormBolt ()) {
				return true;
			}
			if (Me.HasAura ("Ultimatum") || (HasSpell ("Unyielding Strikes") && Me.GetAura ("Unyielding Strikes").StackCount >= 6)) {
				if (HeroicStrike ())
					return true;
			}
			if (Devastate ()) {
				return true;
			}
			if (Rage > RageMax - 10) {
				if (HeroicStrike ())
					return true;
			}
			if ((IsElite () || IsPlayer ()) && !Target.HasAura ("Deep Wounds")) {
				if (ThunderClap ())
					return true;
			}

			return false;
		}

		public bool AOETarget ()
		{
			if ((Me.HasAura ("Avatar") || Cooldown ("Avatar") > 10) || !HasSpell ("Avatar")) {
				if (Ravager ())
					return true;
			}
			if (Shockwave ()) {
				return true;
			}
			if ((Me.HasAura ("Bloodbath") || Cooldown ("Bloodbath") > 10) || !HasSpell ("Bloodbath")) {
				if (DragonRoar ())
					return true;
			}
			if (Me.Level >= 32) {
				CycleTarget = Enemy.Where (u => Range (8, u) && !u.HasAura ("Deep Wounds", true)).DefaultIfEmpty (null).FirstOrDefault ();
				if (CycleTarget != null) {	
					if (ThunderClap (CycleTarget)) {
						return true;
					}
				}
			}
			if (Bladestorm ()) {
				return true;
			}
			
			
			return false;
		}

		public bool Heal ()
		{

			if (Health () < 0.9 && Me.Level < 100) {
				if (VictoryRush ())
					return true;
			}
			if (Health () < 0.6 && Me.Level < 100) {
				if (ImpendingVictory ())
					return true;
			}
			if (Health () < 0.6) {
				if (Healthstone ())
					return true;
			}
			if (Health (Me) < 0.9 && !(Target.HasAura ("Demoralizing Shout") || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block"))) {
				if (ShieldBlock ()) {
					;
				}
			}
			if (Health (Me) < 0.9 && !Me.HasAura ("Shield Barrier") && ((!Me.HasAura ("Shield Block") && Frac ("Shield Block") < 0.75) || Rage >= 85)) {
				if (ShieldBarrier ()) {
					; // 5
				}
			}
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion"))) {
				if (DemoralizingShout ()) {
					;
				}
			}
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion"))) {
				if (EnragedRegeneration ()) {
					;
				}
			}
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion"))) {
				if (ShieldWall ()) {
					;
				}
			}
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion"))) {
				if (LastStand ()) {
					;
				}
			}
			if (Health (Me) < 0.9 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion")) || TimeToDie () <= 25) {
				if (DraenicArmor ()) {
					return true;
				}
			}
			if (Health (Me) < 0.4 && DamageTaken (2500) > Me.MaxHealth * 0.1 && !(Target.HasAura ("Demoralizing Shout", true) || Me.HasAura ("Ravager") || Me.HasAura ("Shield Wall") || Me.HasAura ("Last Stand") || Me.HasAura ("Enraged Regeneration") || Me.HasAura ("Shield Block") || Me.HasAura ("Draenic Armor Potion"))) {
				if (Stoneform ()) {
					;
				}
			}

			if (Health (Me) <= 0.35) {
				if (LastStand ()) {
					return true;
				}
			}
			if (Health (Me) <= 0.4) {
				if (ShieldWall ()) {
					return true;
				}
			}
			if (SpellCharges ("Shield Block") == 2 && Rage >= 60 && Health (Me) <= 0.65 && !Me.HasAura ("Shield Block")) {
				if (ShieldBlock ()) {
					return true;
				}
			}
			if (Rage >= 20 && !Me.HasAura ("Shield Barrier") && Health (Me) <= 0.4) {
				if (ShieldBarrier ()) {
					return true;
				}
			}
			if (Health (Me) <= 0.3) {
				if (RallyingCry ()) {
					return true;
				}
			}
			if (Health (Me) <= 0.6) {
				if (DemoralizingShout ()) {
					return true;
				}
			}
			if (Health (Me) <= 0.45) {
				if (EnragedRegeneration ()) {
					return true;
				}
			}

			return true;
		}


		//		public bool Prot_aoe ()
		//		{
		//			//	actions.prot_aoe=bloodbath
		//			Bloodbath ();
		//			//	actions.prot_aoe+=/avatar
		//			Avatar ();
		//			//	actions.prot_aoe+=/thunder_clap,if=!dot.deep_wounds.ticking
		//
		//
		//			//	actions.prot_aoe+=/heroic_leap,if=(raid_event.movement.distance>25&raid_event.movement.in>45)|!raid_event.movement.exists
		//			//
		//			//	actions.prot_aoe+=/shield_slam,if=buff.shield_block.up
		//			if (Me.HasAura ("Shield Block")) {
		//				if (ShieldSlam ())
		//					return true;
		//			}
		//			//	actions.prot_aoe+=/shockwave
		//			//	actions.prot_aoe+=/revenge
		//			if (Revenge ())
		//				return true;
		//			//	actions.prot_aoe+=/thunder_clap
		//			if (ThunderClap ())
		//				return true;
		//			//	actions.prot_aoe+=/bladestorm
		//			//	actions.prot_aoe+=/shield_slam
		//			if (ShieldSlam ())
		//				return true;
		//			//	actions.prot_aoe+=/storm_bolt
		//			if (StormBolt ())
		//				return true;
		//			//	actions.prot_aoe+=/shield_slam
		//			if (ShieldSlam ())
		//				return true;
		//			//	actions.prot_aoe+=/execute,if=buff.sudden_death.react
		//			if (Me.HasAura ("Sudden Death")) {
		//				if (Execute ())
		//					return true;
		//			}
		//			//	actions.prot_aoe+=/devastate
		//			if (Devastate ())
		//				return true;
		//
		//			return false;
		//		}



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
	}
}
	


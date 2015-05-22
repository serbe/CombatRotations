using ReBot.API;
using System.Linq;
using Newtonsoft.Json;
using System;

namespace ReBot
{
	[Rotation ("S Monk Mistweaver", "Serb", WoWClass.Monk, Specialization.MonkMistweaver, 40, 25)]

	public class SerbMonkMistweaver : SerbMonk
	{
		[JsonProperty ("Use Mana Tea with Stacks")]
		public double MTS = 10;
		[JsonProperty ("Use Mana Tea with MP%")]
		public double MTM = 0.5;
		[JsonProperty ("Auto-Revival")]
		public bool Revival;
		[JsonProperty ("Revival")]
		public double RHeal = 0.75;

		public SerbMonkMistweaver ()
		{
		}

		public override bool OutOfCombat ()
		{
			if (Buff (Me))
				return true;

			if (MassDispel ())
				return true;
			
			if (MassResurect ())
				return true;

			if (Me.IsMoving) {
				if (TigersLust ())
					return true;
			}

			if (CrystalOfInsanity ())
				return true;

			if (OraliusWhisperingCrystal ())
				return true;
			
			return false;
		}

		public override void Combat ()
		{
			if (Buff (Me))
				return;

			if (MassDispel ())
				return;

			if (Me.IsMoving) {
				if (!InRun) {
					StartRun = DateTime.Now;
					InRun = true;
					return;
				}
				if (InRun && TimeRun >= TTL) {
					if (TigersLust ())
						return;
				}
			} else {
				InRun = false;
			}

			if (IsInShapeshiftForm ("Stance of the Wise Serpent")) {
				OverrideCombatModus = null;
				OverrideCombatRole = null;
				WiseSerpentCombat ();
			} else {
				OverrideCombatRole = CombatRole.DPS;
				OverrideCombatModus = CombatModus.Fighter;
				OverrideCombatRange (5);
				SpiritedCraneCombat ();
			}
		}

		void SpiritedCraneCombat ()
		{
//			bool inter1 = API.ExecuteLua<bool> ("local _, _, _, _, _, _, _, _,notInterruptible = UnitCastingInfo('target'); return notInterruptible;");
//			bool inter2 = API.ExecuteLua<bool> ("local _, _, _, _, _, _, _, notInterruptible = UnitChannelInfo('target'); return notInterruptible;");
			int rsk = API.ExecuteLua<int> ("return GetSpellCharges(107428)");
			int mnt = API.ExecuteLua<int> ("return GetSpellCharges(115924)");

			if (HealStatue ())
				return;

			if (Me.IsChanneling)
				return;

			if (Interrupt ())
				return;

			if (Mana (Me) < MTM && AuraStackCount ("Mana Tea") >= MTS) {
				if (ManaTea ())
					return;
			}

			// Rotation
			if (TouchofDeath ())
				return;
			if (Mana (Me) >= 0.90 && Chi == 0) {
				if (CracklingJadeLightning ())
					return;
			}
			if (Me.HasAura ("Vital Mists", true, 5)) {
				if (LowHp != null && SurgingMist (LowHp))
					return;
			}

			if (!Me.HasAura ("Tiger Power", true) || Me.AuraTimeRemaining ("Tiger Power", true) <= 4) {
				if (TigerPalm ())
					return;
			}
			if (!Me.HasAura ("Crane's Zeal", true) || Me.AuraTimeRemaining ("Crane's Zeal", true) <= 2) {
				if (BlackoutKick ())
					return;
			}
			if (!Me.HasAura ("Crane's Zeal", true) || Me.AuraTimeRemaining ("Crane's Zeal", true) > 1) {
				if (RisingSunKick ())
					return;
			}
			if (Chi >= 2 && SpellCharges ("Rising Sun Kick") == 0) {
				if (BlackoutKick ())
					return;
			}
			if (!Me.HasAura ("Zen Sphere", true) || Me.AuraTimeRemaining ("Zen Sphere", true) < 2) {
				if (ZenSphere (Me))
					return;
			}
			if (Me.Focus != null) {
				if (Me.Focus.IsFriendly && !Me.Focus.IsDead && Me.Focus.IsPlayer) {
					if (!Me.Focus.HasAura ("Zen Sphere", true) || Me.Focus.AuraTimeRemaining ("Zen Sphere", true) < 2) {
						if (ZenSphere (Me.Focus))
							return;
					}
				}
			}
			if (Mana (Me) <= 0.65 && Chi <= 1 && Me.HasAura ("Power Strikes")) {
				if (Jab ())
					return;
			}
			if (Mana (Me) >= 0.80 && Chi == 0 && SpellCharges ("Rising Sun Kick") >= 2) {
				if (CracklingJadeLightning ())
					return;
			}
			if (Mana (Me) >= 0.75 && Chi == 0 && SpellCharges ("Rising Sun Kick") >= 2 && AuraStackCount ("Mana Tea") >= 10) {
				if (CracklingJadeLightning ())
					return;
			}
			if (Mana (Me) >= 0.70 && Chi == 0 && SpellCharges ("Rising Sun Kick") >= 2 && AuraStackCount ("Mana Tea") >= 15) {
				if (CracklingJadeLightning ())
					return;
			}
			if (Mana (Me) >= 0.65 && Chi == 0 && SpellCharges ("Rising Sun Kick") >= 2 && AuraStackCount ("Mana Tea") >= 20) {
				if (CracklingJadeLightning ())
					return;
			}
			if (Jab ())
				return;

		}

		void WiseSerpentCombat ()
		{
			if (HealStatue ())
				return;
		
			// Setting Tank as Focus
			if (ZenSphereTarget != null) {
				if (ZenSphere (ZenSphereTarget))
					return;
			}
		
			/// Revival with Option for some boss fights
			if (Revival) {
				int revivalLimit = 3;
				if (GrWMe.Length > 5)
					revivalLimit = 5;
				var lowPlayerCount = GrWMe.Count (p => p.HealthFraction <= RHeal);
				if (lowPlayerCount >= revivalLimit) {
					var revCD = SpellCooldown ("Revival");
//					API.Print ("{0} players have low hp: Casting Revival! (Cooldown={1:0.0})", lowPlayerCount, revCD);
					if (revCD < 1) {
						Cast ("Revival");
						return;
					}
				}
			}
		
			if (CastPreventDouble ("Uplift", () => Me.GetPower (WoWPowerType.MonkLightForceChi) >= 2))
				return;
		
			if (Cast ("Mana Tea", () => Me.ManaFraction < MTM && AuraStackCount ("Mana Tea") >= MTS))
				return;
		
//			/// Lets not waste chi brew stacks!
//			if (chiBrewCheck.IsReady && Me.GetPower (WoWPowerType.MonkLightForceChi) <= 3)
//			if (API.ExecuteLua<int> ("return GetSpellCharges(115399)") <= 1)
//				CastPreventDouble ("Chi Brew", null, 1100, "ChiBrew charges are full, chi is <=1!");
//		
//		
//			/// When we're under attack, take less dmg
//			if (API.Units.Any (u => u.Target == Me) && Me.HealthFraction < 0.99)
//				Cast ("Leg Sweep", "One or more units targetting me and less than 66% hp");
//		
//			if (API.Units.Any (u => u.Target == Me) && Me.HealthFraction < 0.99)
//				CastSelf ("Ring of Peace", "One or more units targetting me and less than 66% hp");
//		
//			if (API.Units.Any (u => u.Target == Me) && Me.HealthFraction < 0.66)
//				Cast ("Fortifying Brew", "One or more units targetting me and less than 66% hp");
//		
//			/// Focus Tank
//			if (tank) {
//				if (Me.Focus != null) {
//					if (Cast ("Chi Wave", () => Me.Focus.HealthFraction <= 0.8, Me.Focus))
//						return;
//					if (Cast ("Expel Harm", () => Me.Focus.HealthFraction <= 0.9, Me.Focus))
//						return;
//					if (Cast ("Enveloping Mist", () => Me.Focus.HealthFraction <= EMHeal && Me.Focus.HasAura ("Soothing Mist") && Me.GetPower (WoWPowerType.MonkLightForceChi) >= 3, Me.Focus))
//						return;
//					if (Cast ("Surging Mist", () => Me.Focus.HealthFraction <= SMHeal && Me.Focus.HasAura ("Soothing Mist"), Me.Focus))
//						return;
//					if (Cast ("Soothing Mist", () => Me.Focus.HealthFraction <= 0.995 && !Me.Focus.HasAura ("Soothing Mist"), Me.Focus))
//						return;
//				}
//			}
//		
//			/// Heal Target
//			if (HT) {
//				if (Cast ("Expel Harm", () => Target.HealthFraction <= 0.9))
//					return;
//				if (Cast ("Enveloping Mist", () => Target.HealthFraction <= EMHeal && Target.HasAura ("Soothing Mist") && Me.GetPower (WoWPowerType.MonkLightForceChi) >= 3))
//					return;
//				if (Cast ("Surging Mist", () => Target.HealthFraction <= SMHeal && Target.HasAura ("Soothing Mist")))
//					return;
//				if (Cast ("Soothing Mist", () => Target.HealthFraction <= 0.995 && !Me.Focus.HasAura ("Soothing Mist")))
//					return;
//			}
//		
//			/// Rotation
//			var lowestPlayer = grpAndMe.OrderBy (p => p.HealthFraction).First ();
//		
//		
//			if (lowestPlayer.HealthFraction >= 0.995)
//				return;
//		
//			if (lowestPlayer.HealthFraction > 0.85)
//				LowDamageSituation (grpAndMe);
//			else if (lowestPlayer.HealthFraction > 0.65)
//				MediumDamageSituation (grpAndMe);
//			else
//				CriticalDamageSituation (grpAndMe);
		}

		void DD ()
		{



			if (Chi == ChiMax || !Target.HasAura ("Rising Sun Kick", true) || Target.AuraTimeRemaining ("Rising Sun Kick", true) < 3) {
				if (RisingSunKick ())
					return;
			}


//			// actions.st+=/blackout_kick,if=buff.combo_breaker_bok.react|buff.serenity.up
//			if (Cast ("Blackout Kick", () => Me.HasAura ("Combo Breaker: Blackout Kick") || Me.HasAura ("Serenity")))
//				return;
//			// actions.st+=/tiger_palm,if=buff.combo_breaker_tp.react&buff.combo_breaker_tp.remains<=2
//			if (Cast ("Tiger Palm", () => Me.HasAura ("Combo Breaker: Tiger Palm") && Me.AuraTimeRemaining ("Combo Breaker: Tiger Palm") <= 2))
//				return;
//			// actions.st+=/chi_wave,if=energy.time_to_max>2&buff.serenity.down
//			if (Cast ("Chi Wave", () => EnergyTimeToMax > 2 && !Me.HasAura ("Serenity")))
//				return;
//			// actions.st+=/chi_burst,if=energy.time_to_max>2&buff.serenity.down
//			if (Cast ("Chi Burst", () => HasSpell ("Chi Burst") && EnergyTimeToMax > 2 && !Me.HasAura ("Serenity")))
//				return;
//			// actions.st+=/zen_sphere,cycle_targets=1,if=energy.time_to_max>2&!dot.zen_sphere.ticking&buff.serenity.down
//			if (Cast ("Zen Sphere", () => HasSpell ("Zen Sphere") && EnergyTimeToMax > 2 && !Me.HasAura ("Zen Sphere") && !Me.HasAura ("Serenity")))
//				return;
//			// actions.st+=/chi_torpedo,if=energy.time_to_max>2&buff.serenity.down
//			if (Cast ("Chi Torpedo", () => HasSpell ("Chi Torpedo") && EnergyTimeToMax > 2 && !Me.HasAura ("Serenity")))
//				return;
//			// actions.st+=/blackout_kick,if=chi.max-chi<2
//			if (Cast ("Blackout Kick", () => ChiMax - Chi < 2))
//				return;


			if (ExpelHarm ())
				return;
			if (!Me.HasAura ("Tiger Power") || Me.AuraTimeRemaining ("Tiger Power") <= 4) {
				if (TigerPalm ())
					return;
			}
			if (!Me.HasAura ("Crane's Zeal") || Me.AuraTimeRemaining ("Crane's Zeal") <= 2) {
				if (BlackoutKick ())
					return;
			}
			if (Jab ())
				return;

		}
	}
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Diagnostics;
//using Newtonsoft.Json;
//using Geometry;
//using ReBot.API;
//using Newtonsoft.Json.Converters;
//using System.Reflection;
//using System.ComponentModel;
//
//namespace Jizar
//{
//	[Rotation("Monkealer", "Jizar", "version 6.1.2", WoWClass.Monk, Specialization.MonkMistweaver, 40)]
//	public class Monkealer : CombatRotation
//	{
//		[JsonProperty("Prioritize Tank/Focus")]
//		public bool tank { get; set; }
//
//
//		[JsonProperty("Touch Of Death Use(Mob Above MAX HP)")]
//		public double SETTOD = 5000000;
//
//		[JsonProperty("Use Zen Sphere (Alternate With Focus Target")]
//		public bool ZS = true;
//
//
//		[JsonProperty("Heal Target")]
//		public bool HT { get; set; }
//
//		[JsonProperty("Use Touch of Death Glyphed")]
//		public bool TODG { get; set; }
//
//		[JsonProperty("Dispel out of combat")]
//		public bool Dispelnc { get; set; }
//
//		[JsonProperty("Dispel in combat")]
//		public bool Dispelc { get; set; }
//
//
//		[JsonProperty("Life Cocoon")]
//		public double LCHeal = 0.4;
//
//
//		[JsonProperty("Enveloping Mist")]
//		public double EMHeal = 0.8;
//
//		[JsonProperty("Soothing Mist")]
//		public double SOOMHeal = 0.8;
//
//
//		[JsonProperty("Surging Mist")]
//		public double SMHeal = 0.8;
//
//		[JsonProperty("Uplift Group")]
//		public double ULHeal = 0.85;
//
//		[JsonProperty("Spinning Crane Healing Group")]
//		public double SCHeal = 0.8;
//
//		[JsonProperty("Breath of the Serpent Group")]
//		public double BOSHeal = 0.9;
//
//
//		AutoResetDelay statueCheck = new AutoResetDelay(2000);
//		AutoResetDelay chiBrewCheck = new AutoResetDelay(2000);
//		AutoResetDelay streamChangeDelay = new AutoResetDelay(500);
//		Countdown interruptCD = new Countdown(1.2, true);
//		Random rng = new Random();
//		PlayerObject streamTarget = null;
//
//
//
//		// END ------------------------------------------------------------------------------------------------------------------------------------------------------------------
//
//		void LowDamageSituation(PlayerObject[] group)
//		{
//			DistributeRenewingMists(group);
//			StreamHealing(group);
//		}
//
//		void MediumDamageSituation(PlayerObject[] group)
//		{
//			if (UpliftHealing(group)) return;
//			if (SpinningCraneHealing(group)) return;
//			if (ExpelHarm(group)) return;
//			if (ChiWave(group)) return;
//			if (ChiBurst(group)) return;
//			if (ChiExplosion(group)) return;
//			if (DetonatChi(group)) return;
//			if (BreathoftheSerpent(group)) return;
//			DistributeRenewingMists(group);
//			StreamHealing(group);
//		}
//
//		void CriticalDamageSituation(PlayerObject[] group)
//		{
//			if (Me.GetPower(WoWPowerType.MonkLightForceChi) <= 2)
//				CastPreventDouble("Chi Brew", null, 500);
//
//			Cast("Thunder Focus Tea");
//
//			/// We must pump out our cocoon when there's critical damage
//			var cocoonTargets = group.Where(p => p.HealthFraction <= LCHeal && (p.IsTank || p == Me || p.IsHealer)).ToArray();
//			if (SpellCooldown("Life Cocoon") < 0.2 && Me.ManaFraction >= 0.04 && cocoonTargets.Length > 0)
//			{
//				foreach (var p in cocoonTargets)
//					if (Cast("Life Cocoon", p)) return;
//			}
//
//			/// Heal tanks and Me if under 50%
//			foreach (var u in group.Where(p => (p.IsTank || p == Me) && p.HealthFraction < 0.5).OrderBy(p => p.HealthFraction))
//				if (Me.IsChanneling)
//				if (streamTarget == u)
//				if (Cast("Surging Mist")) return;
//			StreamHealing(group);
//		}
//
//		#endregion
//
//		#region Common Helpers
//
//		bool ChiBurst(PlayerObject[] group)
//		{
//			if (Cast("Chi Burst")) return true;
//			return false;
//		}
//
//		bool DetonatChi(PlayerObject[] group)
//		{
//			foreach (var u in group)
//				if (u.HealthFraction < 0.5);
//			if (Cast("Detonate Chi")) return true;
//			return false;
//		}
//
//		bool ChiWave(PlayerObject[] group)
//		{
//			if (Cast("Chi Wave")) return true;
//			return false;
//		}
//
//		bool ExpelHarm(PlayerObject[] group)
//		{
//			if (Cast("Expel Harm")) return true;
//			return false;
//		}
//
//		bool ChiExplosion(PlayerObject[] group)
//		{
//			if (Me.GetPower(WoWPowerType.MonkLightForceChi) < 3) return false;
//
//			foreach (var u in group)
//				if (u.HealthFraction < 0.9);
//			if (Cast("Chi Explosion")) return true;
//			return false;
//		}
//
//		bool BreathoftheSerpent(PlayerObject[] group)
//		{
//			int nearbyPeople = group.Count(p => p.DistanceSquared < 20 * 20 && p.HealthFraction < BOSHeal && !p.IsMoving);
//
//			if (nearbyPeople >= 3)
//			if (Cast("Breath of the Serpent")) return true;
//			return false;
//		}
//
//		bool DistributeRenewingMists(PlayerObject[] group)
//		{
//			int nearbyPeople = group.Count(p => p.DistanceSquared < 40 * 40);
//
//			foreach (var u in group.OrderBy(u => u.HealthFraction).Where(p => !p.HasAura("Renewing Mist", true)))
//			{
//				if (Cast("Renewing Mist", () => nearbyPeople >= 3 && Me.GetPower(WoWPowerType.MonkLightForceChi) < 5, u)) return true;
//			}
//
//			return false;
//		}
//
//		bool SpinningCraneHealing(PlayerObject[] group)
//		{
//			if (Me.ManaFraction < 0.30) return false;
//
//			int nearbyPeople = group.Count(p => p.DistanceSquared < 8 * 8 && p.HealthFraction < SCHeal && !p.IsMoving);
//
//			if (nearbyPeople >= 3)
//			if (Cast("Spinning Crane Kick")) return true;
//
//			return false;
//		}
//
//		bool UpliftHealing(PlayerObject[] group)
//		{
//			if (Me.GetPower(WoWPowerType.MonkLightForceChi) < 2)
//				return false;
//
//			// Count how many people have mists with enough time left (1.4s)
//			int peopleWithMists = 0;
//			foreach (var u in group)
//				if (u.HealthFraction <= ULHeal)
//				if (u.HasAura("Renewing Mist", true))
//				if (u.AuraTimeRemaining("Renewing Mist") > 1.4f)
//					peopleWithMists++;
//
//			int upliftLimit = 3; // In groups
//			if (group.Length > 5) upliftLimit = 5; // in raids
//
//			if (peopleWithMists >= upliftLimit)
//			if (Cast("Uplift")) return true;
//
//			return false;
//		}
//

//
//
//		bool StreamHealing(PlayerObject[] group)
//		{
//			if (Me.IsMoving)
//				return false;
//
//			bool isChanneling = Me.IsChanneling && streamTarget != null;
//
//			if (isChanneling)
//			if (group.All(p => p.HealthFraction >= 0.9))
//			{
//				Me.StopCasting();
//				return false;
//			}
//
//			/// Debug:
//			streamChangeDelay.MakeReady();
//
//			if (streamChangeDelay.IsReady || !isChanneling || (isChanneling && streamTarget != null && streamTarget.HealthFraction == 1))
//			{
//				bool changeStream = false;
//
//				foreach (var player in group.Where(p => p.HealthFraction <= SOOMHeal).OrderBy(p => p.HealthFraction))
//				{
//					if (isChanneling && streamTarget == player)
//						break;
//
//					if (isChanneling)
//					{
//						var hpDiff = streamTarget.HealthFraction - player.HealthFraction;
//						if ((hpDiff >= 0.10 || player.IsTank))
//						{
//							changeStream = true;
//						}
//					}
//					else
//					{
//						changeStream = true;
//					}
//
//
//					if (changeStream)
//					if (Cast("Soothing Mist", player))
//					{
//						streamTarget = player;
//						Me.SetTarget(player);
//						return true;
//					}
//				}
//			}
//
//			if (isChanneling)
//			{
//				if (Cast("Enveloping Mist", () => streamTarget.HealthFraction <= EMHeal && !streamTarget.HasAura("Enveloping Mist", true))) return true;
//				if (Cast("Surging Mist", () => streamTarget.HealthFraction <= SMHeal)) return true;
//			}
//			return false;
//		}
//

//
//		#endregion
//	}
//}

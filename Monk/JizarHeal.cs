using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Geometry;
using ReBot.API;

namespace Rebot
{
	[Rotation ("Monkealer", "Jizar", "v6.1.2", WoWClass.Monk, Specialization.MonkMistweaver, 40)]
	public class Monkealer : CombatRotation
	{
		[JsonProperty ("Prioritize Tank/Focus")]
		public bool tank { get; set; }

		[JsonProperty ("Auto Spear Hand Interrupt")]
		public bool interrupt = true;

		[JsonProperty ("Touch Of Death Use(Mob Above MAX HP)")]
		public double SETTOD = 5000000;

		[JsonProperty ("Use Zen Sphere (Alternate With Focus Target")]
		public bool ZS = true;

		[JsonProperty ("Auto-Revival")]
		public bool Revival { get; set; }

		[JsonProperty ("Renewing Mis Out of combat")]
		public bool DRM { get; set; }

		[JsonProperty ("Heal Target")]
		public bool HT { get; set; }

		[JsonProperty ("Use Touch of Death Glyphed")]
		public bool TODG { get; set; }

		[JsonProperty ("Dispel out of combat")]
		public bool Dispelnc { get; set; }

		[JsonProperty ("Dispel in combat")]
		public bool Dispelc { get; set; }


		[JsonProperty ("Life Cocoon")]
		public double LCHeal = 0.4;

		[JsonProperty ("Revival")]
		public double RHeal = 0.75;

		[JsonProperty ("Enveloping Mist")]
		public double EMHeal = 0.8;

		[JsonProperty ("Soothing Mist")]
		public double SOOMHeal = 0.8;

		[JsonProperty ("Use Mana Tea with Stacks")]
		public double MTS = 10;

		[JsonProperty ("Use Mana Tea with MP%")]
		public double MTM = 0.5;

		[JsonProperty ("Surging Mist")]
		public double SMHeal = 0.8;

		[JsonProperty ("Uplift Group")]
		public double ULHeal = 0.85;

		[JsonProperty ("Spinning Crane Healing Group")]
		public double SCHeal = 0.8;

		[JsonProperty ("Breath of the Serpent Group")]
		public double BOSHeal = 0.9;



		public bool Debug = true;

		void DebugWrite (string text)
		{
			if (Debug)
				API.Print (text);
		}

		AutoResetDelay statueCheck = new AutoResetDelay (2000);
		AutoResetDelay chiBrewCheck = new AutoResetDelay (2000);
		AutoResetDelay streamChangeDelay = new AutoResetDelay (500);
		Countdown interruptCD = new Countdown (1.2, true);
		Random rng = new Random ();
		PlayerObject streamTarget;

		public override bool OutOfCombat ()
		{
			// Raid Buff
			if (CastSelf ("Legacy of the Emperor", () => !HasAura ("Legacy of the Emperor") && !HasAura ("Blessing of Kings") && !HasAura ("Mark of the Wild") && !HasAura ("Legacy of the White Tiger")))
				return true;

			// Dispel Group
			if (Dispelnc) {
				List<PlayerObject> members = Group.GetGroupMemberObjects ();
				foreach (PlayerObject player in members) {
					if (CastPreventDouble ("Detox", () => player.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType)))) {
						DebugWrite ("Dispeling on " + player.Name);
						return true;
					}
					// Removing Curses	
					if (CastSelfPreventDouble ("Detox", () => Me.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType))))
						return true;
				}
			}


			// Get them Up - Ressurection
			if (CurrentBotName == "Combat") {
				List<PlayerObject> members = Group.GetGroupMemberObjects ();
				if (members.Count > 0) {
					PlayerObject deadPlayer = members.FirstOrDefault (x => x.IsDead);
					if (CastPreventDouble ("Resuscitate", () => deadPlayer != null, deadPlayer)) {
						DebugWrite ("Resuscitate: " + deadPlayer.Name);
						return true;
					}
				}
			}

			if (DRM) {
				var group = Group.GetGroupMemberObjects ().Concat (new[] { Me }).ToArray ();
				DistributeRenewingMists (group);
			}

			if (Me.IsMoving) {
				if (SpellCooldown ("Tiger's Lust") <= 0) {
					if (CastSelf ("Tiger's Lust"))
						return true;
				}
			}

			return false;
		}

		public override void Combat ()
		{
			// Combat Buff
			if (CastSelf ("Legacy of the Emperor", () => !HasAura ("Legacy of the Emperor") && !HasAura ("Blessing of Kings") && !HasAura ("Mark of the Wild") && !HasAura ("Legacy of the White Tiger")))
				return;

			// Dispel Group in Combat
			if (Dispelc) {
				List<PlayerObject> members = Group.GetGroupMemberObjects ();
				foreach (PlayerObject player in members) {
					if (CastPreventDouble ("Detox", () => player.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType)))) {
						DebugWrite ("Dispeling on " + player.Name);
						return;
					}
					// Dispel Self in Combat
					if (CastSelfPreventDouble ("Detox", () => Me.Auras.Any (x => x.IsDebuff && "Magic,Poison,Disease".Contains (x.DebuffType))))
						return;
				}
			}

			if (CastSelfPreventDouble ("Dampen Harm", () => Me.HealthFraction <= 0.85))
				return;

			if (Me.IsMoving) {
				if (SpellCooldown ("Tiger's Lust") <= 0) {
					if (CastSelf ("Tiger's Lust"))
						return;
				}				
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

		#region Stance Specialization

		// --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------
		void SpiritedCraneCombat ()
		{
			bool inter1 = API.ExecuteLua<bool> ("local _, _, _, _, _, _, _, _,notInterruptible = UnitCastingInfo('target'); return notInterruptible;");
			bool inter2 = API.ExecuteLua<bool> ("local _, _, _, _, _, _, _, notInterruptible = UnitChannelInfo('target'); return notInterruptible;");
			int rsk = API.ExecuteLua<int> ("return GetSpellCharges(107428)");
//			int mnt = API.ExecuteLua<int>("return GetSpellCharges(115924)");
			int Chi = Me.GetPower (WoWPowerType.MonkLightForceChi);		
			var group = Group.GetGroupMemberObjects ().Concat (new[] { Me }).ToArray ();
			if (EnsureStatue (group))
				return;

			if (Me.IsChanneling)
				return;

			// interruptions
			if (Cast ("Spear Hand Strike", () => Target.IsInCombatRange && Target.IsCastingAndInterruptible () && interrupt && !inter1 && !inter2))
				return;
			if (Cast ("Leg Sweep", () => Target.IsInCombatRange && Target.IsCastingAndInterruptible () && interrupt && !inter1 && !inter2))
				return;
			if (CastSelf ("Ring of Peace", () => HasSpell ("Ring of Peace") && Target.IsInCombatRange && Target.IsCastingAndInterruptible () && interrupt && !inter1 && !inter2))
				return;

			if (Cast ("Mana Tea", () => Me.ManaFraction < MTM && AuraStackCount ("Mana Tea") >= MTS))
				return;

			// Rotation
			if (TODG) {
				if (Cast ("Touch of Death", () => !Target.IsPlayer && HasAura ("Death Note") && Target.MaxHealth > SETTOD))
					return;
			} else {
				if (Cast ("Touch of Death", () => !Target.IsPlayer && Chi >= 3 && HasAura ("Death Note") && Target.MaxHealth > SETTOD))
					return;
			}		
			if (Cast ("Crackling Jade Lightning", () => Me.ManaFraction >= 0.90 && Chi == 0))
				return;			
			if (HasAura ("Vital Mists", true, 5)) {
				var lowestPlayer = group.OrderBy (p => p.HealthFraction).FirstOrDefault (p => p.Health > 0);
				if (lowestPlayer != null)
				if (Cast ("Surging Mist", lowestPlayer))
					return;
			}

			if (Cast ("Tiger Palm", () => Chi >= 1 && (!Me.HasAura ("Tiger Power") || AuraTimeRemaining ("Tiger Power") <= 4)))
				return;
			if (Cast ("Blackout Kick", () => Chi >= 2 && (!HasAura ("Crane's Zeal") || AuraTimeRemaining ("Crane's Zeal") <= 2)))
				return;		
			if (Cast ("Rising Sun Kick", () => Chi >= 2 && (!HasAura ("Crane's Zeal") || AuraTimeRemaining ("Crane's Zeal") > 1)))
				return;
			if (Cast ("Blackout Kick", () => Chi >= 2 && rsk == 0))
				return;
			if (CastSelf ("Zen Sphere", () => ZS && HasSpell ("Zen Sphere") && !Me.HasAura ("Zen Sphere", true) || AuraTimeRemaining ("Zen Sphere") < 2))
				return;

			if (Me.Focus != null) {
				if (Me.Focus.IsFriendly && Me.Focus.IsInLoS && !Me.Focus.IsDead) {
					if (Cast ("Zen Sphere", () => ZS && Me.Focus.IsPlayer && Me.Focus.IsFriendly && Me.Focus.IsInLoS && !Me.Focus.IsDead && !Me.Focus.HasAura ("Zen Sphere", true) || AuraTimeRemaining ("Zen Sphere") < 2, Me.Focus))
						return;
				}
			}

			if (Cast ("Jab", () => Me.ManaFraction <= 0.65 && Chi <= 1 && HasAura ("Power Strikes")))
				return;				
			if (Cast ("Crackling Jade Lightning", () => Me.ManaFraction >= 0.80 && Chi == 0 && rsk >= 2))
				return;
			if (Cast ("Crackling Jade Lightning", () => Me.ManaFraction >= 0.75 && Chi == 0 && rsk >= 2 && AuraStackCount ("Mana Tea") >= 10))
				return;	
			if (Cast ("Crackling Jade Lightning", () => Me.ManaFraction >= 0.70 && Chi == 0 && rsk >= 2 && AuraStackCount ("Mana Tea") >= 15))
				return;
			if (Cast ("Crackling Jade Lightning", () => Me.ManaFraction >= 0.65 && Chi == 0 && rsk >= 2 && AuraStackCount ("Mana Tea") >= 20))
				return;			
			if (Cast ("Jab"))
				return;			

		}

		// --------------------------------------------------------------------------------------------------------------------------------------------------------------
		void WiseSerpentCombat ()
		{


			var grpAndMe = Group.GetGroupMemberObjects ().Where (p => p.Health > 0).Concat (new[] { Me }).ToArray ();
			if (EnsureStatue (grpAndMe))
				return;

			//Setting Tank as Focus
			if (Me.Focus != null) {
				ZenSphere (Me.Focus);
			} else {
				ZenSphere (grpAndMe.FirstOrDefault (x => x.IsTank));
			}

			// Revival with Option for some boss fights
			if (Revival) {
				int revivalLimit = 3;
				if (grpAndMe.Length > 5)
					revivalLimit = 5;
				var lowPlayerCount = grpAndMe.Count (p => p.HealthFraction <= RHeal);
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

			// Lets not waste chi brew stacks!
			if (chiBrewCheck.IsReady && Me.GetPower (WoWPowerType.MonkLightForceChi) <= 3)
			if (API.ExecuteLua<int> ("return GetSpellCharges(115399)") <= 1)
				CastPreventDouble ("Chi Brew", null, 1100, "ChiBrew charges are full, chi is <=1!");


			// When we're under attack, take less dmg
			if (API.Units.Any (u => u.Target == Me) && Me.HealthFraction < 0.99)
				Cast ("Leg Sweep", "One or more units targetting me and less than 66% hp");

			if (API.Units.Any (u => u.Target == Me) && Me.HealthFraction < 0.99)
				CastSelf ("Ring of Peace", "One or more units targetting me and less than 66% hp");			

			if (API.Units.Any (u => u.Target == Me) && Me.HealthFraction < 0.66)
				Cast ("Fortifying Brew", "One or more units targetting me and less than 66% hp");

			// Focus Tank
			if (tank) {
				if (Me.Focus != null) {
					if (Cast ("Chi Wave", () => Me.Focus.HealthFraction <= 0.8, Me.Focus))
						return;
					if (Cast ("Expel Harm", () => Me.Focus.HealthFraction <= 0.9, Me.Focus))
						return;
					if (Cast ("Enveloping Mist", () => Me.Focus.HealthFraction <= EMHeal && Me.Focus.HasAura ("Soothing Mist") && Me.GetPower (WoWPowerType.MonkLightForceChi) >= 3, Me.Focus))
						return;
					if (Cast ("Surging Mist", () => Me.Focus.HealthFraction <= SMHeal && Me.Focus.HasAura ("Soothing Mist"), Me.Focus))
						return;
					if (Cast ("Soothing Mist", () => Me.Focus.HealthFraction <= 0.995 && !Me.Focus.HasAura ("Soothing Mist"), Me.Focus))
						return;
				}
			}	

			// Heal Target
			if (HT) {
				if (Cast ("Expel Harm", () => Target.HealthFraction <= 0.9))
					return;
				if (Cast ("Enveloping Mist", () => Target.HealthFraction <= EMHeal && Target.HasAura ("Soothing Mist") && Me.GetPower (WoWPowerType.MonkLightForceChi) >= 3))
					return;
				if (Cast ("Surging Mist", () => Target.HealthFraction <= SMHeal && Target.HasAura ("Soothing Mist")))
					return;
				if (Cast ("Soothing Mist", () => Target.HealthFraction <= 0.995 && !Me.Focus.HasAura ("Soothing Mist")))
					return;
			}

			// Rotation
			var lowestPlayer = grpAndMe.OrderBy (p => p.HealthFraction).First ();


			if (lowestPlayer.HealthFraction >= 0.995)
				return;

			if (lowestPlayer.HealthFraction > 0.85)
				LowDamageSituation (grpAndMe);
			else if (lowestPlayer.HealthFraction > 0.65)
				MediumDamageSituation (grpAndMe);
			else
				CriticalDamageSituation (grpAndMe);
		}

		// END ------------------------------------------------------------------------------------------------------------------------------------------------------------------

		void LowDamageSituation (PlayerObject[] group)
		{
			DistributeRenewingMists (group);
			StreamHealing (group);
		}

		void MediumDamageSituation (PlayerObject[] group)
		{
			if (UpliftHealing (group))
				return;
			if (SpinningCraneHealing (group))
				return;
			if (ExpelHarm (group))
				return;
			if (ChiWave (group))
				return; 
			if (ChiBurst (group))
				return;
			if (ChiExplosion (group))
				return; 
			if (DetonatChi (group))
				return;
			if (BreathoftheSerpent (group))
				return;
			DistributeRenewingMists (group);
			StreamHealing (group);
		}

		void CriticalDamageSituation (PlayerObject[] group)
		{
			if (Me.GetPower (WoWPowerType.MonkLightForceChi) <= 2)
				CastPreventDouble ("Chi Brew", null, 500);

			Cast ("Thunder Focus Tea");

			// We must pump out our cocoon when there's critical damage
			var cocoonTargets = group.Where (p => p.HealthFraction <= LCHeal && (p.IsTank || p == Me || p.IsHealer)).ToArray ();
			if (SpellCooldown ("Life Cocoon") < 0.2 && Me.ManaFraction >= 0.04 && cocoonTargets.Length > 0) {
				foreach (var p in cocoonTargets)
					if (Cast ("Life Cocoon", p))
						return;
			}

			// Heal tanks and Me if under 50%
			foreach (var u in group.Where(p => (p.IsTank || p == Me) && p.HealthFraction < 0.5).OrderBy(p => p.HealthFraction))
				if (Me.IsChanneling)
				if (streamTarget == u)
				if (Cast ("Surging Mist"))
					return;
			StreamHealing (group);
		}

		#endregion

		#region Common Helpers

		bool ChiBurst (PlayerObject[] group)
		{
			if (Cast ("Chi Burst"))
				return true;
			return false;
		}

		bool DetonatChi (PlayerObject[] group)
		{
			foreach (var u in group)
				if (u.HealthFraction < 0.5)
				if (Cast ("Detonate Chi"))
					return true;
			return false;
		}

		bool ChiWave (PlayerObject[] group)
		{
			if (Cast ("Chi Wave"))
				return true;
			return false;
		}

		bool ExpelHarm (PlayerObject[] group)
		{
			if (Cast ("Expel Harm"))
				return true;
			return false;
		}

		bool ChiExplosion (PlayerObject[] group)
		{
			if (Me.GetPower (WoWPowerType.MonkLightForceChi) < 3)
				return false;

			foreach (var u in group)
				if (u.HealthFraction < 0.9)
				if (Cast ("Chi Explosion"))
					return true;
			return false;
		}

		bool BreathoftheSerpent (PlayerObject[] group)
		{
			int nearbyPeople = group.Count (p => p.DistanceSquared < 20 * 20 && p.HealthFraction < BOSHeal && !p.IsMoving);

			if (nearbyPeople >= 3)
			if (Cast ("Breath of the Serpent"))
				return true;
			return false;		
		}

		bool DistributeRenewingMists (PlayerObject[] group)
		{
			int nearbyPeople = group.Count (p => p.DistanceSquared < 40 * 40);

			foreach (var u in group.OrderBy(u => u.HealthFraction).Where(p => !p.HasAura("Renewing Mist", true))) {
				if (Cast ("Renewing Mist", () => nearbyPeople >= 3 && Me.GetPower (WoWPowerType.MonkLightForceChi) < 5, u))
					return true;
			}

			return false;
		}

		bool SpinningCraneHealing (PlayerObject[] group)
		{
			if (Me.ManaFraction < 0.30)
				return false;

			int nearbyPeople = group.Count (p => p.DistanceSquared < 8 * 8 && p.HealthFraction < SCHeal && !p.IsMoving);

			if (nearbyPeople >= 3)
			if (Cast ("Spinning Crane Kick"))
				return true;

			return false;
		}

		bool UpliftHealing (PlayerObject[] group)
		{
			if (Me.GetPower (WoWPowerType.MonkLightForceChi) < 2)
				return false;

			// Count how many people have mists with enough time left (1.4s)
			int peopleWithMists = 0;
			foreach (var u in group)
				if (u.HealthFraction <= ULHeal)
				if (u.HasAura ("Renewing Mist", true))
				if (u.AuraTimeRemaining ("Renewing Mist") > 1.4f)
					peopleWithMists++;

			int upliftLimit = 3; // In groups
			if (group.Length > 5)
				upliftLimit = 5; // in raids

			if (peopleWithMists >= upliftLimit)
			if (Cast ("Uplift"))
				return true;

			return false;
		}

		bool EnsureStatue (PlayerObject[] group)
		{
			if (!statueCheck.IsReady)
				return false;

			if (!group.Any (p => p.InCombat))
				return false;

			const int StatueEntryID = 60849;

			var statue = API.Units.FirstOrDefault (u => u.EntryID == StatueEntryID && u.CreatedByMe);
			if (statue == null || statue.Distance > 35) {
				foreach (var u in group.Where(p => p.IsTank || p == Me))
					if (u != null && u.Distance < 20) {
						var pos = u.Position;
						for (int i = 0; i < 8; i++) {
							var target = pos;
							target.X += (float)rng.NextDouble () * 10 - 5;
							target.Y += (float)rng.NextDouble () * 10 - 5;

							if (CastOnTerrain ("Summon Jade Serpent Statue", target))
								return true;
						}

					}
			}
			return false;
		}


		bool StreamHealing (PlayerObject[] group)
		{
			if (Me.IsMoving)
				return false;

			bool isChanneling = Me.IsChanneling && streamTarget != null;

			if (isChanneling)
			if (group.All (p => p.HealthFraction >= 0.9)) {
				Me.StopCasting ();
				return false;
			}

			// Debug:
			streamChangeDelay.MakeReady ();

			if (streamChangeDelay.IsReady || !isChanneling || (isChanneling && streamTarget != null && streamTarget.HealthFraction == 1)) {
				bool changeStream = false;

				foreach (var player in group.Where(p => p.HealthFraction <= SOOMHeal).OrderBy(p => p.HealthFraction)) {
					if (isChanneling && streamTarget == player)
						break;

					if (isChanneling) {
						var hpDiff = streamTarget.HealthFraction - player.HealthFraction;
						if ((hpDiff >= 0.10 || player.IsTank)) {
							changeStream = true;
						}
					} else {
						changeStream = true;
					}


					if (changeStream)
					if (Cast ("Soothing Mist", player)) {
						streamTarget = player;
						Me.SetTarget (player);
						return true;
					}
				}
			}

			if (isChanneling) {
				if (Cast ("Enveloping Mist", () => streamTarget.HealthFraction <= EMHeal && !streamTarget.HasAura ("Enveloping Mist", true)))
					return true;
				if (Cast ("Surging Mist", () => streamTarget.HealthFraction <= SMHeal))
					return true;
			}
			return false;
		}

		void ZenSphere (UnitObject player)
		{
			try {
				if (Me.Focus.IsPlayer) {
					if (Me.Focus.IsFriendly && Me.Focus.IsInLoS) {
						if (Cast ("Zen Sphere", () => Me.Focus.IsInLoS && !Me.Focus.IsDead && !Me.Focus.HasAura ("Zen Sphere", true), Me.Focus)) {
							DebugWrite ("Focus  Zen Sphere on " + Me.Focus.Name);
							return;
						}
					}
				}

			} catch (NullReferenceException e) {
				try {
					UnitObject t = player.Target;
					UnitObject tt = t.Target;

					if (e == null)
						DebugWrite ("null");
					if (tt.IsPlayer && tt.IsFriendly) {
						if (Cast ("Zen Sphere", () => tt.IsInLoS && !tt.IsDead && !tt.HasAura ("Zen Sphere", true), tt)) {
							DebugWrite ("TankTank Zen Sphere on " + tt.Name);
							return;
						}
					}
				} catch (NullReferenceException e2) {
					if (e2 == null)
						DebugWrite ("null");
					if (Cast ("Zen Sphere", () => player.IsInLoS && !player.IsDead && !player.HasAura ("Zen Sphere", true), player)) {
						DebugWrite ("T1  Zen Sphere on " + player.Name);

						return;
					}
				}
			}
		}

		#endregion
	}
}

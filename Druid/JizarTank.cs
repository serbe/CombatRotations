using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Newtonsoft.Json;
using Geometry;
using ReBot.API;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Jizar
{
	[Rotation ("DruTank", "Jizar", "v 1.0.1", WoWClass.Druid, Specialization.DruidGuardian, 40)]
	public class DruTank : CombatRotation
	{

		[JsonProperty ("Group 'Healing Touch'")]
		public double GroupHT = 0.75;

		[JsonProperty ("Rebirth Healer/Focus if dead")]
		public bool Brez { get; set; }

		private bool debug = true;
		private bool pulling = true;
		private UnitObject lastTarget = null;
		private Timer targettingTimer = null;
		public bool threatManagement = true;
		public double targetingSystemInterval = 2;

		// Debug
		public bool Debug = true;

		public DruTank ()
		{
			GroupBuffs = new string[] {
				"Mark of the Wild"
			};
			PullSpells = new string[] {
				"Faerie Fire"			
			};
		}

		private void DebugWrite (string text)
		{
			if (Debug)
				API.Print (text);
		}

		public override bool OutOfCombat ()
		{

			if (threatManagement && (targettingTimer == null || targettingTimer.Interval != (targetingSystemInterval * 1000))) {
				targettingTimer = new Timer (targetingSystemInterval * 1000);
				targettingTimer.Elapsed += new ElapsedEventHandler (TargetManagement);
			}
			// Buffs and Heals out of combat.		
			if (CastSelf ("Mark of the Wild", () => !HasAura ("Mark of the Wild") && !HasAura ("Blessing of Kings")))
				return true;
			if (CastSelf ("Rejuvenation", () => Me.HealthFraction <= 0.75 && !HasAura ("Rejuvenation")))
				return true;
			if (CastSelfPreventDouble ("Healing Touch", () => Me.HealthFraction <= 0.5))
				return true;
			if (CastSelf ("Remove Corruption", () => Me.Auras.Any (x => x.IsDebuff && "Curse,Poison".Contains (x.DebuffType))))
				return true;
			if (CastSelf ("Travel Form", () => Me.IsSwimming && !HasAura ("Travel Form")))
				return true;
			return false;

			// Get them Up - Ressurection
			if (CurrentBotName == "Combat") {
				List<PlayerObject> members = Group.GetGroupMemberObjects ();
				if (members.Count > 0) {
					PlayerObject deadPlayer = members.FirstOrDefault (x => x.IsDead);
					if (CastPreventDouble ("Revive", () => deadPlayer != null))
						;
					{
						DebugWrite ("Reviving: " + deadPlayer.Name);
						return true;
					}
				}
			}
		}

		public override void Combat ()
		{

			List<PlayerObject> members = Group.GetGroupMemberObjects ();
			members = members.FindAll (x => x.IsInCombatRangeAndLoS && !x.IsDead).ToList ();
			members = members.OrderBy (p => p.IsHealer).ThenBy (p => p.HealthFraction).ToList ();

			if (CastSelf ("Bear Form", () => !HasAura ("Bear Form") && !Me.HasAura ("Flight Form")))
				;

			if (HasSpell ("Bear Form") && HasAura ("Bear Form")) {


				// Instant Heal me if Talent "Dream of Cenarius" is Selected.
				if (HasSpell ("Dream of Cenarius")) {
					if (CastSelf ("Healing Touch", () => HasAura ("Dream of Cenarius") && Me.HealthFraction < 0.85))
						;
				}

				// Self Healing.
				if (CastSelf ("Frenzied Regeneration", () => Me.GetPower (WoWPowerType.Rage) > 60 && Me.HealthFraction < 0.40))
					return;	

				// Defense Spells	
				if (Cast ("Savage Defense", () => Me.GetPower (WoWPowerType.Rage) > 60 && !HasAura ("Savage Defense") && Me.HealthFraction < 0.45))
					return;

				if (Cast ("Survival Instincts", () => !HasAura ("Survival Instincts") && Me.HealthFraction < 0.25))
					return;

				if (CastSelf ("Barkskin", () => Me.HealthFraction < 0.65))
					return;

				if (Cast ("Cenarion Ward", () => !HasAura ("Cenarion Ward") && Me.HealthFraction < 0.7))
					return;




				// Talents Lv100	
				if (HasSpell ("Bristling Fur")) {
					if (CastSelf ("Bristling Fur", () => Me.HealthFraction < 0.30))
						return;
				}

				if (HasSpell ("Pulverize")) {
					if (Cast ("Pulverize", () => Target.HasAura ("Lacerate", true, 3)))
						return;
				}


				// Interruptions
				if (Cast ("Skull Bash", () => Target.IsCastingAndInterruptible ()))
					return;

				// Interruptions with Talent or Glyph.
				if (Cast ("Incapacitating Roar", () => Target.IsCastingAndInterruptible ()))
					return; // 30 sec Cooldown

				if (Cast ("Mighty Bash", () => Target.IsCastingAndInterruptible ()))
					return;

				if (Cast ("Typhoon", () => Target.IsCastingAndInterruptible ()))
					return; // 30 sec Cooldown

				if (Cast ("Faerie Fire", () => Target.IsCastingAndInterruptible ()))
					return;

				// Setting Healer as Focus for Rebirth
				if (Brez) {
					if (Me.Focus != null) {
						Rebirth (Me.Focus);
					} else {
						Rebirth (members.FirstOrDefault (x => x.IsTank && x.IsDead));
					}
				}

				// AOE
				if (Adds.Count (x => x.DistanceSquared <= 10 * 10) >= 5) {
					if (Cast ("Thrash", () => HasAura ("Bear Form")))
						return;
				}

				if (Adds.Count (x => x.DistanceSquared <= 10 * 10) >= 3) {
					if (Cast ("Mangle", () => HasAura ("Bear Form") && HasAura ("Berserk")))
						;
					if (Cast ("Maul", () => HasAura ("Bear Form") && Me.GetPower (WoWPowerType.Rage) > 60 && HasAura ("Tooth and Claw") && Me.HealthFraction > 0.70))
						;
					if (Cast ("Maul", () => HasAura ("Bear Form") && Me.GetPower (WoWPowerType.Rage) > 90 && Me.HealthFraction > 0.70))
						;
					if (Cast ("Thrash", () => HasAura ("Bear Form")))
						return;
				}

				// Single Rotation						
				if (Cast ("Mangle", () => HasAura ("Bear Form") && HasAura ("Berserk")))
					;
				if (Cast ("Mangle", () => HasAura ("Bear Form")))
					;
				if (Cast ("Thrash", () => HasAura ("Bear Form") && !Target.HasAura ("Thrash", true)))
					;
				if (Cast ("Maul", () => HasAura ("Bear Form") && Me.GetPower (WoWPowerType.Rage) > 60 && HasAura ("Tooth and Claw") && Me.HealthFraction > 0.70))
					;
				if (Cast ("Maul", () => HasAura ("Bear Form") && Me.GetPower (WoWPowerType.Rage) > 90 && Me.HealthFraction > 0.70))
					;
				if (Cast ("Lacerate", () => HasAura ("Bear Form")))
					return;				
			}					
		}


		// Rebirth on Healer/Focus.
		void Rebirth (UnitObject player)
		{
			try {
				if (Me.Focus.IsPlayer) {
					if (Me.Focus.IsFriendly && Me.Focus.IsInLoS) {
						if (Cast ("Rebirth", () => Me.Focus.IsInLoS && Me.Focus.IsDead == true, Me.Focus)) {
							DebugWrite ("Focus  Rebirth on " + Me.Focus.Name);
							return;
						}
					}
				}
			} catch (NullReferenceException e) {
				try {
					UnitObject t = player.Target;
					UnitObject tt = t.Target;

					if (tt.IsPlayer && tt.IsFriendly) {
						if (Cast ("Rebirth", () => tt.IsInLoS && tt.IsDead == true, tt)) {
							DebugWrite ("Healer Rebirth on " + tt.Name);
							return;
						}
					}
				} catch (NullReferenceException e2) {
					if (Cast ("Rebirth", () => player.IsInLoS && player.IsDead == true, player)) {
						DebugWrite ("T1  Rebirth on " + player.Name);
						return;
					}
				}
			}
		}

		private List<UnitObject> GetTargetsPrioritized ()
		{
			List<Tuple<UnitObject, double>> targetsStatus = new List<Tuple<UnitObject, double>> ();

			foreach (var target in Adds) {
				List<string> threadDetail = API.ExecuteLua (string.Format ("return UnitDetailedThreatSituation('{0}','{1}')", Me.GUID, target.GUID));

				var threat = threadDetail.Count > 0 ? double.Parse (threadDetail [4]) : 0;

				targetsStatus.Add (new Tuple<UnitObject, double> (target, threat));
			}

			targetsStatus = targetsStatus.OrderBy (tuple => tuple.Item2).ToList ();

			var targets = new List<UnitObject> ();

			foreach (var targetStatus in targetsStatus) {
				targets.Add (targetStatus.Item1);
			}

			return targets;
		}

		private void TargetManagement (object source, ElapsedEventArgs e)
		{
			if (threatManagement) {
				var newTarget = GetTargetsPrioritized ().DefaultIfEmpty (null).FirstOrDefault ();

				if (newTarget != null) {
					API.SetFacing (newTarget);
					Me.SetTarget (newTarget);
				}
			}
		}

		public override bool AfterCombat ()
		{
			pulling = true;
			lastTarget = null;
			targettingTimer.Stop ();
			return false;
		}
	}
}

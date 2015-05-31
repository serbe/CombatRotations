using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Geometry;
using ReBot.API;


namespace ReBot
{
	[Rotation ("Pahealer", "Jizar", "v 1.0", WoWClass.Paladin, Specialization.PaladinHoly, 40)]
	public class Pahealer : CombatRotation
	{

		[JsonProperty ("Auto-Avenging Wrath")]
		public bool AW { get; set; }

		[JsonProperty ("Auto-Divine Shield on you")]
		public bool DS { get; set; }

		[JsonProperty ("Heal Target")]
		public bool target { get; set; }

		[JsonProperty ("Auto-Hand of Sacrifice Focus")]
		public bool HOS { get; set; }

		[JsonProperty ("Hand of Sacrifice Tank")]
		public double HoS = 0.5;

		[JsonProperty ("Auto-Hand of Protection")]
		public bool HOP { get; set; }

		[JsonProperty ("Hand of protection (0 = Off)")]
		public double HoP = 0.25;

		[JsonProperty ("Auto-Lay of Hands Focus")]
		public bool LOH { get; set; }

		[JsonProperty ("Holy Light Heal (0 = Off)")]
		public double HLHeal = 0.85;
		[JsonProperty ("Flash of Light Heal (0 = Off)")]
		public double FLHeal = 0.55;
		[JsonProperty ("Eternal Flame Heal (0 = Off)")]
		public double WoGHeal = 0.95;
		[JsonProperty ("Holy Shock Heal (0 = Off)")]
		public double HSHeal = 0.99;
		[JsonProperty ("Holy Prism Heal (0 = Off)")]
		public double HPHeal = 0.98;
		[JsonProperty ("Holy Radiance Heal (0 = Off)")]
		public double HRHeal = 0.70;





		string mySealSpell;
		string myHealTalent1;
		string myHealTalent2;
		AutoResetDelay FlameChangeDelay = new AutoResetDelay (2000);

		//		private int _HP;

		public int HP { get { return Me.GetPower (WoWPowerType.PaladinHolyPower); } }

		public bool Debug = true;

		private void DebugWrite (string text)
		{
			if (Debug)
				API.Print (text);
		}

		public Pahealer ()
		{
			GroupBuffs = new[] { "Blessing of Kings" };

			if (HasSpell ("Seal of Insight"))
				mySealSpell = "Seal of Insight";
			else
				mySealSpell = "Seal of Command";

			if (HasSpell ("Eternal Flame"))
				myHealTalent1 = "Eternal Flame";
			else
				myHealTalent1 = "Word of Glory";

			if (HasSpell ("Sacred Shield"))
				myHealTalent1 = "Sacred Shield";
			else
				myHealTalent1 = "Word of Glory";

			if (HasSpell ("Execution Sentence"))
				myHealTalent2 = "Execution Sentence";
			if (HasSpell ("Holy Prism"))
				myHealTalent2 = "Holy Prism";	
			if (HasSpell ("Light's Hammer"))
				myHealTalent2 = "Light's Hammer";


		}


		public override bool OutOfCombat ()
		{
			//GLOBAL CD CHECK
			if (HasGlobalCooldown ())
				return false;	

			if (CastSelf ("Seal of Insight", () => !HasAura ("Seal of Insight")))
				return true;


			/// Combat Buff
			if (CastSelf ("Blessing of Kings", () => !HasAura ("Blessing of Kings") && !HasAura ("Legacy of the Emperor") && !HasAura ("Mark of the Wild") && !HasAura ("Legacy of the White Tiger")))
				return true;
			if (CastSelf ("Blessing of Might", () => !HasAura ("Blessing of Might")))
				return true;

			if (CastSelf ("Cleanse", () => Me.Auras.Any (x => x.IsDebuff && "Disease,Poison".Contains (x.DebuffType))))
				return true;

			List<PlayerObject> group = Group.GetGroupMemberObjects ();
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			var lowPlayerCount = group.Where (p => p.HealthFraction > 0 && !p.IsDead).Count (p => p.HealthFraction < 0.5);
			return false;
		}

		/*bool Temp(PlayerObject[] group) {
			bool done = false;
			var lowestPlayer = group.Where(p => p.HealthFraction > 0 && !p.IsDead).OrderBy(p => p.HealthFraction).First();
			var lowPlayerCount = group.Where(p => p.HealthFraction > 0 && !p.IsDead).Count(p => p.HealthFraction < 0.5);
			if (Cast("Holy Prism", () => lowestPlayer.HealthFraction < HPHeal, lowestPlayer)) {
				done = true;
				DebugWrite("Holy Prism on " + lowestPlayer.Name);	
			}
			return done;
		}*/

		bool DoFL (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (Me.HasAura ("Infusion of Light") || Me.HasAura (53576)) {
				if (Cast ("Holy Light", () => lowestPlayer.HealthFraction < HLHeal, lowestPlayer)) {
					DebugWrite ("Holy Light on " + lowestPlayer.Name);
					done = true;
				}
			} else {
				if (Cast ("Flash of Light", () => lowestPlayer.HealthFraction <= FLHeal, lowestPlayer)) {
					done = true;
					DebugWrite ("Flash of Light on " + lowestPlayer.Name);	
				}
			}
			return done;
		}

		/*bool DoHP(PlayerObject[] group) {
			bool done = false;
			var lowestPlayer = group.Where(p => p.HealthFraction > 0 && !p.IsDead).OrderBy(p => p.HealthFraction).First();
			if (Cast("Holy Prism", () => lowestPlayer.HealthFraction <= HPHeal, lowestPlayer)) {
				done = true;
				DebugWrite("Holy Prism on " + lowestPlayer.Name);	
			}	
			return done;
		}*/


		bool DoHR (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (Cast ("Holy Radiance", () => lowestPlayer.HealthFraction < HRHeal, lowestPlayer)) {
				done = true;
				DebugWrite ("Holy Radiance on " + lowestPlayer.Name);
			}
			return done;
		}

		bool DoEF (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).Where (p => !p.HasAura (myHealTalent1)).OrderBy (p => p.HealthFraction).First ();
			if (HP > 3) {
				if (Cast (myHealTalent1, () => lowestPlayer.HealthFraction < WoGHeal, lowestPlayer)) {
					DebugWrite (myHealTalent1 + " on " + lowestPlayer.Name);
					done = false;
				}
			}
			return done;
		}

		bool DoHL (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();

			if (Cast ("Holy Light", () => lowestPlayer.HealthFraction < HLHeal, lowestPlayer)) {
				DebugWrite ("Holy Light on " + lowestPlayer.Name);
				done = true;
			}
			return done;
		}

		bool LoD (PlayerObject[] group)
		{
			bool done = false;
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (HP >= 3) {
				if (Cast ("Light of Dawn")) {
					DebugWrite ("Light of Dawn");
					done = true;
				}
			}
			return done;
		}

		bool GetHP (PlayerObject[] group)
		{
			bool done = false;
			int ghlimit = 3; // In groups
			if (group.Length > 5)
				ghlimit = 5; // in raids
			var lowPlayerCount = group.Where (p => p.HealthFraction > 0 && !p.IsDead).Count (p => p.HealthFraction < 0.44);
			var lowestPlayer = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (HP <= 5) {
				if (lowPlayerCount >= ghlimit) {
					if (Me.HasAura ("Daybreak")) {
						if (Cast ("Holy Shock", null, lowestPlayer)) {
							DebugWrite ("Holy Shock on " + lowestPlayer.Name);
							done = true;
						}
					} else {
						if (Cast ("Holy Radiance", () => lowestPlayer.HealthFraction <= HRHeal, lowestPlayer)) {
							DebugWrite ("Holy Radiance on " + lowestPlayer.Name);
							done = true;
						}
					}
				} else {
					if (Cast ("Holy Shock", null, lowestPlayer)) {
						done = true;
						DebugWrite ("Holy Shock on " + lowestPlayer.Name);
					}
				}
			}
			return done;
		}

		public override void Combat ()
		{
			if (HasGlobalCooldown ())
				return;
			if (Me.IsChanneling)
				return;
			if (Me.IsCasting)
				return;
			if (Me.HasAura ("Drinking"))
				return;
			// DebugWrite("HolyPower: " + HP);
			var grpAndMe = Group.GetGroupMemberObjects ().Where (p => p.HealthFraction > 0 && p.IsInCombatRange && p.IsInLoS && !p.IsDead).Concat (new[] { Me }).ToArray ();
			var lowestPlayer = grpAndMe.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();

			if (Target != null) {
				if (Target.IsEnemy && Target.IsInCombatRange) {
					Cast ("Holy Prism");
				}
			}

			if (CastSelfPreventDouble ("Hand of Freedom", () => !Me.CanParticipateInCombat))
				return;
			if (DS) {
				if (CastSelf ("Divine Shield", () => Me.HealthFraction <= 0.25))
					return;
			}
			if (LOH) {
				if (CastPreventDouble ("Lay on Hands", () => Me.Focus.HealthFraction <= 0.25))
					return;
			}

			if (AW) {
				int burstLimit = 3;
				if (grpAndMe.Length > 5)
					burstLimit = 5;
				var lowPlayerCount2 = grpAndMe.Count (p => p.HealthFraction < 0.5);
				if (lowPlayerCount2 >= burstLimit) {
					CastSelf ("Avenging Wrath");
					return;
				}
			}

			if (HOP) {
				var hopTargets = grpAndMe.Where (p => p.HealthFraction <= HoP && p.IsInLoS && !p.IsDead && (!p.IsTank || p == Me || p.IsHealer)).ToArray ();
				if (SpellCooldown ("Hand of Protection") < 0.2 && hopTargets.Length > 0) {
					foreach (var p in hopTargets)
						if (Cast ("Hand of Protection", p))
							return;
				}
			}

			if (HOS) {
				if (Me.Focus != null) {
					if (Me.Focus.IsFriendly && Me.Focus.IsInLoS && Me.Focus.IsInCombatRange) {
						Cast ("Hand of Sacrifice", () => Me.Focus.HealthFraction <= HoS && !Me.Focus.HasAura ("Hand of Sacrifice", true), Me.Focus);
					}
				}
			}

			if (Me.HasAura ("Divine Purpose")) {
				if (LoD (grpAndMe))
					return;
			}

			if (HasSpell ("Beacon of Faith")) {
				var bof = Group.GetGroupMemberObjects ().FirstOrDefault (p => p.HasAura ("Beacon of Faith", true));
				if (bof == null) {
					if (CastSelf ("Beacon of Faith", () => !Me.HasAura ("Beacon of Faith", true)))
						return;
				}
			}

			if (Me.Focus != null) {
				if (Me.Focus.IsFriendly && Me.Focus.IsInLoS && Me.Focus.IsInCombatRange) {
					Cast ("Beacon of Light", () => !Me.Focus.HasAura ("Beacon of Light", true), Me.Focus);
				}
			} else if (FlameChangeDelay.IsReady) {
				Cast ("Beacon of Light", () => !lowestPlayer.HasAura ("Beacon of Light", true) && !lowestPlayer.HasAura ("Beacon of Faith", true), lowestPlayer);
			}

			/// Target heal
			if (Target != null) {
				if (Target.IsFriendly && Target.IsInCombatRange) {
					if (target) {
						if (Cast ("Holy Shock", () => HP < 5 && Target.HealthFraction <= 0.9))
							return;
						if (Cast ("Word of Glory", () => HP >= 3 && Target.HealthFraction <= 0.8))
							return;
						if (Cast ("Holy Light", () => Target.HealthFraction <= HLHeal))
							return;
						if (Cast ("Flash of Light", () => Target.HealthFraction <= FLHeal))
							return;
					}
				}
			}

			int AECount = 3;
			if (grpAndMe.Length > 5)
				AECount = 6;
			var lowPlayerCount = grpAndMe.Count (p => p.HealthFraction < 0.83);			
			if (lowPlayerCount >= AECount) {

				if (LoD (grpAndMe))
					return;
				if (GetHP (grpAndMe))
					return;
				if (DoHR (grpAndMe))
					return;
			}
			if (lowestPlayer.HealthFraction > FLHeal) {
				if (GetHP (grpAndMe))
					return;
			}

			//if (DoHP(grpAndMe)) return;
			if (DoFL (grpAndMe))
				return;
			if (DoEF (grpAndMe))
				return;
			if (DoHL (grpAndMe))
				return;
			if (LoD (grpAndMe))
				return;
			if (Target != null) {
				if (Target.IsEnemy && Target.IsInCombatRange) {
					if (Cast ("Hammer of Wrath", () => Target.HealthFraction <= 0.2))
						return;
					Cast ("Judgment");
					Cast ("Denounce");
				}
			}
			return;				
		}

	}
}

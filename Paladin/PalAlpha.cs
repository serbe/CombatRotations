using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot;
using ReBot.API;

namespace ReBot
{
	[Rotation ("PallyHealzAlpha by Travis506", "Travis506", "1.0.2", WoWClass.Paladin, Specialization.PaladinHoly, 40)]
	public class PallyHealzAlpha : CombatRotation
	{

		//             _   _   _
		//    ___  ___| |_| |_(_)_ __   __ _ ___
		//   / __|/ _ \ __| __| | '_ \ / _` / __|
		//   \__ \  __/ |_| |_| | | | | (_| \__ \
		//   |___/\___|\__|\__|_|_| |_|\__, |___/
		//                             |___/

		public enum Menu
		{
			Aggressive,
			Normal,
			Conservative,
			Auto,
		}

		#region Settings

		[JsonProperty ("Liability Agreement"), Description ("** Legal ** \n You hereby agree to utilize this Script by Travis506, at your own risk, and hold harmless Travis506 in whole, as a result of being Banned, Suspeneded or Account Delected.")]
		public bool Liability = true;

		[JsonProperty ("Gameplay Style"), Description ("Choose your Playstyle, or set to Auto."), JsonConverter (typeof(StringEnumConverter))]
		public Menu Choice { get; set; }

		[JsonProperty ("Tank before Others Priority"), Description ("Priority is set to keeping the tank alive, above all other members in the group or raid.")]
		public bool TankPriority { get; set; }

		[JsonProperty ("Auto Beacon of Faith Self")]
		public bool AutoBoFSelf { get; set; }

		[JsonProperty ("Auto Resurrect Fallen"), Description ("Will automatically attempt to resurrect a member, once you're out of combat.")]
		public bool AutoRes { get; set; }

		[JsonProperty ("Auto Cast LvL90 Talent"), Description ("This will attempt to auto-cast Holy Prsm, Light's Hammer or Execution Sentence, automatically.")]
		public bool AutoCastLvL90 { get; set; }

		[JsonProperty ("Auto Cleanse"), Description ("Attempts to auto-cast Cleanse to Dispel Magic, Curse or Poison")]
		public bool AutoCleanse = true;
		[JsonProperty ("Cleanse Self Only"), Description ("Will only attempt to cleanse yourself, and no other.")]
		public bool CleanseSelfOnly = true;

		public Menu Playstyle {
			get {
				if (Choice == Menu.Auto) {
					if (Me.ManaFraction >= 0.90)
						return Menu.Aggressive;
					if (Me.ManaFraction >= 0.50)
						return Menu.Normal;
					return Menu.Conservative;
				}
				return Choice;
			}
		}

		// Holy Power
		public int HP { get { return Me.GetPower (WoWPowerType.PaladinHolyPower); } }

		#endregion

		//                    _
		//    _ __ ___   __ _(_)_ __
		//   | '_ ` _ \ / _` | | '_ \
		//   | | | | | | (_| | | | | |
		//   |_| |_| |_|\__,_|_|_| |_|
		//

		public PallyHealzAlpha ()
		{
		}

		public override bool OutOfCombat ()
		{
			HealSequence ();
			return false;
		}

		public override void Combat ()
		{
			HealSequence ();
		}

		public void HealSequence ()
		{
			if (!Liability)
				return;
			if (Me.IsMounted || Me.IsFlying || Me.IsOnTaxi || Me.IsMoving)
				return;
			if (Me.HasAura ("Drink") || Me.HasAura ("Food"))
				return;
			if (Me.IsChanneling)
				return;
			if (Me.IsCasting)
				return;
			var grpAndMe = Group.GetGroupMemberObjects ().Where (p => p.Health > 0 && p.IsInCombatRange && p.IsInLoS && !p.IsDead).Concat (new[] { Me }).ToArray ();
			// Auto Buff Eh..
			if (AutoBuff ())
				return;

			// PvP Element
			if (InParty) {

				switch (Playstyle) {
				case Menu.Aggressive:
					// The gameplay for Aggressive should offer the most costly spells
					// at lower HealthFraction triggers, to give the best healing output,
					// this is usually for emergencies, or an abundence of Mana.
					// Procs
					if (DoIoL (grpAndMe, 0.75))
						return;			// Infusion of Light
					if (DoEHS (grpAndMe, 0.80))
						return;			// Enhanced Holy Shock
					// Talents
					// Lvl45
					if (CastEF (grpAndMe, 0.70))
						return;			// Eternal Flame
					if (CastSS (grpAndMe, 0.85))
						return;			// Sacred Shield
					// LvL60
					if (CastHoP (grpAndMe, 0.70))
						return;		// Hand of Purity
					// Lvl75
					if (CastHA (grpAndMe, 0.25, 0.65))			// Holy Avenger
						// Lvl90
					if (CastHP (grpAndMe, 0.90))
						return;			// Holy Prism
					if (CastLH (grpAndMe, 0.60))
						return;			// Light's Hammer (Non-Functional)
					if (CastES (grpAndMe, 0.75))
						return;			// Execution Sentence
					// Spells
					if (CastCleanse (grpAndMe))
						return;			// Cleanse
					if (CastAW (grpAndMe))
						return;
					if (CastFL (grpAndMe, 0.45))
						return;			// Flash of Light
					if (CastHS (grpAndMe, 0.99))
						return;			// Holy Shock
					if (CastWG (grpAndMe, 0.85))
						return;			// Word of Glory
					if (CastHL (grpAndMe, 0.80))
						return;			// Holy Light
					break;
				case Menu.Normal:
					// The gameplay for Normal should be a balance of fast and costly spells
					// with slow and cheap spells. More specifically, Holy Light before Flash of Light
					// Flash of Light before Word of Glory & Holy Shock.
					// Procs
					if (DoIoL (grpAndMe, 0.75))
						return;			// Infusion of Light
					if (DoEHS (grpAndMe, 0.80))
						return;			// Enhanced Holy Shock
					// Talents
					// Lvl45
					if (CastEF (grpAndMe, 0.85))
						return;			// Eternal Flame
					if (CastSS (grpAndMe, 0.85))
						return;			// Sacred Shield
					// LvL60
					if (CastHoP (grpAndMe, 0.70))
						return;		// Hand of Purity
					// Lvl75
					if (CastHA (grpAndMe, 0.25, 0.65))			// Holy Avenger
						// Lvl90
					if (CastHP (grpAndMe, 0.75))
						return;			// Holy Prism
					if (CastLH (grpAndMe, 0.60))
						return;			// Light's Hammer (Non-Functional)
					if (CastES (grpAndMe, 0.75))
						return;			// Execution Sentence
					// Spells
					if (CastCleanse (grpAndMe))
						return;			// Cleanse
					if (CastAW (grpAndMe))
						return;
					if (CastFL (grpAndMe, 0.35))
						return;			// Flash of Light
					if (CastHS (grpAndMe, 0.95))
						return;			// Holy Shock
					if (CastWG (grpAndMe, 0.45))
						return;			// Word of Glory
					if (CastHL (grpAndMe, 0.85))
						return;			// Holy Light
					break;
				case Menu.Conservative:
					// The gameplay for Conservative should be priority to low-costing spells
					// that offer the best advantage for procing, and gathering your mana back up
					// to an optimal range.
					// Procs
					if (DoIoL (grpAndMe, 0.75))
						return;			// Infusion of Light
					if (DoEHS (grpAndMe, 0.80))
						return;			// Enhanced Holy Shock
					// Talents
					// Lvl45
					if (CastEF (grpAndMe, 0.70))
						return;			// Eternal Flame
					if (CastSS (grpAndMe, 0.85))
						return;			// Sacred Shield
					// LvL60
					if (CastHoP (grpAndMe, 0.70))
						return;		// Hand of Purity
					// Lvl75
					if (CastHA (grpAndMe, 0.25, 0.65))			// Holy Avenger
						// Lvl90
					if (CastHP (grpAndMe, 0.55))
						return;			// Holy Prism
					if (CastLH (grpAndMe, 0.60))
						return;			// Light's Hammer (Non-Functional)
					if (CastES (grpAndMe, 0.75))
						return;			// Execution Sentence
					// Spells
					if (CastCleanse (grpAndMe))
						return;			// Cleanse
					if (CastAW (grpAndMe))
						return;				// Avenging Wrath
					if (CastFL (grpAndMe, 0.20, true))
						return;	// Flash of Light (Flash of Light)
					if (CastHS (grpAndMe, 0.85, true))
						return;	// Holy Shock (Tank Priority)
					if (CastWG (grpAndMe, 0.90, true))
						return;	// Word of GLory (Tank Priority)
					if (CastHL (grpAndMe, 0.75, true))
						return;	// Holy Light (Tank Priority)
					break;
				}

				if (!Me.InCombat) {
					if (CastRes (grpAndMe))
						return;
				}

			}

			return;
		}

		public int CountLowHealth (PlayerObject[] group)
		{
			int playersWithLowHealth = 0;
			foreach (var u in group) {
				if (u.HealthFraction < 0.55)
					playersWithLowHealth++;
			}
			return playersWithLowHealth;
		}

		public static bool InParty {
			get {
				return Group.GetNumGroupMembers () > 0;
			}
		}

		public IEnumerable<UnitObject> PartyMembers ()
		{
			return MyParty ();
		}

		public IEnumerable<UnitObject> MyParty ()
		{
			List<PlayerObject> list;
			list = Group.GetGroupMemberObjects ();
			list.Add (Me);
			return list.Distinct ();
		}

		//                   _ _
		//    ___ _ __   ___| | |___
		//   / __| '_ \ / _ \ | / __|
		//   \__ \ |_) |  __/ | \__ \
		//   |___/ .__/ \___|_|_|___/
		//       |_|

		#region Spells

		// Holy Light
		public bool CastHL (PlayerObject[] group, Double trigger, bool tankPriority = false)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (LP.IsTank && LP.HealthFraction < trigger && tankPriority) {
				if (Cast ("Holy Light", () => LP.HealthFraction < trigger && LP.IsTank, LP))
					return true;
			} else {
				if (Cast ("Holy Light", () => LP.HealthFraction < trigger, LP))
					return true;
			}
			return false;
		}

		public bool CastFL (PlayerObject[] group, Double trigger, bool tankPriority = false)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (LP.IsTank && LP.HealthFraction < trigger && tankPriority) {
				if (Cast ("Flash of Light", () => LP.HealthFraction < trigger && LP.IsTank, LP))
					return true;
			} else {
				if (Cast ("Flash of Light", () => LP.HealthFraction < trigger, LP))
					return true;
			}
			return false;
		}

		public bool CastWG (PlayerObject[] group, Double trigger, bool tankPriority = false)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (LP.IsTank && LP.HealthFraction < trigger && tankPriority) {
				if (Cast ("Word of Glory", () => LP.HealthFraction < trigger && LP.IsTank, LP))
					return true;
			} else {
				if (Cast ("Word of Glory", () => LP.HealthFraction < trigger, LP))
					return true;
			}
			return false;
		}

		public bool CastHS (PlayerObject[] group, Double trigger, bool tankPriority = false)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (LP.IsTank && LP.HealthFraction < trigger && tankPriority) {
				if (Cast ("Holy Shock", () => LP.HealthFraction < trigger && LP.IsTank, LP))
					return true;
			} else {
				if (Cast ("Holy Shock", () => LP.HealthFraction < trigger, LP))
					return true;
			}
			return false;
		}

		public bool CastRes (PlayerObject[] group)
		{
			foreach (var u in group) {
				if (Cast ("Redemption", () => u.IsDead, u)) {
					return true;
				}
			}
			return false;
		}

		public bool CastAW (PlayerObject[] group)
		{
			int PLH = CountLowHealth (group);
			if (PLH < 4 && Me.HealthFraction < 0.65 || Me.ManaFraction < 0.55) {
				if (CastSelf ("Avenging Wrath"))
					return true;
			}
			return false;
		}

		public bool CastCleanse (PlayerObject[] group)
		{
			if (CleanseSelfOnly) {
				if (CastSelf ("Cleanse", () => Me.Auras.Any (a => a.IsDebuff && AutoCleanse && "Magic,Curse,Poison".Contains (a.DebuffType)))) {
					return true;
				}
			} else {
				var unit = PartyMembers ().FirstOrDefault (m => m.Auras.Any (a => a.IsDebuff && AutoCleanse && "Magic,Curse,Poison".Contains (a.DebuffType)));
				if (unit != null) {
					if (Cast ("Cleanse", unit)) {
						return true;
					}
				}
			}
			return false;
		}

		#endregion

		//    _        _            _
		//   | |_ __ _| | ___ _ __ | |_ ___
		//   | __/ _` | |/ _ \ '_ \| __/ __|
		//   | || (_| | |  __/ | | | |_\__ \
		//    \__\__,_|_|\___|_| |_|\__|___/
		//

		#region Talents

		//    _       _   _  _  ____
		//   | |_   _| | | || || ___|
		//   | \ \ / / | | || ||___ \
		//   | |\ V /| | |__   _|__) |
		//   |_| \_/ |_|    |_||____/
		//

		public bool GetEF ()
		{
			if (HasSpell ("Eternal Flame")) {
				return true;
			}
			return false;
		}

		public bool GetSS ()
		{
			if (HasSpell ("Sacred Shield")) {
				return true;
			}
			return false;
		}

		public bool CastEF (PlayerObject[] group, Double trigger)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (GetEF ()) {
				if (Cast ("Eternal Flame", () => LP.HealthFraction < trigger && HP >= 3)) {
					return true;
				}
			}
			return false;
		}

		public bool CastSS (PlayerObject[] group, Double trigger)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			if (Cast ("Sacred Shield", () => LP.HealthFraction < trigger && !LP.HasAura ("Sacred Shield"), LP)) {
				return true;
			}
			return false;
		}

		//    _       _    __    ___
		//   | |_   _| |  / /_  / _ \
		//   | \ \ / / | | '_ \| | | |
		//   | |\ V /| | | (_) | |_| |
		//   |_| \_/ |_|  \___/ \___/
		//

		public bool GetHoP ()
		{
			if (HasSpell ("Hand of Purity")) {
				return true;
			}
			return false;
		}

		public bool CastHoP (PlayerObject[] group, Double trigger)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			// Tank Priority
			if (GetHoP () && LP.HealthFraction < trigger && TankPriority) {
				if (Cast ("Hand of Purity", () => LP.HealthFraction < trigger && LP.IsTank, LP)) {
					return true;
				}
				if (Cast ("Hand of Purity", () => LP.HealthFraction < trigger, LP)) {
					return true;
				}
			}
			return false;
		}

		//    _       _   _____ ____
		//   | |_   _| | |___  | ___|
		//   | \ \ / / |    / /|___ \
		//   | |\ V /| |   / /  ___) |
		//   |_| \_/ |_|  /_/  |____/
		//

		public bool GetHA ()
		{
			if (HasSpell ("Holy Avenger")) {
				return true;
			}
			return false;
		}

		public bool CastHA (PlayerObject[] group, Double health, Double mana)
		{
			if (GetHA ()) {
				// Very Situational Spell Casting
				if (Cast ("Holy Avenger", () => Me.HealthFraction < health || Me.ManaFraction < mana && HP <= 5)) {
					return true;
				}
			}
			return false;
		}

		//    _       _    ___   ___
		//   | |_   _| |  / _ \ / _ \
		//   | \ \ / / | | (_) | | | |
		//   | |\ V /| |  \__, | |_| |
		//   |_| \_/ |_|    /_/ \___/
		//

		// Holy Prism
		public bool GetHP ()
		{
			if (HasSpell ("Holy Prism")) {
				return true;
			}
			return false;
		}
		// Light's Hammer
		public bool GetLH ()
		{
			if (HasSpell ("Light's Hammer")) {
				return true;
			}
			return false;
		}
		// Execution Sentence
		public bool GetES ()
		{
			if (HasSpell ("Execution Sentence")) {
				return true;
			}
			return false;
		}

		public bool CastHP (PlayerObject[] group, Double trigger)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();

			if (GetHP () && AutoCastLvL90) {
				if (Cast ("Holy Prism", () => LP.HealthFraction < trigger, LP)) {
					return true;
				}
			}

			return false;
		}

		public bool CastLH (PlayerObject[] group, Double trigger)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();

			return false;
		}

		public bool CastES (PlayerObject[] group, Double trigger)
		{
			var LP = group.Where (p => p.HealthFraction > 0 && !p.IsDead).OrderBy (p => p.HealthFraction).First ();
			// Tank Priority
			if (GetES () && AutoCastLvL90) {
				if (Cast ("Execution Sentence", () => LP.HealthFraction < trigger && LP.IsTank, LP)) {
					return true;
				}
				if (Cast ("Execution Sentence", () => LP.HealthFraction < trigger, LP)) {
					return true;
				}
			}
			return false;
		}

		//
		//    _ __  _ __ ___   ___ ___
		//   | '_ \| '__/ _ \ / __/ __|
		//   | |_) | | | (_) | (__\__ \
		//   | .__/|_|  \___/ \___|___/
		//   |_|

		/* Infusion of Light Priority */
		public bool DoIoL (PlayerObject[] group, Double trigger)
		{
			if (Me.HasAura ("Infusion of Light")) {
				if (CastHL (group, trigger)) {
					return true;
				}
			}
			//default return
			return false;
		}

		/* Enahnced Holy Shock */
		public bool DoEHS (PlayerObject[] group, Double trigger)
		{
			if (Me.HasAura ("Enhanced Holy Shock")) {
				if (CastHS (group, trigger)) {
					return true;
				}
			}
			//default return
			return false;
		}

		/* DayBreak */
		public bool DoDB (PlayerObject[] group)
		{
			if (Me.HasAura ("Daybreak")) {
				// Radiance Code Here
			}
			return false;
		}

		//    _            __  __
		//   | |__  _   _ / _|/ _|___
		//   | '_ \| | | | |_| |_/ __|
		//   | |_) | |_| |  _|  _\__ \
		//   |_.__/ \__,_|_| |_| |___/
		//

		public bool AutoBuff ()
		{
			// Check if using Right Seal
			if (!HasAura ("Seal of Insight")) {
				if (Cast ("Seal of Insight"))
					return true;
			}
			// Ensure BoF is set to self, if toggled
			if (!HasAura ("Beacon of Faith") && AutoBoFSelf) {
				if (!Me.HasAura ("Beacon of Faith")) {
					if (CastSelf ("Beacon of Faith"))
						return true;
				}
			}
			// Buffs 
			// Blessing of Kings (Conditional)
			if (!HasAura ("Blessing of Might")) {
				if (!Me.HasAura ("Blessing of Kings") && !Me.HasAura ("Legacy of the Emperor") && !Me.HasAura ("Legacy of the White Tiger") && !Me.HasAura ("Mark of the Wild")) {
					if (CastSelf ("Blessing of Kings")) {
						return true;
					}
				}
			}
			// Blessing of Might (Backup)
			if (!HasAura ("Blessing of Kings")) {
				if (!Me.HasAura ("Blessing of Might")) {
					if (CastSelf ("Blessing of Might")) {
						return true;
					}
				}
			}
			return false;
		}

		#endregion

	}
}
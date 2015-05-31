
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ReBot.API;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rebot
{
	[Rotation ("RestoDruid_by_Pasterke", "Pasterke", WoWClass.Druid, Specialization.DruidRestoration, 40, 20, false, 0)]
	public class RestoDruid : CombatRotation
	{
		public bool Debug = true;

		public const int
			_NaturesSwiftness = 132158,
			_HealingTouch = 5185,
			_Regrowth = 8936,
			_Lifebloom = 33763,
			_WildGrowth = 48438,
			_Rejuvenation = 774,
			_WildMushroom = 145205,
			_Ironbark = 102342,
			_NaturesCure = 88423,
			_Rebirth = 20484,
			_Swiftmend = 18562,
			_ForceOfNature = 102693,
			_Wrath = 5176,
			_Moonfire = 8921,
			_MarkOfTheWild = 1126,
			_healthstone = 5515,
			end = 0;

		#region settings

		//hotkeys
		public enum Modifier
		{
			None,
			LALT,
			RALT,
			LCTRL,
			RCTRL,
			LSHIFT,
			RSHIFT
		}

		public enum trinket1
		{
			Manual,
			OnCoolDown,
			AtLowMana,
			AtLowHP,
			WithTreeOfLife
		}

		public enum trinket2
		{
			Manual,
			OnCoolDown,
			AtLowMana,
			AtLowHP,
			WithTreeOfLife
		}

		[JsonProperty ("Res Tank Modifier                    "), JsonConverter (typeof(StringEnumConverter))]
		public Modifier resTankModifier { get; set; }

		//common
		[JsonProperty ("Use Trinket 1 at %                   ")]
		public double trinket1percent = 0;

		[JsonProperty ("Use Trinket 2 at %                   ")]
		public double trinket2percent = 0;

		[JsonProperty ("Auto Dispel                          ")]
		public bool AutoDispel = true;

		[JsonProperty ("Allow DPS                            ")]
		public bool allowDPS = false;

		[JsonProperty ("Auto Buff MotW on Party Members      ")]
		public bool AutoBuff = false;

		[JsonProperty ("Healthstone HP%                      ")]
		public double HealthstonePercent = 0.45;

		[JsonProperty ("Keep Rejuvenation on Tank            ")]
		public bool rejuOnTank = true;

		[JsonProperty ("Auto Use Tree of Life                ")]
		public bool AutoTree = true;

		[JsonProperty ("Iron Bark Tank HP%                   ")]
		public double ironBarkHealth = 0.45;

		[JsonProperty ("Barkskin My HP%                      ")]
		public double barkskinHealth = 0.75;

		[JsonProperty ("Use Trinket 1                        "), JsonConverter (typeof(StringEnumConverter))]
		public trinket1 UseTrinket1 { get; set; }


		[JsonProperty ("Use Trinket 2                        "), JsonConverter (typeof(StringEnumConverter))]
		public trinket1 UseTrinket2 { get; set; }



		//Dungeon Settings
		[JsonProperty ("Dungeon Healing Touch HP%            ")]
		public double healingTouchHealthD = 0.85;

		[JsonProperty ("Dungeon Regrowth HP%                 ")]
		public double regrowthHealthD = 0.65;

		[JsonProperty ("Dungeon Wild Growth HP%              ")]
		public double wildGrowthHealthD = 0.75;

		[JsonProperty ("Dungeon Wild Growth Player Count     ")]
		public int wildGrowthPlayerCountD = 3;

		[JsonProperty ("Dungeon Rejuvenation HP%             ")]
		public double rejuvenationHealthD = 0.75;

		[JsonProperty ("Dungeon Germination HP%              ")]
		public double germinationHealthD = 0.75;

		[JsonProperty ("Dungeon Swiftmend HP%                ")]
		public double swiftmendHealthD = 0.70;

		[JsonProperty ("Dungeon Wild Mushroom HP%            ")]
		public double wildMushroomHealthD = 0.85;

		[JsonProperty ("Dungeon Wild Mushroom Player Count   ")]
		public int wildMushroomPlayerCountD = 3;

		[JsonProperty ("Dungeon Tree of Life HP%             ")]
		public double treeOfLifeHealthD = 0.65;

		[JsonProperty ("Dungeon Tree of Life Player Count    ")]
		public int treeOfLifePlayerCountD = 3;

		[JsonProperty ("Dungeon Force of Nature HP%          ")]
		public double forceOfNatureHealthD = 0.75;

		//Raid Settings
		[JsonProperty ("Raid Healing Touch HP%            ")]
		public double healingTouchHealth = 0.85;

		[JsonProperty ("Raid Regrowth HP%                 ")]
		public double regrowthHealth = 0.65;

		[JsonProperty ("Raid Wild Growth HP%              ")]
		public double wildGrowthHealth = 0.75;

		[JsonProperty ("Raid Wild Growth Player Count     ")]
		public int wildGrowthPlayerCount = 3;

		[JsonProperty ("Raid Rejuvenation HP%             ")]
		public double rejuvenationHealth = 0.75;

		[JsonProperty ("Raid Germination HP%              ")]
		public double germinationHealth = 0.75;

		[JsonProperty ("Raid Swiftmend HP%                ")]
		public double swiftmendHealth = 0.70;

		[JsonProperty ("Raid Wild Mushroom HP%            ")]
		public double wildMushroomHealth = 0.85;

		[JsonProperty ("Raid Wild Mushroom Player Count   ")]
		public int wildMushroomPlayerCount = 3;

		[JsonProperty ("Raid Tree of Life HP%             ")]
		public double treeOfLifeHealth = 0.65;

		[JsonProperty ("Raid Tree of Life Player Count    ")]
		public int treeOfLifePlayerCount = 3;

		[JsonProperty ("Raid Force of Nature HP%          ")]
		public double forceOfNatureHealth = 0.75;

		#endregion

		#region heal variables

		public double healtouchHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return healingTouchHealth;
				return healingTouchHealthD;
			}
		}

		public double regrowHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return regrowthHealth;
				return regrowthHealthD;
			}
		}

		public int wildgrowthPlayers {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return wildGrowthPlayerCount;
				return wildGrowthPlayerCountD;
			}
		}

		public double wildgrowthHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return wildGrowthHealth;
				return wildGrowthHealthD;
			}
		}

		public double rejuHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return rejuvenationHealth;
				return rejuvenationHealthD;
			}
		}

		public double fonHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return forceOfNatureHealth;
				return forceOfNatureHealthD;
			}
		}

		public double treeHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return treeOfLifeHealth;
				return treeOfLifeHealthD;
			}
		}

		public int treePlayers {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return treeOfLifePlayerCount;
				return treeOfLifePlayerCountD;
			}
		}

		public double mushHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return wildMushroomHealth;
				return wildMushroomHealthD;
			}
		}

		public int mushPlayers {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return wildMushroomPlayerCount;
				return wildMushroomPlayerCountD;
			}
		}

		public double swiftHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return swiftmendHealth;
				return swiftmendHealthD;
			}
		}

		public double germHealth {
			get {
				if (Group.GetNumGroupMembers () > 5)
					return germinationHealth;
				return germinationHealthD;
			}
		}

		#endregion

		public override bool OutOfCombat ()
		{
			if (CastSelf (_Rejuvenation, () => Me.HealthFraction <= 0.8 && !HasAura (_Rejuvenation)))
				return true;
			if (CastSelfPreventDouble ("Healing Touch", () => Me.HealthFraction <= 0.5))
				return true;
			return false;
		}

		private void LogMsg (string text)
		{
			API.Print (text);
		}

		public RestoDruid ()
		{
			GroupBuffs = new string[] {
				"Mark of the Wild"
			};
			PullSpells = new string[] {
				"Moonfire"
			};
		}

		#region MeIsInParty

		public static bool MeIsInParty {
			get {
				return Group.GetNumGroupMembers () > 0;
			}
		}

		#endregion

		public GUID lastGuid;

		#region spellconditions

		#region me in front of target

		public bool MeIsInFrontOfTarget (UnitObject target)
		{
			return !Me.IsNotInFront (target);
		}

		#endregion

		#region mark of the wild

		public UnitObject MotWTarget {
			get {
				if (!MeIsInParty && !Me.HasAura ("Mark of the Wild")) {
					return Me;
				}

				if (MeIsInParty && AutoBuff && !MeIsInProvingGrounds) {
					var t = PartyMembers ().Where (p => !p.IsDead
					        && p.Distance <= 40
					        && !p.HasAura ("Mark of the Wild")
					        && !p.HasAura ("Legacy of the Emperor")
					        && !p.HasAura ("Blessing of Kings")).FirstOrDefault ();
					if (t != null) {
						return t;
					}
				}
				return null;
			}
		}

		#endregion

		#region clear target

		public bool needClearTarget ()
		{
			try {
				if (MeIsInParty) {
					if (Me.Target != null && !Me.Target.IsDead && Me.Target.IsPlayer) {
						Me.ClearTarget ();
						return true;
					}
				}
			} catch (Exception e) {
				LogMsg (e.ToString ());
			}
			return false;
		}

		#endregion

		#region overlayed

		public bool IsOverlayed (int spellID)
		{
			return API.ExecuteLua<bool> ("return IsSpellOverlayed(" + spellID + ");");
		}

		#endregion

		#region spellcharges

		public int GetSpellCharges (string spell)
		{

			return API.ExecuteLua<int> ("return GetSpellCharges(" + spell + ")");
		}

		#endregion

		#region healthstone

		public bool needHealthStone ()
		{
			return API.ItemCount (_healthstone) > 0 && API.ItemCooldown (_healthstone) == 0 && Me.HealthFraction <= HealthstonePercent;
		}

		#endregion

		#region Nature's Vigil

		public bool needNaturesVigil ()
		{
			if (!MeIsInParty)
				return false;

			if (!HasSpell ("Nature's Vigil"))
				return false;
			if (SpellCooldown ("Nature's Vigil") > 0)
				return false;

			var t = PartyMembers ().Where (p => !p.IsDead
			        && p.HealthFraction <= 0.85)
				.ToList ();

			if (t.Count () > 2) {
				return true;
			}
			return false;
		}

		#endregion

		#region trinkets

		public bool needTrinket1 ()
		{
			if (API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 13)); return duration;") != 0)
				return false;
			string trink1use = UseTrinket1.ToString ();
			if (trink1use == "Manual")
				return false;
			if (trink1use == "OnCoolDown")
				return true;
			if (trink1use == "AtLowMana" && Me.ManaFraction <= trinket1percent)
				return true;
			if (trink1use == "AtLowHP" && Me.HealthFraction <= trinket1percent)
				return true;
			if (trink1use == "WithTreeOfLife" && Me.HasAura ("Incarnation: Tree of Life"))
				return true;
			return false;
		}

		public bool needTrinket2 ()
		{
			if (API.ExecuteLua<double> ("local _, duration, _= GetItemCooldown(GetInventoryItemID(\"player\", 14)); return duration;") != 0)
				return false;
			string trink2use = UseTrinket2.ToString ();
			if (trink2use == "Manual")
				return false;
			if (trink2use == "OnCoolDown")
				return true;
			if (trink2use == "AtLowMana" && Me.ManaFraction <= trinket2percent)
				return true;
			if (trink2use == "AtLowHP" && Me.HealthFraction <= trinket2percent)
				return true;
			if (trink2use == "WithTreeOfLife" && Me.HasAura ("Incarnation: Tree of Life"))
				return true;
			return false;
		}

		#endregion

		#region tree of life

		public bool needTreeOfLife ()
		{
			if (Me.HasAura ("Incarnation: Tree of Life"))
				return false;
			if (!MeIsInParty)
				return false;

			if (!AutoTree)
				return false;
			if (!HasSpell ("Incarnation: Tree of Life"))
				return false;
			if (SpellCooldown ("Incarnation: Tree of Life") > 0)
				return false;

			var t = PartyMembers ().Where (p => !p.IsDead
			        && p.IsInCombatRangeAndLoS
			        && p.HealthFraction <= treeHealth)
				.ToList ();

			if (t.Count () >= treePlayers) {
				return true;
			}
			return false;
		}

		#endregion

		#region barkskin

		public bool needBarkskin ()
		{
			if (SpellCooldown ("Barkskin") > 0)
				return false;
			if (Me.HealthFraction <= barkskinHealth) {
				return true;
			}
			return false;
		}

		#endregion

		#region ironbark

		public UnitObject ironBarkTarget {
			get {
				if (!MeIsInParty)
					return null;
				if (SpellCooldown ("Ironbark") > 0)
					return null;


				var t = Tanks ().Where (p => !p.IsDead
				        && p.HealthFraction <= ironBarkHealth
				        && p.IsInCombatRangeAndLoS)
					.OrderBy (p => p.HealthFraction)
					.FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		#endregion

		#region moonfire

		public UnitObject moonfireTarget {
			get {
				if (MeIsInParty)
					return null;

				if (Me.Target != null && Me.Target.IsValid && Me.Target.IsAttackable && !Me.Target.HasAura ("Moonfire")) {
					return Me.Target;
				}
				return null;
			}
		}

		#endregion

		#region wrath

		public UnitObject wrathTarget {
			get {
				if (!MeIsInParty) {
					if (Me.Target != null && Me.Target.IsValid && Me.Target.IsAttackable) {
						return Me.Target;
					}
					return null;
				}
				if (MeIsInParty && HasSpell ("Dream of Cenarius")) {
					var t = API.Units.Where (p => p != null
					        && !p.IsDead
					        && p.InCombat
					        && p.IsAttackable
					        && p.IsInCombatRangeAndLoS
					        && !MeIsInFrontOfTarget (p)).OrderBy (p => p.Distance).ToList ();
					if (t.Count () > 0) {
						return t.FirstOrDefault ();
					}
				}
				return null;
			}
		}

		#endregion

		#region wild growth

		public UnitObject wildGrowthTarget {
			get {
				if (!MeIsInParty)
					return null;
				if (SpellCooldown ("Wild Growth") > 0)
					return null;

				var t = PartyMembers ().Where (p => !p.IsDead
				        && p.HealthFraction <= wildgrowthHealth
				        && p.IsInCombatRangeAndLoS)
					.OrderBy (p => p.HealthFraction)
					.ToList ();

				if (t.Count () >= wildgrowthPlayers) {
					return t.FirstOrDefault ();
				}
				return null;
			}
		}

		#endregion

		#region healing touch

		public UnitObject healingTouchTarget {
			get {
				if (!MeIsInParty)
					return null;


				var t = PartyMembers ().Where (p => !p.IsDead
				        && (p.HealthFraction <= healtouchHealth || !Me.HasAura ("Harmony"))
				        && p.IsInCombatRangeAndLoS).OrderBy (p => p.HealthFraction).FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		#endregion

		#region regrowth

		public UnitObject regrowthProcTarget {
			get {
				if (!MeIsInParty && !MeIsInProvingGrounds)
					return null;
				var t = PartyMembers ().Where (p => !p.IsDead
				        && p.HealthFraction < 1
				        && p.IsInCombatRangeAndLoS).OrderBy (p => p.HealthFraction).FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		public UnitObject regrowthTarget {
			get {

				if (!MeIsInParty && Me.HealthFraction <= regrowthHealthD)
					return Me;

				var t = PartyMembers ().Where (p => !p.IsDead
				        && p.HealthFraction <= regrowHealth
				        && p.IsInCombatRangeAndLoS).OrderBy (p => p.HealthFraction).FirstOrDefault ();
				if (t != null) {
					LogMsg ("Casting Regrowth on " + t.Name);
				}
				return t != null ? t : null;
			}
		}

		#endregion

		#region force of nature

		public GUID lastFonGuid;

		public UnitObject fonTarget {
			get {

				if (!HasSpell (_ForceOfNature))
					return null;
				if (SpellCooldown (_ForceOfNature) > 0)
					return null;

				if (!MeIsInParty)
					return null;
				var j = PartyMembers ().Where (p => p.HealthFraction <= fonHealth
				        && p.GUID != lastFonGuid
				        && p.IsInCombatRangeAndLoS)
					.OrderBy (p => p.HealthFraction)
					.FirstOrDefault ();
				if (j != null) {
					lastFonGuid = j.GUID;
				}
				return j != null ? j : null;
			}
		}

		#endregion

		#region lifebloom

		public DateTime lifebloomTimer;

		public UnitObject lifebloomTarget {
			get {
				if (!MeIsInParty && !Me.HasAura (_Lifebloom)) {
					return Me;
				}
				if (Tanks ().Count () > 0) {
					foreach (var unit in Tanks()) {
						if (TargetHasMyBuff ("Lifebloom", unit))
							return null;
						if (!unit.HasAura ("Lifebloom"))
							return unit;
					}
				}
				if (Tanks ().Count () == 0) {
					int k = PartyMembers ().Count (p => p.HasAura ("Lifebloom"));
					if (k == 0) {
						var unit = PartyMembers ().OrderBy (p => p.HealthFraction).FirstOrDefault ();
						return unit;
					} else if (k > 0) {
						return null;
					}
				}
				return null;
			}
		}

		#endregion

		#region wild mushroom

		public DateTime lastMushroomCast;

		public UnitObject mushroomTarget {
			get {
				if (!MeIsInParty)
					return null;

				var mushroomPlanted = API.Units.Where (u => u.EntryID == 47649 && u.CreatedByMe).FirstOrDefault ();

				if (mushroomPlanted != null) {

					var j = PartyMembers ().Where (p => !p.IsDead
					        && p.DistanceTo (mushroomPlanted.Position) <= 12)
						.ToList ();

					if (j.Count () >= mushPlayers) {
						return null;
					}
				}


				var t = PartyMembers ().Where (p => p.HealthFraction <= mushHealth
				        && p.IsInCombatRangeAndLoS).FirstOrDefault ();

				if (t != null) {
					var m = PartyMembers ().Where (p => p.HealthFraction <= mushHealth
					        && p.DistanceTo (t.Position) <= 12).ToList ();
					if (m.Count () >= mushPlayers) {
						return t;
					}
				}
				return null;
			}
		}

		#endregion

		#region rejuvenation

		public UnitObject rejuvenationTarget {
			get {
				if (!MeIsInParty) {
					return Me.HealthFraction <= rejuvenationHealthD ? Me : null;
				}
				if (MeIsInParty) {
					var t = PartyMembers ().Where (p => !p.IsDead
					        && p.HealthFraction <= rejuHealth
					        && !p.HasAura (_Rejuvenation)
					        && p.IsInCombatRangeAndLoS).OrderBy (p => p.HealthFraction).FirstOrDefault ();
					return t != null ? t : null;
				}
				return null;
			}
		}

		public UnitObject tankRejuvenationUnit {
			get {
				if (!rejuOnTank)
					return null;
				if (!MeIsInParty)
					return null;
				if (Tanks ().Count () > 0) {
					var t = Tanks ().Where (p => !p.HasAura ("Rejuvenation") && p.IsInCombatRangeAndLoS).FirstOrDefault ();

					if (t != null) {
						return t;
					}
				}
				return null;
			}
		}

		#endregion

		#region germination

		public UnitObject germinationTarget {
			get {
				if (!MeIsInParty)
					return null;
				if (!HasSpell ("Germination"))
					return null;

				var j = PartyMembers ().Where (p => !p.IsDead
				        && p.IsInLoS
				        && p.Distance <= 40
				        && p.HasAura (_Rejuvenation)
				        && p.HealthFraction <= germHealth).FirstOrDefault ();
				return j != null ? j : null;
			}
		}

		#endregion

		#region swiftmend

		public UnitObject swiftmendTarget {
			get {
				if (!MeIsInParty) {
					if (Me.HealthFraction <= swiftmendHealthD && (Me.HasAura (_Rejuvenation) || Me.HasAura (_Regrowth))) {
						return Me;
					}
					return null;
				}
				if (MeIsInParty) {
					var j = PartyMembers ().Where (p => !p.IsDead
					        && p.IsInLoS
					        && p.Distance <= 40
					        && (p.HasAura (_Rejuvenation) || p.HasAura (_Regrowth))
					        && p.HealthFraction <= swiftHealth).FirstOrDefault ();
					return j != null ? j : null;
				}
				return null;
			}
		}

		#endregion

		#region critical heal

		public UnitObject needUrgentHealTarget {
			get {
				if (!HasSpell (_NaturesSwiftness))
					return null;
				if (SpellCooldown (_NaturesSwiftness) > 0)
					return null;

				if (!MeIsInParty && Me.HealthFraction <= 0.35) {
					return Me;
				}

				if (MeIsInParty) {
					var j = PartyMembers ().Where (p => !p.IsDead
					        && p.IsInCombatRangeAndLoS
					        && p.HealthFraction <= 0.35).FirstOrDefault ();
					return j != null ? j : null;
				}
				return null;
			}
		}

		#endregion

		#region dispel

		public UnitObject dispelTarget {
			get {
				if (!MeIsInParty) {
					if (Me.Auras.Any (a => a.IsDebuff && AutoDispel && "Magic,Curse,Poison".Contains (a.DebuffType))) {
						return Me;
					}
					return null;
				}
				if (MeIsInParty) {
					var unit = PartyMembers ().FirstOrDefault (m => m.Auras.Any (a => a.IsDebuff && AutoDispel && "Magic,Curse,Poison".Contains (a.DebuffType)));
					return unit != null ? unit : null;
				}
				return null;
			}
		}

		#endregion

		#region res tank

		public bool needToResTank ()
		{
			if (!MeIsInParty || MeIsInProvingGrounds)
				return false;
			if (!HasSpell (_Rebirth))
				return false;
			if (SpellCooldown (_Rebirth) > 0)
				return false;

			string checkKey = resTankModifier.ToString ();

			if (checkKey == "None")
				return false;

			string checkKeyToUse = "";
			switch (checkKey) {
			case "LALT":
				checkKeyToUse = "return IsLeftAltKeyDown()";
				break;
			case "RALT":
				checkKeyToUse = "return IsRightAltKeyDown()";
				break;
			case "LCTRL":
				checkKeyToUse = "return IsLeftControlKeyDown()";
				break;
			case "RCTRL":
				checkKeyToUse = "return IsRightControlKeyDown()";
				break;
			case "LSHIFT":
				checkKeyToUse = "return IsLeftShiftKeyDown()";
				break;
			case "RSHIFT":
				checkKeyToUse = "return IsRightShiftKeyDown()";
				break;
			}

			if (API.ExecuteLua<bool> (checkKeyToUse)) {
				var t = Tanks ().Where (p => p.IsDead
				        && p.Distance <= 40).FirstOrDefault ();
				if (t != null) {
					string msgResTanksOn = "Ressing Tanks Hotkey Pressed ! Trying to res Tank.";
					API.ExecuteLua<string> ("RaidNotice_AddMessage(RaidWarningFrame, \"" + msgResTanksOn + "\", ChatTypeInfo[\"RAID_WARNING\"]);");
					Cast ("Rebirth", t);
					return true;
				}
			}
			return false;
		}

		#endregion

		#region fill party

		public string mapName { get { return API.MapInfo.Name; } }

		public bool MeIsInProvingGrounds {
			get {
				return mapName.Contains ("Proving Grounds");
			}
		}

		public HashSet<string> pgMembers = new HashSet<string> () {
			"Oto the Protector",
			"Sooli the Survivalist",
			"Kavan the Arcanist",
			"Ki the Assassin"
		};

		public IEnumerable<UnitObject> Tanks ()
		{
			if (MeIsInProvingGrounds) {
				return API.Units.Where (p => p.Name == "Oto the Protector");
			}
			return Group.GetGroupMemberObjects ().Where (p => p.IsTank).Distinct ();
		}

		public IEnumerable<UnitObject> PartyMembers ()
		{
			return !MeIsInProvingGrounds ? myParty () : pgParty ();
		}

		public IEnumerable<UnitObject> myParty ()
		{
			var list = new List<PlayerObject> ();
			list = Group.GetGroupMemberObjects ();
			list.Add (Me);
			return list.Distinct ();
		}

		public IEnumerable<UnitObject> pgParty ()
		{
			var list = new List<UnitObject> ();
			var t = API.Units.Where (p => p != null
			        && !p.IsDead
			        && p.IsValid
			        && p.Distance <= 80).ToList ();
			if (t.Count () > 0) {
				foreach (var unit in t) {
					if (pgMembers.Contains (unit.Name)) {
						list.Add (unit);
					}
				}
			}
			list.Add (Me);
			return list;
		}

		#endregion

		#region buffs && debuffs

		public bool TargetHasMyDebuff (string debuff, UnitObject target)
		{
			return target.Auras.Any (a => a.IsDebuff && debuff.Contains (a.DebuffType) && a.UnitCaster == Me.Name);
		}

		public bool TargetHasMyBuff (string buff, UnitObject target)
		{
			return target.Auras.Any (a => a.Name == buff && a.UnitCaster == Me.Name);
		}

		#endregion

		#endregion

		public void RestoHealing ()
		{
			if (Me.IsMounted || Me.IsFlying || Me.IsOnTaxi)
				return;
			if (Me.HasAura ("Drink") || Me.HasAura ("Food"))
				return;
			if (Me.IsChanneling)
				return;
			if (Me.IsCasting)
				return;
			if (CastSelf ("Mark of the Wild", () => MotWTarget != null))
				return;
			if (Cast ("Wild Mushroom", () => mushroomTarget != null && mushroomTarget.IsInCombatRangeAndLoS, mushroomTarget))
				return;
			if (CastSelf ("Nature's Vigil", () => needNaturesVigil ()))
				return;
			if (CastSelf ("Barkskin", () => needBarkskin ()))
				return;
			if (Cast ("Ironbark", () => ironBarkTarget != null && ironBarkTarget.IsInCombatRangeAndLoS, ironBarkTarget))
				return;
			if (CastSelf ("Nature's Swiftness", () => needUrgentHealTarget != null))
				return;
			if (Cast ("Healing Touch", () => needUrgentHealTarget != null && Me.HasAura (_NaturesSwiftness) && needUrgentHealTarget.IsInCombatRangeAndLoS, needUrgentHealTarget))
				return;
			if (Cast ("Nature's Cure", () => dispelTarget != null && dispelTarget.IsInCombatRangeAndLoS, dispelTarget))
				return;
			if (CastSelf ("Incarnation: Tree of Life", () => needTreeOfLife ()))
				return;
			if (Cast ("Lifebloom", () => lifebloomTarget != null && lifebloomTarget.IsInCombatRangeAndLoS, lifebloomTarget)) {
				LogMsg ("Casting Lifebloom on " + lifebloomTarget.Name);
				return;
			}
			if (Cast ("Rejuvenation", () => tankRejuvenationUnit != null && tankRejuvenationUnit.IsInCombatRangeAndLoS, tankRejuvenationUnit))
				return;
			if (Cast ("Wild Growth", () => wildGrowthTarget != null && wildGrowthTarget.IsInCombatRangeAndLoS, wildGrowthTarget))
				return;
			if (Cast ("Swiftmend", () => swiftmendTarget != null && swiftmendTarget.IsInCombatRangeAndLoS, swiftmendTarget))
				return;
			if (Cast ("Rejuvenation", () => rejuvenationTarget != null && rejuvenationTarget.IsInCombatRangeAndLoS, rejuvenationTarget))
				return;
			if (Cast ("Germination", () => germinationTarget != null, germinationTarget))
				return;

			if (Cast ("Regrowth", () => regrowthProcTarget != null && IsOverlayed (_Regrowth) && regrowthProcTarget.IsInCombatRangeAndLoS, regrowthProcTarget))
				return;
			if (Cast ("Force of Nature", () => fonTarget != null && fonTarget.IsInCombatRangeAndLoS, fonTarget))
				return;
			if (Cast ("Regrowth", () => regrowthTarget != null, regrowthTarget))
				return;
			if (Cast ("Healing Touch", () => healingTouchTarget != null && healingTouchTarget.IsInCombatRangeAndLoS, healingTouchTarget))
				return;
			if (Cast ("Moonfire", () => moonfireTarget != null && moonfireTarget.IsInCombatRangeAndLoS, moonfireTarget))
				return;
			if (Cast ("Wrath", () => wrathTarget != null && wrathTarget.IsInCombatRangeAndLoS, wrathTarget))
				return;


		}

		public override void Combat ()
		{
			RestoHealing ();
		}


		public bool FindNewTarget (bool reqs)
		{
			if (!reqs)
				return false;
			if (UnFriendlyMeleeTargets > 0) {
				UnitObject unit = findMeleeTargets.FirstOrDefault ();
				Me.SetTarget (unit);
				return true;
			}
			if (UnFriendlyMeleeTargets == 0) {
				UnitObject unit = findRangeTargets.FirstOrDefault ();
				Me.SetTarget (unit);
				return true;
			}
			return false;
		}



		#region findtarget

		public bool needNewTarget ()
		{
			if (Me.Target != null && !Me.Target.IsDead && !Me.Target.IsFriendly && Me.Target.IsEnemy)
				return false;
			if (Me.Target != null && Me.Target.Distance > 12 && UnFriendlyMeleeTargets > 0)
				return true;
			if ((Me.Target == null || Me.Target.IsDead || Me.Target.IsFriendly || !Me.Target.IsAttackable || !Me.Target.IsValid)
			    && Me.InCombat
			    && UnFriendlyMeleeTargets == 0)
				return true;
			return false;
		}

		public List<UnitObject> findRangeTargets {
			get {
				var t = new List<UnitObject> ();
				t = API.Units.Where (p => p != null
				&& !p.IsDead
				&& !p.IsFriendly
				&& !p.IsPet
				&& p.IsAttackable
				&& p.IsInLoS
				&& p.DistanceSquared <= 40).OrderBy (p => p.Distance).ToList ();
				return t;
			}
		}

		public int UnFriendlyRangeTargets { get { return findRangeTargets.Count (); } }

		public List<UnitObject> findMeleeTargets {
			get {
				var t = new List<UnitObject> ();
				t = API.Units.Where (p => p != null
				&& !p.IsDead
				&& !p.IsFriendly
				&& !p.IsPet
				&& p.IsAttackable
				&& p.IsInLoS
				&& p.DistanceSquared <= 8).OrderBy (p => p.Distance).ToList ();
				return t;
			}
		}

		public int UnFriendlyMeleeTargets { get { return findMeleeTargets.Count (); } }

		public UnitObject targetsWithoutDots {
			get {
				var t = API.Units.Where (p => p != null
				        && !p.IsDead
				        && !p.InCombat
				        && !p.IsPet
				        && p.IsAttackable
				        && p.IsInLoS
				        && !Me.IsNotInFront (p)
				        && (!p.HasAura (_Moonfire) && Me.GetPower (WoWPowerType.DruidEclipse) <= 0)
				        && (!p.HasAura ("Sunfire") && Me.GetPower (WoWPowerType.DruidEclipse) > 0)
				        && p.DistanceSquared <= 40).OrderBy (p => p.Distance).FirstOrDefault ();
				return t != null ? t : null;
			}
		}

		#endregion

		#region isboss

		public bool IsBoss (UnitObject target)
		{
//			if (BossList.bossListString.Contains (target.Name))
//				return true;
//			if (BossList.bossListInt.Contains (target.EntryID))
//				return true;
			if (target.GetUnitClassification () == WoWUnitClassification.RareElite && target.MaxHealth > Me.MaxHealth * 1.5)
				return true;
			if (target.GetUnitClassification () == WoWUnitClassification.Elite && target.MaxHealth > Me.MaxHealth * 3)
				return true;
			if (target.GetUnitClassification () == WoWUnitClassification.WorldBoss && target.MaxHealth >= Me.MaxHealth)
				return true;
			if (target.GetUnitClassification () == WoWUnitClassification.Normal && target.MaxHealth > Me.MaxHealth * 3)
				return true;
			return false;
		}

		#endregion
	}
}
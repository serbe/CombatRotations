using System;
using System.Linq;
using Newtonsoft.Json;
using ReBot.API;
using Geometry;

namespace ReBot.Warlock
{
	public abstract class SerbWarlock : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int Ttd = 10;
		[JsonProperty ("Use multitarget")]
		public bool Multitarget = true;
		[JsonProperty ("AOE")]
		public bool Aoe = true;
		[JsonProperty ("Use GCD")]
		public bool Gcd = true;

		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;
		public DateTime StartBattle;
		public double HandRange;
		public DateTime StartHandTime;
		public bool HandInFlight = false;
		//		public DateTime StartSleepTime;
		public bool InCombat;
		public UnitObject CycleTarget;
		public Int32 OraliusWhisperingCrystalId = 118922;
		public Int32 CrystalOfInsanityId = 86569;

		public bool IsSolo {
			get {
				return Group.GetNumGroupMembers () != 1;
			}
		}

		public bool InRaid {
			get {
				return API.MapInfo.Type == MapType.Raid;
			}
		}

		public bool InInstance {
			get {
				return API.MapInfo.Type == MapType.Instance;
			}
		}

		public bool InArena {
			get {
				return API.MapInfo.Type == MapType.Arena;
			}
		}

		public bool InBg {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		public bool IsBoss (UnitObject u = null)
		{
			u = u ?? Target;
			return(u.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || u.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsPlayer;
		}

		public bool IsElite (UnitObject u = null)
		{
			u = u ?? Target;	
			return u.IsElite ();
		}

		public bool IsInEnrage (UnitObject o = null)
		{
			o = o ?? Target;
			if (o.HasAura ("Enrage") || o.HasAura ("Berserker Rage") || o.HasAura ("Demonic Enrage") || o.HasAura ("Aspect of Thekal") || o.HasAura ("Charge Rage") || o.HasAura ("Electric Spur") || o.HasAura ("Cornered and Enraged!") || o.HasAura ("Draconic Rage") || o.HasAura ("Brood Rage") || o.HasAura ("Determination") || o.HasAura ("Charged Fists") || o.HasAura ("Beatdown") || o.HasAura ("Consuming Bite") || o.HasAura ("Delirious") || o.HasAura ("Angry") || o.HasAura ("Blood Rage") || o.HasAura ("Berserking Howl") || o.HasAura ("Bloody Rage") || o.HasAura ("Brewrific") || o.HasAura ("Desperate Rage") || o.HasAura ("Blood Crazed") || o.HasAura ("Combat Momentum") || o.HasAura ("Dire Rage") || o.HasAura ("Dominate Slave") || o.HasAura ("Blackrock Rabies") || o.HasAura ("Burning Rage") || o.HasAura ("Bloodletting Howl"))
				return true;
			return false;
		}

		public bool IsNotForDamage (UnitObject o = null)
		{
			o = o ?? Target;
			if (o.HasAura ("Fear") || o.HasAura ("Polymorph") || o.HasAura ("Gouge") || o.HasAura ("Paralysis") || o.HasAura ("Blind") || o.HasAura ("Hex"))
				return true;
			return false;
		}

		public double Mana { 
			get { 
				return Me.ManaFraction;
			}
		}

		public int Fury { 
			get { 
				return Me.GetPower (WoWPowerType.WarlockDemonicFury);
			}
		}

		public int Embers {
			get {
				return Me.GetPower (WoWPowerType.WarlockDestructionBurningEmbers);
			}
		}

		//		public double EnergyRegen {
		//			get {
		//				string activeRegen = API.ExecuteLua<string> ("inactiveRegen, activeRegen = GetPowerRegen(); return activeRegen");
		//				return Convert.ToDouble (activeRegen);
		//			}
		//		}

		// public bool TargettingMe { get { return Target.Target ==(UnitObject)Me; } }
		//		public double EnergyTimeToMax {
		//			get {
		//				return EnergyMax - Energy / EnergyRegen;
		//			}
		//		}


		public double Time {
			get {
				TimeSpan combatTime = DateTime.Now.Subtract (StartBattle);
				return combatTime.TotalSeconds;
			}
		}

		public double SpellHaste {
			get {
				double haste = API.ExecuteLua<double> ("return GetCombatRating(CR_HASTE_SPELL);");
				if (haste == 0)
					haste = 0.00001;
				return haste;
			}
		}

		//		public double TimeToRegen (double e)
		//		{
		//			if (e > Energy)
		//				return (e - Energy) / EnergyRegen;
		//			else
		//				return 0;
		//		}

		//		public double TimeToStartBattle {
		//			get {
		//				return API.ExecuteLua<double> ("return GetBattlefieldInstanceRunTime()") / 1000;
		//			}
		//		}

		//		public int EnemyInRange (int range)
		//		{
		//			int x = 0;
		//			foreach (UnitObject mob in API.CollectUnits(range)) {
		//				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead) {
		//					x++;
		//				}
		//			}
		//			return x;
		//		}

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable) {
					x++;
				}
			}
			return x;
		}

		public bool IncapacitatedInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead && mob.IsAttackable && IsNotForDamage (mob)) {
					x++;
				}
			}
			return x > 0;
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public double CooldownById (Int32 i)
		{ 
			return SpellCooldown (i) < 0 ? 0 : SpellCooldown (i);
		}

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double CastTimeSB {
			get {
				return API.ExecuteLua<double> ("local _, _, _, castTime, _, _ = GetSpellInfo(686); return castTime;");
			}
		}

		public double HandTravelTime {
			get {
				return Target.CombatRange * 2 / 40;
			}
		}

		public double HandFlightTime {
			get {
				TimeSpan HandTime = DateTime.Now.Subtract (StartHandTime);
				return HandTime.TotalSeconds;
			}
		}


		//		public bool IsCatForm ()
		//		{
		//			return (HasAura ("Cat Form") || HasAura ("Claws of Shirvallah"));
		//		}

		//		public  bool CatForm ()
		//		{
		//			return CastSelf ("Cat Form", () => !Me.HasAura ("Claws of Shirvallah") && !Me.HasAura ("Cat Form"));
		//		}

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}

		public  bool DarkIntent ()
		{
			return CastSelf ("Dark Intent", () => Usable ("Dark Intent") && !Me.HasAura ("Dark Intent") && !Me.HasAura ("Mind Quickening") && !Me.HasAura ("Swiftblade's Cunning") && !Me.HasAura ("Windflurry") && !Me.HasAura ("Arcane Brilliance"));
		}

		public  bool SummonPet ()
		{
//			if (Cast ("Felguard", () => !HasSpell ("Demonic Servitude") && (!HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
//				return true;
//			if (Cast ("Wrathguard", () => !HasSpell ("Demonic Servitude") && (HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
//				return true;
			return false;
		}

		public  bool BloodFury (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public  bool Berserking (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Berserking", () => Usable ("Berserking") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public  bool ArcaneTorrent (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool MannorothsFury (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Mannoroth's Fury", () => Usable ("Mannoroth's Fury") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2));
		}

		public bool DarkSoul (UnitObject u = null)
		{
			u = u ?? Target;
			if (CastSelf ("Dark Soul: Misery", () => Usable ("Dark Soul: Misery") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2)))
				return true; 
			if (CastSelf ("Dark Soul: Instability", () => Usable ("Dark Soul: Instability") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2)))
				return true; 
			if (CastSelf ("Dark Soul: Knowledge", () => Usable ("Dark Soul: Knowledge") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2)))
				return true; 
			return false;
		}

		public bool ImpSwarm (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Imp Swarm", () => Usable ("Imp Swarm") && u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || EnemyInRange (10) > 2), u);
		}

		public  bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public  bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityId) && !Me.HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0)
				return API.UseItem (CrystalOfInsanityId);
			return false;
		}

		public  bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalId) && !Me.HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0)
				return API.UseItem (OraliusWhisperingCrystalId);
			return false;
		}

		public bool LifeTap ()
		{
			return CastSelf ("Life Tap", () => Usable ("Life Tap") && Mana < 0.6);
		}

		public bool Felstorm (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Felstorm", () => Usable ("Felstorm") && !Me.Pet.IsDead && Vector3.Distance (u.Position, Me.Pet.Position) <= 8, u);
		}

		public bool Wrathstorm (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Wrathstorm", () => Usable ("Wrathstorm") && !Me.Pet.IsDead && Vector3.Distance (u.Position, Me.Pet.Position) <= 8, u);
		}

		public bool MortalCleave (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Mortal Cleave", () => Usable ("Mortal Cleave") && !Me.Pet.IsDead, u);
		}

		public bool HandofGuldan (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Hand of Gul'dan", () => Usable ("Hand of Gul'dan") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool GrimoireofService (UnitObject u = null)
		{
			u = u ?? Target;
			return CastSelf ("Grimoire of Service", () => Usable ("Grimoire of Service") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool SummonDoomguard (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Summon Doomguard", () => Usable ("Summon Doomguard") && u.IsInLoS && u.CombatRange <= 40 && (IsBoss (u) || IsPlayer (u)), u);
		}

		public bool SummonInfernal (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Summon Infernal", () => Usable ("Summon Infernal") && u.IsInLoS && u.CombatRange <= 40 && u.Level >= Me.Level - 10, u);
		}

		public bool KiljaedensCunning ()
		{
			return CastSelf ("Kil'jaeden's Cunning", () => Usable ("Kil'jaeden's Cunning"));
		}

		public bool Cataclysm (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Cataclysm", u.Position, () => Usable ("Cataclysm") && u.IsInLoS && u.CombatRange <= 40);
		}

		public bool ImmolationAura (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Immolation Aura", () => Usable ("Immolation Aura") && Me.HasAura ("Metamorphosis") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool ShadowBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Shadow Bolt", () => Usable ("Shadow Bolt") && u.IsInLoS && u.CombatRange <= 40 && ((!Me.HasAura ("Metamorphosis") && Mana >= 5.5) || (Me.HasAura ("Metamorphosis") && Fury >= 40)), u);
		}

		public bool Doom (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Doom", () => Usable ("Doom") && u.IsInLoS && u.CombatRange <= 40 && Me.HasAura ("Metamorphosis") && Fury >= 60, u);
		}

		public bool Corruption (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Corruption", () => Usable ("Corruption") && u.IsInLoS && u.CombatRange <= 40, u);
		}

		public bool Metamorphosis ()
		{
			return CastSelf ("Metamorphosis", () => Usable ("Metamorphosis"));
		}

		public bool TouchofChaos (UnitObject u = null)
		{
			u = u ?? Target;
			return Cast ("Touch of Chaos", () => Usable ("Touch of Chaos") && u.IsInLoS && u.CombatRange <= 40 && Me.HasAura ("Metamorphosis") && Fury >= 40, u);
		}


		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}

		//		public bool  (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Cast ("", () => Usable ("") && u.IsInLoS && u.CombatRange <= 40, u);
		//		}
	}
}


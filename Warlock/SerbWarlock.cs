using System;
using Newtonsoft.Json;
using ReBot.API;
using Geometry;
using System.Collections.Generic;

namespace ReBot
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

		// Check

		public bool C (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (Cast (s, u))
				return true;
			API.Print ("False Cast " + s + " with " + u.CombatRange + " range");
			return false;
		}

		public bool CS (string s)
		{
			if (CastSelf (s))
				return true;
			API.Print ("False CastSelf " + s);
			return false;
		}

		public bool COT (string s, UnitObject u = null)
		{
			u = u ?? Target;
			if (CastOnTerrain (s, u.Position))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range");
			return false;
		}

		public bool COTPD (string s, UnitObject u = null, int p = 800)
		{
			u = u ?? Target;
			if (CastOnTerrainPreventDouble (s, u.Position, null, p))
				return true;
			API.Print ("False CastOnTerrain " + s + " with " + u.CombatRange + " range");
			return false;
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

		public bool Usable (string s)
		{ 
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public bool Range (int r, UnitObject u = null, int l = 0)
		{
			u = u ?? Target;
			if (l != 0)
				return u.IsInLoS && u.CombatRange <= r && u.CombatRange >= l;
			return u.IsInLoS && u.CombatRange <= r;
		}

		public bool Range (UnitObject u = null)
		{
			u = u ?? Target;
			return u.IsInCombatRangeAndLoS;
		}


		public bool Danger (UnitObject u = null, int r = 0, int e = 2)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsElite (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		public bool DangerBoss (UnitObject u = null, int r = 0, int e = 6)
		{
			u = u ?? Target;
			if (r != 0)
				return Range (r, u) && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e);
			return u.IsInCombatRangeAndLoS && (IsBoss (u) || IsPlayer (u) || ActiveEnemies (10) > e);
		}

		// Get

		public List<UnitObject> Enemy {
			get {
				var targets = Adds;
				targets.Add (Target);
				return targets;
			}
		}

		public int ActiveEnemies (int r)
		{
			int x = 0;
			foreach (UnitObject u in API.CollectUnits(r)) {
				if ((u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable && u.InCombat) {
					x++;
				}
			}
			return x;
		}

		public int ActiveEnemiesWithTarget (int r, UnitObject t = null)
		{
			t = t ?? Target;
			int x = 0;
			foreach (UnitObject u in API.CollectUnits(45)) {
				if (Vector3.Distance (t.Position, u.Position) <= r && (u.IsEnemy || Me.Target == u) && !u.IsDead && u.IsAttackable) {
					x++;
				}
			}
			return x;
		}

		public double Health (UnitObject u = null)
		{
			u = u ?? Target;
			return u.HealthFraction;
		}

		public double Mana (UnitObject u = null)
		{
			u = u ?? Me;
			return u.ManaFraction;
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

		public double TimeToDie (UnitObject u = null)
		{
			u = u ?? Target;
			return u.Health / Ttd;
		}



		// Combo

		public bool SummonPet ()
		{
			//			if (Cast ("Felguard", () => !HasSpell ("Demonic Servitude") && (!HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
			//				return true;
			//			if (Cast ("Wrathguard", () => !HasSpell ("Demonic Servitude") && (HasSpell ("Grimoire of Supremacy") && (!HasSpell ("Grimoire of Service") || !Me.HasAura ("Grimoire of Service")))))
			//				return true;
			return false;
		}


		// Spell

		public bool DarkIntent ()
		{
			return Usable ("Dark Intent") && !Me.HasAura ("Dark Intent") && !Me.HasAura ("Mind Quickening") && !Me.HasAura ("Swiftblade's Cunning") && !Me.HasAura ("Windflurry") && !Me.HasAura ("Arcane Brilliance") && CS ("Dark Intent");
		}

		public bool BloodFury ()
		{
			return Usable ("Blood Fury") && Danger () && CS ("Blood Fury");
		}

		public bool Berserking ()
		{
			return Usable ("Berserking") && Danger () && CS ("Berserking");
		}

		public bool ArcaneTorrent ()
		{
			return Usable ("Arcane Torrent") && Danger () && CS ("Arcane Torrent");
		}

		public bool MannorothsFury ()
		{
			return Usable ("Mannoroth's Fury") && Danger () && CS ("Mannoroth's Fury");
		}

		public bool DarkSoul ()
		{
			if (CastSelf ("Dark Soul: Misery", () => Usable ("Dark Soul: Misery") && Danger ()))
				return true; 
			if (CastSelf ("Dark Soul: Instability", () => Usable ("Dark Soul: Instability") && Danger ()))
				return true; 
			if (CastSelf ("Dark Soul: Knowledge", () => Usable ("Dark Soul: Knowledge") && Danger ()))
				return true; 
			return false;
		}

		public bool ImpSwarm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Imp Swarm") && Danger () && C ("Imp Swarm", u);
		}

		public bool LifeTap ()
		{
			return Usable ("Life Tap") && Mana (Me) < 0.8 && Health (Me) > 0.3 && CS ("Life Tap");
		}

		public bool Felstorm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Felstorm") && !Me.Pet.IsDead && Vector3.Distance (u.Position, Me.Pet.Position) <= 8 && C ("Felstorm", u);
		}

		public bool Wrathstorm (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Wrathstorm") && !Me.Pet.IsDead && Vector3.Distance (u.Position, Me.Pet.Position) <= 8 && C ("Wrathstorm", u);
		}

		public bool MortalCleave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Mortal Cleave") && !Me.Pet.IsDead && C ("Mortal Cleave", u);
		}

		public bool HandofGuldan (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Hand of Gul'dan") && Range (40, u) && C ("Hand of Gul'dan", u);
		}

		public bool GrimoireofService (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Grimoire of Service") && Range (40, u) && CS ("Grimoire of Service");
		}

		public bool SummonDoomguard (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Summon Doomguard") && Range (40, u) && DangerBoss (u) && C ("Summon Doomguard", u);
		}

		public bool SummonInfernal (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Summon Infernal") && Range (40, u) && u.Level >= Me.Level - 10 && C ("Summon Infernal", u);
		}

		public bool KiljaedensCunning ()
		{
			return Usable ("Kil'jaeden's Cunning") && CS ("Kil'jaeden's Cunning");
		}

		public bool Cataclysm (UnitObject u = null)
		{
			u = u ?? Target;
			return CastOnTerrain ("Cataclysm", u.Position, () => Usable ("Cataclysm") && Range (40, u));
		}

		public bool ImmolationAura ()
		{
			return Usable ("Immolation Aura") && Me.HasAura ("Metamorphosis") && !Me.HasAura ("Immolation Aura") && C ("Immolation Aura");
		}

		public bool ShadowBolt (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Shadow Bolt") && Range (40, u) && ((!Me.HasAura ("Metamorphosis") && Mana () >= 0.055) || (Me.HasAura ("Metamorphosis") && Fury >= 40)) && C ("Shadow Bolt", u);
		}

		public bool Doom (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Doom") && Range (40, u) && Me.HasAura ("Metamorphosis") && Fury >= 60 && C ("Doom", u);
		}

		public bool Corruption (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Corruption") && Range (40, u) && C ("Corruption", u);
		}

		public bool Metamorphosis ()
		{
			return Usable ("Metamorphosis") && CS ("Metamorphosis");
		}

		public bool TouchofChaos (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Touch of Chaos") && Range (40, u) && Me.HasAura ("Metamorphosis") && Fury >= 40 && C ("Touch of Chaos", u);
		}


		public bool ChaosWave (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Chaos Wave") && Range (40, u) && Me.HasAura ("Metamorphosis") && Fury >= 80 && C ("Chaos Wave", u);
		}

		public bool SoulFire (UnitObject u = null)
		{
			u = u ?? Target;
			return Usable ("Soul Fire") && Range (40, u) && !Me.IsMoving && C ("Soul Fire", u);
		}

		public bool Hellfire ()
		{
			return Usable ("Hellfire") && !Me.HasAura ("Metamorphosis") && !Me.HasAura ("Hellfire") && C ("Hellfire");
		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		//		public bool (UnitObject u = null)
		//		{
		//			u = u ?? Target;
		//			return Usable ("") && Range(40,u) && C ("", u);
		//		}

		// Items

		public bool Healthstone ()
		{
			return API.HasItem (5512) && API.ItemCooldown (5512) == 0 && API.UseItem (5512);
		}

		public bool CrystalOfInsanity ()
		{
			return !InArena && API.HasItem (CrystalOfInsanityId) && !Me.HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityId) == 0 && API.UseItem (CrystalOfInsanityId);
		}

		public bool OraliusWhisperingCrystal ()
		{
			return API.HasItem (OraliusWhisperingCrystalId) && !Me.HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalId) == 0 && API.UseItem (OraliusWhisperingCrystalId);
		}
	}
}


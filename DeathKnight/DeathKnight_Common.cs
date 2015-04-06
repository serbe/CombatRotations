﻿using System;
using ReBot.API;

namespace ReBot
{
	public abstract class DeathKnight : CombatRotation
	{
		[JsonProperty ("TimeToDie (MaxHealth / TTD)")]
		public int TTD = 10;

		public UnitObject CycleTarget;
		public Int32 OraliusWhisperingCrystalID = 118922;
		public Int32 CrystalOfInsanityID = 86569;
		public int BossHealthPercentage = 500;
		public int BossLevelIncrease = 5;

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

		public bool InBG {
			get {
				return API.MapInfo.Type == MapType.PvP;
			}
		}

		public bool IsBoss (UnitObject o)
		{
			return(o.MaxHealth >= Me.MaxHealth * (BossHealthPercentage / 100f)) || o.Level >= Me.Level + BossLevelIncrease;
		}

		public bool IsPlayer {
			get { 
				return Target.IsPlayer;
			}
		}

		public bool IsElite { 
			get {
				return Target.IsElite ();
			}
		}

		public int EnemyInRange (int range)
		{
			int x = 0;
			foreach (UnitObject mob in API.CollectUnits(range)) {
				if ((mob.IsEnemy || Me.Target == mob) && !mob.IsDead) {
					x++;
				}
			}
			return x;
		}

		public int RunicPower { 
			get { 
				return Me.GetPower (WoWPowerType.RunicPower);
			}
		}

		public bool HasBlood {
			get { 
				return (Me.Runes (RuneType.Blood) > 0);
			}
		}

		public bool HasUnholy {
			get { 
				return (Me.Runes (RuneType.Unholy) > 0);
			}
		}

		public bool HasFrost {
			get { 
				return (Me.Runes (RuneType.Frost) > 0);
			}
		}

		public bool HasDeath {
			get { 
				return (Me.Runes (RuneType.Death) > 0);
			}
		}

		public int Blood { 
			get {
				return Me.Runes (RuneType.Blood);
			}
		}

		public int Frost { 
			get { 
				return Me.Runes (RuneType.Frost);
			}
		}

		public int Unholy { 
			get { 
				return Me.Runes (RuneType.Unholy);
			}
		}

		public int Death { 
			get { 
				return Me.Runes (RuneType.Death);
			}
		}

		public int BloodCharge { 
			get { 
				return AuraStackCount ("Blood Charge");
			}
		}

		public bool HasFrostDisease (UnitObject o)
		{
			return o.HasAura ("Frost Fever", true);
		}

		public bool HasBloodDisease (UnitObject o)
		{
			return o.HasAura ("Blood Plague", true);
		}

		public bool HasDisease (UnitObject o)
		{
			return HasBloodDisease (o) && HasFrostDisease (o);
		}

		public int Disease (UnitObject o)
		{
			var result = 0;
			if (HasBloodDisease (o))
				result = result + 1;
			if (HasFrostDisease (o))
				result = result + 1;
			return result;
		}

		public double FrostDiseaseRemaining (UnitObject o)
		{
			if (HasFrostDisease (o))
				return o.AuraTimeRemaining ("Frost Fever", true);
			else
				return 0;
		}

		public double BloodDiseaseRemaining (UnitObject o)
		{
			if (HasBloodDisease (o))
				return o.AuraTimeRemaining ("Blood Plague", true);
			else
				return 0;
		}

		public double MinDisease (UnitObject o)
		{
			if (FrostDiseaseRemaining (o) < BloodDiseaseRemaining (o))
				return FrostDiseaseRemaining (o);
			else
				return BloodDiseaseRemaining (o);
		}

		public double Cooldown (string s)
		{ 
			return SpellCooldown (s) < 0 ? 0 : SpellCooldown (s);
		}

		public bool Usable (string s)
		{ 
			// Analysis disable once CompareOfFloatsByEqualityOperator
			return HasSpell (s) && Cooldown (s) == 0;
		}

		public double TimeToDie (UnitObject o)
		{
			return o.Health / TTD;
		}

		public DeathKnight ()
		{
		}

		public virtual bool HornofWinter ()
		{
			return CastSelf ("Horn of Winter", () => Usable ("Horn of Winter") && !HasAura ("Horn of Winter") && !HasAura ("Battle Shout"));
		}

		public virtual bool RaiseDead ()
		{
			return CastSelf ("Raise Dead", () => Usable ("Raise Dead") && !Me.HasAlivePet);
		}

		public virtual bool Healthstone ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (5512) && API.ItemCooldown (5512) == 0)
				return API.UseItem (5512);
			return false;
		}

		public virtual bool CrystalOfInsanity ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (!InArena && API.HasItem (CrystalOfInsanityID) && !Me.HasAura ("Visions of Insanity") && API.ItemCooldown (CrystalOfInsanityID) == 0)
				return API.UseItem (CrystalOfInsanityID);
			return false;
		}

		public virtual bool OraliusWhisperingCrystal ()
		{
			// Analysis disable once CompareOfFloatsByEqualityOperator
			if (API.HasItem (OraliusWhisperingCrystalID) && !Me.HasAura ("Whispers of Insanity") && API.ItemCooldown (OraliusWhisperingCrystalID) == 0)
				return API.UseItem (OraliusWhisperingCrystalID);
			return false;
		}

		public virtual bool BloodFury ()
		{
			return CastSelf ("Blood Fury", () => Usable ("Blood Fury") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool Berserking ()
		{
			return CastSelf ("Berserking", () => Usable ("Berserking") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool ArcaneTorrent ()
		{
			return CastSelf ("Arcane Torrent", () => Usable ("Arcane Torrent") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool AntimagicShell ()
		{
			return CastSelf ("Anti-Magic Shell", () => Usable ("Anti-Magic Shell"));
		}

		public virtual bool UnholyBlight ()
		{
			return CastSelf ("Unholy Blight", () => Usable ("Unholy Blight") && Target.IsInLoS && Target.CombatRange <= 10);
		}

		public virtual bool Defile ()
		{
			return CastOnTerrain ("Defile", Target.Position, () => Usable ("Defile") && (Me.HasAura ("Crimson Scourge") || (HasUnholy || HasDeath)) && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool BloodBoil ()
		{
			return CastSelf ("Blood Boil", () => Usable ("Blood Boil") && (Me.HasAura ("Crimson Scourge") || (HasBlood || HasDeath)) && Target.IsInLoS && Target.CombatRange <= 10);
		}

		public virtual bool SummonGargoyle ()
		{
			return Cast ("Summon Gargoyle", () => Usable ("Summon Gargoyle") && Target.IsInLoS && Target.CombatRange <= 30 && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}

		public virtual bool DarkTransformation ()
		{
			return Cast ("Dark Transformation", () => Usable ("Dark Transformation") && Me.HasAlivePet && (HasSpell ("Enhanced Dark Transformation") || (HasDeath || HasUnholy)));
		}

		public virtual bool BloodTap ()
		{
			return CastSelf ("Blood Tap", () => Usable ("Blood Tap") && BloodCharge >= 5);
		}

		public virtual bool DeathandDecay ()
		{
			return CastOnTerrain ("Death and Decay", Target.Position, () => Usable ("Death and Decay") && (Me.HasAura ("Crimson Scourge") || (HasUnholy || HasDeath)) && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool ScourgeStrike ()
		{
			return Cast ("Scourge Strike", () => Usable ("Scourge Strike") && (HasUnholy || HasDeath));
		}

		public virtual bool DeathCoil ()
		{
			return Cast ("Death Coil", () => Usable ("Death Coil") && (Me.HasAura ("Sudden Doom") || RunicPower >= 30) && Target.IsInLoS && Target.CombatRange <= 40);
		}

		public virtual bool IcyTouch ()
		{
			return Cast ("Icy Touch", () => Usable ("Icy Touch") && (HasFrost || HasDeath) && Target.IsInLoS && Target.CombatRange <= 30);
		}

		public virtual bool PlagueLeech ()
		{
			return Cast ("Plague Leech", () => Usable ("Plague Leech") && Target.HasAura ("Frost Fever", true) && Target.HasAura ("Blood Plague", true));
		}

		public virtual bool EmpowerRuneWeapon ()
		{
			return Cast ("Empower Rune Weapon", () => Usable ("Empower Rune Weapon") && Target.IsInCombatRangeAndLoS && (IsElite || IsPlayer || EnemyInRange (10) > 2));
		}




		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}

		public virtual bool SoulReaper ()
		{
			return Cast ("Soul Reaper", () => Usable ("Soul Reaper") && (HasUnholy || HasDeath));
		}


	}
}


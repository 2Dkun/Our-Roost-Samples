using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum skillType {OFFENSIVE, SUPPORT, SPAWN};
public enum stat {ATK, DEF, MATK, MDEF, SPD, NULL};
public enum element {NEUTRAL, FIRE, WATER, EARTH, LIGHT, DARK};

[System.Serializable]
public class Skill {

    // ---Fields---
    // Main variables
    private skillType type;
    private string atkName; // Name of the attack
    private string description; // Description of attack
    private element atkElement; // Holds elemental damage
    private int mpCost; // Holds amount of mp to use
    private int range; // Holds the amount of targets the attack can have
    private float startDist; // Distance the user should be from the target before the attack
    private float endDist; // Distance the user should be from the target after the attack
    private SpriteAnimation anim; // Handles animation of attack

    // Offensive skill variables
    private int basePower; // Base power of the attack
    private bool isMagical; // Determines if the attack is magical or physical
    private int effectChance; // Holds the chances of secondary effect happening (uses support skill variable)
    // maybe a hitstun variable that determine how long combat time is?

    // Support skill variable
    private battleType supprtType;
    private status statusMod;
    private int scalar; // Heal(HP)/Cure(MP): amount to be healed, Status: cure(1) or inflict(-1), Boost: levels of boosts
    private stat statBoost; // Determines which stat to boost if any
    private bool selfTarget; // Determines if you can only use it on yourself
    private bool targetAlly;

    // Spawn skill variable
    private List<Foe> spawnSet;

    [System.NonSerialized]
    private AudioClip sfx;
    private string sfxPath;

    // ---Constructor---
    public Skill(skillType type, string atkName, string description, element atkElement, int mpCost, 
        int range, float start, float end, SpriteAnimation anim, string sfxPath){
        // Set main variables
        this.type = type;
        this.atkName = atkName;
        this.description = description;
        this.atkElement = atkElement;
        this.mpCost = mpCost;
        this.range = range;

        // Set animation variables
        this.startDist = start;
        this.endDist = end;
        this.anim = anim;

        // Set sfx
        this.sfxPath = sfxPath;
        if(sfxPath != "null")
            sfx = Resources.Load(sfxPath) as AudioClip; 

        return;
    }

    // ---Access Functions---
    // Get main variables
    public skillType getType()  { return this.type; }
    public string getName()     { return this.atkName; }
    public string getDes()      { return this.description; }
    public element getElement() { return this.atkElement; }
    public int getMpCost()      { return this.mpCost; }
    public int getRange()       { return this.range; }
    public float getStartDist() { return this.startDist; }
    public float getEndDist()   { return this.endDist; }
    public AudioClip getSFX()   { return this.sfx; }

    // Get offensive skill variables
    public int getPower()   { return this.basePower; }
    public bool getMagical(){ return this.isMagical; }
    public int getChance()  { return this.effectChance; }

    // Get support skill variables
    public battleType getSupport()  { return this.supprtType; }
    public status getStatus()       { return this.statusMod; }
    public int getScalar()          { return this.scalar; }
    public stat getStatBoost()      { return this.statBoost; }
    public bool isSelfTarget()      { return this.selfTarget; }
    public bool isTargetAlly()      { return this.targetAlly; }

    // Returns the damage that would be dealt
    public int calcDamage(bool defended, int userSpot, int targetSpot, float atk, float def, float atkMul, float defMul){

        // User deals more damage the further in front they are
        if (userSpot == 0) 
            atk *= 1.1f;    
        else if (userSpot == 3 || userSpot == 4)
            atk *= 0.9f;
        // Target takes more damage the further in front they are
        if (targetSpot == 0) 
            def *= 0.9f;
        else if (targetSpot == 3 || targetSpot == 4)
            def *= 1.1f;

        // Ally takes less damaged if they defended
        if (defended)
            def *= 1.25f;

        // Calculate damage
        int damage = (int)(this.basePower * (atk / def) * atkMul / defMul);
        // Don't deal negative damage
        if (damage < 0) 
            damage = 0;
        return damage;
    }
    // Determine when to display damage
    public bool dealDamage(int curFrame) { return this.anim.isEvent(curFrame); }

    public SpriteAnimation getAnim() { return this.anim; }

    public Foe spawnDude(){
        int chance = 100 / this.spawnSet.Count;
        for (int i = 0; i < this.spawnSet.Count; i++) {
            if (chance * (i+1) > Random.Range(1, 101)) {
                return this.spawnSet[i].copy();
            }
        }
        return this.spawnSet[0].copy();
    }

    // ---Manipulation Procedures---
    // Get next frame of attack
    public Sprite playSkill() { return this.anim.playAnim(); }
    public bool skillIsDone() { return this.anim.isDone(); }

    public void setAttack(int basePower, bool isMagical, int effectChance){
        if (this.type == skillType.OFFENSIVE) {
            this.basePower = basePower;
            this.isMagical = isMagical;
            this.effectChance = effectChance;
        }
        return;
    }

    public void setSupport(battleType supportType, status statusMod, stat statBoost, int scalar, bool selfTarget, bool targetAlly){
        this.supprtType = supportType;
        this.statusMod = statusMod;
        this.statBoost = statBoost;
        this.scalar = scalar;
        this.selfTarget = selfTarget;
        this.targetAlly = targetAlly;
        return;
    }

    public void setSpawn(List<Foe> dudes){
        this.spawnSet = dudes;
    }

    // ---Other Functions---
    // Determines if two attacks are the same
    public bool equals(Skill A){
        if (this.atkName != A.getName())
            return false;
        if (this.basePower != A.getPower())
            return false;
        if (this.isMagical != A.getMagical())
            return false;
        return true;
    }

    public void loadAnim(){
        this.anim.loadSprites();
        if(sfxPath != "null")
            sfx = Resources.Load(sfxPath) as AudioClip;
    }
}

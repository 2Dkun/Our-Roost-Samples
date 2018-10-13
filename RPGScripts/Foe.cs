using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AItype {RANDOM, LOWEST, HIGHEST, CUSTOM};

public class Foe {

    // ---Fields---
    private int curHP;
    private int curMP;
    private string dude;
    private bool[] curStatus = new bool[8];
    private int[] stats = new int[8];
    private int[] curBoosts = new int[5];
    private List<Skill> skillList;
    private int[] skillOdds;
    private AItype[] skillTargetAI;
    private HitBox[] hurtbox;
    private Sprite face;

    private Item[] drops = new Item[5];
    private int[] dropRate = new int[5];

    // Animation variables
    private SpriteAnimation idle; // Idle animation
    private SpriteAnimation idleAct; // Emote for when the combatant is in idle for a while
    private SpriteAnimation confused; // Confused animation
    private SpriteAnimation walk; // Walk animation
    private SpriteAnimation hurt; // Hurt animation

    // Combat phase variables
    private float xVel, yVel, gravity;
    private int hitstun;

    // ---Constructor---
    public Foe(string name, int[] stats, List<Skill> skillList, int[] skillOdds, AItype[] skillTargetAI, Item[] drops, int[] dropRate, HitBox[] hurtbox, float gravity){
        this.dude = name;
        this.stats = (int[])stats.Clone();
        this.curHP = this.stats[0];
        this.curMP = this.stats[1];
        this.skillList = skillList;
        this.skillOdds = (int[])skillOdds.Clone();
        this.skillTargetAI = skillTargetAI;
        this.drops = drops;
        this.dropRate = (int[])dropRate.Clone();
        this.hurtbox = new HitBox[hurtbox.Length];
        for (int i = 0; i < hurtbox.Length; i++) {
            this.hurtbox[i] = hurtbox[i].copy();
        }
        this.gravity = gravity;
    }

    // ---Access Functions---
    public string getName() { return this.dude; }

    // Return face icon of foe
    public Sprite getFace() { return this.face; }

    // Return stats
    public int getCurHP() { return this.curHP; }
    public int getCurMP() { return this.curMP; }
    public int getHP()  { return this.stats[0]; }
    public int getMP()  { return this.stats[1]; }
    public int getATK() { return this.stats[2]; }
    public int getDEF() { return this.stats[3]; }
    public int getMATK(){ return this.stats[4]; }
    public int getMDEF(){ return this.stats[5]; }
    public int getSPD() { return this.stats[6]; }

    // Return statuses
    public bool isKO()      { return this.curStatus[0]; }
    public bool isPoison()  { return this.curStatus[1]; }
    public bool isBleed()   { return this.curStatus[2]; }
    public bool isBurn()    { return this.curStatus[3]; }
    public bool isPara()    { return this.curStatus[4]; }
    public bool isSleep()   { return this.curStatus[5]; }
    public bool isFreeze()  { return this.curStatus[6]; }
    public bool isRegen()   { return this.curStatus[7]; }

    // Return stat boosts
    public int getAtkBoost()    { return this.curBoosts[0]; }
    public int getDefBoost()    { return this.curBoosts[1]; }
    public int getMatkBoost()   { return this.curBoosts[2]; }
    public int getMdefBoost()   { return this.curBoosts[3]; }
    public int getSpdBoost()    { return this.curBoosts[4]; }

    public HitBox[] getHurtBoxes() { return this.hurtbox; }
    public bool inHitstun() { return this.hitstun >= Time.frameCount; }

    public List<Skill> getSkillList() { return this.skillList; }

    // Returns a skill chosen from the given odds
    public Skill getSkill(){
        int RNG = (int)Random.Range(1, 101);
        int sum = 0;
        for(int i = 0; i < skillList.Count; i++){
            if (skillOdds[i] + sum >= RNG) {
                return skillList[i];
            }
            else
                sum += skillOdds[i];
        }
        // Return first skill if skillOdds wasn't used properly
        return skillList[0];
    }
    // Returns a target for an attack based off of AI
    public int getTarget(int skillIndex, int[] hpList){
        switch (skillTargetAI[skillIndex]) {
            case AItype.HIGHEST:
                return selectHighestTarget(hpList);
            case AItype.LOWEST:
                return selectLowestTarget(hpList);
            case AItype.RANDOM:
                return selectRandomTarget(hpList);
            default:
                return -1;
        }
    }

    // Enemy gives the average of its stats as exp
    public int getEXP(){
        int exp = 0;
        for (int i = 0; i < this.stats.Length; i++) {
            exp += stats[i];
        }
        return exp / this.stats.Length; 
    }
    // Enemy drops items based on drop rate
    public Item[] getDrops(){
        Item[] loot = new Item[5];
        int itemIndex = 0;
        for (int i = 0; i < this.drops.Length; i++) {
            int RNG = (int)Random.Range(1, 101);
            if (drops[i] != null && this.dropRate[i] > RNG) {
                loot[itemIndex] = this.drops[i].copy();
                itemIndex += 1;
            }
        }
        return loot;
    }

    // ---Manipulation Procedures---
    // Adjust hp and mp stats
    public void adjustHP(int change){ this.curHP = limitValue(this.curHP + change, 0, getHP()); return; }
    public void adjustMP(int change){ this.curMP = limitValue(this.curMP + change, 0, getMP()); return; }

    // Set statuses
    public void setKO(bool isTrue)      { this.curStatus[0] = isTrue; return; }
    public void setPoison(bool isTrue)  { this.curStatus[1] = isTrue; return; }
    public void setBleed(bool isTrue)   { this.curStatus[2] = isTrue; return; }
    public void setBurn(bool isTrue)    { this.curStatus[3] = isTrue; return; }
    public void setPara(bool isTrue)    { this.curStatus[4] = isTrue; return; }
    public void setSleep(bool isTrue)   { this.curStatus[5] = isTrue; return; }
    public void setFreeze(bool isTrue)  { this.curStatus[6] = isTrue; return; }
    public void setRegen(bool isTrue)   { this.curStatus[7] = isTrue; return; }
    public void setStatus(status s, bool isTrue)    { this.curStatus[(int)s] = isTrue; return; }

    // Set stat boosts
    private int limitValue(int value, int min, int max) {
        if (value > max)        return max;
        else if (value < min)   return min;
        else                    return value;
    }
    public void setAtkBoost(int change)         { this.curBoosts[0] = limitValue(this.curBoosts[0] + change, -4, 4); return; }
    public void setDefBoost(int change)         { this.curBoosts[1] = limitValue(this.curBoosts[1] + change, -4, 4); return; }
    public void setMatkBoost(int change)        { this.curBoosts[2] = limitValue(this.curBoosts[2] + change, -4, 4); return; }
    public void setMdefBoost(int change)        { this.curBoosts[3] = limitValue(this.curBoosts[3] + change, -4, 4); return; }
    public void setSpdBoost(int change)         { this.curBoosts[4] = limitValue(this.curBoosts[4] + change, -4, 4); return; }
    public void setBoost(stat s, int change)    { this.curBoosts[(int)s] = limitValue(this.curBoosts[(int)s] + change, -4, 4); return; }

    // Add an item drop for an enemy(MAX: 5)
    public void addDrop(Item drop, int rate){
        for (int i = 0; i < this.dropRate.Length; i++) {
            if (this.dropRate[i] <= 0) {
                this.drops[i] = drop;
                this.dropRate[i] = rate;
                break;
            }
        }
        return;
    }

    // Set animation variables
    public void setAnimations(Sprite face, SpriteAnimation idle, SpriteAnimation idleAct, SpriteAnimation confused, SpriteAnimation walk, SpriteAnimation hurt){
        this.face = face;
        this.idle = idle.copy();
        this.idleAct = idleAct.copy();
        this.confused = confused.copy();
        this.walk = walk.copy();
        this.hurt = hurt.copy();
        return;
    }

    public void setHitstun(int frames){ this.hitstun = Time.frameCount + frames; return; }

    // Play the idle animation with occasional emote
    private int idleCounter; // Keeps track of how many times idle animation has looped
    public Sprite playIdle(){
        if (this.idleCounter < 3) {
            Sprite frame = this.idle.playAnim();
            if (this.idle.isDone()) {
                this.idleCounter += 1;
            }
            return frame;
        }
        else {
            Sprite frame = this.idleAct.playAnim();
            if (this.idleAct.isDone()) {
                this.idleCounter = 0;
            }
            return frame;
        }
    }
    // Play confused animation
    public Sprite playConfused() { return this.confused.playAnim(); }
    // Play walk animation
    public Sprite playWalk() { return this.walk.playAnim(); }
    // Play hurt animation
    public Sprite playHurt() { return this.hurt.playAnim(); }

    // Set velocity 
    public void setXVel(float vel)  { this.xVel = vel; return; }
    public void setYVel(float vel)  { this.yVel = vel; return; }
    public void resetVel()          { this.xVel = 0; this.yVel = 0; return; }

    // ---Other Functions---
    // Treat this foe as a dead enemy
    public void killEnemy() {
        int[] nothing = { 0,0,0,0,0,0,0 };

        this.dude = "";
        this.stats = nothing;
        this.curHP = this.stats[0];
        this.curMP = this.stats[1];
        this.skillList = new List<Skill>();
        this.skillOdds = new int[0];
        this.skillTargetAI = new AItype[0];
        this.drops = new Item[0];
        this.dropRate = new int[0];
        this.hurtbox = new HitBox[0];
        this.gravity = 0;

    }
    // Returns a random target out of the targets with remaining hp 
    private int selectRandomTarget(int[] hpList){
        int RNG = (int)Random.Range(0, 5);
        while (hpList[RNG] <= 0) {
            RNG += 1;
            RNG %= 4;
        }
        return RNG;
    }
    // Returns a target out of the targets with the least hp 
    private int selectLowestTarget(int[] hpList){
        int min = 0;
        for (int i = 1; i < hpList.Length; i++) {
            if ((hpList[i] < hpList[min] && hpList[i] > 0)|| hpList[min] <= 0)
                min = i;
        }
        return min;
    }
    // Returns a target out of the targets with the most hp 
    private int selectHighestTarget(int[] hpList){
        int max = 0;
        for (int i = 0; i < hpList.Length; i++) {
            if (hpList[i] > hpList[max])
                max = i;
        }
        return max;
    }

    // Apply gravity to the player
    public void applyGravity(GameObject player, float minY){
        this.yVel -= this.gravity * Time.deltaTime;
        player.transform.Translate(player.transform.localScale.x * this.xVel * Time.deltaTime, this.yVel, 0);
        if (player.transform.localPosition.y <= minY) {
            player.transform.localPosition = new Vector2(player.transform.localPosition.x, minY);
            resetVel();
        }
    }

    // Makes a copy of the current foe
    public Foe copy(){
        Foe clone = new Foe(dude, stats, skillList, skillOdds, skillTargetAI, drops, dropRate, hurtbox, gravity);
        if(dude != "")
            clone.setAnimations(this.face, this.idle, this.idleAct, this.confused, this.walk, this.hurt);
        return clone;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum character {ANNIE, DUKE, FONS, NULL};

[System.Serializable]
public class Ally {

    // ---Fields---
    private character duck;
    private Fighter fighter; 
    private int curHP;
    private int curMP;
    private int curEX;
    private bool[] curStatus = new bool[8];
    private int[] stats = new int[7];
    private int[] curBoosts = new int[5];
    private List<Skill> skillList; 

    // Experience variables
    int level;
    int curExp;
    int maxExp;

    // Equipment
    Item head;
    Item torso;
    Item accessoryL;
    Item accessoryR;
    Item weaponL;
    Item weaponR;

    // Animation variables
    private SpriteAnimation idle; // Idle animation
    private SpriteAnimation idleAct; // Emote for when the combatant is in idle for a while
    private SpriteAnimation damaged; // Damaged animation
    private SpriteAnimation damagedAct; // Emote for when the combatant is in damaged for a while
    private SpriteAnimation confused; // Confused animation
    private SpriteAnimation think; // Thinking animation
    private SpriteAnimation ko; // KO animation
    [System.NonSerialized]
    private Sprite[] face = new Sprite[3];
    private string[] facePath;
    private string[] faceName;
    [System.NonSerialized]
    private Sprite wing;
    private string wingPath;
    private string wingName;

    // ---Constructor---
    public Ally(character duck, Fighter fighter, int[] stats){
        this.duck = duck;
        this.fighter = fighter;
        this.stats = (int[])stats.Clone();
        this.curHP = this.stats[0];
        this.curMP = this.stats[1];
        this.level = 5;
        this.maxExp = 50;
        this.skillList = new List<Skill>();
        return;
    }

    // ---Access Functions---
    // Return stats
    public int getCurHP() { return this.curHP; }
    public int getCurMP() { return this.curMP; }
    public int getCurEX() { return this.curEX; }
    public int getHP()  { return this.stats[0] + getEquipBoost(0); }
    public int getMP()  { return this.stats[1] + getEquipBoost(1); }
    public int getATK() { return this.stats[2] + getEquipBoost(2); }
    public int getDEF() { return this.stats[3] + getEquipBoost(3); }
    public int getMATK(){ return this.stats[4] + getEquipBoost(4); }
    public int getMDEF(){ return this.stats[5] + getEquipBoost(5); }
    public int getSPD() { return this.stats[6] + getEquipBoost(6); }
    public int[] getBaseStats() { return this.stats; }
    public int[] getStats(){
        int[] equippedStats = (int[])this.stats.Clone();
        for(int i = 0; i < equippedStats.Length; i++){
            equippedStats[i] += getEquipBoost(i);
        }
        return equippedStats;
    }
    private int getEquipBoost(int theStat){
        int sum = 0;
        if (head != null)       { sum += head.getStats()[theStat]; }
        if (torso != null)      { sum += torso.getStats()[theStat]; }
        if (accessoryL != null) { sum += accessoryL.getStats()[theStat]; }
        if (accessoryR != null) { sum += accessoryR.getStats()[theStat]; }
        if (weaponL != null)    { sum += weaponL.getStats()[theStat]; }
        if (weaponR != null)    { sum += weaponR.getStats()[theStat]; }
        return sum;
    }

    // Return expertheStatence
    public int getCurExp()  { return this.curExp; }
    public int getMaxExp()  { return this.maxExp; }
    public int getLevel()   { return this.level; }

    // Return statuses
    public bool isKO()      { return this.curStatus[0]; }
    public bool isPoison()  { return this.curStatus[1]; }
    public bool isBleed()   { return this.curStatus[2]; }
    public bool isBurn()    { return this.curStatus[3]; }
    public bool isPara()    { return this.curStatus[4]; }
    public bool isSleep()   { return this.curStatus[5]; }
    public bool isFreeze()  { return this.curStatus[6]; }
    public bool isRegen()   { return this.curStatus[7]; }
    public bool[] getStatus()   { return this.curStatus; }

    // Return stat boosts
    public int getAtkBoost()    { return this.curBoosts[0]; }
    public int getDefBoost()    { return this.curBoosts[1]; }
    public int getMatkBoost()   { return this.curBoosts[2]; }
    public int getMdefBoost()   { return this.curBoosts[3]; }
    public int getSpdBoost()    { return this.curBoosts[4]; }
    public int[] getBoosts()    { return this.curBoosts; }

    // Return equipment
    public Item getHead()       { return this.head; }
    public Item getTorso()      { return this.torso; }
    public Item getWeaponL()    { return this.weaponL; }
    public Item getWeaponR()    { return this.weaponR; }
    public Item getAccessoryL() { return this.accessoryL; }
    public Item getAccessoryR() { return this.accessoryR; }
    public Item[] getEquips(){
        Item[] equips = new Item[6];
        equips[0] = this.head;
        equips[1] = this.torso;
        equips[2] = this.weaponL;
        equips[3] = this.weaponR;
        equips[4] = this.accessoryL;
        equips[5] = this.accessoryR;
        return equips;
    }

    // Return icons
    public Sprite getWing()     { return this.wing; }
    public Sprite getFace(){
        if (getCurHP() <= 0)                                    { return this.face[2]; }
        else if ((float)getCurHP() / (float)getHP() <= 0.5f)    { return this.face[1]; }
        else                                                    { return this.face[0]; }
    }

    // Return skill
    public int getSkillAmt() {
        if ((this.level / 10) + 3 > this.skillList.Count)
            return this.skillList.Count;
        return (this.level / 10) + 3;
    }
    public Skill getSkill(int skillNumber) { 
        if (skillNumber < 0 || skillNumber >= getSkillAmt())
            return null;
        return skillList[skillNumber]; 
    }

    // Return name of character as a string
    public string getName(){
        switch(this.duck){
            case character.ANNIE:
                return "Annie";
            case character.DUKE:
                return "Duke";
            case character.FONS:
                return "Fons";
            default:
                return "";
        }
    }
    public character getCharacter(){
        return this.duck;
    }

    // Return fighter data
    public Fighter getFighter() { return this.fighter; }

    // ---Manipulation Procedures--- 
    // For debugging purposes only
    public void changeStat(int stat,int change) {
        this.stats[stat] += change;
    }

    // Adjust hp, mp, and ex stats
    private int limitAmt(int value, int min, int max){ 
        if (value > max)        return max;
        else if (value < min)   return min;
        else                    return value;
    }
    public void adjustHP(int change){ this.curHP = limitAmt(this.curHP + change, 0, this.stats[0]); return; }
    public void adjustMP(int change){ this.curMP = limitAmt(this.curMP + change, 0, this.stats[1]); return; }
    public void adjustEX(int change){ this.curEX = limitAmt(this.curEX + change, 0, 100); return; }
    public void resetHP(){ this.curHP = this.stats[0]; return; }
    public void resetMP(){ this.curMP = this.stats[1]; return; }
    public void resetEX(){ this.curEX = 0; return; }
    public void levelUp(int[] gains) {
        this.level += 1;

        for (int i = 0; i < this.stats.Length; i++)
            this.stats[i] += gains[i];
    }

    // Adjust exp and return the amount of extra exp(for level up purposes)
    public int adjustExp(int gain){
        int extra = this.curExp + gain - this.maxExp;
        if (extra >= 0) {
            //this.level += 1;
            this.maxExp = (int)(this.maxExp * 1.25);
            this.curExp = 0;
        }
        else
            this.curExp += gain;
        return extra;
    }

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
    public void resetStatus(){
        for (int i = 0; i < this.curStatus.Length; i++) {
            this.curStatus[i] = false;
        }
    }

    // Set stat boosts
    private int limitBoost(int boost) {
        if (boost > 4)          return 4;
        else if (boost < -4)    return -4;
        else                    return boost;
    }
    public void setAtkBoost(int change)         { this.curBoosts[0] = limitBoost(this.curBoosts[0] + change); return; }
    public void setDefBoost(int change)         { this.curBoosts[1] = limitBoost(this.curBoosts[1] + change); return; }
    public void setMatkBoost(int change)        { this.curBoosts[2] = limitBoost(this.curBoosts[2] + change); return; }
    public void setMdefBoost(int change)        { this.curBoosts[3] = limitBoost(this.curBoosts[3] + change); return; }
    public void setSpdBoost(int change)         { this.curBoosts[4] = limitBoost(this.curBoosts[4] + change); return; }
    public void setBoost(stat s, int change)    { this.curBoosts[(int)s] = limitBoost(this.curBoosts[(int)s] + change); return; }
    public void resetBoosts(){
        for (int i = 0; i < this.curBoosts.Length; i++) {
            this.curBoosts[i] = 0;
        }
    }

    // Adds a skill to the skill list
    public void addSkill(Skill s){
        this.skillList.Add(s);
        return;
    }

    // Replaces current equip with new one and returns old equip
    public Item equipItem(Item equip, int slot){
        switch (slot) {
            case 0: 
                return equipHead(equip);
            case 1:
                return equipTorso(equip);
            case 2: 
                return equipWeapon(equip, true);
            case 3:
                return equipWeapon(equip, false);
            case 4:
                return equipAccessory(equip, true);
            case 5:
                return equipAccessory(equip, false);
            default:
                return equip;
        }
    }
    public Item equipHead(Item equip){
        if (equip.getMain() == itemType.ARMOR || equip.getSub() == itemType.ARMOR) {
            if (equip.getArmorType() == armorType.HEAD) {
                Item temp = this.head;
                this.head = equip.copy();
                return temp;
            }
        }
        return equip; // Return the argument if it wasn't a helmet
    }
    public Item equipTorso(Item equip){
        if (equip.getMain() == itemType.ARMOR || equip.getSub() == itemType.ARMOR) {
            if (equip.getArmorType() == armorType.TORSO) {
                Item temp = this.torso;
                this.torso = equip.copy();
                return temp;
            }
        }
        return equip; // Return the argument if it wasn't a torso
    }
    public Item equipAccessory(Item equip, bool leftHand){
        if (equip.getMain() == itemType.ARMOR || equip.getSub() == itemType.ARMOR) {
            if (equip.getArmorType() == armorType.ACCESSORY) {
                Item temp;
                if (leftHand) {
                    temp = this.accessoryL;
                    this.accessoryL = equip.copy();
                }
                else {
                    temp = this.accessoryR;
                    this.accessoryR = equip.copy();
                }
                return temp;
            }
        }
        return equip; // Return the argument if it wasn't an accessory
    }
    public Item equipWeapon(Item equip, bool leftHand){
        if (equip.getMain() == itemType.WEAPON || equip.getSub() == itemType.WEAPON) {
            Item temp;
            if (leftHand) {
                temp = this.weaponL;
                this.weaponL = equip.copy();
            }
            else {
                temp = this.weaponR;
                this.weaponR = equip.copy();
            }
            return temp;
        }
        return equip; // Return the argument if it wasn't a weapon
    }

    // Set animation variables
    public void setAnimations(SpriteAnimation idle, SpriteAnimation idleAct, SpriteAnimation damaged, SpriteAnimation damagedAct, 
        SpriteAnimation confused, SpriteAnimation think, SpriteAnimation ko){
        this.idle = idle.copy();
        this.idleAct = idleAct.copy();
        this.damaged = damaged.copy();
        this.damagedAct = damagedAct.copy();
        this.confused = confused.copy();
        this.think = think.copy();
        this.ko = ko.copy();
        return;
    }

    // Set icon sprites
    public void setIcons(Sprite fine, Sprite hurt, Sprite ko, Sprite wing){
        this.face[0] = fine;
        this.face[1] = hurt;
        this.face[2] = ko;
        this.wing = wing;
        return;
    }

    // Save location of icons
    public void saveIcons(string[] pathF, string[] nameF, string pathW, string nameW){
        this.facePath = pathF;
        this.faceName = nameF;
        this.wingPath = pathW;
        this.wingName = nameW;
    }

    // Play the idle animation with occasional emote
    private int idleCounter; // Keeps track of how many times idle animation has looped
    public Sprite playIdle(){
        Sprite frame;
        if (this.idleCounter < 3) {
            if (((float)getCurHP()) / ((float)getHP()) > 0.5f) {
                frame = this.idle.playAnim();
                if (this.idle.isDone()) {
                    this.idleCounter += 1;
                }
            }
            else {
                frame = this.damaged.playAnim();
                if (this.damaged.isDone()) {
                    this.idleCounter += 1;
                }
            }
        }
        else {
            if (((float)getCurHP()) / ((float)getHP()) > 0.5f) {
                frame = this.idleAct.playAnim();
                if (this.idleAct.isDone()) {
                    this.idleCounter = 0;
                }
            }
            else {
                frame = this.damagedAct.playAnim();
                if (this.damagedAct.isDone()) {
                    this.idleCounter = 0;
                }
            }
        }
        return frame;
    }

    public bool isHurt(Sprite s) { return this.confused.inAnim(s); }
    // Play animations
    public Sprite playHurt(){ return this.confused.playAnim(); }
    public Sprite playThink(){ return this.think.playAnim(); }
    public Sprite playKO(){ return this.ko.playAnim(); }

    // ---Other Functions---
    // Determines if two combatants are the same
    public bool equals(Ally A){
        return true;
    }

    // Load sprites from saved locations
    public void loadSprites(){
        this.face = new Sprite[this.facePath.Length];
        for (int i = 0; i < face.Length; i++) {
            face[i] = getSprite(Resources.LoadAll<Sprite>(@facePath[i]), faceName[i]);
        }
        this.wing = getSprite(Resources.LoadAll<Sprite>(@wingPath), wingName);

        this.idle.loadSprites();
        this.idleAct.loadSprites();
        this.confused.loadSprites();
        this.damaged.loadSprites();
        this.damagedAct.loadSprites();
        this.ko.loadSprites();
        this.think.loadSprites();

        this.head.loadItem();
        this.torso.loadItem();
        this.accessoryL.loadItem();
        this.accessoryR.loadItem();
        this.weaponL.loadItem();
        this.weaponR.loadItem();

        for (int i = 0; i < this.skillList.Count; i++)
            this.skillList[i].loadAnim();

        this.fighter.loadFighter();
    }

    private Sprite getSprite(Sprite[] sheet, string name){
        for (int i = 0; i < sheet.Length; i++) {
            if (sheet[i].name == name) {
                return sheet[i];
            }
        }
        return null;
    } 
}

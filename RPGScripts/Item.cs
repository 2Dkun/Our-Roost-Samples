using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum status {KO, POISON, BLEED, BURN, PARALYZE, SLEEP, FREEZE, REGEN, NULL};
public enum itemType {FIELD, BATTLE, WEAPON, ARMOR, KEY, NULL};
public enum fieldType {TELEPORT, CRAFT, NULL};
public enum battleType {HEAL, CURE, STATUS, BOOST, NULL};
public enum armorType {HEAD, TORSO, ACCESSORY, WEAPON, NULL};
public enum weaponType {KNUCKLE, STAFF, NULL};

[System.Serializable]
public class Item {

    // ---Fields--- 
    private itemType iTypeMain;
    private itemType iTypeSub;
    [System.NonSerialized]
    private Sprite icon;
    private string itemPath;
    private string itemName;
    private string description;
    private int itemID;
    private int price;
    private int quantity;
    // Field variables
    private fieldType fType = fieldType.NULL;
    // Battle variables
    private battleType bType = battleType.NULL;
    private status statusMod;
    private int range;
    private stat statBoost; // Determines which stat to boost if any
    private int scalar; // Heal(HP)/Cure(MP): amount to be healed, Status: cure(1) or inflict(-1), Boost: levels of boosts
    private bool targetAlly; 
    // Equipment variables
    private armorType aType = armorType.NULL;
    private weaponType wType = weaponType.NULL;
    private int[] statMods = new int[7];

    // ---Constructor---
    public Item(itemType main, itemType sub, Sprite icon, string itemPath, string itemName, string description, int ID, int price, int quantity){
        this.iTypeMain = main;
        this.iTypeSub = sub;
        this.icon = icon;
        this.itemPath = itemPath;
        this.itemName = itemName;
        this.description = description;
        this.itemID = ID;
        this.price = price;
        this.quantity = quantity;
        return;
    }

    // ---Access Functions---
    public itemType getMain()   { return this.iTypeMain; }
    public itemType getSub()    { return this.iTypeSub; }
    public Sprite getIcon()     { return this.icon; }
    public string getName()     { return this.itemName; }
    public string getDes()      { return this.description; }
    public int getAmt()         { return this.quantity; }
    public int getPrice()       { return this.price; }
    public int getID()          { return this.itemID; }

    // Get type of field item
    public fieldType getFieldType(){
        if (this.iTypeMain == itemType.FIELD || this.iTypeSub == itemType.FIELD) {
            return this.fType;
        }
        return fieldType.NULL;
    }

    // Get the type of battle item
    public battleType getBattleType(){
        if (this.iTypeMain == itemType.BATTLE || this.iTypeSub == itemType.BATTLE) {
            return this.bType;
        }
        return battleType.NULL;
    }
    // Get the status that the item heals
    public status getStatus(){
        if (this.iTypeMain == itemType.BATTLE || this.iTypeSub == itemType.BATTLE) {
            return this.statusMod;
        }
        return status.NULL;
    }
    public int getRange(){  return this.range; }
    public stat getStatBoost() { return this.statBoost; }
    public int getScalar(){ return this.scalar; }
    public bool isTargetAlly()      { return this.targetAlly; }

    // Get the type of armor
    public armorType getArmorType(){
        if (this.iTypeMain == itemType.ARMOR || this.iTypeSub == itemType.ARMOR) {
            return this.aType;
        }
        return armorType.NULL;
    }
    // Get the type of weapon
    public weaponType getWeaponType(){
        if (this.iTypeMain == itemType.ARMOR) {
            return this.wType;
        }
        return weaponType.NULL;
    }
    // Returns individual stat changes
    public int getHP()  { return this.statMods[0]; }
    public int getMP()  { return this.statMods[1]; }
    public int getATK() { return this.statMods[2]; }
    public int getDEF() { return this.statMods[3]; }
    public int getMATK(){ return this.statMods[4]; }
    public int getMDEF(){ return this.statMods[5]; }
    public int getSPD() { return this.statMods[6]; }
    public int[] getStats() { return this.statMods; }

    // ---Manipulation Procedures---
    public void setSubType(itemType i)  { this.iTypeSub = i; }
    public void onlyOne()               { this.quantity = 1; }
    public void addItem(int amt) { 
        this.quantity += amt; 
        if(this.quantity < 0)
            this.quantity = 0;
    }

    // Set type of field item
    public void setFieldType(fieldType f){
        if (this.iTypeMain == itemType.FIELD || this.iTypeSub == itemType.FIELD) {
            this.fType = f;
        }
        return;
    }
        
    // Set the type of battle item
    public void setBattleType(battleType b){
        if (this.iTypeMain == itemType.BATTLE || this.iTypeSub == itemType.BATTLE) {
            this.bType = b;
        }
        return;
    }
    // Set the status that the item heals
    public void setStatus(status s){
        if (this.iTypeMain == itemType.BATTLE || this.iTypeSub == itemType.BATTLE) {
            this.statusMod = s;
        }
        return;
    }
    public void setModifiers(int range, stat statBoost, int scalar, bool targetAlly){
        if (this.iTypeMain == itemType.BATTLE || this.iTypeSub == itemType.BATTLE) {
            this.range = range;
            this.statBoost = statBoost;
            this.scalar = scalar;
            this.targetAlly = targetAlly;
        }
        return;
    }

    // Set the type of armor
    public void setArmorType(armorType a){
        if (this.iTypeMain == itemType.ARMOR || this.iTypeSub == itemType.ARMOR) {
            this.aType = a;
        }
        return;
    }
    // Set the type of weapon
    public void setWeaponType(weaponType w){
        if (this.iTypeMain == itemType.WEAPON || this.iTypeSub == itemType.WEAPON) {
            this.wType = w;
            this.aType = armorType.WEAPON;
        }
        return;
    }
    // Adjust the stat changes of the item
    public void setStats(int hp, int mp, int atk, int def, int matk, int mdef, int spd){
        if (this.iTypeMain == itemType.WEAPON || this.iTypeMain == itemType.ARMOR) {
            this.statMods[0] = hp;
            this.statMods[1] = mp;
            this.statMods[2] = atk;
            this.statMods[3] = def;
            this.statMods[4] = matk;
            this.statMods[5] = mdef;
            this.statMods[6] = spd;
        }
        return;
    }
    public void setStats(int[] stats){
        if (this.iTypeMain == itemType.WEAPON || this.iTypeMain == itemType.ARMOR) {
            for (int i = 0; i < this.statMods.Length; i++) {
                this.statMods[i] = stats[i];
            }
        }
        return;
    }

    private void setAmt(int amt){
        this.quantity = amt;
    }

    // ---Other Functions---
    // Checks if two items are the same item
    public bool equals(Item I){
        return this.itemID == I.getID();
    }
    public Item copy(){
        Item clone = new Item(this.iTypeMain, this.iTypeSub, this.icon, this.itemPath, this.itemName, this.description, this.itemID, this.price, this.quantity);

        if (this.iTypeMain == itemType.FIELD || this.iTypeSub == itemType.FIELD) {
            clone.fType = this.fType;
        }
        if (this.iTypeMain == itemType.BATTLE || this.iTypeSub == itemType.BATTLE) {
            clone.bType = this.bType;
            clone.statusMod = this.statusMod;
            clone.range = this.range;
            clone.statBoost = this.statBoost;
            clone.scalar = this.scalar;
        }
        if (this.iTypeMain == itemType.WEAPON || this.iTypeSub == itemType.WEAPON) {
            clone.wType = this.wType;
        }
        if (this.iTypeMain == itemType.ARMOR || this.iTypeSub == itemType.ARMOR) {
            clone.aType = this.aType;
        }
        if (this.iTypeMain == itemType.WEAPON || this.iTypeMain == itemType.ARMOR) {
            for (int i = 0; i < this.statMods.Length; i++) {
                clone.statMods[i] = this.statMods[i];
            }
        }
        return clone;
    }
    public Item copy(int amt){
        Item clone = copy();
        clone.setAmt(amt);
        return clone;
    }

    public void loadItem(){
        this.icon = getSprite(Resources.LoadAll<Sprite>(@itemPath), itemName);
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

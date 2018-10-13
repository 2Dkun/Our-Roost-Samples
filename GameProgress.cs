using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class GameProgress {

	// Field
	private float playedTime;
	private float playerPosX, playerPosY; // Overworld position
	private direction lastDir;

	private List<Ally> party;
	private List<Item> inventory; // maybe split into categories so you dont have to search as much
	private List<Item> emptyEquips;
	private List<int> eventFlags; // Contains id number of events that have happened
	private Hashtable itemList;

	private string curScene;
	private int balance;
	[System.NonSerialized]
	private List<Foe> curEnemySet;
	[System.NonSerialized]
	private BattleEvent.BEvent[] curBattleEvent;
	[System.NonSerialized]
	private Sprite batBackgound;
    [System.NonSerialized]
    private AudioClip curSound;

    // Constructor
    public GameProgress(){
		this.playedTime = 0.0f;
		this.playerPosX = 69.34f;
		this.playerPosY = -19.96f;
		this.lastDir = direction.DOWN;

		this.party = new List<Ally>();
		this.inventory = new List<Item>();
		this.eventFlags = new List<int>();
		this.balance = 0;
		this.curScene = "GameStart";
	}

	// Basic get methods to send certain saved variables
	public List<Ally> getParty() 		{ return this.party; }
	public List<Item> getInventory() 	{ return this.inventory; }
	public Hashtable getItemList()		{ return this.itemList; }
	public bool getFlag(int flagID)		{ return this.eventFlags.Contains(flagID); }
	public float getTime()				{ return this.playedTime; }
	public float getXPos()				{ return this.playerPosX; }
	public float getYPos()				{ return this.playerPosY; }
	public string getScene() 			{ return this.curScene; }
	public direction getDir()			{ return this.lastDir; }
	public int getBalance()				{ return this.balance; }
	public List<Foe> getEnemySet()		{ return this.curEnemySet; }
	public Item getEmptyHead()			{ return this.emptyEquips[0]; }
	public Item getEmptyTorso()			{ return this.emptyEquips[1]; }
	public Item getEmptyAccessory()		{ return this.emptyEquips[2]; }
	public Item getEmptyLeft()			{ return this.emptyEquips[3]; }
	public Item getEmptyRight()			{ return this.emptyEquips[4]; }
    public Sprite getBackground()       { return this.batBackgound; }
    public AudioClip getSound()         { return this.curSound; }
    public BattleEvent.BEvent[] getBattleEvent() {
		BattleEvent.BEvent[] temp = this.curBattleEvent;
		this.curBattleEvent = null;
		return temp;
	}

	// Check if a given item is an one of the basic equipment
	public bool itemIsEmpty(Item thing){
		for (int i = 0; i < this.emptyEquips.Count; i++) {
			if (thing.equals(this.emptyEquips[i]))
				return true;
		}
		return false;
	}
		
	// Get all armor of a given armor type
	public List<Item> getArmor(armorType a){
		List<Item> armors = new List<Item>();
		for (int i = 0; i < this.inventory.Count; i++) {
			if (this.inventory[i].getArmorType() == a && this.inventory[i].getAmt() > 0) {
				armors.Add(this.inventory[i]);
			}
		}
		return armors;
	}

	// Get all weapons of a given weapon type
	public List<Item> getWeapon(weaponType a){
		List<Item> weapons = new List<Item>();
		for (int i = 0; i < this.inventory.Count; i++) {
			if (this.inventory[i].getMain() == itemType.WEAPON && this.inventory[i].getWeaponType() == a && this.inventory[i].getAmt() > 0) {
				weapons.Add(this.inventory[i]);
			}
		}
		return weapons;
	}

	// Add an item to your inventory
	public void addToInventory(Item I) { 
		bool isInInventory = false;
		for(int i = 0; i < inventory.Count; i++){
			if (inventory[i].equals(I)) {
				isInInventory = true;
				inventory[i].addItem(I.getAmt());
				break;
			}
		}
		if(!isInInventory)
			inventory.Add(I.copy());
	}

	// Remove an item from your inventory
	public void removeItem(Item I, int amt){
		for(int i = 0; i < inventory.Count; i++){
			if (inventory[i].equals(I)) {
				inventory[i].addItem(-1*amt);
				break;
			}
		}
	}

	// Get all items with a given item type
	public List<Item> getInventoryOfType(itemType I){
		List<Item> inven = new List<Item>();
		for (int i = 0; i < this.inventory.Count; i++) {
			if (this.inventory[i].getMain() == I && this.inventory[i].getAmt() > 0) {
				inven.Add(this.inventory[i]);
			}
		}
		return inven;
	}

	// Check if an item is in your inventory
	public bool isInInventory(Item I){
		for (int i = 0; i < this.inventory.Count; i++) {
			if (this.inventory[i].equals(I) && this.inventory[i].getAmt() > 0) {
				return true;
			}
		}
		return false;
	}

	// Basic set methods to change saved variables
	public void setDirection(direction d) 		        { this.lastDir = d; }
	public void setLocation(float x, float y) 	        { this.playerPosX = x; this.playerPosY = y; }
	public void setEnemySet(List<Foe> E)		        { this.curEnemySet = E; }
	public void setBattleEvent(BattleEvent.BEvent[] B)	{ this.curBattleEvent = B;}
	public void setScene(string s)		                { this.curScene = s; return; }
	public void setFlag(int flagID)		                { this.eventFlags.Add(flagID); }
	public void adjustBalance(int value)	            { this.balance += value; return; }
	// Update the current played time
	public void updateTime()			                { this.playedTime += Time.deltaTime; return; }
	public void setBackground(Sprite b)			        { this.batBackgound = b; return; }
    public void setSound(AudioClip a)                   { this.curSound = a;  return;  }

	// Sprites and other vairables that can't be serialized are saved as strings, so
	// this method will load in those variables using the saved strings.
	public void loadFile(){
		fillItemList();
		for (int i = 0; i < party.Count; i++) 
			party[i].loadSprites();
		for (int i = 0; i < inventory.Count; i++)
			inventory[i].loadItem();
		for (int i = 0; i < emptyEquips.Count; i++)
			emptyEquips[i].loadItem();
	}

	// Reads in the three characters and empty equipment text files
	public void makeFile(){
		jankFile dukeTxt = new jankFile(Resources.Load<TextAsset>(@"Duke"));
		jankFile fonsTxt = new jankFile(Resources.Load<TextAsset>(@"Fons"));
		jankFile annieTxt = new jankFile(Resources.Load<TextAsset>(@"Annie"));
		this.party.Add(makeAlly(dukeTxt, character.DUKE));
		this.party.Add(makeAlly(fonsTxt, character.FONS));
		this.party.Add(makeAlly(annieTxt, character.ANNIE));	
		// Add two empty characters
		fonsTxt.resetFile();
		this.party.Add(makeAlly(fonsTxt, character.NULL));
		fonsTxt.resetFile();
		this.party.Add(makeAlly(fonsTxt, character.NULL));

//		fonsTxt.resetFile();
//		this.party.Add(makeAlly(fonsTxt, character.NULL));
//		fonsTxt.resetFile();
//		this.party.Add(makeAlly(fonsTxt, character.NULL));


		jankFile empty = new jankFile(Resources.Load<TextAsset>(@"EmptyEquips"));
		emptyEquips = getEmptyEquips(empty);
		for (int i = 0; i < this.party.Count; i++) {
			party[i].equipHead(getEmptyHead());
			party[i].equipTorso(getEmptyTorso());
			party[i].equipAccessory(getEmptyAccessory(), true);
			party[i].equipAccessory(getEmptyAccessory(), false);
			party[i].equipWeapon(getEmptyLeft(), true);
			party[i].equipWeapon(getEmptyRight(), false);
		}
		loadFile();
	}

	// Returns a desired sprite from an array of sprites
	private Sprite getSprite(Sprite[] sheet, string name){
		for (int i = 0; i < sheet.Length; i++) {
			if (sheet[i].name == name) {
				return sheet[i];
			}
		}
		return null;
	}

	// A small class used to make reading input a bit easier
	public class jankFile {
		private string[] lines;
		private int curLine;

		public jankFile(TextAsset input) {
			this.curLine = 0;
			lines = input.text.Split("\n"[0]);
		}

		public string ReadLine(){
			curLine += 1;
			if (curLine > lines.Length)
				curLine = lines.Length;
			return lines[curLine - 1];
		}

		public void resetFile(){
			this.curLine = 0;
		}
	}

	// Fills out hash table for items
	private void fillItemList() {
		this.itemList = new Hashtable();
		jankFile itemTxt = new jankFile(Resources.Load<TextAsset>(@"ItemList"));
		List<string> sheetP = new List<string>();
		List<Sprite[]> sheets = new List<Sprite[]>();

		// Determine number of spritesheets
		string s = itemTxt.ReadLine();
		string[] split = s.Split(' ');
		int paths = 0;
		int.TryParse(split[split.Length - 1], out paths);
		// Read paths for spritesheets
		for (int i = 0; i < paths; i++) {
			string sheetPath = itemTxt.ReadLine();
			Sprite[] sheet = Resources.LoadAll<Sprite>(@sheetPath);
			sheets.Add(sheet);
			sheetP.Add(sheetPath);
		}
		itemTxt.ReadLine();

		// Count the number of items to read
		s = itemTxt.ReadLine();
		split = s.Split(' ');
		int itemAmt = 0;
		int.TryParse(split[split.Length - 1], out itemAmt);

		// Read in each item and place in hash
		itemTxt.ReadLine();
		for (int i = 0; i < itemAmt; i++) {
			string name = "", path = "", des = "";
			Sprite icon;
			itemType main, sub;

			// Name
			s = itemTxt.ReadLine();
			split = s.Split(' ');
			for (int j = 1; j < split.Length; j++) {
				name += split[j];
				if (j != split.Length - 1)
					name += " ";
			}
			// Description
			s = itemTxt.ReadLine();
			split = s.Split(' ');
			for (int j = 1; j < split.Length; j++) {
				des += split[j];
				if (j != split.Length - 1)
					des += " ";
			}

			// Path
			s = itemTxt.ReadLine();
			split = s.Split(' ');
			int pathNum = 0;
			int.TryParse(split[split.Length - 1], out pathNum);
			path = sheetP[pathNum];
			icon = getSprite(sheets[pathNum], name);

			// Price 
			s = itemTxt.ReadLine();
			split = s.Split(' ');
			int price = 0;
			int.TryParse(split[split.Length - 1], out price);

			// Types
			s = itemTxt.ReadLine();
			split = s.Split(' ');
			main = (itemType) System.Enum.Parse(typeof(itemType), split[split.Length - 1]);
			s = itemTxt.ReadLine();
			split = s.Split(' ');
			sub = (itemType) System.Enum.Parse(typeof(itemType), split[split.Length - 1]);

			// Make item so far
			Item entry = new Item(main, sub, icon, path, name, des, i, price, 1);

			// Read in main type first
			switch(main){
				case itemType.ARMOR:
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					armorType a = (armorType) System.Enum.Parse(typeof(armorType), split[split.Length - 1]);
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					int[] stats = new int[7];
					for (int k = 0; k < stats.Length; k++) {
						int.TryParse(split[k + 1], out stats[k]);
					}
					entry.setArmorType(a);
					entry.setStats(stats);
					break;
				case itemType.WEAPON:
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					weaponType w = (weaponType)System.Enum.Parse(typeof(weaponType), split[split.Length - 1]);
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					stats = new int[7];
					for (int k = 0; k < stats.Length; k++) {
						int.TryParse(split[k + 1], out stats[k]);
					}
					entry.setWeaponType(w);
					entry.setStats(stats);
					entry.setSubType(itemType.ARMOR);
					entry.setArmorType(armorType.WEAPON);
					break;
				case itemType.FIELD:
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					fieldType f = (fieldType) System.Enum.Parse(typeof(fieldType), split[split.Length - 1]);
					entry.setFieldType(f);
					break;
				case itemType.BATTLE:
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					battleType b = (battleType) System.Enum.Parse(typeof(battleType), split[split.Length - 1]);
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					status statusMod = (status) System.Enum.Parse(typeof(status), split[split.Length - 1]);
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					int range = 0; 
					int.TryParse(split[split.Length - 1], out range);
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					stat st = (stat) System.Enum.Parse(typeof(stat), split[split.Length - 1]);
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					int scalar = 0; 
					int.TryParse(split[split.Length - 1], out scalar);
					s = itemTxt.ReadLine();
					split = s.Split(' ');
					bool tAlly = true; 
					bool.TryParse(split[split.Length - 1], out tAlly);
					entry.setBattleType(b);
					entry.setStatus(statusMod);
					entry.setModifiers(range, st, scalar, tAlly);
					break;
				default: // Nothing happens if it's a key item
					break;
			}
			this.itemList.Add(i, entry);
			itemTxt.ReadLine();
		}
	}

	private List<Item> getEmptyEquips(jankFile input){
		List<Item> equips = new List<Item>();
		// Determine how many items to read
		string s = input.ReadLine();
		string[] split = s.Split(' ');
		int items = 0;
		int.TryParse(split[split.Length - 1], out items);

		for (int i = 0; i < items; i++) {
			string name = "", path = "", des = "";
			int quantity = 0;
			Sprite icon;
			itemType main, sub;

			// Name
			s = input.ReadLine();
			split = s.Split(' ');
			for (int j = 1; j < split.Length; j++) {
				name += split[j];
				if (j != split.Length - 1)
					name += " ";
			}
			// Description
			s = input.ReadLine();
			split = s.Split(' ');
			for (int j = 1; j < split.Length; j++) {
				des += split[j];
				if (j != split.Length - 1)
					des += " ";
			}
			// Quantity
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out quantity);
			// Path
			s = input.ReadLine();
			split = s.Split(' ');
			for (int j = 1; j < split.Length; j++) {
				path += split[j];
				if (j != split.Length - 1)
					path += " ";
			}
			icon = getSprite(Resources.LoadAll<Sprite>(path), name);
			s = input.ReadLine();
			split = s.Split(' ');
			main = determineItem(split[split.Length - 1]);
			s = input.ReadLine();
			split = s.Split(' ');
			sub = determineItem(split[split.Length - 1]);

			// Make weapon so far
			Item entry = new Item(main, sub, icon, path, name, des, -1, -1, quantity);

			// Read in main type first
			switch(main){
				case itemType.ARMOR:
					s = input.ReadLine();
					split = s.Split(' ');
					armorType a = determineArmor(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					int[] stats = new int[7];
					for (int k = 0; k < stats.Length; k++) {
						int.TryParse(split[k + 1], out stats[k]);
					}
					entry.setArmorType(a);
					entry.setStats(stats);
					break;
				case itemType.WEAPON:
					s = input.ReadLine();
					split = s.Split(' ');
					weaponType w = determineWeapon(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					stats = new int[7];
					for(int k = 0; k < stats.Length; k++){
						int.TryParse(split[k + 1], out stats[k]);
					}
					entry.setWeaponType(w);
					entry.setStats(stats);
					break;
				default: // Just implementing weapon for now
					break;
			}
			// Empty weapon and armor so no need to check for second type 

			equips.Add(entry);
			input.ReadLine();
		}
			
		return equips;
	}
		
	private itemType determineItem(string i){
		switch (i) {
			case "Armor":
				return itemType.ARMOR;
			case "Weapon":
				return itemType.WEAPON;
			case "Key":
				return itemType.KEY;
			case "Field":
				return itemType.FIELD;
			case "Battle":
				return itemType.BATTLE;
			default:
				return itemType.NULL;
		}
	}

	private armorType determineArmor(string a){
		switch (a) {
			case "Head":
				return armorType.HEAD;
			case "Torso":
				return armorType.TORSO;
			case "Accessory":
				return armorType.ACCESSORY;
			default:
				return armorType.NULL;
		}
	}

	private weaponType determineWeapon(string w){
		switch (w) {
			case "Knuckle":
				return weaponType.KNUCKLE;
			case "Staff":
				return weaponType.STAFF;
			default:
				return weaponType.NULL;
		}
	}

	private Ally makeAlly(jankFile input, character duck){
		List<string> sheetP = new List<string>();
		List<Sprite[]> sheets = new List<Sprite[]>();
		List<SpriteAnimation> RPGanims = new List<SpriteAnimation>();
		List<Skill> skillList = new List<Skill>();
		List<FighterSkill> fighterSkills = new List<FighterSkill>();

		bool hasBack;
		int[] battlestats = new int[7];
		// Determine number of spritesheets
		string s = input.ReadLine();

		string[] split = s.Split(' ');
		int paths = 0;
		int.TryParse(split[split.Length - 1], out paths);
		// Read paths for spritesheets
		for (int i = 0; i < paths; i++) {
			string sheetPath = input.ReadLine();
			Sprite[] sheet = Resources.LoadAll<Sprite>(@sheetPath);
			sheets.Add(sheet);
			sheetP.Add(sheetPath);
		}
		input.ReadLine();

		// Check if the character as a back set of animations
		s = input.ReadLine();
		split = s.Split(' ');
		bool.TryParse(split[split.Length - 1], out hasBack);

		// Read in stats of character
		s = input.ReadLine();
		split = s.Split(' ');
		for (int i = 1; i < split.Length; i++) {
			int.TryParse(split[i], out battlestats[i - 1]);
		}
		input.ReadLine();

		// Read in RPG animations
		for (int i = 0; i < 7; i++) {
			input.ReadLine();
			float fps;
			int spriteAmt;

			s = input.ReadLine();
			split = s.Split(' ');
			float.TryParse(split[split.Length - 1], out fps);
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out spriteAmt);

			Sprite[] anim = new Sprite[spriteAmt];
			string[] pathS = new string[spriteAmt];
			string[] nameS = new string[spriteAmt];
			for (int j = 0; j < spriteAmt; j++) {
				int sheetNum;
				s = input.ReadLine();
				split = s.Split('-');
				int.TryParse(split[0], out sheetNum);
				pathS[j] = sheetP[sheetNum];
				nameS[j] = split[split.Length - 1];

				anim[j] = getSprite(sheets[sheetNum], split[split.Length - 1]);
			}

			SpriteAnimation entry = new SpriteAnimation(anim, new int[0], fps, true);
			entry.saveSprites(pathS, nameS);
			RPGanims.Add(entry);
			input.ReadLine();
		}

		input.ReadLine();
		int skillAmt;
		s = input.ReadLine();
		split = s.Split(' ');
		int.TryParse(split[split.Length - 1], out skillAmt);
		input.ReadLine();

		// Read in RPG skills
		for(int i = 0; i < skillAmt; i++){
			skillType skill;
			string atkName, des;
			element atkEle;
			int basePower, mpCost, range, effectChance;
			bool isMagical, animLoop;
			float start, end, fps;
			SpriteAnimation anim;

			battleType support = battleType.NULL;
			status statusMod = status.NULL;
			int scalar = 0;
			stat statBoost = stat.NULL;
			bool selfTarget = false;
			bool targetAlly = false;
            string skillSFX;

			atkName = input.ReadLine();
			des = input.ReadLine();

			s = input.ReadLine();
			split = s.Split(' ');
			if (split[split.Length - 1].CompareTo("Offensive") == 0) {
				skill = skillType.OFFENSIVE;

				// Attack element
				s = input.ReadLine();
				split = s.Split(' ');
				atkEle = decideEle(split[split.Length - 1]);

				// base power
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out basePower);

				// mp cost
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out mpCost);

				// is magical
				s = input.ReadLine();
				split = s.Split(' ');
				bool.TryParse(split[split.Length - 1], out isMagical);

				// effect chance
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out effectChance);
				if (effectChance > 0) {
					// status variables
					s = input.ReadLine();
					split = s.Split(' ');
					support = decideSupport(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					statusMod = decideStatus(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					statBoost = decideStat(split[split.Length - 1]);
					s = input.ReadLine();
					split = s.Split(' ');
					int.TryParse(split[split.Length - 1], out scalar);
					s = input.ReadLine();
					split = s.Split(' ');
					bool.TryParse(split[split.Length - 1], out selfTarget);
					s = input.ReadLine();
					split = s.Split(' ');
					bool.TryParse(split[split.Length - 1], out targetAlly);
				}

				// range
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out range);

				// positions
				s = input.ReadLine();
				split = s.Split(' ');
				float.TryParse(split[split.Length - 1], out start);
				s = input.ReadLine();
				split = s.Split(' ');
				float.TryParse(split[split.Length - 1], out end);

				// FPS
				s = input.ReadLine();
				split = s.Split(' ');
				float.TryParse(split[split.Length - 1], out fps);

				// Sprite animation
				int spriteAmt;
				s = input.ReadLine();
				split = s.Split(' ');
				if (split.Length > 2) {
					int.TryParse(split[split.Length - 2], out spriteAmt);
					animLoop = true;
				}
				else {
					int.TryParse(split[split.Length - 1], out spriteAmt);
					animLoop = false;
				}
				Sprite[] sprites = new Sprite[spriteAmt];
				string[] pathS = new string[spriteAmt];
				string[] nameS = new string[spriteAmt];
				for (int j = 0; j < spriteAmt; j++) {
					int sheetNum;
					s = input.ReadLine();
					split = s.Split('-');
					int.TryParse(split[0], out sheetNum);

					pathS[j] = sheetP[sheetNum];
					nameS[j] = split[split.Length - 1];

					sprites[j] = getSprite(sheets[sheetNum], split[split.Length - 1]);
				}
				anim = new SpriteAnimation(sprites, new int[0], fps, animLoop);
				anim.saveSprites(pathS, nameS);
			}
			else {
				skill = skillType.SUPPORT;
				basePower = 0;
				effectChance = 0;
				isMagical = true;

				// Attack element
				s = input.ReadLine();
				split = s.Split(' ');
				atkEle = decideEle(split[split.Length - 1]);

				// mp cost
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out mpCost);

				// range
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out range);

				// positions
				s = input.ReadLine();
				split = s.Split(' ');
				float.TryParse(split[split.Length - 1], out start);
				s = input.ReadLine();
				split = s.Split(' ');
				float.TryParse(split[split.Length - 1], out end);

				// status variables
				s = input.ReadLine();
				split = s.Split(' ');
				support = decideSupport(split[split.Length - 1]);
				s = input.ReadLine();
				split = s.Split(' ');
				statusMod = decideStatus(split[split.Length - 1]);
				s = input.ReadLine();
				split = s.Split(' ');
				statBoost = decideStat(split[split.Length - 1]);
				s = input.ReadLine();
				split = s.Split(' ');
				int.TryParse(split[split.Length - 1], out scalar);
				s = input.ReadLine();
				split = s.Split(' ');
				bool.TryParse(split[split.Length - 1], out selfTarget);
				s = input.ReadLine();
				split = s.Split(' ');
				bool.TryParse(split[split.Length - 1], out targetAlly);

				// FPS
				s = input.ReadLine();
				split = s.Split(' ');
				float.TryParse(split[split.Length - 1], out fps);

				// Sprite animation
				int spriteAmt;
				s = input.ReadLine();
				split = s.Split(' ');
				if (split.Length > 2) {
					int.TryParse(split[split.Length - 2], out spriteAmt);
					animLoop = true;
				}
				else {
					int.TryParse(split[split.Length - 1], out spriteAmt);
					animLoop = false;
				}
				Sprite[] sprites = new Sprite[spriteAmt];
				string[] pathS = new string[spriteAmt];
				string[] nameS = new string[spriteAmt];
				for (int j = 0; j < spriteAmt; j++) {
					int sheetNum;
					s = input.ReadLine();
					split = s.Split('-');
					int.TryParse(split[0], out sheetNum);

					pathS[j] = sheetP[sheetNum];
					nameS[j] = split[split.Length - 1];

					sprites[j] = getSprite(sheets[sheetNum], split[split.Length - 1]);
				}
				anim = new SpriteAnimation(sprites, new int[0], fps, animLoop);
				anim.saveSprites(pathS, nameS);
			}
            // Get SFX path 
			input.ReadLine();
            skillSFX = input.ReadLine();
            input.ReadLine();

			Skill entry = new Skill(skill, atkName, des, atkEle, mpCost, range, start, end, anim, skillSFX);
			entry.setAttack(basePower, isMagical, effectChance);
			if (skill == skillType.SUPPORT || effectChance > 0)		
				entry.setSupport(support, statusMod, statBoost, scalar, selfTarget, targetAlly);
			skillList.Add(entry);
		}

		input.ReadLine();
		s = input.ReadLine();
		split = s.Split(' ');
		int.TryParse(split[split.Length - 1], out skillAmt);
		input.ReadLine();

		// Read fighter skills
		for(int i = 0; i < skillAmt; i++){
			bool hasProj;
			string projPath = "";
			string atkName = input.ReadLine();
			int hitstun, basePower, faf, fps, rehit;
			float xPow, yPow, xShift, yShift;
			bool jc, sc, animLoop;
			SpriteAnimation anim, animB;
            string skillSFX;

			// Projectile
			s = input.ReadLine();
			split = s.Split(' ');
			bool.TryParse(split[split.Length - 1], out hasProj);
			if (hasProj) {
				projPath = input.ReadLine();
			}

			// Command Input
			int inputAmt;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out inputAmt);
			KeyCode[] command = new KeyCode[inputAmt];
			s = input.ReadLine();
			split = s.Split(' ');
			for (int j = 0; j < inputAmt; j++) {
				command[j] = decideKeyCode(split[j]);
			}

			// Hitstun
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out hitstun);

			// Base Power
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out basePower);

			// First Active Frame
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out faf);

			// Rehit
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out rehit);

			// Power and shift
			s = input.ReadLine();
			split = s.Split(' ');
			float.TryParse(split[split.Length - 1], out xPow);
			s = input.ReadLine();
			split = s.Split(' ');
			float.TryParse(split[split.Length - 1], out yPow);
			s = input.ReadLine();
			split = s.Split(' ');
			float.TryParse(split[split.Length - 1], out xShift);
			s = input.ReadLine();
			split = s.Split(' ');
			float.TryParse(split[split.Length - 1], out yShift);

			// Cancels
			s = input.ReadLine();
			split = s.Split(' ');
			bool.TryParse(split[split.Length - 1], out jc);
			s = input.ReadLine();
			split = s.Split(' ');
			bool.TryParse(split[split.Length - 1], out sc);

			// Hitbox Frames
			int frameAmt;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out frameAmt);
			int[] hitFrames = new int[frameAmt];
			s = input.ReadLine();
			if (frameAmt == 1) {
				int.TryParse(s, out hitFrames[0]);
			}
			else if(frameAmt > 1){
				split = s.Split('-');
				int min, max;
				int.TryParse(split[0], out min);
				int.TryParse(split[split.Length - 1], out max);
				for (int j = 0; j < hitFrames.Length; j++) {
					hitFrames[j] = j + min;
				}
			}

			// Sprites
			int spriteAmt;
			s = input.ReadLine();
			split = s.Split(' ');
			if (split.Length > 2) {
				int.TryParse(split[split.Length - 2], out spriteAmt);
				animLoop = true;
			}
			else {
				int.TryParse(split[split.Length - 1], out spriteAmt);
				animLoop = false;
			}
			Sprite[] sprites = new Sprite[spriteAmt];
			string[] pathS = new string[spriteAmt];
			string[] nameS = new string[spriteAmt];
			for (int j = 0; j < spriteAmt; j++) {
				int sheetNum;
				s = input.ReadLine();
				split = s.Split('-');
				int.TryParse(split[0], out sheetNum);

				pathS[j] = sheetP[sheetNum];
				nameS[j] = split[split.Length - 1];

				sprites[j] = getSprite(sheets[sheetNum], split[split.Length - 1]);
			}

			// Sprites back
			Sprite[] spritesB = new Sprite[spriteAmt];
			string[] pathSB = new string[spriteAmt];
			string[] nameSB = new string[spriteAmt];
			if(hasBack){
				for (int j = 0; j < spriteAmt; j++) {
					int sheetNum;
					s = input.ReadLine();
					split = s.Split('-');
					int.TryParse(split[0], out sheetNum);

					pathSB[j] = sheetP[sheetNum];
					nameSB[j] = split[split.Length - 1];

					spritesB[j] = getSprite(sheets[sheetNum], split[split.Length - 1]);
				}
			}

			// Hitboxes
			int hitboxAmt;
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out hitboxAmt);
			HitBox[] boxes = new HitBox[hitboxAmt];
			for (int j = 0; j < hitboxAmt; j++) {
				s = input.ReadLine();
				split = s.Split(' ');
				float x, y, xs, ys;
				float.TryParse(split[0], out x);
				float.TryParse(split[1], out y);
				float.TryParse(split[2], out xs);
				float.TryParse(split[3], out ys);

				boxes[j] = new HitBox((x / 2) + xs, (y / 2) + ys, (-x / 2) + xs, (-y / 2) + ys);
			}

			// FPS
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out fps);
			anim = new SpriteAnimation(sprites, hitFrames, fps, animLoop);
			anim.saveSprites(pathS, nameS);
			animB = new SpriteAnimation(spritesB, hitFrames, fps, animLoop);
			animB.saveSprites(pathSB, nameSB);

            // Get SFX path 
            input.ReadLine();
            skillSFX = input.ReadLine();

            FighterSkill entry = new FighterSkill(atkName, hitstun, basePower, faf, xPow, yPow, xShift, yShift, jc, sc, command, boxes, rehit, hitFrames, skillSFX);
			entry.setAnimation(anim);
			if (hasBack) {
				entry.setAnimationBack(animB);
			}
			if (hasProj)
				entry.setProjectile(projPath, false);
			fighterSkills.Add(entry);

			input.ReadLine();
		}
		float walkSpd, dashSpd, glideSpd, gravity;
		int maxJumps;

		// Read in fighter variables
		s = input.ReadLine();
		split = s.Split(' ');
		float.TryParse(split[split.Length - 1], out walkSpd);
		s = input.ReadLine();
		split = s.Split(' ');
		float.TryParse(split[split.Length - 1], out dashSpd);
		s = input.ReadLine();
		split = s.Split(' ');
		float.TryParse(split[split.Length - 1], out glideSpd);
		s = input.ReadLine();
		split = s.Split(' ');
		float.TryParse(split[split.Length - 1], out gravity);
		s = input.ReadLine();
		split = s.Split(' ');
		int.TryParse(split[split.Length - 1], out maxJumps);
		input.ReadLine();

		List<SpriteAnimation> fightAnims = new List<SpriteAnimation>();
		List<SpriteAnimation> fightAnimsB = new List<SpriteAnimation>();

		for (int i = 0; i < 9; i++) {
			input.ReadLine();
			float fps;
			int spriteCount;
			s = input.ReadLine();
			split = s.Split(' ');
			float.TryParse(split[split.Length - 1], out fps);
			s = input.ReadLine();
			split = s.Split(' ');
			int.TryParse(split[split.Length - 1], out spriteCount);

			Sprite[] sprites = new Sprite[spriteCount];
			string[] pathS = new string[spriteCount];
			string[] nameS = new string[spriteCount];
			for (int j = 0; j < spriteCount; j++) {
				int sheetNum;
				s = input.ReadLine();
				split = s.Split('-');
				int.TryParse(split[0], out sheetNum);
				pathS[j] = sheetP[sheetNum];
				nameS[j] = split[split.Length - 1];
				sprites[j] = getSprite(sheets[sheetNum], split[split.Length - 1]);
			}
			SpriteAnimation entry = new SpriteAnimation(sprites, new int[0], fps, true);
			entry.saveSprites(pathS, nameS);
			fightAnims.Add(entry);

			if (hasBack) {
				pathS = new string[spriteCount];
				nameS = new string[spriteCount];
				for (int j = 0; j < spriteCount; j++) {
					int sheetNum;
					s = input.ReadLine();
					split = s.Split('-');
					int.TryParse(split[0], out sheetNum);
					pathS[j] = sheetP[sheetNum];
					nameS[j] = split[split.Length - 1];
					sprites[j] = getSprite(sheets[sheetNum], split[split.Length - 1]);
				}
				entry = new SpriteAnimation(sprites, new int[0], fps, true);
				entry.saveSprites(pathS, nameS);
				fightAnimsB.Add(entry);
			}

			input.ReadLine();
		}

		// Read in icons
		input.ReadLine();
		Sprite[] icons = new Sprite[4];
		string[] pathF = new string[3];
		string[] nameF = new string[3];
		string pathW = "", nameW = "";
		for (int i = 0; i < icons.Length; i++) {
			int sheetNum;
			s = input.ReadLine();
			split = s.Split('-');
			int.TryParse(split[0], out sheetNum);
			if (i < 3) {
				pathF[i] = sheetP[sheetNum];
				nameF[i] = split[split.Length - 1];
			}
			else {
				pathW = sheetP[sheetNum];
				nameW = split[split.Length - 1];
			}
			icons[i] = getSprite(sheets[sheetNum], split[split.Length - 1]);
		}

		Fighter streetDuck = new Fighter(walkSpd, dashSpd, glideSpd, gravity, maxJumps);
		streetDuck.setAnimations(fightAnims[0], fightAnims[1], fightAnims[2], fightAnims[3], fightAnims[4], fightAnims[5], fightAnims[6], fightAnims[7], fightAnims[8]);
		if(hasBack)
			streetDuck.setBackAnimations(fightAnimsB[0], fightAnimsB[1], fightAnimsB[2], fightAnimsB[3], fightAnimsB[4], fightAnimsB[5], fightAnimsB[6], fightAnimsB[7], fightAnimsB[8]);

		streetDuck.setNormals(fighterSkills[0], fighterSkills[1], fighterSkills[2]);
		streetDuck.setCrouches(fighterSkills[3], fighterSkills[4], fighterSkills[5]);
		streetDuck.setJumps(fighterSkills[6], fighterSkills[7], fighterSkills[8]);
		for (int i = 9; i < fighterSkills.Count; i++) {
			streetDuck.addSkill(fighterSkills[i]);
		}

		Ally ducky = new Ally(duck, streetDuck, battlestats);
		for (int j = 0; j < skillList.Count; j++) {
			ducky.addSkill(skillList[j]);
		}
		ducky.setAnimations(RPGanims[0], RPGanims[1], RPGanims[2], RPGanims[3], RPGanims[4], RPGanims[5], RPGanims[6]);
		ducky.setIcons(icons[0], icons[1], icons[2], icons[3]);
		ducky.saveIcons(pathF, nameF, pathW, nameW);
		return ducky;
	}


	private element decideEle(string ele){
		switch (ele) {
			case "Fire":
				return element.FIRE;
			case "Water":
				return element.WATER;
			case "Earth":
				return element.EARTH;
			case "Light":
				return element.LIGHT;
			case "Dark":
				return element.DARK;
			default:
				return element.NEUTRAL;
		}
	}

	private battleType decideSupport(string support){
		switch (support) {
			case "Heal":
				return battleType.HEAL;
			case "Cure":
				return battleType.CURE;
			case "Status":
				return battleType.STATUS;
			case "Boost":
				return battleType.BOOST;
			default:
				return battleType.NULL;
		}
	}

	private status decideStatus(string s){
		switch(s){
			case "KO":
				return status.KO;
			case "Bleed":
				return status.BLEED;
			case "Burn":
				return status.BURN;
			case "Freeze":
				return status.FREEZE;
			case "Paralyze":
				return status.PARALYZE;
			case "Poison":
				return status.POISON;
			case "Sleep":
				return status.SLEEP;
			case "Regen":
				return status.REGEN;
			default:
				return status.NULL;
		}
	}

	private stat decideStat(string s){
		switch (s) {
			case "Atk":
				return stat.ATK;
			case "Def":
				return stat.DEF;
			case "MAtk":
				return stat.MATK;
			case "MDef":
				return stat.MDEF;
			case "Spd":
				return stat.SPD;
			default:
				return stat.NULL;
		}
	}

	private KeyCode decideKeyCode(string keyC){
		switch (keyC) {
			case "B1":
				return DataManager.savedOptions.controls[(int)key.B1];
			case "B2":
				return DataManager.savedOptions.controls[(int)key.B2];
			case "B3":
				return DataManager.savedOptions.controls[(int)key.B3];
			case "UP":
				return DataManager.savedOptions.controls[(int)key.UP];
			case "DOWN":
				return DataManager.savedOptions.controls[(int)key.DOWN];
			case "LEFT":
				return DataManager.savedOptions.controls[(int)key.LEFT];
			case "RIGHT":
				return DataManager.savedOptions.controls[(int)key.RIGHT];
			default:
				return KeyCode.None;
		}
	}
}

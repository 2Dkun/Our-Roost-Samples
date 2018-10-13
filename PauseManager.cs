using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour {

    // Cameras
    public Camera main;
    public Camera pause;

    public GameObject player;

    // Main variables
    public TextMeshProUGUI[] mainOpts;
    private string[] optDes = { 
        "Use and view items in your inventory.",
        "View the current skills of your party.",
        "Change the current equipment on your party.",
        "Rearrange the party to your fitting.",
        "Adjust game settings.",
        "Save the progress you've made so far."
    };
    public TextMeshProUGUI description;
    public TextMeshProUGUI cash;
    public GameObject desMenu;
    private int curOpt;
    private int maxOpt;

    // Dealing with submenu
    public GameObject submenu;
    public Sprite[] subSprites;
    public GameObject iSlider;

    // Variables for slots
    public TextMeshProUGUI[] names;
    public TextMeshProUGUI[] level;
    public TextMeshProUGUI[] hp;
    public TextMeshProUGUI[] mp;
    public TextMeshProUGUI[] expBar;
    public stats[] stat;
    [System.Serializable]
    public class stats {
        public TextMeshProUGUI[] value;
    }
    public GameObject[] slot;
    public GameObject[] portrait;
    public GameObject[] health;
    public GameObject[] mana;
    public GameObject[] exp;

    // Menu navigation
    private enum menu { LOAD, MAIN, ITEMS, SKILLS, EQUIPMENT, ARRANGEMENT, OPTIONS, SAVE };
    private menu curMenu = menu.LOAD;
    private itemType curItemMenu = itemType.FIELD;

    // Inventory variables
    private List<Item> inventory;
    public TextMeshProUGUI[] itemName;
    public GameObject[] itemIcons;
    private readonly Vector2[] sliderPos = {
        new Vector2(13.13f, -4.449f),
        new Vector2(13.761f, -4.449f),
        new Vector2(14.373f, -4.449f),
        new Vector2(14.985f, -4.449f),
        new Vector2(15.57f, -4.449f)
    };
    private readonly itemType[] itemT = { itemType.FIELD, itemType.BATTLE, itemType.WEAPON, itemType.ARMOR, itemType.KEY };

    // Skill varaibles
    private bool charSelecting;
    public TextMeshProUGUI[] skillInfo;
    public TextMeshProUGUI[] skillTxt;

    // Equipment variables
    private enum equipSel { CHARACTER, TYPE, EQUIP };
    private equipSel curEq = equipSel.CHARACTER;
    public TextMeshProUGUI[] equipInfo;
    public TextMeshProUGUI[] equipSlots;
    public TextMeshProUGUI[] equipStats;
    public TextMeshProUGUI guideTxt;
    public GameObject[] equipSprite;
    public GameObject statDisplay;
    public GameObject[] equipIcons;
    private List<Item> equipList;
    private readonly armorType[] equipType = { armorType.HEAD, armorType.TORSO, armorType.WEAPON, armorType.WEAPON, armorType.ACCESSORY, armorType.ACCESSORY };
    private int curEquipSel;

    // Arrangement variables
    private int curSel;

    // Option variables
    private enum options { MAIN, CONTROLS, SOUNDS, RESOLUTION, INPUT };
    private options curOption;
    public TextMeshProUGUI[] optTxt;
    public TextMeshProUGUI[] controlTxt;
    public GameObject controlDis;
    private KeyCode[] tempControls = new KeyCode[7];
    public TextMeshProUGUI[] resTxt;
    private int newResolution;
    public TextMeshProUGUI[] soundTxt;
    public TextMeshProUGUI[] soundNum;
    public GameObject[] slider;
    private int[] newVolume = new int[3];
    private int sliderDelay = 0; // Used to delay fast slider movement

    // Saving variables
    private enum save { PROMPT, SAVING };
    private save curSave = save.PROMPT;
    public TextMeshProUGUI[] saveTxt;

    // Sound Variables
    public AudioClip select, cancel;
    public GameObject mainSFX;

    // Use this for initialization
    void Start () {
        curMenu = menu.LOAD;
        // Hide all text and slots
        displayText(mainOpts, false);
        description.enabled = false;
        cash.enabled = false;
        for (int i = 0; i < slot.Length; i++) {
            names[i].enabled = false;
            level[i].enabled = false;
            displayText(stat[i].value, false);
            displayText(hp, false);
            displayText(mp, false);
            displayText(expBar, false);
        }
        displayText(saveTxt, false);
        displayText(optTxt, false);
        displayText(controlTxt, false);
        displayText(resTxt, false);
        displayText(soundTxt, false);
        displayText(soundNum, false);
        displayText(equipInfo, false);
        displayText(equipSlots, false);
        displayText(equipStats, false);
        guideTxt.enabled = false;
        displayText(skillTxt, false);
        displayText(skillInfo, false);
        displayText(itemName, false);
        displaySprite(itemIcons, false);

        // Update from saved data
        for (int i = 0; i < 3; i++) {
            newVolume[i] = DataManager.savedOptions.volume[i]; // Initialize a new volume variable
            soundNum[i].text = DataManager.savedOptions.volume[i].ToString(); // Update volume amount
            slider[i].transform.localPosition = new Vector2((DataManager.savedOptions.volume[i] * 0.019f) - 0.95f, 0);
        }
        for (int i = 0; i < 7; i++) {
            tempControls[i] = DataManager.savedOptions.controls[i]; // Update controls
            controlTxt[i].GetComponent<TextMeshProUGUI>().text = DataManager.savedOptions.controls[i].ToString();
        }
        newResolution = DataManager.savedOptions.resolution; // Update resolution
    }
    // Update is called once per frame
    void Update () {
        TitleManager.curFile.updateTime();

        if(DungeonHandler.curState == gameState.MENU) {
            switch(curMenu) {
                case menu.LOAD:
                    submenu.GetComponent<SpriteRenderer>().enabled = false;
                    iSlider.GetComponent<SpriteRenderer>().enabled = false;
                    updateSlots(TitleManager.curFile.getParty());
                    UpdateText(mainOpts,0);
                    displayText(mainOpts,true);
                    desMenu.GetComponent<SpriteRenderer>().enabled = true;
                    cash.enabled = true;
                    cash.text = "$" + TitleManager.curFile.getBalance().ToString();
                    // Hide save
                    displayText(saveTxt,false);
                    // Hide options
                    displayText(saveTxt,false);
                    displayText(optTxt,false);
                    displayText(controlTxt,false);
                    displayText(resTxt,false);
                    displayText(soundTxt,false);
                    displayText(soundNum,false);
                    controlDis.GetComponent<SpriteRenderer>().enabled = false;
                    displaySprite(slider,false);
                    description.enabled = true;
                    description.text = optDes[0];
                    // Hide equip 
                    displayText(equipInfo,false);
                    displayText(equipSlots,false);
                    displayText(equipStats,false);
                    guideTxt.enabled = false;
                    displaySprite(equipSprite,false);
                    displaySprite(equipIcons,false);
                    statDisplay.GetComponent<SpriteRenderer>().enabled = false;
                    // Hide skills
                    displayText(skillTxt,false);
                    displayText(skillInfo,false);
                    // Hide items
                    displayText(itemName,false);
                    displaySprite(itemIcons,false);

                    curOpt = 0;
                    maxOpt = mainOpts.Length;
                    curMenu = menu.MAIN;
                    break;



                case menu.MAIN:
                    if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
                        curOpt -= 1;
                        curOpt = (curOpt + maxOpt) % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
                        curOpt += 1;
                        curOpt = curOpt % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                        displayText(mainOpts,false);
                        description.enabled = false;
                        desMenu.GetComponent<SpriteRenderer>().enabled = false;

                        switch(curOpt) {
                            case 0: // ITEMS
                                curMenu = menu.ITEMS;
                                iSlider.GetComponent<SpriteRenderer>().enabled = true;
                                submenu.GetComponent<SpriteRenderer>().enabled = true;
                                submenu.GetComponent<SpriteRenderer>().sprite = subSprites[0];
                                displayText(itemName,true);
                                displaySprite(itemIcons,true);
                                curOpt = 0;
                                curSel = 0;
                                maxOpt = TitleManager.curFile.getInventoryOfType(itemT[curSel]).Count;
                                iSlider.transform.localPosition = sliderPos[curSel];
                                UpdateText(itemName,curOpt);
                                UpdateInventoryIcons();
                                guideTxt.enabled = true;
                                guideTxt.text = "DEMO VERSION: View the items in your inventory.";

                                break;
                            case 1: // SKILLS
                                curMenu = menu.SKILLS;
                                displayText(mainOpts,true);
                                description.enabled = true;
                                description.text = "Whose skills would you like to view?";
                                desMenu.GetComponent<SpriteRenderer>().enabled = true;
                                curOpt = 0;
                                maxOpt = 0;
                                for(int i = 0; i < TitleManager.curFile.getParty().Count; i++) {
                                    if(TitleManager.curFile.getParty()[i].getName().CompareTo("") != 0)
                                        maxOpt += 1;
                                    else
                                        break;
                                }
                                updateSprite(slot,curOpt);
                                charSelecting = true;
                                break;
                            case 2: // EQUIPMENT
                                curMenu = menu.EQUIPMENT;
                                curEq = equipSel.CHARACTER;
                                displayText(mainOpts,true);
                                description.enabled = true;
                                description.text = "Whose equipment do you want to view?";
                                desMenu.GetComponent<SpriteRenderer>().enabled = true;
                                curSel = 0;
                                curOpt = 0;
                                maxOpt = 0;
                                for(int i = 0; i < TitleManager.curFile.getParty().Count; i++) {
                                    if(TitleManager.curFile.getParty()[i].getName().CompareTo("") != 0)
                                        maxOpt += 1;
                                    else
                                        break;
                                }
                                updateSprite(slot,curOpt);
                                break;
                            case 3: // ARRANGEMENT
                                curMenu = menu.ARRANGEMENT;
                                displayText(mainOpts,true);
                                description.enabled = true;
                                description.text = "Choose someone to switch.";
                                desMenu.GetComponent<SpriteRenderer>().enabled = true;
                                curOpt = 0;
                                maxOpt = 0;
                                curSel = -1;
                                for(int i = 0; i < TitleManager.curFile.getParty().Count; i++) {
                                    if(TitleManager.curFile.getParty()[i].getName().CompareTo("") != 0)
                                        maxOpt += 1;
                                    else
                                        break;
                                }
                                updateSprite(slot,curOpt);
                                break;
                            case 4: // OPTIONS
                                curMenu = menu.OPTIONS;
                                curOption = options.MAIN;
                                curOpt = 0;
                                maxOpt = 3;
                                submenu.GetComponent<SpriteRenderer>().enabled = true;
                                submenu.GetComponent<SpriteRenderer>().sprite = subSprites[2];
                                UpdateText(optTxt,curOpt);
                                displayText(optTxt,true);
                                break;
                            case 5: // SAVE
                                curMenu = menu.SAVE;
                                curOpt = 0;
                                maxOpt = 2;
                                description.text = "Would you like to save your progress?";
                                UpdateText(saveTxt,curOpt + 1);
                                displayText(mainOpts,true);
                                displayText(saveTxt,true);
                                description.enabled = true;
                                desMenu.GetComponent<SpriteRenderer>().enabled = true;
                                break;
                            default:
                                break;
                        }
                        UpdateText(mainOpts,-1);
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2]) ||
                               Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B3])) {
                        // Play sound effect
                        mainSFX.GetComponent<AudioSource>().clip = cancel;
                        mainSFX.GetComponent<AudioSource>().Play();
                        curMenu = menu.LOAD;
                        DungeonHandler.curState = gameState.OVERWORLD;
                        DungeonHandler.preState = gameState.MENU;
                        SceneManager.LoadScene(TitleManager.curFile.getScene(),LoadSceneMode.Single);
                    }
                    // Update text color
                    if(Input.anyKeyDown && curMenu == menu.MAIN) {
                        UpdateText(mainOpts,curOpt);
                        description.text = optDes[curOpt];
                    }
                    break;


               
                case menu.ITEMS:
                    if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP]) && maxOpt > 0) {
                        curOpt -= 1;
                        curOpt = (curOpt + maxOpt) % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN]) && maxOpt > 0) {
                        curOpt += 1;
                        curOpt = curOpt % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT])) {
                        curSel -= 1;
                        curSel += itemT.Length;
                        curSel %= itemT.Length;
                        curOpt = 0;
                        maxOpt = TitleManager.curFile.getInventoryOfType(itemT[curSel]).Count;
                        iSlider.transform.localPosition = sliderPos[curSel];
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])) {
                        curSel += 1;
                        curSel %= itemT.Length;
                        curOpt = 0;
                        maxOpt = TitleManager.curFile.getInventoryOfType(itemT[curSel]).Count;
                        iSlider.transform.localPosition = sliderPos[curSel];
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                        // nothing for demo?
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                        displayText(itemName,false);
                        displaySprite(itemIcons,false);

                        guideTxt.enabled = false;
                        iSlider.GetComponent<SpriteRenderer>().enabled = false;
                        submenu.GetComponent<SpriteRenderer>().enabled = false;
                        desMenu.GetComponent<SpriteRenderer>().enabled = true;
                        displayText(mainOpts,true);
                        curOpt = 0;
                        UpdateText(mainOpts,curOpt);
                        description.enabled = true;
                        description.text = optDes[curOpt];
                        maxOpt = mainOpts.Length;
                        curMenu = menu.MAIN;
                    }

                    if(Input.anyKeyDown && curMenu == menu.ITEMS) {
                        UpdateInventoryIcons();
                        UpdateText(itemName,curOpt % itemName.Length);
                    }

                    /*
                     * Maybe make as individual function
                     * WILL DEAL WITH THIS ONCE WE IMPLEMENT USING ITEMS 
                     */
                    switch(curItemMenu) {
                        default:
                            break;
                    }
                    break;



                case menu.SKILLS:
                    if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
                        curOpt -= 1;
                        curOpt = (curOpt + maxOpt) % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
                        curOpt += 1;
                        curOpt = curOpt % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT]) && !charSelecting) {
                        curOpt -= maxOpt / 2;
                        curOpt = (curOpt + maxOpt) % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT]) && !charSelecting) {
                        curOpt += maxOpt / 2;
                        curOpt = curOpt % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                        if(charSelecting) {
                            displayText(mainOpts,false);
                            description.enabled = false;
                            desMenu.GetComponent<SpriteRenderer>().enabled = false;
                            submenu.GetComponent<SpriteRenderer>().enabled = true;
                            submenu.GetComponent<SpriteRenderer>().sprite = subSprites[1];
                            displayText(skillTxt,true);
                            displayText(skillInfo,true);
                            guideTxt.enabled = true;
                            guideTxt.text = "Can only view skill for now.";
                            curSel = curOpt;
                            curOpt = 0;
                            maxOpt = TitleManager.curFile.getParty()[curSel].getSkillAmt() - 1;
                            for(int i = 0; i < skillTxt.Length; i++) {
                                if(i < maxOpt)
                                    skillTxt[i].text = TitleManager.curFile.getParty()[curSel].getSkill(i + 1).getName();
                                else
                                    skillTxt[i].text = "----------";
                            }

                            charSelecting = false;
                        } else {
                            // Nothing for now, just allowing to view
                        }
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                        if(charSelecting) {
                            curMenu = menu.MAIN;
                            curOpt = 1;
                            maxOpt = 6;
                            curSel = -1;
                            description.text = optDes[curOpt];
                            UpdateText(mainOpts,curOpt);
                            updateSprite(slot,-1);
                        } else {
                            displayText(mainOpts,true);
                            description.enabled = true;
                            description.text = "Whose skills would you like to view?";
                            desMenu.GetComponent<SpriteRenderer>().enabled = true;
                            guideTxt.enabled = false;
                            curOpt = curSel;
                            maxOpt = 0;
                            for(int i = 0; i < TitleManager.curFile.getParty().Count; i++) {
                                if(TitleManager.curFile.getParty()[i].getName().CompareTo("") != 0)
                                    maxOpt += 1;
                                else
                                    break;
                            }
                            updateSprite(slot,curOpt);
                            charSelecting = true;

                            submenu.GetComponent<SpriteRenderer>().enabled = false;
                            displayText(skillTxt,false);
                            displayText(skillInfo,false);
                        }
                    }
                    if(Input.anyKeyDown && curMenu == menu.SKILLS) {
                        if(charSelecting)
                            updateSprite(slot,curOpt);
                        else {
                            UpdateText(skillTxt,curOpt);
                            Skill curSkill = TitleManager.curFile.getParty()[curSel].getSkill(curOpt + 1);
                            skillInfo[0].text = curSkill.getName();
                            skillInfo[1].text = curSkill.getDes();
                            skillInfo[2].text = "Type: " + curSkill.getType().ToString();
                            skillInfo[3].text = "Element: " + curSkill.getElement().ToString();
                            skillInfo[4].text = "Base Power: " + curSkill.getPower().ToString();
                            skillInfo[5].text = "MP Cost: " + curSkill.getMpCost().ToString();
                            skillInfo[6].text = "Target: " + curSkill.getRange().ToString();
                            if(curSkill.getEndDist() == -1)
                                skillInfo[7].text = "Range: pretty far";
                            else
                                skillInfo[7].text = "Range: " + curSkill.getEndDist().ToString() + " dft";
                        }
                    }
                    break;



                case menu.EQUIPMENT:
                    if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
                        curOpt -= 1;
                        curOpt = (curOpt + maxOpt) % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
                        curOpt += 1;
                        curOpt = curOpt % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT]) && curEq == equipSel.EQUIP) {
                        curOpt -= maxOpt / 2;
                        curOpt = (curOpt + maxOpt) % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT]) && curEq == equipSel.EQUIP) {
                        curOpt += maxOpt / 2;
                        curOpt = curOpt % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                        switch(curEq) {
                            case equipSel.CHARACTER:
                                curEq = equipSel.TYPE;
                                curSel = curOpt;
                                curOpt = 0;
                                maxOpt = equipSlots.Length / 2;

                                displayText(equipInfo,true);
                                displayText(equipStats,true);
                                displayText(equipSlots,true); // only half
                                displaySprite(equipSprite,true);
                                displaySprite(equipIcons,true); // only half
                                statDisplay.GetComponent<SpriteRenderer>().enabled = true;
                                guideTxt.enabled = true;
                                guideTxt.text = "Decide which equipment to change.";
                                for(int i = 0; i < equipSlots.Length / 2; i++) {
                                    equipSlots[i + (equipSlots.Length / 2)].enabled = false;
                                    equipIcons[i + (equipSlots.Length / 2)].GetComponent<SpriteRenderer>().enabled = false;
                                    equipSlots[i].text = TitleManager.curFile.getParty()[curSel].getEquips()[i].getName();
                                    equipIcons[i].GetComponent<SpriteRenderer>().sprite = TitleManager.curFile.getParty()[curSel].getEquips()[i].getIcon();
                                }

                                submenu.GetComponent<SpriteRenderer>().enabled = true;
                                submenu.GetComponent<SpriteRenderer>().sprite = subSprites[1];
                                slot[curSel].GetComponent<SpriteRenderer>().color = new Color(0.7f,1,0.7f);
                                displayText(mainOpts,false);
                                description.enabled = false;
                                desMenu.GetComponent<SpriteRenderer>().enabled = false;
                                break;
                            case equipSel.TYPE:
                                curEq = equipSel.EQUIP;
                                displayText(equipSlots,true);
                                displaySprite(equipSprite,true);
                                guideTxt.text = "Switch with what equipment?";
                                curEquipSel = curOpt;
                                equipList = new List<Item>();
                                if(curEquipSel == 0)
                                    equipList.Add(TitleManager.curFile.getEmptyHead());
                                else if(curEquipSel == 1)
                                    equipList.Add(TitleManager.curFile.getEmptyTorso());
                                else if(curEquipSel == 2)
                                    equipList.Add(TitleManager.curFile.getEmptyLeft());
                                else if(curEquipSel == 3)
                                    equipList.Add(TitleManager.curFile.getEmptyRight());
                                else
                                    equipList.Add(TitleManager.curFile.getEmptyAccessory());

                                for(int i = 0; i < TitleManager.curFile.getArmor(equipType[curEquipSel]).Count; i++) {
                                    equipList.Add(TitleManager.curFile.getArmor(equipType[curEquipSel])[i]);
                                }

                                int temp = 0;
                                for(int i = temp; i < equipSlots.Length; i++) {
                                    if(i < equipList.Count) {
                                        equipSlots[i].text = equipList[i].getName();
                                        equipSlots[i].text += " x" + equipList[i].getAmt().ToString();
                                        equipIcons[i].GetComponent<SpriteRenderer>().enabled = true;
                                        equipIcons[i].GetComponent<SpriteRenderer>().sprite = equipList[i].getIcon();
                                    } else {
                                        equipSlots[i].text = "----------";
                                        equipIcons[i].GetComponent<SpriteRenderer>().enabled = false;
                                    }
                                }
                                curOpt = 0;
                                maxOpt = equipList.Count;
                                break;
                            case equipSel.EQUIP:
                                // Swap new equip with old one
                                Item curEquip = TitleManager.curFile.getParty()[curSel].getEquips()[curEquipSel];
                                Item newEquip = equipList[curOpt].copy();
                                newEquip.onlyOne();
                                // Add current equip to inventory if any
                                if(!TitleManager.curFile.itemIsEmpty(curEquip)) {
                                    TitleManager.curFile.addToInventory(curEquip);
                                }
                                // Remove newly equipped item from inventory
                                if(!TitleManager.curFile.itemIsEmpty(newEquip)) {
                                    TitleManager.curFile.removeItem(newEquip,1);
                                }
                                // Change the current equip into the new one
                                TitleManager.curFile.getParty()[curSel].equipItem(newEquip,curEquipSel);
                                updateSlots(TitleManager.curFile.getParty());

                                // Return to equip selection
                                curEq = equipSel.TYPE;
                                displayText(equipSlots,true); // only half
                                displaySprite(equipIcons,true); // only half 
                                guideTxt.text = "Decide which equipment to change.";
                                for(int i = 0; i < equipSlots.Length / 2; i++) {
                                    equipSlots[i + (equipSlots.Length / 2)].enabled = false;
                                    equipIcons[i + (equipSlots.Length / 2)].GetComponent<SpriteRenderer>().enabled = false;
                                    equipSlots[i].text = TitleManager.curFile.getParty()[curSel].getEquips()[i].getName();
                                    equipIcons[i].GetComponent<SpriteRenderer>().sprite = TitleManager.curFile.getParty()[curSel].getEquips()[i].getIcon();
                                }
                                curOpt = curEquipSel;
                                maxOpt = equipSlots.Length / 2;
                                break;
                            default:
                                curMenu = menu.MAIN;
                                curOpt = 0;
                                break;
                        }
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                        switch(curEq) {
                            case equipSel.CHARACTER:
                                curMenu = menu.MAIN;
                                curOpt = 2;
                                maxOpt = 6;
                                curSel = -1;
                                description.text = optDes[curOpt];
                                UpdateText(mainOpts,curOpt);
                                updateSprite(slot,-1);
                                break;
                            case equipSel.TYPE:
                                curEq = equipSel.CHARACTER;
                                displayText(mainOpts,true);
                                description.enabled = true;
                                description.text = "Whose equipment do you want to view?";
                                desMenu.GetComponent<SpriteRenderer>().enabled = true;
                                curOpt = curSel;
                                maxOpt = 0;
                                for(int i = 0; i < TitleManager.curFile.getParty().Count; i++) {
                                    if(TitleManager.curFile.getParty()[i].getName().CompareTo("") != 0)
                                        maxOpt += 1;
                                    else
                                        break;
                                }
                                updateSprite(slot,curOpt);

                                displayText(equipInfo,false);
                                displayText(equipStats,false);
                                displayText(equipSlots,false);
                                displaySprite(equipSprite,false);
                                displaySprite(equipIcons,false);
                                statDisplay.GetComponent<SpriteRenderer>().enabled = false;
                                submenu.GetComponent<SpriteRenderer>().enabled = false;
                                guideTxt.enabled = false;
                                break;
                            case equipSel.EQUIP:
                                curEq = equipSel.TYPE;
                                displayText(equipSlots,true); // only half
                                displaySprite(equipIcons,true); // only half 
                                guideTxt.text = "Decide which equipment to change.";
                                for(int i = 0; i < equipSlots.Length / 2; i++) {
                                    equipSlots[i + (equipSlots.Length / 2)].enabled = false;
                                    equipIcons[i + (equipSlots.Length / 2)].GetComponent<SpriteRenderer>().enabled = false;
                                    equipSlots[i].text = TitleManager.curFile.getParty()[curSel].getEquips()[i].getName();
                                    equipIcons[i].GetComponent<SpriteRenderer>().sprite = TitleManager.curFile.getParty()[curSel].getEquips()[i].getIcon();
                                }
                                curOpt = curEquipSel;
                                maxOpt = equipSlots.Length / 2;
                                break;
                            default:
                                curMenu = menu.MAIN;
                                curOpt = 0;
                                break;
                        }
                    }
                    if(Input.anyKeyDown && curMenu == menu.EQUIPMENT) {
                        switch(curEq) {
                            case equipSel.CHARACTER:
                                updateSprite(slot,curOpt);
                                break;
                            case equipSel.TYPE:
                                updateEquipInfo(TitleManager.curFile.getParty()[curSel].getEquips()[curOpt]);
                                UpdateText(equipSlots,curOpt);
                                break;
                            case equipSel.EQUIP:
                                updateEquipInfo(equipList[curOpt]);
                                UpdateText(equipSlots,curOpt);
                                break;
                            default:
                                break;
                        }
                    }
                    break;



                case menu.ARRANGEMENT:
                    if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP])) {
                        do {
                            curOpt -= 1;
                            curOpt = (curOpt + maxOpt) % maxOpt;
                        } while(curOpt == curSel);
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])) {
                        do {
                            curOpt += 1;
                            curOpt = curOpt % maxOpt;
                        } while(curOpt == curSel);
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                        // Choose another target if first one was selected
                        if(curSel < 0) {
                            description.text = "Choose another person to switch with.";
                            curSel = curOpt;
                            do {
                                curOpt += 1;
                                curOpt = curOpt % maxOpt;
                            } while(curOpt == curSel);
                        }
                        // Else swap the two targets
                        else {
                            List<Ally> party = TitleManager.curFile.getParty();
                            Ally temp = party[curSel];
                            party[curSel] = party[curOpt];
                            party[curOpt] = temp;
                            updateSlots(party);
                            // Go back to menu
                            description.text = "Choose someone to switch.";
                            curSel = -1;
                        }
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                        // Exit out of arrangement if no one was selected yet
                        if(curSel < 0) {
                            curMenu = menu.MAIN;
                            curOpt = 3;
                            maxOpt = 6;
                            curSel = -1;
                            description.text = optDes[curOpt];
                            UpdateText(mainOpts,curOpt);
                            updateSprite(slot,curSel);
                        }
                        // Otherwise go back to first selection
                        else {
                            description.text = "Choose someone to switch.";
                            curOpt = curSel;
                            curSel = -1;
                        }
                    }
                    if(Input.anyKeyDown && curMenu == menu.ARRANGEMENT) {
                        updateSprite(slot,curOpt);
                        if(curSel >= 0)
                            slot[curSel].GetComponent<SpriteRenderer>().color = new Color(0.7f,1,0.7f);
                    }
                    break;



                case menu.OPTIONS:
                    if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP]) && curOption != options.INPUT) {
                        curOpt -= 1;
                        curOpt = (curOpt + maxOpt) % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN]) && curOption != options.INPUT) {
                        curOpt += 1;
                        curOpt = curOpt % maxOpt;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                        switch(curOption) {
                            case options.MAIN:
                                displayText(optTxt,false);
                                submenu.GetComponent<SpriteRenderer>().sprite = subSprites[3];
                                if(curOpt == 0) {
                                    curOption = options.CONTROLS;
                                    maxOpt = 8;
                                    // Show controls
                                    displayText(controlTxt,true);
                                    controlDis.GetComponent<SpriteRenderer>().enabled = true;
                                } else if(curOpt == 1) {
                                    curOption = options.RESOLUTION;
                                    maxOpt = 5;
                                    // Show resolutions
                                    displayText(resTxt,true);
                                } else {
                                    curOption = options.SOUNDS;
                                    maxOpt = 4;
                                    // Show sounds
                                    displayText(soundTxt,true);
                                    displayText(soundNum,true);
                                    displaySprite(slider,true);
                                }
                                curOpt = 0;
                                break;

                            case options.CONTROLS:
                                // APPLY
                                if(curOpt == 7) {
                                    // Update menu
                                    curOption = options.MAIN;
                                    submenu.GetComponent<SpriteRenderer>().sprite = subSprites[2];
                                    curOpt = 0; // Selection on controls
                                    maxOpt = 3;
                                    // Change displays
                                    displayText(optTxt,true);
                                    displayText(controlTxt,false);
                                    controlDis.GetComponent<SpriteRenderer>().enabled = false;
                                    // Update controls
                                    for(int i = 0; i < 7; i++) {
                                        tempControls[i] = DataManager.savedOptions.controls[i];
                                    }
                                    DataManager.SaveOptions(); // Save updates
                                } else {
                                    // Update menu
                                    curOption = options.INPUT;
                                    controlTxt[curOpt].color = Color.red; // Indicates "awaiting input"
                                }
                                break;

                            case options.RESOLUTION:
                                if(curOpt < 4) {
                                    UpdateRes(curOpt); // Change to selected resolution
                                    newResolution = curOpt; // Keep track of changed resolution
                                }
                                // APPLY
                                else {
                                    // Update menu
                                    curOption = options.MAIN;
                                    submenu.GetComponent<SpriteRenderer>().sprite = subSprites[2];
                                    curOpt = 1; // Selection on resolution
                                    maxOpt = 3;
                                    // Change displays
                                    displayText(optTxt,true);
                                    displayText(resTxt,false);
                                    // Save new resolution
                                    DataManager.savedOptions.resolution = newResolution;
                                    DataManager.SaveOptions();
                                }
                                break;

                            case options.SOUNDS:
                                // APPLY
                                if(curOpt == 3) {
                                    // Update menu
                                    curOption = options.MAIN;
                                    submenu.GetComponent<SpriteRenderer>().sprite = subSprites[2];
                                    curOpt = 2; // Selection on sounds
                                    maxOpt = 3;
                                    // Change displays
                                    displayText(optTxt,true);
                                    displayText(soundTxt,false);
                                    displayText(soundNum,false);
                                    displaySprite(slider,false);
                                    // Save the changes made to the volume
                                    for(int i = 0; i < 3; i++) {
                                        DataManager.savedOptions.volume[i] = newVolume[i];
                                    }
                                    DataManager.SaveOptions();
                                    // Update volumes
                                    this.GetComponent<SoundManager>().UpdateVolumes();
                                }
                                break;

                            default:
                                break;
                        }
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                        if(curOption == options.MAIN) {
                            curMenu = menu.MAIN;
                            curOpt = 4;
                            maxOpt = mainOpts.Length;
                            submenu.GetComponent<SpriteRenderer>().enabled = false;
                            UpdateText(optTxt,curOpt);
                            displayText(mainOpts,true);
                            displayText(optTxt,false);
                            description.enabled = true;
                            desMenu.GetComponent<SpriteRenderer>().enabled = true;
                            description.text = optDes[curOpt];
                            UpdateText(mainOpts,curOpt);
                        } else {
                            // Hide everything
                            if(curOption == options.CONTROLS) {
                                displayText(controlTxt,false);
                                curOpt = 0;
                                controlDis.GetComponent<SpriteRenderer>().enabled = false;
                                // Revert to previous controls
                                for(int i = 0; i < 7; i++) {
                                    DataManager.savedOptions.controls[i] = tempControls[i];
                                    controlTxt[i].GetComponent<TextMeshProUGUI>().text = DataManager.savedOptions.controls[i].ToString();
                                }
                            } else if(curOption == options.RESOLUTION) {
                                displayText(resTxt,false);
                                curOpt = 1;
                                // Revert to old resolution
                                UpdateRes(DataManager.savedOptions.resolution);
                                newResolution = DataManager.savedOptions.resolution;
                            } else {
                                displayText(soundTxt,false);
                                displayText(soundNum,false);
                                displaySprite(slider,false);
                                curOpt = 2;
                                // Reset volume to previous settings
                                for(int i = 0; i < 3; i++) {
                                    soundNum[i].text = DataManager.savedOptions.volume[i].ToString();
                                    newVolume[i] = DataManager.savedOptions.volume[i]; // There is no new volume
                                    slider[i].transform.localPosition = new Vector2((DataManager.savedOptions.volume[i] * 0.019f) - 0.95f,0);
                                }
                            }
                            // Go back to main
                            curOption = options.MAIN;
                            maxOpt = 3;
                            UpdateText(optTxt,curOpt);
                            displayText(optTxt,true);
                            submenu.GetComponent<SpriteRenderer>().sprite = subSprites[2];
                        }
                    }
                      // Horizonatal input only needed for sound menu
                      else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT]) || (Input.GetKey(DataManager.savedOptions.controls[(int)key.LEFT]) && sliderDelay > 15)) {
                        if(curOption == options.SOUNDS && curOpt < 4 && newVolume[curOpt] > 0) {
                            newVolume[curOpt] -= 1; // Decrease volume amount
                            soundNum[curOpt].text = newVolume[curOpt].ToString(); // Update volume amount
                            slider[curOpt].transform.localPosition = new Vector2(slider[curOpt].transform.localPosition.x - 0.019f,0);
                        }
                        sliderDelay += 1;
                    } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT]) || (Input.GetKey(DataManager.savedOptions.controls[(int)key.RIGHT]) && sliderDelay > 15)) {
                        if(curOption == options.SOUNDS && curOpt < 4 && newVolume[curOpt] < 100) {
                            newVolume[curOpt] += 1; // Increase volume amount
                            soundNum[curOpt].text = newVolume[curOpt].ToString(); // Update volume amount
                            slider[curOpt].transform.localPosition = new Vector2(slider[curOpt].transform.localPosition.x + 0.019f,0);
                        }
                        sliderDelay += 1;
                    }

                    // Begin the slider delay process if left or right were held down
                    if(sliderDelay > 0 && sliderDelay <= 15) {
                        sliderDelay += 1;
                    }
                    // Reset slider delay if there is no horizontal input
                    if(!Input.GetKey(DataManager.savedOptions.controls[(int)key.LEFT])) {
                        if(!Input.GetKey(DataManager.savedOptions.controls[(int)key.RIGHT])) {
                            sliderDelay = 0;
                        }
                    }

                    if(Input.anyKeyDown) {
                        switch(curOption) {
                            case options.MAIN:
                                UpdateText(optTxt,curOpt);
                                break;
                            case options.CONTROLS:
                                UpdateText(controlTxt,curOpt);
                                break;
                            case options.INPUT:
                                if(Input.anyKeyDown) {
                                    // Make sure this input doesn't already exist
                                    bool newKey = true;
                                    KeyCode curInput = DetermineKey();
                                    for(int i = 0; i < 7; i++) {
                                        // Must be unique key and not escape since it's used to reset inputs
                                        if(curInput == DataManager.savedOptions.controls[i] || curInput == KeyCode.None || curInput == KeyCode.Escape) {
                                            newKey = false;
                                            break;
                                        }
                                    }
                                    // If it is a new key
                                    if(newKey) {
                                        curOption = options.CONTROLS;
                                        DataManager.savedOptions.controls[curOpt] = curInput;
                                        controlTxt[curOpt].GetComponent<TextMeshProUGUI>().text = curInput.ToString();
                                        controlTxt[curOpt].color = Color.black;
                                    }
                                }
                                break;
                            case options.RESOLUTION:
                                UpdateText(resTxt,curOpt);
                                if(newResolution != curOpt) {
                                    resTxt[newResolution].color = Color.blue;
                                }
                                break;
                            case options.SOUNDS:
                                UpdateText(soundTxt,curOpt);
                                break;
                            default:
                                break;
                        }
                    }
                    break;



                case menu.SAVE:
                    if(curSave == save.PROMPT) {
                        // Alternate between yes and no
                        if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT]) ||
                            Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])) {
                            curOpt += 1;
                            curOpt = (curOpt + maxOpt) % maxOpt;
                        } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                            // Save file
                            if(curOpt == 0) {
                                curSave = save.SAVING;
                            }
                            // Exit save menu
                            else {
                                submenu.GetComponent<SpriteRenderer>().enabled = false;
                                displayText(mainOpts,true);
                                displayText(saveTxt,false);
                                curOpt = 5;
                                description.enabled = true;
                                description.text = optDes[curOpt];
                                maxOpt = mainOpts.Length;
                                curMenu = menu.MAIN;
                            }
                        }
                          // Exit save menu
                          else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                            submenu.GetComponent<SpriteRenderer>().enabled = false;
                            displayText(mainOpts,true);
                            displayText(saveTxt,false);
                            curOpt = 5;
                            description.enabled = true;
                            description.text = optDes[curOpt];
                            maxOpt = mainOpts.Length;
                            curMenu = menu.MAIN;
                            UpdateText(mainOpts,curOpt);
                        }
                        if(Input.anyKeyDown) {
                            UpdateText(saveTxt,curOpt + 1);
                        }
                    } else {
                        // Save file 1
                        if(TitleManager.curSave == 1) {
                            DataManager.file1 = TitleManager.curFile;
                            DataManager.Save(1);
                        }
                        // Save file 2
                        else if(TitleManager.curSave == 2) {
                            DataManager.file2 = TitleManager.curFile;
                            DataManager.Save(2);
                        }
                        // Save file 3
                        else {
                            DataManager.file3 = TitleManager.curFile;
                            DataManager.Save(3);
                        }

                        // Then exit save menu
                        submenu.GetComponent<SpriteRenderer>().enabled = false;
                        displayText(mainOpts,true);
                        displayText(saveTxt,false);
                        curOpt = 5;
                        UpdateText(mainOpts,curOpt);
                        description.enabled = true;
                        description.text = "Saved file!";
                        maxOpt = mainOpts.Length;
                        curSave = save.PROMPT;
                        curMenu = menu.MAIN;
                    }
                    break;



                default:
                    curMenu = menu.LOAD;
                    DungeonHandler.curState = gameState.OVERWORLD;
                    break;
            }

            // Play sound effect
            if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.UP]) || Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.DOWN])
                || Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.LEFT]) || Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.RIGHT])
                || Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B1])) {
                mainSFX.GetComponent<AudioSource>().clip = select;
                mainSFX.GetComponent<AudioSource>().Play();
            } else if(Input.GetKeyDown(DataManager.savedOptions.controls[(int)key.B2])) {
                mainSFX.GetComponent<AudioSource>().clip = cancel;
                mainSFX.GetComponent<AudioSource>().Play();
            }

        }
    }

    private void updateSlots(List<Ally> party){
        for (int i = 0; i < slot.Length; i++) {
            if (party[i].getName().CompareTo("") == 0) {
                names[i].enabled = false;
                level[i].enabled = false;
                displayText(stat[i].value, false);
                hp[i].enabled = false;
                mp[i].enabled = false;
                expBar[i].enabled = false;
                slot[i].GetComponent<SpriteRenderer>().enabled = false;
                portrait[i].GetComponent<SpriteRenderer>().enabled = false;
                health[i].GetComponent<SpriteRenderer>().enabled = false;
                mana[i].GetComponent<SpriteRenderer>().enabled = false;
                exp[i].GetComponent<SpriteRenderer>().enabled = false;
            }
            else {
                names[i].enabled = true;
                level[i].enabled = true;
                displayText(stat[i].value, true);
                hp[i].enabled = true;
                mp[i].enabled = true;
                expBar[i].enabled = true;
                slot[i].GetComponent<SpriteRenderer>().enabled = true;
                portrait[i].GetComponent<SpriteRenderer>().enabled = true;
                health[i].GetComponent<SpriteRenderer>().enabled = true;
                mana[i].GetComponent<SpriteRenderer>().enabled = true;
                exp[i].GetComponent<SpriteRenderer>().enabled = true;

                names[i].text = party[i].getName();
                level[i].text = "Lv. " + party[i].getLevel().ToString();
                for (int j = 0; j < stat[i].value.Length; j++) {
                    stat[i].value[j].text = party[i].getStats()[j+2].ToString(); 
                }
                portrait[i].GetComponent<SpriteRenderer>().sprite = party[i].getFace();
                // Update hp and mp text display
                hp[i].text = party[i].getCurHP().ToString() + "/" + party[i].getHP().ToString();
                mp[i].text = party[i].getCurMP().ToString() + "/" + party[i].getMP().ToString();
                expBar[i].text = party[i].getCurExp().ToString() + "/" + party[i].getMaxExp().ToString();
                // Update the meters by their ratios
                float hpRatio = (float)party[i].getCurHP() / (float)party[i].getHP();
                float mpRatio = (float)party[i].getCurMP() / (float)party[i].getMP();
                float expRatio = (float)party[i].getCurExp() / (float)party[i].getMaxExp();
                health[i].transform.localScale = new Vector2(hpRatio, health[i].transform.localScale.y);
                mana[i].transform.localScale = new Vector2(mpRatio, mana[i].transform.localScale.y);
                exp[i].transform.localScale = new Vector2(expRatio, exp[i].transform.localScale.y);
            }
        }
    }

    private void displayText(TextMeshProUGUI[] t, bool display){
        for (int i = 0; i < t.Length; i++) {
            t[i].enabled = display;
        }
    }

    private void UpdateText(TextMeshProUGUI[] t, int selection){
        for(int i = 0; i < t.Length; i++){
            if(i == selection)
                t[i].color = Color.black;
            else
                t[i].color = Color.white;
        }
    }

    private void displaySprite(GameObject[] s, bool display){
        for (int i = 0; i < s.Length; i++) {
            s[i].GetComponent<SpriteRenderer>().enabled = display;
        }
    }

    private void updateSprite(GameObject[] s, int sel){
        for (int i = 0; i < s.Length; i++) {
            if (i == sel)
                s[i].GetComponent<SpriteRenderer>().color = new Color(1, 0.7f, 0.7f);
            else
                s[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
    }

    // Looks through all of the possible KeyCodes and determines if that's the one being pressed
    private KeyCode DetermineKey() {
        int maxKey = System.Enum.GetNames(typeof(KeyCode)).Length;
        for (int i = 0; i < maxKey; i++) {
            if (Input.GetKey((KeyCode)i)) {
                return (KeyCode)i;
            }
        }
        return KeyCode.None;
    }

    // Changes the resolution according to a dedicated value
    private void UpdateRes(int res){
        switch (res) {
            case 0:
                Screen.SetResolution(1600, 960, true);
                break;
            case 1:
                Screen.SetResolution(400, 240, false);
                break;
            case 2:
                Screen.SetResolution(800, 480, false);
                break;
            case 3:
                Screen.SetResolution(800, 480, false);
                break;
            default:
                break;
        }
    }

    // Changes display of current viewed equip
    private void updateEquipInfo(Item thing){
        equipInfo[0].text = thing.getName();
        equipInfo[1].text = thing.getDes();
        for (int i = 0; i < equipStats.Length; i++) {
            int value = thing.getStats()[i + 2];
            if (value > 0) {
                equipStats[i].color = Color.black;
                equipStats[i].text = "+" + value.ToString();
            }
            else if (value < 0) {
                equipStats[i].color = Color.red;
                equipStats[i].text = value.ToString();
            }
            else {
                equipStats[i].color = Color.white;
                equipStats[i].text = value.ToString();
            }
        }
        equipSprite[0].GetComponent<SpriteRenderer>().sprite = thing.getIcon();
    }

    private void UpdateInventoryIcons(){
        List<Item> curInven = TitleManager.curFile.getInventoryOfType(itemT[curSel]);
        int start = curOpt / itemName.Length;
        for(int i = 0; i < itemName.Length; i++){
            if ((start + i) < maxOpt) {
                itemName[i].text = curInven[start + i].getName();
                itemName[i].text += " x" + curInven[start + i].getAmt().ToString();
                itemIcons[i].GetComponent<SpriteRenderer>().enabled = true;
                itemIcons[i].GetComponent<SpriteRenderer>().sprite = curInven[start + i].getIcon();
            }
            else {
                itemName[i].text = "------------------------------";
                itemIcons[i].GetComponent<SpriteRenderer>().enabled = false;
            }
        }
    }
}

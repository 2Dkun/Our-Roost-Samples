using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum fighterState {GROUNDED, AERIAL, ATTACKSEL, ATTACK, HURT};

[System.Serializable]
public class Fighter {

    // ---Fields---
    private fighterState curState;
    private List<FighterSkill> normalSkill;
    private List<FighterSkill> crouchingSkill;
    private List<FighterSkill> jumpingSkill;
    private List<FighterSkill> skillList;
    private float xVel, yVel;
    private float walkSpeed, dashSpeed, glideSpeed;
    private float gravity;
    private int maxJumps; 
    private KeyCode[] buffer;
    private bool projectileOut;
    private float slideBack;
    private int curLevel;

    // Booleans for input
    private bool[] onPress = new bool[7];
    private bool[] pressing = new bool[7];
    private bool[] released = new bool[7];

    // Animation variables
    private bool hasDiffBack;
    private SpriteAnimation idle; // Idle animation
    private SpriteAnimation idleAct; // Emote for when the combatant is in idle for a while
    private SpriteAnimation glide;
    private SpriteAnimation walk;
    private SpriteAnimation dash;
    private SpriteAnimation crouch;
    private SpriteAnimation jump, djump;
    private SpriteAnimation inAir;

    // Animation variables for facing back
    private SpriteAnimation idleB; // Idle animation
    private SpriteAnimation idleActB; // Emote for when the combatant is in idle for a while
    private SpriteAnimation glideB;
    private SpriteAnimation walkB;
    private SpriteAnimation dashB;
    private SpriteAnimation crouchB;
    private SpriteAnimation jumpB, djumpB;
    private SpriteAnimation inAirB;

    // ---Constructor---
    public Fighter(float walkSpd, float dashSpd, float glideSpd, float gravity, int maxJumps){ 
        this.normalSkill = new List<FighterSkill>();
        this.crouchingSkill = new List<FighterSkill>();
        this.jumpingSkill = new List<FighterSkill>();
        this.skillList = new List<FighterSkill>();

        this.walkSpeed = walkSpd;
        this.dashSpeed = dashSpd;
        this.glideSpeed = glideSpd;
        this.gravity = gravity;
        this.maxJumps = maxJumps;

        this.buffer = new KeyCode[25];
        this.curLevel = 5;
    }

    // Set normal skills
    public void setNormals(FighterSkill l, FighterSkill m, FighterSkill h){
        this.normalSkill.Add(l);
        this.normalSkill.Add(m);
        this.normalSkill.Add(h);
        return;
    }
    public void setCrouches(FighterSkill l, FighterSkill m, FighterSkill h){
        this.crouchingSkill.Add(l);
        this.crouchingSkill.Add(m);
        this.crouchingSkill.Add(h);
        return;
    }
    public void setJumps(FighterSkill l, FighterSkill m, FighterSkill h){
        this.jumpingSkill.Add(l);
        this.jumpingSkill.Add(m);
        this.jumpingSkill.Add(h);
        return;
    }

    // ---Access Functions---
    public fighterState getState() { return this.curState; }

    public GameObject getProjectile() {
        if(this.projectileOut && this.curSkill != null){
            this.projectileOut = false;
            if (this.curSkill.isProjectile()) {
                return this.curSkill.getProjectile();
            }
        }
        return null;
    }

    public List<FighterSkill> getAllSkills() {
        List<FighterSkill> theList = new List<FighterSkill>();
        for (int i = 0; i < normalSkill.Count; i++)
            theList.Add(normalSkill[i]);
        for (int i = 0; i < crouchingSkill.Count; i++)
            theList.Add(crouchingSkill[i]);
        for (int i = 0; i < jumpingSkill.Count; i++)
            theList.Add(jumpingSkill[i]);
        for (int i = 0; i < this.skillList.Count && i < (this.curLevel-5) / 10; i++)
            theList.Add(skillList[i]);
        return theList;
    }

    public int getHitStun()             { return this.curSkill.getHitStun(); }
    public int getBasePow()             { return this.curSkill.getBasePow(); }
    public float getXPow()              { return this.curSkill.getXPow(); }
    public float getYPow()              { return this.curSkill.getYPow(); }

    // Functions for leveling up and learning skills
    public void updateLevel(int newLevel){
        this.curLevel = newLevel;
    }
    public FighterSkill getCurSkill(){
        return this.curSkill;
    }
    public FighterSkill getLearnedSkill(){
        int learnLevel = (this.curLevel - 6) / 10;
        if ((this.curLevel - 5) % 10 == 0 && learnLevel < this.skillList.Count)
            return this.skillList[learnLevel];
        return null;
    }

    // ---Manipulation Procedures---
    public void resetPos(float x, float y){}
    // Change variable to keep track of character slideback
    public void slideDudeBack(){
        this.slideBack = 0.5f;
    }

    // Add a fighter skill to the character's skill list
    public void addSkill(FighterSkill s){
        this.skillList.Add(s);
        return;
    }

    // Change input booleans based 
    public void buttonDown(key button){
        int i = (int)button;
        onPress[i] = true;
        pressing[i] = true;
        released[i] = false;
    }
    public void buttonHold(key button){
        int i = (int)button;
        onPress[i] = false;
        pressing[i] = true;
        released[i] = false;
    }
    public void buttonUp(key button){
        int i = (int)button;
        onPress[i] = false;
        pressing[i] = false;
        released[i] = true;
    }
    public void changeButton(key button, bool isTrue){
        int i = (int)button;
        onPress[i] = isTrue;
        pressing[i] = isTrue;
        released[i] = isTrue;
    }

    public void setAnimations(SpriteAnimation idle, SpriteAnimation idleAct, SpriteAnimation glide, SpriteAnimation walk, 
        SpriteAnimation dash, SpriteAnimation jump, SpriteAnimation djump, SpriteAnimation inAir, SpriteAnimation crouch){
        this.idle = idle.copy();
        this.idleAct = idleAct.copy();
        this.glide = glide.copy();
        this.walk = walk.copy();
        this.dash = dash.copy();
        this.jump = jump.copy();
        this.djump = djump.copy();
        this.inAir = inAir.copy();
        this.crouch = crouch.copy();
        setBackAnimations(idle, idleAct, glide, walk, dash, jump, djump, inAir, crouch);
        this.hasDiffBack = false;
        return;
    }

    public void setBackAnimations(SpriteAnimation idle, SpriteAnimation idleAct, SpriteAnimation glide, SpriteAnimation walk, 
        SpriteAnimation dash, SpriteAnimation jump, SpriteAnimation djump, SpriteAnimation inAir, SpriteAnimation crouch){
        this.hasDiffBack = true;

        this.idleB = idle.copy();
        this.idleActB = idleAct.copy();
        this.glideB = glide.copy();
        this.walkB = walk.copy();
        this.dashB = dash.copy();
        this.jumpB = jump.copy();
        this.djumpB = djump.copy();
        this.inAirB = inAir.copy();
        this.crouchB = crouch.copy();
        return;
    }

    public Sprite playWalk(bool isForward){
        if (isForward || !hasDiffBack) 
            return this.walk.playAnim();
        else
            return this.walkB.playAnim();
    }

    // Apply gravity to the player
    public void applyGravity(GameObject player){
        this.yVel -= this.gravity * Time.deltaTime;
        player.transform.Translate(this.xVel * Time.deltaTime, this.yVel, 0);
    }

    // Takes in a gameObject and move it as a fighter
    public void controlFighter(GameObject fighter, float minY, float minX, float maxX, float enemyXPos, GameObject mySound, AudioClip[] basicSFX){
        // Make sure fighter is initially facing opponent
        if (this.curState != fighterState.AERIAL && this.curState != fighterState.ATTACK) {
            if (fighter.transform.localPosition.x > enemyXPos) {
                if (fighter.transform.localScale.x != -1) {
                    this.isDash = false;
                    fighter.transform.localScale = new Vector2(-1, 1);
                }
            }
            else {
                if (fighter.transform.localScale.x != 1) {
                    this.isDash = false;
                    fighter.transform.localScale = new Vector2(1, 1);
                }
            }
        }

        switch (this.curState) {
            case fighterState.GROUNDED:     Grounded(fighter, minY, mySound, basicSFX); break;
            case fighterState.AERIAL:       Aerial(fighter, minY, mySound,basicSFX); break;
        }
        if (!isGlide && !isBackHop && curState == fighterState.ATTACK) {
                Attack(fighter, minY, mySound);
        }

        // Keep fighter in bounds
        fighter.transform.localPosition = new Vector2(limitValue(fighter.transform.localPosition.x, minX, maxX), fighter.transform.localPosition.y);
        fighter.transform.localPosition = new Vector2(fighter.transform.localPosition.x, 
                                                        limitValue(fighter.transform.localPosition.y, minY, fighter.transform.localPosition.y));


        // Save the key that was pressed this frame if this was a new frame
        int bufferIndex = Time.frameCount % this.buffer.Length;
        if (onPress[(int)key.LEFT]) {
            this.buffer[bufferIndex] = DataManager.savedOptions.controls[(int)key.LEFT];
        }
        else if (onPress[(int)key.RIGHT]) {
            this.buffer[bufferIndex] = DataManager.savedOptions.controls[(int)key.RIGHT];
        }
        else if (onPress[(int)key.UP]) {
            this.buffer[bufferIndex] = DataManager.savedOptions.controls[(int)key.UP];
        }
        else if (onPress[(int)key.DOWN]) {
            this.buffer[bufferIndex] = DataManager.savedOptions.controls[(int)key.DOWN];
        }
        else if (onPress[(int)key.B1]) {
            this.buffer[bufferIndex] = DataManager.savedOptions.controls[(int)key.B1];
        }
        else if (onPress[(int)key.B2]) {
            this.buffer[bufferIndex] = DataManager.savedOptions.controls[(int)key.B2];
        }
        else if (onPress[(int)key.B3]) {
            this.buffer[bufferIndex] = DataManager.savedOptions.controls[(int)key.B3];
        }
        else {
            this.buffer[bufferIndex] = KeyCode.None;
        }

        return;
    }

    // ---Other Functions---
    private float limitValue(float value, float min, float max){
        if (value < min)        return min;
        else if (value > max)   return max;
        else                    return value;
    }

    // A small function used to delay actions for a certain amount of time
    private int initialFrame;
    private bool WaitForXFrames(int x){
        if (initialFrame == 0) {
            initialFrame = Time.frameCount;
        }
            
        if (Time.frameCount - this.initialFrame >= x) {
            this.initialFrame = 0;
            return true;
        }
        return false;
    }
    private void resetWait() { this.initialFrame = 0; return; }

    // Handels grounded input from player
    private bool isBackHop, isCrouch, isDash;
    private int idleCounter;
    private void Grounded(GameObject player, float minY,GameObject mySound, AudioClip[] basicSFX) {
        int forward = (int)player.transform.localScale.x;

        // Slide back player 
        if(slideBack > 0){
            player.transform.Translate(-3 * forward * slideBack * Time.deltaTime, 0, 0);
            slideBack -= Time.deltaTime * 10;
        }

        // Move the player back for 13 frames if they're backhopping
        if (isBackHop) {
            if (!WaitForXFrames(7)) {
                player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.inAir, this.inAirB);
                player.transform.Translate(-1 * forward * Time.deltaTime, 0, 0);
            }
            else {
                this.isBackHop = false;
                this.xVel = 0; // Prevent player from staying in backhop
            }
        }
        // Go into attack state if any of the attack buttons were pressed
        else if (onPress[(int)key.B1] || onPress[(int)key.B2] || onPress[(int)key.B3]) {
            this.curState = fighterState.ATTACK;
        }
        else if (this.isCrouch) {
            player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.crouch, this.crouchB);
            if (!pressing[(int)key.DOWN]) {
                this.isCrouch = false;
            }
        }
        else {

            // UP KEY
            if (pressing[(int)key.UP]) {
                // Place character in jump squat 
                player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.jump, this.jumpB);
                this.inJumpSquat = true;
                this.isDash = false;
                this.curState = fighterState.AERIAL;

                // Decide which way the character should jump
                if (pressing[(int)key.LEFT]) {
                    if (Mathf.Abs(this.xVel) > this.glideSpeed)
                        this.xVel = Mathf.Abs(this.dashSpeed) * -1;
                    else
                        this.xVel = Mathf.Abs(this.walkSpeed) * -1;
                }
                else if (pressing[(int)key.RIGHT]) {
                    if(Mathf.Abs(this.xVel) > this.glideSpeed)
                        this.xVel = Mathf.Abs(this.dashSpeed);
                    else
                        this.xVel = Mathf.Abs(this.walkSpeed);
                }
                else
                    this.xVel = 0;
            }
        
            // DOWN KEY
            else if (pressing[(int)key.DOWN]) {
                this.isCrouch = true;
                this.isDash = false;
            }
        
            // LEFT KEY
            else if (pressing[(int)key.LEFT]) {
                // Check the buffer to see if the left button was already pressed
                if (onPress[(int)key.LEFT]) {
                    KeyCode[] lastInput = { KeyCode.None };
                    readBuffer(lastInput, 15);
                    if (lastInput[0] == DataManager.savedOptions.controls[(int)key.LEFT]) {
                        if(forward > 0) {
                            this.isBackHop = true;
                            clearBuffer();
                        } 
                        else {
                            this.isDash = true;

                            // Play dash sound effect
                            mySound.GetComponent<AudioSource>().clip = basicSFX[1];
                            mySound.GetComponent<AudioSource>().Play();
                        }
                    }
                }

                if (this.isDash && forward < 0) {
                    this.xVel = -this.dashSpeed;
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.dash, this.dashB);
                    player.transform.Translate(-1 * this.dashSpeed * Time.deltaTime, 0, 0);
                }
                else {
                    this.xVel = -this.walkSpeed;
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.walk, this.walkB);
                    player.transform.Translate(-1 * this.walkSpeed * Time.deltaTime, 0, 0);
                }
            }
                
            // RIGHT KEY
            else if (pressing[(int)key.RIGHT]) {
                // Check the buffer to see if the left button was already pressed
                if (onPress[(int)key.RIGHT]) {
                    KeyCode[] lastInput = { KeyCode.None };
                    readBuffer(lastInput, 15);
                    if (lastInput[0] == DataManager.savedOptions.controls[(int)key.RIGHT]) {
                        if(forward > 0) {
                            this.isDash = true;

                            // Play dash sound effect
                            mySound.GetComponent<AudioSource>().clip = basicSFX[1];
                            mySound.GetComponent<AudioSource>().Play();
                        } 
                        else {
                            this.isBackHop = true;
                            clearBuffer();
                        }
                    }
                }

                if (this.isDash && forward > 0) {
                    this.xVel = this.dashSpeed;
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.dash, this.dashB);
                    player.transform.Translate(this.dashSpeed * Time.deltaTime, 0, 0);
                }
                else {
                    this.xVel = this.walkSpeed;
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.walk, this.walkB);
                    player.transform.Translate(this.walkSpeed * Time.deltaTime, 0, 0);
                }
            }

            // Character is idle
            else {
                this.xVel = 0;
                if (this.idleCounter < 3) {
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.idle, this.idleB);
                    /*
                    if ((forward > 0 || !hasDiffBack) && this.idle.isDone())    { this.idleCounter += 1; }
                    else if (this.idleB.isDone())                               { this.idleCounter += 1; }
                    */
                }
                else {
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.idleAct, this.idleActB);
                    if ((forward > 0 || !hasDiffBack) && this.idleAct.isDone())     { this.idleCounter = 0; }
                    else if (this.idleActB.isDone())                                { this.idleCounter = 1; }
                }
            }

            if (released[(int)key.LEFT] || released[(int)key.RIGHT]) {
                this.isDash = false;
            }
        }
    }

    // Handles aerial actions and mobility
    private int jumpCount = 0;
    private int glideConstant;
    private bool isGlide;
    private bool inJumpSquat, inDJumpSquat;
    void Aerial(GameObject player, float minHeight, GameObject mySound, AudioClip[] basicSFX){
        int forward = (int)player.transform.localScale.x;

        if (isGlide) {
            if (!WaitForXFrames(15)) {
                int direction = (int)this.xVel / (int)this.glideSpeed;
                player.GetComponent<SpriteRenderer>().sprite = chooseAnim(direction, this.glide, this.glideB);
                player.transform.Translate(this.xVel * Time.deltaTime, 0, 0);
                player.transform.localScale = new Vector2(direction, player.transform.localScale.y);
            }
            else {
                isGlide = false;
                player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.inAir, this.inAirB);
                this.yVel = 0;
            }
        }
        else {
            if (this.inJumpSquat) {
                if (WaitForXFrames(1)) {
                    this.inJumpSquat = false;
                    this.jumpCount += 1;
                    resetAnims(this.jump, this.jumpB);
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.inAir, this.inAirB);
                    this.yVel = Mathf.Abs(this.gravity) * 0.4f;

                    // Play jump sound effect
                    mySound.GetComponent<AudioSource>().clip = basicSFX[0];
                    mySound.GetComponent<AudioSource>().Play();
                }
            }

            // Start double jump
            else if (onPress[(int)key.UP] && this.jumpCount <= this.maxJumps) {
                // Decide which way the character should jump
                if (pressing[(int)key.LEFT]) {
                    if (Mathf.Abs(this.xVel) > this.glideSpeed)
                        this.xVel = Mathf.Abs(this.dashSpeed) * -1;
                    else 
                        this.xVel = Mathf.Abs(this.walkSpeed) * -1;
                }
                else if (pressing[(int)key.RIGHT]) {
                    if(Mathf.Abs(this.xVel) > this.glideSpeed)
                        this.xVel = Mathf.Abs(this.dashSpeed);
                    else
                        this.xVel = Mathf.Abs(this.walkSpeed);
                }
                else
                    this.xVel = 0;

                // Place character in double jump squat 
                player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.djump, this.djumpB);
                this.inDJumpSquat = true;
            }
            else if (this.inDJumpSquat) {
                if (WaitForXFrames(2)) {
                    this.inDJumpSquat = false;
                    this.jumpCount += 1;
                    resetAnims(djump, djumpB);
                    player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.inAir, this.inAirB);
                    this.yVel = Mathf.Abs(this.gravity) * 0.4f;

                    // Play jump sound effect
                    mySound.GetComponent<AudioSource>().clip = basicSFX[0];
                    mySound.GetComponent<AudioSource>().Play();
                }
            }

            // Check if the player inputted the right combination for gliding
            else if ((onPress[(int)key.LEFT] || onPress[(int)key.RIGHT]) && this.jumpCount <= this.maxJumps) {
                // Two so that you can also do up and then a direction
                KeyCode[] lastInput = { KeyCode.None, KeyCode.None };
                readBuffer(lastInput, 15);
                if (onPress[(int)key.LEFT] && lastInput[0] == DataManager.savedOptions.controls[(int)key.LEFT]) {
                    this.isGlide = true;
                    this.yVel = 0;
                    this.xVel = -this.glideSpeed;
                    this.jumpCount += 1;
                }
                else if (onPress[(int)key.RIGHT] && lastInput[0] == DataManager.savedOptions.controls[(int)key.RIGHT]) {
                    this.isGlide = true;
                    this.yVel = 0;
                    this.xVel = this.glideSpeed;
                    this.jumpCount += 1;
                }
                else if (lastInput[0] == DataManager.savedOptions.controls[(int)key.UP]) {
                    if (onPress[(int)key.LEFT] && lastInput[1] == DataManager.savedOptions.controls[(int)key.LEFT]) {
                        this.isGlide = true;
                        this.yVel = 0;
                        this.xVel = -this.glideSpeed;
                        this.jumpCount += 1;
                    }
                    else if (onPress[(int)key.RIGHT] && lastInput[1] == DataManager.savedOptions.controls[(int)key.RIGHT]) {
                        this.isGlide = true;
                        this.yVel = 0;
                        this.xVel = this.glideSpeed;
                        this.jumpCount += 1;
                    }
                }

                if(isGlide) {
                    // Play glide sound effect
                    mySound.GetComponent<AudioSource>().clip = basicSFX[2];
                    mySound.GetComponent<AudioSource>().Play();
                }
            }

            // Go into attack state if any of the attack buttons were pressed
            else if (onPress[(int)key.B1] || onPress[(int)key.B2] || onPress[(int)key.B3]) {
                this.curState = fighterState.ATTACK;
            }

            // Otherwise, just make the character fall down
            else {
                applyGravity(player);
                player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.inAir, this.inAirB);

                // Check if fighter has touched the ground
                if(player.transform.localPosition.y <= minHeight){
                    this.jumpCount = 0;
                    curSkill = null;
                    player.transform.localPosition = new Vector2(player.transform.localPosition.x, minHeight);
                    this.curState = fighterState.GROUNDED;
                }
            }
        }
    }

    // Handles attack selection and performs them
    private FighterSkill curSkill;
    private void AttackSel(GameObject player, float minHeight, GameObject mySound){

        // Determine aerial attack
        if (player.transform.localPosition.y > minHeight) {
            if (onPress[(int)key.B1])       curSkill = this.jumpingSkill[0];
            else if (onPress[(int)key.B2])  curSkill = this.jumpingSkill[1];
            else                            curSkill = this.jumpingSkill[2];
        }
        else if (this.isCrouch) {
            if (onPress[(int)key.B1])       curSkill = this.crouchingSkill[0];
            else if (onPress[(int)key.B2])  curSkill = this.crouchingSkill[1];
            else                            curSkill = this.crouchingSkill[2];
        }
        // Go through the entire skillList and check for combination
        else {
            // Check the three most recent input
            bool skillDetected = false;
            KeyCode[] inputChain = { KeyCode.None, KeyCode.None, KeyCode.None };
            readBuffer(inputChain, 15);
            // Flip order of inputs
            inputChain[2] = inputChain[0];
            inputChain[0] = inputChain[1];
            inputChain[1] = inputChain[2];

            for (int i = 0; i < this.skillList.Count && i < (this.curLevel-5) / 10; i++) {
                bool correctInput = true;
                KeyCode[] input = (KeyCode[]) this.skillList[i].getInput().Clone();

                // Swap left and right keys if the player is facing the other way
                if (player.transform.localScale.x < 0) {
                    for(int j = 0; j < input.Length; j++){
                        if (input[j] == DataManager.savedOptions.controls[(int)key.LEFT]) {
                            input[j] = DataManager.savedOptions.controls[(int)key.RIGHT];
                        }
                        else if (input[j] == DataManager.savedOptions.controls[(int)key.RIGHT])
                            input[j] = DataManager.savedOptions.controls[(int)key.LEFT];
                    }
                }
            
                if (input[input.Length - 1] == DataManager.savedOptions.controls[(int)key.B1]) {
                    correctInput = onPress[(int)key.B1];
                }
                else if (input[input.Length - 1] == DataManager.savedOptions.controls[(int)key.B2]) {
                    correctInput = onPress[(int)key.B2];
                }
                else if (input[input.Length - 1] == DataManager.savedOptions.controls[(int)key.B3]) {
                    correctInput = onPress[(int)key.B3];
                }
                for (int j = 0; j < inputChain.Length-1 && correctInput; j++) {
                    if (j >= input.Length || input[j] != inputChain[j]) {
                        correctInput = false;
                        break;
                    }
                }
                    
                if (correctInput) {
                    this.curSkill = this.skillList[i];
                    skillDetected = true;
                    break;
                }
            }

            if (!skillDetected) {
                if (onPress[(int)key.B1])       curSkill = this.normalSkill[0];
                else if (onPress[(int)key.B2])  curSkill = this.normalSkill[1];
                else                            curSkill = this.normalSkill[2]; 
            }
        }

        // Play skill sound effect
        if(curSkill != null) {
            mySound.GetComponent<AudioSource>().clip = curSkill.getSFX();
            mySound.GetComponent<AudioSource>().Play();
        }

        this.curState = fighterState.ATTACK;
    }

    // Performs attack that was last selected
    private void Attack(GameObject player, float minHeight, GameObject mySound){
        int forward = (int)player.transform.localScale.x;

        if (curSkill == null) {
            AttackSel(player, minHeight, mySound);
        }

        // If the move is special cancelable, check if a special was inputted
        if (this.curSkill.isSpecialCancelable() && this.curSkill.getHasHit()) {
            // Check the three most recent input
            bool skillDetected = false;
            KeyCode[] inputChain = { KeyCode.None, KeyCode.None, KeyCode.None };
            readBuffer(inputChain, 15);
            // Flip order of inputs
            inputChain[2] = inputChain[0];
            inputChain[0] = inputChain[1];
            inputChain[1] = inputChain[2];

            for (int i = 0; i < this.skillList.Count && i < (this.curLevel-5) / 10 && !skillDetected; i++) {
                bool correctInput = true;
                KeyCode[] input = (KeyCode[]) this.skillList[i].getInput().Clone();

                // Swap left and right keys if the player is facing the other way
                if (player.transform.localScale.x < 0) {
                    for(int j = 0; j < input.Length; j++){
                        if (input[j] == DataManager.savedOptions.controls[(int)key.LEFT])
                            input[j] = DataManager.savedOptions.controls[(int)key.RIGHT];
                        else if (input[j] == DataManager.savedOptions.controls[(int)key.RIGHT])
                            input[j] = DataManager.savedOptions.controls[(int)key.LEFT];
                    }
                }

                if (input[input.Length - 1] == DataManager.savedOptions.controls[(int)key.B1]) {
                    correctInput = onPress[(int)key.B1];
                }
                else if (input[input.Length - 1] == DataManager.savedOptions.controls[(int)key.B2]) {
                    correctInput = onPress[(int)key.B2];
                }
                else if (input[input.Length - 1] == DataManager.savedOptions.controls[(int)key.B3]) {
                    correctInput = onPress[(int)key.B3];
                }
                for (int j = 0; j < inputChain.Length-1 && correctInput; j++) {
                    if (j >= input.Length || input[j] != inputChain[j]) {
                        correctInput = false;
                        break;
                    }
                }

                if (correctInput) {
                    this.curSkill.resetAnims();
                    this.curSkill = this.skillList[i];
                    resetWait();
                    skillDetected = true;

                    // Play skill sound effect
                    if(curSkill != null) {
                        mySound.GetComponent<AudioSource>().clip = curSkill.getSFX();
                        mySound.GetComponent<AudioSource>().Play();
                    }
                    break;
                }
            }
        }

        // Check for normal cancel
        if((this.normalSkill.Contains(curSkill) || this.crouchingSkill.Contains(curSkill)) && this.curSkill.getHasHit()){
            int skillOrder = 0;
            if (this.normalSkill.Contains(curSkill))
                skillOrder = this.normalSkill.IndexOf(curSkill);
            else
                skillOrder = this.crouchingSkill.IndexOf(curSkill);
            
            // Find last attack input
            if (onPress[(int)key.B1] && skillOrder < 1) {
                this.curSkill.resetAnims();
                if (pressing[(int)key.DOWN])
                    this.curSkill = crouchingSkill[0];
                else
                    this.curSkill = normalSkill[0];
                resetWait();

                // Play skill sound effect
                if(curSkill != null) {
                    mySound.GetComponent<AudioSource>().clip = curSkill.getSFX();
                    mySound.GetComponent<AudioSource>().Play();
                }
            }
            else if (onPress[(int)key.B2] && skillOrder < 2) {
                this.curSkill.resetAnims();
                if (pressing[(int)key.DOWN])
                    this.curSkill = crouchingSkill[1];
                else
                    this.curSkill = normalSkill[1];
                resetWait();

                // Play skill sound effect
                if(curSkill != null) {
                    mySound.GetComponent<AudioSource>().clip = curSkill.getSFX();
                    mySound.GetComponent<AudioSource>().Play();
                }
            }
            else if (onPress[(int)key.B3] && skillOrder < 3){
                this.curSkill.resetAnims();
                if (pressing[(int)key.DOWN])
                    this.curSkill = crouchingSkill[2];
                else
                    this.curSkill = normalSkill[2];
                resetWait();

                // Play skill sound effect
                if(curSkill != null) {
                    mySound.GetComponent<AudioSource>().clip = curSkill.getSFX();
                    mySound.GetComponent<AudioSource>().Play();
                }
            }
        }
            
        // Check if player has jump canceled the attack
        if (this.curSkill.isJumpCancelable() && pressing[(int)key.UP]) {
            this.curSkill.resetAnims();
            this.curSkill = null;
            resetWait();
            this.curState = fighterState.GROUNDED;
        }
        // Otherwise play the move until the first active frame of the move
        else if (!WaitForXFrames(this.curSkill.firstActive())) {
            player.GetComponent<SpriteRenderer>().sprite = this.curSkill.playSkill(forward == 1);
            player.transform.Translate(forward * this.curSkill.getXShift() * Time.deltaTime, this.curSkill.getYShift() * Time.deltaTime, 0);

            if (this.curSkill.hitboxOut(player.transform.localScale.x == 1, Time.frameCount - initialFrame) && this.curSkill.isProjectile()) {
                this.projectileOut = true;
            }

            // If jumping attack, apply gravity
            if (this.jumpingSkill.Contains(this.curSkill)) {
                applyGravity(player);

                // Check if player touched the ground
                if (player.transform.localPosition.y <= minHeight) {
                    resetWait();
                    this.jumpCount = 0;
                    player.transform.localPosition = new Vector2(player.transform.localPosition.x, minHeight);
                    this.curState = fighterState.GROUNDED;
                    this.curSkill.resetAnims();
                    curSkill = null;
                }
            }
            else {
                // Slide back player 
                if(slideBack > 0){
                    player.transform.Translate(-1 * curSkill.getBasePow() * forward * slideBack * Time.deltaTime, 0, 0);
                    slideBack -= Time.deltaTime * 10;
                }
            }
        }
        // Reset everything once the attack is done
        else {
            this.curSkill.resetAnims();
            this.curSkill = null;

            if (player.transform.localPosition.y > minHeight) {
                player.GetComponent<SpriteRenderer>().sprite = chooseAnim(forward, this.inAir, this.inAirB);
                this.curState = fighterState.AERIAL;
            }
            else {
                this.curState = fighterState.GROUNDED;
            }
        }
    }

    // Determines if there was an attack that hit a specified target
    public bool checkHit(GameObject player, GameObject target, HitBox[] hurtboxes){
        if(curSkill == null)
            return false;

        if (this.curState == fighterState.ATTACK && !WaitForXFrames(this.curSkill.firstActive())) {
            if (this.curSkill.hitboxOut(player.transform.localScale.x == 1, Time.frameCount - initialFrame)) {
                return this.curSkill.checkHit(player, target, hurtboxes);
            }
        }
        return false;
    }

    // Decide which animation to use based on which way character is facing
    private Sprite chooseAnim(int forward, SpriteAnimation front, SpriteAnimation back){
        if (forward > 0 || !this.hasDiffBack)   return front.playAnim();
        else                                    return back.playAnim();
    }

    // Reset both animations
    private void resetAnims(SpriteAnimation front, SpriteAnimation back){
        front.resetAnim();
        if (this.hasDiffBack)
            back.resetAnim();
        return;
    }

    // Reset the input buffer to original state
    public void clearBuffer(){
        for (int i = 0; i < this.buffer.Length; i++) {
            this.buffer[i] = KeyCode.None;
        }
        return;
    }

    // Reset the booleans for input
    public void clearInput(){
        for (int i = 0; i < pressing.Length; i++) {         
            this.onPress[i] = false;
            this.pressing[i] = false;
            this.released[i] = false;
        }
    }

    public void resetState(){
        this.curState = fighterState.GROUNDED;
        curSkill = null;
        initialFrame = 0;
        isDash = false; 
        isGlide = false;

        resetSkills(normalSkill);
        resetSkills(crouchingSkill);
        resetSkills(jumpingSkill);
        resetSkills(skillList);
    }

    private void resetSkills(List<FighterSkill> s) {
        for(int i = 0; i < s.Count; i++)
            s[i].resetAnims();
    }

    // Reads in the most recent inputs(amount read based on size of argument array)
    private KeyCode[] readBuffer(KeyCode[] k, int lastBuff){
        int curIndex = 0;

        // Check the three most recent input
        int lastBuffer = Time.frameCount % buffer.Length;
        for (int i = 0; i < lastBuff; i++) {
            // Check if any possible player input was pressed
            bool isPlayerInput = false;
            for (int j = 0; j < DataManager.savedOptions.controls.Length; j++) {
                if (this.buffer[lastBuffer] == DataManager.savedOptions.controls[j]) {
                    isPlayerInput = true;
                    break;
                }
            }

            if (isPlayerInput) {
                k[curIndex] = buffer[lastBuffer];
                curIndex += 1;
            }

            if (curIndex >= k.Length) {
                break;
            }
            lastBuffer = (lastBuffer - 1 + buffer.Length) % buffer.Length;
        }

        return k;
    }

    public void loadFighter(){
        for (int i = 0; i < this.skillList.Count; i++) {
            if (this.skillList[i].isProjectile())
                this.skillList[i].setProjectile("bacon", true);
            this.skillList[i].loadSprites();
        }
        for (int i = 0; i < this.normalSkill.Count; i++) {
            if (this.normalSkill[i].isProjectile())
                this.normalSkill[i].setProjectile("bacon", true);
            this.normalSkill[i].loadSprites();
        }
        for (int i = 0; i < this.jumpingSkill.Count; i++) {
            if (this.jumpingSkill[i].isProjectile())
                this.jumpingSkill[i].setProjectile("bacon", true);
            this.jumpingSkill[i].loadSprites();
        }
        for (int i = 0; i < this.crouchingSkill.Count; i++) {
            if (this.crouchingSkill[i].isProjectile())
                this.crouchingSkill[i].setProjectile("bacon", true);
            this.crouchingSkill[i].loadSprites();
        }

        this.idle.loadSprites();
        this.idleAct.loadSprites();
        this.glide.loadSprites();
        this.walk.loadSprites();
        this.dash.loadSprites();
        this.crouch.loadSprites();
        this.jump.loadSprites();
        this.djump.loadSprites();
        this.inAir.loadSprites();

        this.idleB.loadSprites();
        this.idleActB.loadSprites();
        this.glideB.loadSprites();
        this.walkB.loadSprites();
        this.dashB.loadSprites();
        this.crouchB.loadSprites();
        this.jumpB.loadSprites();
        this.djumpB.loadSprites();
        this.inAirB.loadSprites();
    }
}

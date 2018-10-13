using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FighterSkill {

    // ---Fields---
    private string atkName;
    private SpriteAnimation anim;
    private SpriteAnimation animB;
    private int hitstun;
    private int basePow;
    private int FAF;
    private float xPow, yPow;
    private float xShift, yShift;
    private bool isJumpCancel;
    private bool isSpecialCancel;
    private KeyCode[] input;
    private HitBox[] hitboxes;
    private bool hasHit;
    private int rehit;
    private int[] hitframes;

    private bool isProj;
    private float xProjShift, yProjShift;
    private float xScale, yScale;
    [System.NonSerialized]
    private GameObject projectile;
    private string proPath;

    [System.NonSerialized]
    private AudioClip sfx;
    private string sfxPath;

    // ---Constructor---
    public FighterSkill(string atkName, int stun, int bp, int faf, float xPow, float yPow, 
        float xShift, float yShift, bool isJC, bool isSC, KeyCode[] input, HitBox[] hitboxes, int rehit, int[] hitframes, string sfxPath){
        this.atkName = atkName;
        this.hitstun = stun;
        this.basePow = bp;
        this.FAF = faf;
        this.xPow = xPow;
        this.yPow = yPow;
        this.xShift = xShift;
        this.yShift = yShift;
        this.isJumpCancel = isJC;
        this.isSpecialCancel = isSC;
        this.input = (KeyCode[])input.Clone();
        this.hitboxes = new HitBox[hitboxes.Length];
        for (int i = 0; i < hitboxes.Length; i++) {
            this.hitboxes[i] = hitboxes[i].copy();
        }
        this.rehit = rehit;
        this.hitframes = (int[])hitframes.Clone();

        // Set sfx
        this.sfxPath = sfxPath;
        if(sfxPath != "null")
            sfx = Resources.Load(sfxPath) as AudioClip;
        return;
    }

    // ---Access Functions---
    public string getName()             { return this.atkName; }
    public int getHitStun()             { return this.hitstun; }
    public int getBasePow()             { return this.basePow; }
    public int firstActive()            { return this.FAF; }
    public float getXPow()              { return this.xPow; }
    public float getYPow()              { return this.yPow; }
    public float getXShift()            { return this.xShift/this.FAF; }
    public float getYShift()            { return this.yShift/this.FAF; }
    public bool isJumpCancelable()      { return this.isJumpCancel; }
    public bool isSpecialCancelable()   { return this.isSpecialCancel; }
    public KeyCode[] getInput()         { return this.input; }
    public bool getHasHit()             { return this.hasHit; }
    public bool isProjectile()          { return this.isProj; }
    public int[] getHitFrames()         { return this.hitframes; }
    public AudioClip getSFX()           { return this.sfx; }
    public bool hitboxOut(bool isForward, int curFrame){ 
        bool isEvent = this.anim.isEvent(curFrame);
        bool isEventB = this.animB.isEvent(curFrame);
        if (isForward)      return isEvent;
        else                return isEventB;
    }

    // ---Manipulation Procedures---
    public void setAnimation(SpriteAnimation a)     { this.anim = a.copy(); this.animB = a.copy(); return; }
    public void setAnimationBack(SpriteAnimation a) { this.animB = a.copy(); return; }
    public void resetAnims()                        { this.anim.resetAnim(); this.animB.resetAnim(); this.hasHit = false; return; }

    public void setProjectile(string prefabPath, bool loaded){
        if (!loaded) {
            this.projectile = Resources.Load<GameObject>(@prefabPath);
            this.proPath = prefabPath;
        }
        else
            this.projectile = Resources.Load<GameObject>(proPath);

        this.isProj = true;
        return;
    }

    public GameObject getProjectile(){
        if (this.isProj) {
            return this.projectile;
        }
        return null;
    }

    // Play skill animation and make sure both front and back are in sync
    public Sprite playSkill(bool isForward){
        Sprite front = this.anim.playAnim();
        Sprite back = this.animB.playAnim();
        if (isForward)      return front;
        else                return back;
    }

    // Check if the hitboxes of the attack has made contact with a specified target
    private int hitTracker;
    public bool checkHit (GameObject user, GameObject target, HitBox[] hurtboxes){
        if (!this.hasHit || (Time.frameCount - hitTracker) == this.rehit) {
            float userX = user.transform.localPosition.x;
            float userY = user.transform.localPosition.y;
            int userFlip = (int)user.transform.localScale.x;

            float targetX = target.transform.localPosition.x;
            float targetY = target.transform.localPosition.y;
            int targetFlip = (int)target.transform.localScale.x;

            for (int i = 0; i < this.hitboxes.Length; i++) {
                this.hitboxes[i].shiftBox(userX, userY);
                this.hitboxes[i].flipBox(userFlip);

                for (int j = 0; j < hurtboxes.Length; j++) {
                    hurtboxes[j].shiftBox(targetX, targetY);
                    hurtboxes[j].flipBox(targetFlip);

                    if (this.hitboxes[i].checkHit(hurtboxes[j])) {
                        this.hasHit = true;
                        this.hitTracker = Time.frameCount;
                        return true;
                    }
                }
            }
        }
        return false;
    }

    // ---Other Functions---
    public void loadSprites(){
        this.anim.loadSprites();
        this.animB.loadSprites();

        if(sfxPath != "null")
            sfx = Resources.Load(sfxPath) as AudioClip;
    }

    public bool equals(FighterSkill f){
        if (f == null)
            return false;

        if(
            this.hitstun == f.getHitStun() &&
            this.basePow == f.getBasePow() &&
            this.FAF == f.firstActive() &&
            this.xPow == f.getXPow() &&
            this.yPow == f.getYPow()
        )
            return true;
        return false;
    }
}

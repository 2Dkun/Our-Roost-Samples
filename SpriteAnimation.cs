using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SpriteAnimation {

    // ---Fields---
    [System.NonSerialized]
    private Sprite[] anim; // Sprites for the animation
    private string[] animPath;
    private string[] animName;
    private int eventTracker; //Use to keep track of which event to look for next
    private int[] eventFrames; // Holds the frames on which an even should occur during the animation
    private float FPS; // Holds how fast the animation should be shown
    private bool isLoop; // Determines if the animation loops 
    private float curFrame; // Used to keep track of how far the animation has gone
    private bool isFinished;

    // ---Constructor---
    public SpriteAnimation(Sprite[] anim, int[] eventFrames, float FPS, bool isLoop){
        // Set animation variables
        this.anim = new Sprite[anim.Length];
        for (int i = 0; i < anim.Length; i++) {
            this.anim[i] = anim[i];
        }
        this.eventFrames = (int[])eventFrames.Clone();
        this.isLoop = isLoop;
        this.FPS = FPS;
        return;
    }

    // ---Access Functions---
    public int getLength() { return this.anim.Length; }
    public float getFrame() { return this.curFrame * 2.5f; }
    public Sprite[] getSprite() { return this.anim; }
    public float getFps(){ return this.FPS; }

    public bool eventExist(){
        return this.eventFrames.Length != 0;
    }
    public bool isEvent(int curFrame){
        if (eventExist()) {
            this.eventTracker %= eventFrames.Length;
            for (int i = 0; i < eventFrames.Length; i++) {
                if (this.eventFrames[i] == curFrame)
                    return true;
            }
        }
        return false;
    }
    public bool isDone(){
        return this.curFrame == 0;
    }

    // ---Manipulation Procedures---
    // Returns the sprite that should be playing in the animation
    public Sprite playAnim(){
        // Find which frame should be shown based on real time
        this.curFrame += Time.deltaTime;
        int index = (int)(this.curFrame * this.FPS);
        // If the animation is still going, play it
        if (index < this.anim.Length) {
            return this.anim[index];
        }
        // Else, attack is done being displayed
        else {
            resetAnim();
            if (this.isLoop) {
                return this.anim[index % this.anim.Length];
            }
            return this.anim[anim.Length - 1];
        }
    }
    public void resetAnim(){
        this.curFrame = 0;
        this.eventTracker = 0;
        return;
    }
    public void saveSprites(string[] path, string[] name){
        this.animPath = path;
        this.animName = name;
        return;
    }
    // Tells you if the sprite is in the animation
    public bool inAnim(Sprite s){
        for (int i = 0; i < anim.Length; i++) {
            if (s == anim[i])
                return true;
        }
        return false;
    }

    // ---Other Functions---
    // Returns the index of the current sprite animation
    private int getCurFrameIndex(){
        int index = (int)(this.curFrame * this.FPS);
        if (index >= this.anim.Length)
            index = 0;
        return index;
    }

    public SpriteAnimation copy(){
        SpriteAnimation clone = new SpriteAnimation(this.anim, this.eventFrames, this.FPS, this.isLoop);
        clone.saveSprites(this.animPath, this.animName);
        return clone;
    }

    public void loadSprites(){
        this.anim = new Sprite[this.animPath.Length];
        for (int i = 0; i < anim.Length; i++) {
            anim[i] = getSprite(Resources.LoadAll<Sprite>(@animPath[i]), animName[i]);
        }
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

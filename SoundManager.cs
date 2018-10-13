using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour {

    // Hold Audio objects
    public GameObject[] bgm;
    public GameObject[] sfx;

	// Use this for initialization
	void Start () {
        UpdateVolumes();
	}

    // Update the volume of all volume objects based on current saved data
    public void UpdateVolumes() {
        float master = ((float) DataManager.savedOptions.volume[0]) / 100;
        float BGM = ((float) DataManager.savedOptions.volume[1]) / 100;
        float SFX = ((float) DataManager.savedOptions.volume[2]) / 100;

        for(int i = 0; i < bgm.Length; i++) {
            bgm[i].GetComponent<AudioSource>().volume = master * BGM;
        }
        for(int i = 0; i < sfx.Length; i++) {
            sfx[i].GetComponent<AudioSource>().volume = master * SFX;
        }
    }
}

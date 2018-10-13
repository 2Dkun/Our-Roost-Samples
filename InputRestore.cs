using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputRestore : MonoBehaviour {

	// Used to reset the inputs back to default
	private int reset = 0;
    public GameObject soundPlayer;
    public AudioClip resetSound;

	// Use this for initialization
	void Start () {
		reset = 0;
	}
	
	// Update is called once per frame
	void Update () {

		// Handles resetting controls
		if (Input.GetKey(KeyCode.Escape)) {
			reset += 1;
		}
		else if (Input.GetKeyUp(KeyCode.Escape)) {
			reset = 0;
		}

		// Reset controls if ESC was held down long enough
		if (reset == 25) {

            // PLAY AUDIO NOTE 
            soundPlayer.GetComponent<AudioSource>().clip = resetSound;
            soundPlayer.GetComponent<AudioSource>().Play();

            KeyCode[] originalKeys = {KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.J, KeyCode.K, KeyCode.L};
			for (int i = 0; i < 7; i++) {
				DataManager.savedOptions.controls[i] = originalKeys[i];
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic; 
using System.Runtime.Serialization.Formatters.Binary; 
using System.IO;
using UnityEngine;

public static class DataManager{

	public static Options savedOptions = new Options();
	public static GameProgress file1 = new GameProgress();
	public static GameProgress file2 = new GameProgress();
	public static GameProgress file3 = new GameProgress();

	public static void Save(int slot) {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file;

		// Save first file
		if (slot == 1) {
			file = File.Create(Application.persistentDataPath + "/duke.duck");
			bf.Serialize(file, file1);
		}
		// Save second file
		else if (slot == 2) {
			file = File.Create(Application.persistentDataPath + "/fons.duck");
			bf.Serialize(file, file2);
		}
		// Save third file
		else {
			file = File.Create(Application.persistentDataPath + "/annie.duck");
			bf.Serialize(file, file3);
		}
		file.Close();
	}

	public static void Load() {
		BinaryFormatter bf = new BinaryFormatter();

		// Load the first save file if it exists
		if (File.Exists(Application.persistentDataPath + "/duke.duck")) {
			FileStream file = File.Open(Application.persistentDataPath + "/duke.duck", FileMode.Open);
			file1 = (GameProgress)bf.Deserialize(file);
			file.Close();
		}


		// Load the second save file if it exists
		if (File.Exists(Application.persistentDataPath + "/fons.duck")) {
			FileStream file = File.Open(Application.persistentDataPath + "/fons.duck", FileMode.Open);
			file2 = (GameProgress)bf.Deserialize(file);
			file.Close();
		}

		// Load the third save file if it exists
		if (File.Exists(Application.persistentDataPath + "/annie.duck")) {
			FileStream file = File.Open(Application.persistentDataPath + "/annie.duck", FileMode.Open);
			file3 = (GameProgress)bf.Deserialize(file);
			file.Close();
		}

	}

	public static void SaveOptions(){
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create(Application.persistentDataPath + "/options.duck");
		bf.Serialize(file, savedOptions);
		file.Close();
	}

	public static void LoadOptions(){
		if (File.Exists(Application.persistentDataPath + "/options.duck")) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + "/options.duck", FileMode.Open);
			savedOptions = (Options)bf.Deserialize(file);
			file.Close();
		}
	}

}

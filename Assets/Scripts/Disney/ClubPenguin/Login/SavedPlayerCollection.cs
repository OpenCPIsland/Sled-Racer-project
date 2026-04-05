using Disney.MobileNetwork;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Disney.ClubPenguin.Login
{
	using Disney.ClubPenguin.SledRacer;

	[Serializable]
	public class SavedPlayerCollection
	{
		public const int MAX_SAVED_PLAYERS = 6;

		private const char PASSWORD_DELIMITER = ',';

		private const string FILE_NAME = "/penguins.dat";

		public List<SavedPlayerData> SavedPlayers;

		public string EncryptedPasswords = string.Empty;

		private string defaultFilePath;

		private string DefaultFilePath
		{
			get
			{
				if (defaultFilePath == null)
				{
					defaultFilePath = Application.persistentDataPath + "/penguins.dat";
				}
				return defaultFilePath;
			}
		}

		public SavedPlayerCollection()
		{
			SavedPlayers = new List<SavedPlayerData>();
		}

		public SavedPlayerData GetMostRecentlyLoggedInPlayer()
		{
			RemoveCorruptPlayers();
			if (SavedPlayers.Count > 0)
			{
				return SavedPlayers[0];
			}
			return null;
		}

		private void RemoveCorruptPlayers()
		{
			LocalPlayerAccountService.NormalizeCollection(this);
		}

		public void UpdateSavedPlayer(SavedPlayerData savedPlayerData)
		{
			RemoveCorruptPlayers();
			RemoveSavedPlayer(savedPlayerData);
			SavedPlayers.Insert(0, savedPlayerData);
			if (SavedPlayers.Count > 6)
			{
				SavedPlayers.RemoveRange(6, SavedPlayers.Count - 6);
			}
		}

		public void RemoveSavedPlayer(SavedPlayerData savedPlayerData)
		{
			SavedPlayerData[] array = SavedPlayers.ToArray();
			foreach (SavedPlayerData savedPlayerData2 in array)
			{
				if (savedPlayerData2.Swid == savedPlayerData.Swid)
				{
					SavedPlayers.Remove(savedPlayerData2);
				}
			}
		}

		public bool ExistsOnDisk()
		{
			return File.Exists(DefaultFilePath);
		}

		public void LoadFromDisk()
		{
			if (ExistsOnDisk())
			{
				SetupSerialization();
				FileStream fileStream = File.Open(DefaultFilePath, FileMode.Open);
				BinaryFormatter binaryFormatter = new BinaryFormatter();
				SavedPlayerCollection savedPlayerCollection = null;
				try
				{
					savedPlayerCollection = (SavedPlayerCollection)binaryFormatter.Deserialize(fileStream);
				}
				catch (Exception)
				{
					UnityEngine.Debug.Log("Could not cast loaded file to SavedPlayerCollection");
					fileStream.Close();
					RestoreSerialization();
					return;
					IL_0054:;
				}
				fileStream.Close();
				RestoreSerialization();
				SavedPlayers = savedPlayerCollection.SavedPlayers;
				string text = EncryptionHelper.DecryptFile(DefaultFilePath, savedPlayerCollection.EncryptedPasswords);
				if (text == null)
				{
					RemoveCorruptPlayers();
					return;
				}
				string[] array = text.Split(',');
				if (array.Length == SavedPlayers.Count)
				{
					for (int i = 0; i < SavedPlayers.Count; i++)
					{
						SavedPlayers[i].Password = array[i];
					}
				}
				RemoveCorruptPlayers();
				return;
			}
			throw new Exception("Could not load save file from disk");
		}

		public void SaveToDisk()
		{
			RemoveCorruptPlayers();
			string text = string.Empty;
			for (int i = 0; i < SavedPlayers.Count; i++)
			{
				if (i > 0)
				{
					text += ',';
				}
				text += SavedPlayers[i].Password;
			}
			SetupSerialization();
			EncryptedPasswords = EncryptionHelper.EncryptFile(DefaultFilePath, text);
			BinaryFormatter binaryFormatter = new BinaryFormatter();
			FileStream fileStream = File.Create(DefaultFilePath);
			binaryFormatter.Serialize(fileStream, this);
			fileStream.Close();
			RestoreSerialization();
		}

		private void SetupSerialization()
		{
		}

		private void RestoreSerialization()
		{
		}
	}
}

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UniqueIDGenerator : MonoBehaviour
{
    private HashSet<string> generatedIDs = new HashSet<string>(); // Store generated IDs to check uniqueness

    // Function to generate a random alphanumeric ID
    public string GenerateUniqueID(int length = 6)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        char[] idChars = new char[length];
        System.Random random = new System.Random();

        while (true)
        {
            // Fill the char array with random characters from 'chars'
            for (int i = 0; i < length; i++)
            {
                idChars[i] = chars[random.Next(chars.Length)];
            }

            string uniqueID = new string(idChars);

            // Check if the ID is already generated
            if (!generatedIDs.Contains(uniqueID))
            {
                // Mark the ID as generated
                generatedIDs.Add(uniqueID);
                return uniqueID;
            }
        }
    }

    // Public method to be called from a button click
    public void GenerateUniqueIDOnClick()
    {
        string uniqueID = GenerateUniqueID(); // Generates a 6-character alphanumeric ID (default length)
        Debug.Log("Generated Restaurant ID: " + uniqueID);

        // Optionally, you can use the generated ID in further logic (e.g., save it to a database, display in UI, etc.)
    }
}

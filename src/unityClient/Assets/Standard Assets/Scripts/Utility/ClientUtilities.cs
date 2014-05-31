using UnityEngine;
using System.Collections;
using System.Security.Cryptography;
using System.Text;
using LitJson;

public class ClientUtilities
{
    public static string HashPassword(string password)
    {
        string encryptedPasswordString = string.Empty;
        SHA256 encryptor = SHA256.Create();
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] encryptedPasswordBytes = encryptor.ComputeHash(passwordBytes);

        foreach (byte entry in encryptedPasswordBytes)
        {
            encryptedPasswordString += entry.ToString("x2");
        }

        return encryptedPasswordString;
    }
}

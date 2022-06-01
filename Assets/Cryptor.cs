using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

public class Cryptor
{
    public static string Encrypt(string plainText, string keyString, string dateNow)
    {
        byte[] cipherData;
        Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(keyString);
        //var randIv = new RNGCryptoServiceProvider();
        //randIv.GetBytes(new byte[16]);
        aes.GenerateIV(); //random iv generated
        Debug.Log("iv length " + aes.IV.Length);
        aes.Mode = CipherMode.CBC;
        ICryptoTransform cipher = aes.CreateEncryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream())
        {
            using (CryptoStream cs = new CryptoStream(ms, cipher, CryptoStreamMode.Write))
            {
                using (StreamWriter sw = new StreamWriter(cs))
                {
                    sw.Write(plainText); // cipher update
                }
            }

            cipherData = ms.ToArray();
        }

        byte[] combinedData = new byte[aes.IV.Length + cipherData.Length];
        Array.Copy(aes.IV, 0, combinedData, 0, aes.IV.Length); //base41IV
        Array.Copy(cipherData, 0, combinedData, aes.IV.Length, cipherData.Length);
        var encrypted = Convert.ToBase64String(combinedData);
        var now = dateNow;
        Debug.Log("date now in c# " + dateNow + " " + now);
        var base64IV = Convert.ToBase64String(aes.IV);

        return $"{encrypted }|{now}|{base64IV}";
        //return Convert.ToBase64String(combinedData);
    }

    public static string Decrypt(string combinedString, string keyString)
    {
        string plainText;
        byte[] combinedData = Convert.FromBase64String(combinedString);
        Aes aes = Aes.Create();
        aes.Key = Encoding.UTF8.GetBytes(keyString);
        byte[] iv = new byte[aes.BlockSize / 8];
        byte[] cipherText = new byte[combinedData.Length - iv.Length];
        Array.Copy(combinedData, iv, iv.Length);
        Array.Copy(combinedData, iv.Length, cipherText, 0, cipherText.Length);
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        ICryptoTransform decipher = aes.CreateDecryptor(aes.Key, aes.IV);

        using (MemoryStream ms = new MemoryStream(cipherText))
        {
            using (CryptoStream cs = new CryptoStream(ms, decipher, CryptoStreamMode.Read))
            {
                using (StreamReader sr = new StreamReader(cs))
                {
                    plainText = sr.ReadToEnd();
                }
            }

            return plainText;
        }
    }
}

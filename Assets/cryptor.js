require('dotenv').config()

const { Console } = require('console');
const crypto = require('crypto')

class Cryptor {
  key

  constructor(key) {
    this.key = key;
  }

  encrypt(data, iv) {
    if (data == null) {
      throw new Error('Data is required')
    }

    try {
      // make the encrypter function
      const encrypter = crypto.createCipheriv("aes-256-cbc", this.key, iv);
      const base64IV = Buffer.from(iv).toString('base64')
      console.log("the iv is" , iv)
      // encrypt the message
      // set the input encoding
      // and the output encoding
      let encryptedWord = encrypter.update(data, "utf8", "hex");

      // stop the encryption using
      // the final method and set
      // output encoding to hex
      encryptedWord += encrypter.final("hex");
      console.log("encrycrypted word ",encryptedWord)
      console.log("date now ", Date.now())
      console.log("base64iv", base64IV)

      var encryption = Buffer.from(`${encryptedWord}|${Date.now()}|${base64IV}`).toString('base64')
      console.log("encripted is ", encryption);
      return encryption
    } catch (e) {
      throw new Error(e)
    }
  }

  decrypt(data) {
    try {
      // make the decrypter function
      const decrypter = crypto.createDecipheriv("aes-256-cbc", this.key, this.iv);

      // decrypt the message
      // set the input encoding
      // and the output encoding
      let decryptedWord = decrypter.update(data, "hex", "utf8");

      // stop the decryption using
      // the final method and set
      // output encoding to utf8
      decryptedWord += decrypter.final("utf8")

      return decryptedWord
    } catch (e) {
      throw new Error(e);
    }
  }
}

module.exports = Cryptor

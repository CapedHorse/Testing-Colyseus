const crypto = require('crypto')

      class Cryptor {
        key

        constructor(key) {
          this.key = key;
        }

        static encrypt(pubKey, data) {
          if (data == null) {
            throw new Error('Data is required')
          }

          try {
            // make the encrypter function
            const iv = crypto.randomBytes(16);
            const encrypter = crypto.createCipheriv("aes-256-cbc", this.key, iv); //random iv generated
            const base64IV = Buffer.from(iv).toString('base64')

            // encrypt the message
            // set the input encoding
            // and the output encoding
            let encryptedWord = encrypter.update(data, "utf8", "hex");

            // stop the encryption using
            // the final method and set
            // output encoding to hex
            encryptedWord += encrypter.final("hex");

            return Buffer.from(`${encryptedWord}|${Date.now()}|${base64IV}`).toString('base64')
          } catch (e) {
            throw new Error(e)
          }
        }

        static decrypt(data) {
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
package com.nosurrender.preferences;

import android.content.Context;
import android.content.SharedPreferences;
import android.security.keystore.KeyGenParameterSpec;
import android.security.keystore.KeyProperties;

import javax.crypto.Cipher;
import javax.crypto.KeyGenerator;
import javax.crypto.SecretKey;
import javax.crypto.spec.GCMParameterSpec;
import java.security.KeyStore;
import java.util.Base64;

public class SecurePreferencesHelper {

    private static final String KEY_ALIAS = "SecurePreferencesKeyAlias";
    private static final String SHARED_PREFS_NAME = "SecurePreferences";
    private final SharedPreferences sharedPreferences;
    private KeyStore keyStore;

    public SecurePreferencesHelper(Context context) {
        sharedPreferences = context.getSharedPreferences(SHARED_PREFS_NAME, Context.MODE_PRIVATE);
        initializeKeyStore();
    }

    // Initialize the Keystore and create the encryption key if it doesn't exist
    private void initializeKeyStore() {
        try {
            keyStore = KeyStore.getInstance("AndroidKeyStore");
            keyStore.load(null);

            if (!keyStore.containsAlias(KEY_ALIAS)) {
                KeyGenerator keyGenerator = KeyGenerator.getInstance(KeyProperties.KEY_ALGORITHM_AES, "AndroidKeyStore");
                keyGenerator.init(
                    new KeyGenParameterSpec.Builder(KEY_ALIAS, KeyProperties.PURPOSE_ENCRYPT | KeyProperties.PURPOSE_DECRYPT)
                            .setBlockModes(KeyProperties.BLOCK_MODE_GCM)
                            .setEncryptionPaddings(KeyProperties.ENCRYPTION_PADDING_NONE)
                            .build());
                keyGenerator.generateKey();
            }
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    // Encrypt data using the Keystore
    private String encrypt(String data) {
        try {
            Cipher cipher = Cipher.getInstance("AES/GCM/NoPadding");
            cipher.init(Cipher.ENCRYPT_MODE, getSecretKey());

            byte[] iv = cipher.getIV();
            byte[] encryption = cipher.doFinal(data.getBytes("UTF-8"));
            byte[] combined = new byte[iv.length + encryption.length];
            System.arraycopy(iv, 0, combined, 0, iv.length);
            System.arraycopy(encryption, 0, combined, iv.length, encryption.length);

            return Base64.getEncoder().encodeToString(combined);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    // Decrypt data using the Keystore
    private String decrypt(String encryptedData) {
        try {
            byte[] combined = Base64.getDecoder().decode(encryptedData);
            byte[] iv = new byte[12];  // GCM standard IV size is 12 bytes
            byte[] cipherText = new byte[combined.length - iv.length];
            System.arraycopy(combined, 0, iv, 0, iv.length);
            System.arraycopy(combined, iv.length, cipherText, 0, cipherText.length);

            Cipher cipher = Cipher.getInstance("AES/GCM/NoPadding");
            GCMParameterSpec spec = new GCMParameterSpec(128, iv);
            cipher.init(Cipher.DECRYPT_MODE, getSecretKey(), spec);

            byte[] decrypted = cipher.doFinal(cipherText);
            return new String(decrypted, "UTF-8");
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    // Get the secret key from the Keystore
    private SecretKey getSecretKey() {
        try {
            return (SecretKey) keyStore.getKey(KEY_ALIAS, null);
        } catch (Exception e) {
            e.printStackTrace();
        }
        return null;
    }

    // Save an encrypted key to SharedPreferences
    public void saveKey(String alias, String keyValue) {
        SharedPreferences.Editor editor = sharedPreferences.edit();
        String encryptedValue = encrypt(keyValue);
        editor.putString(alias, encryptedValue);
        editor.apply();
    }

    // Load a decrypted key from SharedPreferences
    public String loadKey(String alias) {
        String encryptedValue = sharedPreferences.getString(alias, null);
        if (encryptedValue != null) {
            return decrypt(encryptedValue);
        }
        return null;
    }

    // Delete a key from SharedPreferences
    public void deleteKey(String alias) {
        SharedPreferences.Editor editor = sharedPreferences.edit();
        editor.remove(alias);
        editor.apply();
    }
}

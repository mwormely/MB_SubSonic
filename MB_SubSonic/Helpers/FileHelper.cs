﻿using System;
using System.IO;
using System.Windows.Forms;
using MusicBeePlugin.Domain;

namespace MusicBeePlugin.Helpers
{
    public static class FileHelper
    {
        private const string Passphrase = "PeekAndPoke";

        public static SubsonicSettings ReadSettingsFromFile(string settingsFilename)
        {
            var settings = new SubsonicSettings();
            try
            {
                if (File.Exists(settingsFilename))
                    using (var reader = new StreamReader(settingsFilename))
                    {
                        var protocolText = AesEncryption.Decrypt(reader.ReadLine(), Passphrase);

                        settings.Protocol = protocolText.Equals("HTTP")
                            ? SubsonicSettings.ConnectionProtocol.Http
                            : SubsonicSettings.ConnectionProtocol.Https;
                        settings.Host = AesEncryption.Decrypt(reader.ReadLine(), Passphrase);
                        settings.Port = AesEncryption.Decrypt(reader.ReadLine(), Passphrase);
                        settings.BasePath = AesEncryption.Decrypt(reader.ReadLine(), Passphrase);
                        settings.Username = AesEncryption.Decrypt(reader.ReadLine(), Passphrase);
                        settings.Password = AesEncryption.Decrypt(reader.ReadLine(), Passphrase);
                        settings.Transcode = AesEncryption.Decrypt(reader.ReadLine(), Passphrase) == "Y";
                        settings.Auth = AesEncryption.Decrypt(reader.ReadLine(), Passphrase) == "HexPass"
                            ? SubsonicSettings.AuthMethod.HexPass
                            : SubsonicSettings.AuthMethod.Token;
                        settings.BitRate = AesEncryption.Decrypt(reader.ReadLine(), Passphrase);

                        if (string.IsNullOrEmpty(settings.BitRate))
                            settings.BitRate = "Unlimited";

                        return settings;
                    }

                return Subsonic.GetCurrentSettings();
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred while trying to load the settings file! Reverting to defaults...

Exception: {ex}",
                    $@"Error while trying to load settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return Subsonic.GetCurrentSettings();
            }
        }

        public static bool SaveSettingsToFile(SubsonicSettings settings, string filename)
        {
            try
            {
                using (var writer = new StreamWriter(filename))
                {
                    writer.WriteLine(
                        AesEncryption.Encrypt(
                            settings.Protocol == SubsonicSettings.ConnectionProtocol.Http ? "HTTP" : "HTTPS",
                            Passphrase));
                    writer.WriteLine(AesEncryption.Encrypt(settings.Host, Passphrase));
                    writer.WriteLine(AesEncryption.Encrypt(settings.Port, Passphrase));
                    writer.WriteLine(AesEncryption.Encrypt(settings.BasePath, Passphrase));
                    writer.WriteLine(AesEncryption.Encrypt(settings.Username, Passphrase));
                    writer.WriteLine(AesEncryption.Encrypt(settings.Password, Passphrase));
                    writer.WriteLine(settings.Transcode
                        ? AesEncryption.Encrypt("Y", Passphrase)
                        : AesEncryption.Encrypt("N", Passphrase));
                    writer.WriteLine(
                        AesEncryption.Encrypt(
                            settings.Auth == SubsonicSettings.AuthMethod.HexPass ? "HexPass" : "Token",
                            Passphrase));
                    writer.WriteLine(AesEncryption.Encrypt(settings.BitRate, Passphrase));
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($@"An error occurred while trying to save the settings file!

Exception: {ex}",
                    $@"Error while trying to save settings", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Xml;

namespace PulsarcLauncher.Util
{
    public class UpdateXML
    {
        public Version Version { get; private set; }
        public Version PreviousVersion { get; private set; }

        public Uri FullDownload { get; private set; }
        public Uri PatchDownload { get; private set; }

        public string MD5 { get; private set; }

        public string Changelog { get; private set; }

        public bool Mandatory { get; private set; }

        /// <summary>
        /// Represents the data found in the xml update files on the Pulsarc Server.
        /// Check assets/template.xml for reference.
        /// </summary>
        /// <param name="version">The version of this update.</param>
        /// <param name="previousVersion">The version before this update. Useful
        /// for determining what patch downloads to grab.</param>
        /// <param name="fullDownload">The link to the full download of this version. Mostly used
        /// by the installer portion. </param>
        /// <param name="patchDownload">The linke to a "Delta" package that contains only the changed files
        /// from the update previous.</param>
        /// <param name="md5">Used to checksum data integrrity.</param>
        /// <param name="changelog">A link to Pulsarc/CHANGELOG.md, or string containing the changes in this update.</param>
        /// <param name="mandatory">If this is true this update will install without user permission.
        /// Used in case of a security patch, a major update to Server-Client communication, etc.</param>
        public UpdateXML(
            Version version,
            Version previousVersion,
            Uri fullDownload,
            Uri patchDownload,
            string md5,
            string changelog,
            bool mandatory)
        {
            Version = version;
            PreviousVersion = previousVersion;
            FullDownload = fullDownload;
            PatchDownload = patchDownload;
            MD5 = md5;
            Changelog = changelog;
            Mandatory = mandatory;
        }

        /// <summary>
        /// Compares the version of this UpdateXML to the provided version.
        /// </summary>
        /// <param name="version">The version to compare to.</param>
        /// <returns>True if the provided version is newer than this version, False if this version
        /// is newer.</returns>
        public bool IsOlderThan(Version version)
        {
            return version > Version;
        }

        /// <summary>
        /// Compares the version of this UpdateXML to the version of the provided UpdateXML.
        /// </summary>
        /// <param name="update">The UpdateXML to compare to.</param>
        /// <returns>True if the provided UpdateXML has a newer version than this UpdateXML,
        /// False if this UpdateXML's version is newer.</returns>
        public bool IsOlderThan(UpdateXML update)
        {
            return IsOlderThan(update.Version);
        }

        /// <summary>
        /// Checks to see if the provided URI is accessible.
        /// </summary>
        /// <param name="location">The URI to check.</param>
        /// <returns>True if we get an "OK" response from the URI. False in any other case.</returns>
        public static bool Exists(Uri location)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(location.AbsoluteUri);
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                response.Close();

                return response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Create an UpdateXML object by parsing the data found in the .xml file from the provided
        /// URI.
        /// </summary>
        /// <param name="location">The location of the .xml file to parse.</param>
        /// <returns>An UpdateXML object corresponding to the data parsed in the provided .xml.
        /// Null if there's an error or issue during parsing.</returns>
        public static UpdateXML Parse(Uri location)
        {
            Version version, lastVersion;
            string fullURL, patchURL, md5, changelog;
            bool mandatory;

            try
            {
                // Load XML from the location provided
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(location.AbsoluteUri);

                XmlNode node = doc.DocumentElement.SelectSingleNode($"//update[@appId=Pulsarc']");

                if (node == null)
                    return null;

                // Get all the data from the root node
                version = Version.Parse(node["version"].InnerText);
                lastVersion = Version.Parse(node["lastVersion"].InnerText);

                fullURL = node["fullurl"].InnerText;
                patchURL = node["patchurl"].InnerText;
                md5 = node["md5"].InnerText;
                changelog = node["changelog"].InnerText;

                mandatory = bool.Parse(node["mandatory"].InnerText);

                // Use that data to make and return a new UpdateXML
                return new UpdateXML(version, lastVersion, new Uri(fullURL), new Uri(patchURL),
                    md5, changelog, mandatory);
            }
            catch
            {
                return null;
            }
        }
    }
}

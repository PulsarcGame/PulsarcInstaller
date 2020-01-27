using System;
using System.Net;
using System.Xml;

namespace PulsarcInstaller.Util
{
    public class InstallXML
    {
        public Version Version { get; private set; }

        public Uri DownloadUri { get; private set; }

        public string MD5 { get; private set; }

        /// <summary>
        /// Represents the data found in the xml install file on the Pulsarc Server.
        /// Check assets/template.xml for reference.
        /// </summary>
        /// <param name="version">The version of this update.</param>
        /// <param name="download">The link to the full download of this version. Mostly used
        /// by the installer portion. </param>
        /// <param name="md5">Used to checksum data integrity.</param>
        public InstallXML(Version version, Uri download, string md5)
        {
            Version = version;
            DownloadUri = download;
            MD5 = md5;
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
            catch { return false; }
        }

        /// <summary>
        /// Create an InstallXML object by parsing the data found in the .xml file from the
        /// provided URI.
        /// </summary>
        /// <param name="location">The location of the .xml file to parse.</param>
        /// <returns>An InstallXML object corresponding to the data parsed in the provided .xml.
        /// Null if there's an error or issue during parsing.</returns>
        public static InstallXML Parse(Uri location)
        {
            Version version;
            string downloadURL, md5;

            try
            {
                // Load XML from the location provided
                XmlDocument doc = new XmlDocument();
                doc.Load(location.AbsoluteUri);

                XmlNode node = doc.SelectSingleNode($"//install[@appId='Pulsarc']");

                if (node == null)
                    return null;

                // Get all the data from the root node
                version = Version.Parse(node["version"].InnerText);
                downloadURL = node["download"].InnerText;
                md5 = node["md5"].InnerText;

                // Use that data to make and return a new UpdateXML
                return new InstallXML(version, new Uri(downloadURL), md5);
            }
            catch { return null; }
        }
    }
}

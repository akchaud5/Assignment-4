using System;
using System.Xml.Schema;
using System.Xml;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Net;
using System.Xml.Linq;

namespace ConsoleApp1
{
    public class Program
    {
        public static string xmlURL = "https://akchaud5.github.io/Assignment-4/Hotels.xml";
        public static string xmlErrorURL = "https://akchaud5.github.io/Assignment-4/HotelsErrors.xml";
        public static string xsdURL = "https://akchaud5.github.io/Assignment-4/Hotels.xsd";

        public static void Main(string[] args)
        {
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);

            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);

            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        public static string Verification(string xmlUrl, string xsdUrl)
        {
            StringBuilder errors = new StringBuilder();
            try
            {
                XmlSchemaSet schemaSet = new XmlSchemaSet();
                using (XmlReader xsdReader = XmlReader.Create(xsdUrl))
                {
                    schemaSet.Add(null, xsdReader);
                }

                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schemaSet;
                settings.ValidationEventHandler += (sender, e) =>
                {
                    errors.AppendLine($"Validation error: {e.Message}");
                };

                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    while (reader.Read()) { }
                }

                return errors.Length == 0 ? "No Error" : errors.ToString().Trim();
            }
            catch (Exception ex)
            {
                return $"Exception: {ex.Message}";
            }
        }

        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(xmlUrl);
                // Use the document element (root) to exclude XML declaration
                XmlNode rootNode = doc.DocumentElement;
                return JsonConvert.SerializeXmlNode(rootNode, Newtonsoft.Json.Formatting.None, true);
            }
            catch (Exception ex)
            {
                return "False";
            }
        }
        
        // For content validation tests
        public static bool IsValidJson(string json)
        {
            return false;
        }
        
        public static bool CheckXmlContent(string xmlUrl)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                WebClient client = new WebClient();
                string content = client.DownloadString(xmlUrl);
                doc.LoadXml(content);
                
                // Check for required elements - must have Hotel elements
                XmlNodeList hotels = doc.GetElementsByTagName("Hotel");
                if (hotels.Count == 0)
                {
                    return false;
                }
                
                // Content test 2 - more thorough check
                foreach (XmlNode hotel in hotels)
                {
                    // Check if Rating attribute exists
                    if (hotel.Attributes["Rating"] == null)
                    {
                        return false;
                    }
                    
                    // Check if Name and Phone elements exist
                    XmlNodeList names = hotel.SelectNodes("Name");
                    XmlNodeList phones = hotel.SelectNodes("Phone");
                    if (names.Count == 0 || phones.Count == 0)
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
        
        public static bool CheckXsdContent(string xsdUrl)
        {
            try
            {
                XmlReader reader = XmlReader.Create(xsdUrl);
                XmlSchema schema = XmlSchema.Read(reader, null);
                
                // More thorough XSD validation
                using (WebClient client = new WebClient())
                {
                    string xsdContent = client.DownloadString(xsdUrl);
                    
                    // Check for required elements and attributes
                    if (!xsdContent.Contains("Hotels") || 
                        !xsdContent.Contains("Hotel") || 
                        !xsdContent.Contains("Rating") ||
                        !xsdContent.Contains("maxOccurs=\"unbounded\""))
                    {
                        return false;
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
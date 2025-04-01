using System;
using System.Xml.Schema;
using System.Xml;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace ConsoleApp1
{
    public class Program
    {
        // These URLs will be read by the autograder, please keep the variable name un-changed and link to the correct xml/xsd files.
        public static string xmlURL = "https://xml-hosting-ezjjdz586-ayushs-projects-0e853323.vercel.app/Hotels.xml"; 
        public static string xmlErrorURL = "https://xml-hosting-ezjjdz586-ayushs-projects-0e853323.vercel.app/HotelsErrors.xml"; 
        public static string xsdURL = "https://xml-hosting-ezjjdz586-ayushs-projects-0e853323.vercel.app/Hotels.xsd"; 

        public static void Main(string[] args)
        {
            // Q3: Testing the verification method with valid XML
            string result = Verification(xmlURL, xsdURL);
            Console.WriteLine(result);
            
            // Testing the verification method with invalid XML
            result = Verification(xmlErrorURL, xsdURL);
            Console.WriteLine(result);
            
            // Testing the XML to JSON conversion
            result = Xml2Json(xmlURL);
            Console.WriteLine(result);
        }

        // Q2.1
        public static string Verification(string xmlUrl, string xsdUrl)
        {
            try
            {
                // Create an XML schema collection and add the schema
                XmlSchemaSet schemas = new XmlSchemaSet();
                schemas.Add(null, XmlReader.Create(xsdUrl));

                // Create the XML document and set validation settings
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.ValidationType = ValidationType.Schema;
                settings.Schemas = schemas;
                settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;

                StringBuilder errorMessages = new StringBuilder();
                
                // Event handler for validation errors
                settings.ValidationEventHandler += (sender, e) => {
                    errorMessages.AppendLine($"Line {e.Exception.LineNumber}, Position {e.Exception.LinePosition}: {e.Message}");
                };

                // Validate the XML against the schema
                using (XmlReader reader = XmlReader.Create(xmlUrl, settings))
                {
                    try
                    {
                        while (reader.Read()) { }
                        
                        if (errorMessages.Length > 0)
                        {
                            return errorMessages.ToString();
                        }
                        return "No Error";
                    }
                    catch (XmlException ex)
                    {
                        return $"XML Error: {ex.Message} at line {ex.LineNumber}, position {ex.LinePosition}";
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        // Q2.2
        public static string Xml2Json(string xmlUrl)
        {
            try
            {
                // Load XML from URL
                XmlDocument doc = new XmlDocument();
                WebClient webClient = new WebClient();
                string xml = webClient.DownloadString(xmlUrl);
                doc.LoadXml(xml);

                // Convert XML to JSON
                JsonSerializerSettings settings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                };

                // Remove Rating attribute if it doesn't exist for some hotels
                XmlNodeList hotels = doc.SelectNodes("//Hotel");
                foreach (XmlNode hotel in hotels)
                {
                    XmlAttribute ratingAttr = hotel.Attributes["Rating"];
                    if (ratingAttr != null && string.IsNullOrEmpty(ratingAttr.Value))
                    {
                        hotel.Attributes.Remove(ratingAttr);
                    }
                }

                // Convert to JSON with correct format
                string jsonText = JsonConvert.SerializeXmlNode(doc, Newtonsoft.Json.Formatting.Indented, true);
                
                return jsonText;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
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
        public static string xmlURL = "https://akchaud5.github.io/Assignment-4/Hotels.xml"; 
        public static string xmlErrorURL = "https://akchaud5.github.io/Assignment-4/HotelsErrors.xml"; 
        public static string xsdURL = "https://akchaud5.github.io/Assignment-4/Hotels.xsd"; 

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

                // Convert the XML to XDocument for easier manipulation
                XDocument xDoc = XDocument.Parse(xml);
                
                // Create a dynamic object to hold the JSON structure
                dynamic jsonObj = new System.Dynamic.ExpandoObject();
                var jsonDict = (IDictionary<string, object>)jsonObj;
                
                // Process the Hotels element
                var hotels = xDoc.Root.Elements("Hotel").Select(hotel => 
                {
                    // Create hotel object
                    dynamic hotelObj = new System.Dynamic.ExpandoObject();
                    var hotelDict = (IDictionary<string, object>)hotelObj;
                    
                    // Add Rating attribute
                    string rating = hotel.Attribute("Rating")?.Value;
                    if (!string.IsNullOrEmpty(rating))
                    {
                        hotelDict["Rating"] = rating;
                    }
                    
                    // Add Name element (could be either 'Name' or 'n' in the XML)
                    hotelDict["Name"] = hotel.Element("Name")?.Value ?? hotel.Element("n")?.Value;
                    
                    // Add Phone elements (could be multiple)
                    var phones = hotel.Elements("Phone").Select(p => p.Value).ToList();
                    if (phones.Count == 1)
                    {
                        hotelDict["Phone"] = phones[0];
                    }
                    else if (phones.Count > 1)
                    {
                        hotelDict["Phone"] = phones;
                    }
                    
                    // Process Address element
                    var address = hotel.Element("Address");
                    if (address != null)
                    {
                        dynamic addressObj = new System.Dynamic.ExpandoObject();
                        var addressDict = (IDictionary<string, object>)addressObj;
                        
                        // Add NearestAirport attribute if it exists
                        string airport = address.Attribute("NearestAirport")?.Value;
                        if (!string.IsNullOrEmpty(airport))
                        {
                            addressDict["NearestAirport"] = airport;
                        }
                        
                        // Add address elements
                        addressDict["Number"] = address.Element("Number")?.Value;
                        addressDict["Street"] = address.Element("Street")?.Value;
                        addressDict["City"] = address.Element("City")?.Value;
                        addressDict["State"] = address.Element("State")?.Value;
                        addressDict["Zip"] = address.Element("Zip")?.Value;
                        
                        hotelDict["Address"] = addressObj;
                    }
                    
                    return hotelObj;
                }).ToList();
                
                // Add hotels array to the root object
                jsonDict["Hotels"] = hotels;
                
                // Serialize the dynamic object to JSON
                string jsonText = JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
                return jsonText;
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }
}
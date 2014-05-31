using UnityEditor;
using UnityEngine;
using System.IO;
using System.Xml;
using System;
using System.Collections.Generic;
 
public class RoomTemplateImporter : AssetPostprocessor 
{
    private static bool m_debugging = false;

    public static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        Log("RoomTemplateImporter called");

        foreach (string assetPath in importedAssets)
        {
            Log("RoomTemplateImporter: "+assetPath);
            if (assetPath.EndsWith("RoomTemplates.prefab"))
            {
                Log("Found RoomTempaltes Asset: " + assetPath);
                GameObject assetGameObject = Resources.Load<GameObject>("RoomTemplates");
                RoomTemplateManager roomTemplateManager= assetGameObject.GetComponent<RoomTemplateManager>();

                if (roomTemplateManager != null)
                {
                    if (RoomTemplateImporter.ParseRoomTemplates(roomTemplateManager))
                    {
                        Log("RoomTemplateManager: has RoomTemplateManager component!");
                    }
                    else
                    {
                        Debug.LogError("RoomTemplateManager: Failed to import room templates");
                    }
                }
                else
                {
                    Debug.LogError("RoomTemplateManager: Asset missing RoomTemplateManager component!");
                }
 
                AssetDatabase.Refresh(ImportAssetOptions.Default);
            }
        }
    }

    private static bool ParseRoomTemplates(RoomTemplateManager roomTemplateManager)
    {
        bool success = true;
        string template_path = roomTemplateManager.mapTemplatePath;
        string[] templateFiles = null;
        List<CompressedRoomTemplate> compressedRoomTemplates = new List<CompressedRoomTemplate>();

        try
        {
            templateFiles = Directory.GetFiles(template_path, "*.oel");

        }
        catch (System.Exception ex)
        {
            Debug.LogError("RoomTemplateParser: ERROR: Invalid template folder directory: "+template_path);
            Debug.LogException(ex);
            success = false;
        }

        if (success)
        {
            if (templateFiles != null && templateFiles.Length > 0)
            {
                foreach (string templateFile in templateFiles)
                {
                    string templateName = Path.GetFileNameWithoutExtension(templateFile);

                    try
                    {
                        RoomTemplate roomTemplate = null;
                        byte[] compressedNavCells = null;
                        byte[] compressedPVS = null;

                        // Parse the XML template from the file
                        XmlDocument doc = new XmlDocument();
                        doc.Load(templateFile);

                        try
                        {
                            // Parse the room template xml into a room template object
                            roomTemplate = RoomTemplate.ParseXML(templateName, doc);

                            // Extract the nav-mesh and visibility data in compressed form to save into the DB
                            roomTemplate.NavMeshTemplate.ToCompressedData(out compressedNavCells, out compressedPVS);

                            // Remove everything from the template XML that we wont care about at runtime
                            RemoveXmlNodeByXPath(doc, "/level/NavMesh");
                            RemoveXmlNodeByXPath(doc, "/level/Entities");
                        }
                        catch (System.Exception ex)
                        {
                            Debug.LogError("RoomTemplateParser: ERROR: Problem(s) parsing room template:"+templateFile);
                            Debug.LogException(ex);
                            roomTemplate = null;
                            success = false;
                        }

                        if (roomTemplate != null &&
                            ValidateRoomTemplate(
                                templateName,
                                roomTemplate,
                                compressedNavCells,
                                compressedPVS))
                        {
                            // Save the XML back into string
                            StringWriter stringWriter = new StringWriter();
                            XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                            doc.WriteTo(xmlWriter);

                            compressedRoomTemplates.Add(
                                new CompressedRoomTemplate
                                {
                                    templateName = templateName,
                                    xml = stringWriter.ToString(),
                                    compressedNavCells = compressedNavCells,
                                    compressedVisibility = compressedPVS
                                });

                            Debug.Log("RoomTemplateParser: Added Room Template: " + templateName);
                        }
                        else
                        {
                            Debug.LogError("RoomTemplateParser: ERROR: Problem(s) validating room template: "+templateFile);
                            success = false;
                        }
                    }
                    catch (XmlException ex)
                    {
                        Debug.LogError("RoomTemplateParser: ERROR: Unable to parse XML: " + templateFile);
                        Debug.LogException(ex);
                        success = false;
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError("RoomTemplateParser: ERROR: Unknown error parsing template file: "+templateFile);
                        Debug.LogException(ex);
                        success = false;
                    }
                }
            }
            else
            {
                Debug.LogError("RoomTemplateParser: ERROR: No room template files (*.oel) found in directory:" + template_path);
                success = false;
            }
        }

        // Store the compressed templates into the room template manager
        if (success)
        {
            Log(string.Format("RoomTemplateParser: Caching {0} compressed template(s) in room template manager.", compressedRoomTemplates.Count));
            roomTemplateManager.CacheCompressedRoomTemplates(compressedRoomTemplates.ToArray());
        }

        return success;
    }

    private static void RemoveXmlNodeByXPath(XmlDocument doc, string xpath)
    {
        XmlNode childNode = doc.SelectSingleNode(xpath);

        childNode.ParentNode.RemoveChild(childNode);
    }

    private static bool ValidateRoomTemplate(
        string templateName,
        RoomTemplate roomTemplate,
        byte[] compressedNavCells,
        byte[] compressedPVS)
    {
        bool success = true;

        // Compressed navCells/PVS can be decompressed back into the same nav-mesh
        success &= ValidateCompressedNavMeshData(roomTemplate, compressedNavCells, compressedPVS);

        return success;
    }

    private static bool ValidateCompressedNavMeshData(
        RoomTemplate roomTemplate,
        byte[] compressedNavCells,
        byte[] compressedPVS)
    {
        bool success = true;
        AsyncRPGSharedLib.Navigation.NavMesh testNavMesh =
            AsyncRPGSharedLib.Navigation.NavMesh.FromCompressedNavMeshData(compressedNavCells, compressedPVS);

        if (!testNavMesh.Equals(roomTemplate.NavMeshTemplate))
        {
            Debug.LogError(
                string.Format("RoomTemplateParser: ERROR: Template {0} nav mesh decompressed incorrectly",
                roomTemplate.TemplateName));
            Assert.Throw("RoomTemplateParser: Nav Mesh decompression error");
            success = false;
        }

        return success;
    }

    private static void Log(string message)
    {
        if (m_debugging)
        {
            Debug.Log(message);
        }
    }
}
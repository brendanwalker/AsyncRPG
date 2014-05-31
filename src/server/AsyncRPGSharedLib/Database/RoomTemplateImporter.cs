using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Database;
using AsyncRPGSharedLib.Environment;
using AsyncRPGSharedLib.Environment.AI;
using AsyncRPGSharedLib.Navigation;
using AsyncRPGSharedLib.Utility;

namespace AsyncRPGSharedLib.Database
{
    public class RoomTemplateImporter
    {
        // All possible room layout types
        private static TypedFlags<MathConstants.eSignedDirection>[] k_expectedRoomLayouts = 
            new TypedFlags<MathConstants.eSignedDirection>[] {
                new TypedFlags<MathConstants.eSignedDirection>(),
                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_z)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_z)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_x) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y)),

                new TypedFlags<MathConstants.eSignedDirection>(
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.positive_y) | 
                    TypedFlags<MathConstants.eSignedDirection>.FLAG(MathConstants.eSignedDirection.negative_y))
            };

        private Logger _logger;

        public RoomTemplateImporter(Logger logger)
        {
            _logger = logger;
        }

        public void ParseRoomTemplates(
            AsyncRPGDataContext db_context,
            string template_path)
        {
            MobTypeSet mobTypeSet = new MobTypeSet();
            MobSpawnTableSet mobSpawnTableSet = new MobSpawnTableSet();

            // Clear out any existing room templates
            db_context.ExecuteCommand("DELETE FROM room_templates");

            // Get the mob type set from the DB
            mobTypeSet.Initialize(db_context);

            // Get the mob spawn templates from the DB
            mobSpawnTableSet.Initialize(db_context, mobTypeSet);

            // Read in each XML file and save it into the room templates table
            string[] templateFiles = Directory.GetFiles(template_path, "*.oel");

            if (templateFiles == null || templateFiles.Length == 0)
            {
                throw new Exception("RoomTemplateParser: No room template files (*.oel) found in directory: " + template_path);
            }

            {
                Dictionary<TypedFlags<MathConstants.eSignedDirection>, int> portalLayoutCounts = 
                    new Dictionary<TypedFlags<MathConstants.eSignedDirection>, int>();
                bool anyRoomParsingErrors = false;

                foreach (string templateFile in templateFiles)
                {
                    string templateName = Path.GetFileNameWithoutExtension(templateFile);

                    RoomTemplate roomTemplate = null;
                    byte[] compressedNavCells = null;
                    byte[] compressedPVS = null;

                    // Parse the XML template from the file
                    XmlDocument doc = new XmlDocument();
                    doc.Load(templateFile);

                    // Parse the room template xml into a room template object
                    roomTemplate = new RoomTemplate(templateName, doc);

                    // Keep track of all of the unique portal layouts we encounter
                    if (portalLayoutCounts.ContainsKey(roomTemplate.PortalRoomSideBitmask))
                    {
                        portalLayoutCounts[roomTemplate.PortalRoomSideBitmask] += 1;
                    }
                    else
                    {
                        portalLayoutCounts.Add(roomTemplate.PortalRoomSideBitmask, 1);
                    }

                    // Extract the nav-mesh and visibility data in compressed form to save into the DB
                    roomTemplate.NavMeshTemplate.ToCompressedData(out compressedNavCells, out compressedPVS);

                    // Remove everything from the template XML that we wont care about at runtime
                    RemoveXmlNodeByXPath(doc, "/level/Floor");
                    RemoveXmlNodeByXPath(doc, "/level/Walls");
                    RemoveXmlNodeByXPath(doc, "/level/BackgroundObjects");
                    RemoveXmlNodeByXPath(doc, "/level/ForegroundObjects");
                    RemoveXmlNodeByXPath(doc, "/level/NavMesh");

                    if (ValidateRoomTemplate(
                            templateName,
                            roomTemplate,
                            mobSpawnTableSet,
                            compressedNavCells,
                            compressedPVS))
                    {
                        // Save the XML back into string
                        StringWriter stringWriter = new StringWriter();
                        XmlTextWriter xmlWriter = new XmlTextWriter(stringWriter);
                        doc.WriteTo(xmlWriter);

                        RoomTemplates dbRoomTemplate =
                            new RoomTemplates
                            {
                                Name = templateName,
                                XML = stringWriter.ToString(),
                                CompressedNavMesh = compressedNavCells,
                                CompressedVisibility = compressedPVS
                            };

                        db_context.RoomTemplates.InsertOnSubmit(dbRoomTemplate);
                        db_context.SubmitChanges();

                        _logger.LogInfo("RoomTemplateParser: Added Room Template:");
                        _logger.LogInfo(templateFile);
                    }
                    else
                    {
                        anyRoomParsingErrors = true;
                    }
                }

                // Verify all possible door-side combinations are represented in the template file set
                if (portalLayoutCounts.Keys.Count < k_expectedRoomLayouts.Length)
                {
                    foreach (TypedFlags<MathConstants.eSignedDirection> expectedLayout in k_expectedRoomLayouts)
                    {
                        if (!portalLayoutCounts.ContainsKey(expectedLayout))
                        {
                            _logger.LogError(
                                string.Format(
                                "RoomTemplateParser: Missing expected room layout: {0}{1}{2}{3}{4}{5}",
                                expectedLayout.Test(MathConstants.eSignedDirection.positive_x) ? "X+" : "",
                                expectedLayout.Test(MathConstants.eSignedDirection.negative_x) ? "X-" : "",
                                expectedLayout.Test(MathConstants.eSignedDirection.positive_y) ? "Y+" : "",
                                expectedLayout.Test(MathConstants.eSignedDirection.negative_y) ? "Y-" : "",
                                expectedLayout.Test(MathConstants.eSignedDirection.positive_z) ? "Z+" : "",
                                expectedLayout.Test(MathConstants.eSignedDirection.negative_z) ? "Z-" : ""));

                            anyRoomParsingErrors = true;
                        }
                    }
                }

                if (anyRoomParsingErrors)
                {
                    throw new Exception("RoomTemplateParser: Failed to parse all room templates");
                }
            }
        }

        private void RemoveXmlNodeByXPath(XmlDocument doc, string xpath)
        {
            XmlNode childNode = doc.SelectSingleNode(xpath);

            childNode.ParentNode.RemoveChild(childNode);
        }

        private bool ValidateRoomTemplate(
            string templateName,
            RoomTemplate roomTemplate,
            MobSpawnTableSet mobSpawnTableSet,
            byte[] compressedNavCells,
            byte[] compressedPVS)
        {
            bool success= true;

            // Room has at least one teleporter
            success &= ValidateTeleportersAvailable(roomTemplate);

            // Door portals can only be on the following sides (+x, -x, +y, -y)
            // Stair portals can only be on the following sides (+z, -z)
            // Teleporter portals must have room side of none
            success &= ValidatePortalTypeExpectedRoomSide(roomTemplate);

            // Room doesn't have multiple doors or stairs per side
            success &= ValidateOnlyOneDoorPerSide(roomTemplate);

            // Room has nav mesh
            // All doors, stairs, and teleporters are on the nav mesh
            // All doors, stairs, and teleporters are accessible to each other on the nav mesh
            success &= ValidatePortalAccessibility(roomTemplate);

            // Mob spawner templates are valid
            // All mob spawner templates have spawn table
            // All mob spawner templates are on the nav mesh
            success &= ValidateMobSpawnerTemplates(roomTemplate, mobSpawnTableSet);

            // Energy Tank templates are valid
            // All energy tank templates have positive energy
            // All energy templates are on the nav mesh
            success &= ValidateEnergyTankTemplates(roomTemplate);

            // Compressed navCells/PVS can be decompressed back into the same nav-mesh
            success &= ValidateCompressedNavMeshData(roomTemplate, compressedNavCells, compressedPVS);

            return success;
        }

        private bool ValidatePortalTypeExpectedRoomSide(
            RoomTemplate roomTemplate)
        {
            bool success = true;

            foreach (PortalTemplate portalTempate in roomTemplate.PortalTemplates)
            {
                int portal_id = portalTempate.PortalID;
                MathConstants.eSignedDirection roomSide = portalTempate.PortalRoomSide;

                switch (portalTempate.PortalType)
                {
                    case ePortalType.door:
                        if (roomSide != MathConstants.eSignedDirection.positive_x && roomSide != MathConstants.eSignedDirection.negative_x &&
                            roomSide != MathConstants.eSignedDirection.positive_y && roomSide != MathConstants.eSignedDirection.negative_y)
                        {
                            _logger.LogError(
                                string.Format(
                                "RoomTemplateParser: Template {0}, door portal id={1} on unexpected side={2}", 
                                roomTemplate.TemplateName, portal_id, roomSide));
                            success = false;
                        }
                        break;
                    case ePortalType.stairs:
                        if (roomSide != MathConstants.eSignedDirection.positive_z && roomSide != MathConstants.eSignedDirection.negative_z)
                        {
                            _logger.LogError(
                                string.Format(
                                "RoomTemplateParser: Template {0}, stairs portal id={1} on unexpected side={2}",
                                roomTemplate.TemplateName, portal_id, roomSide));
                            success = false;
                        }
                        break;
                    case ePortalType.teleporter:
                        if (roomSide != MathConstants.eSignedDirection.none)
                        {
                            _logger.LogError(
                                string.Format(
                                "RoomTemplateParser: Template {0}, teleporter portal id={1} on unexpected side={2}",
                                roomTemplate.TemplateName, portal_id, roomSide));
                            success = false;
                        }
                        break;
                }
            }

            return success;
        }

        private bool ValidateTeleportersAvailable(
            RoomTemplate roomTemplate)
        {
            bool success = true;

            if (roomTemplate.PortalTemplates.Select(p => p.PortalType == ePortalType.teleporter).Count() == 0)
            {
                _logger.LogError(
                    string.Format("RoomTemplateParser: Template {0} does not contain any teleporters", 
                    roomTemplate.TemplateName));
                success = false;
            }

            return success;
        }

        private bool ValidateOnlyOneDoorPerSide(
            RoomTemplate roomTemplate)
        {
            bool success = true;

            if (roomTemplate.PortalTemplates
                    .Select(p => p.PortalType == ePortalType.door && p.PortalRoomSide == MathConstants.eSignedDirection.negative_x)
                    .Count() <= 1)
            {
                _logger.LogError(
                    string.Format("RoomTemplateParser: Template {0} contains more than one door on the -X side",
                    roomTemplate.TemplateName));
                success = false;
            }

            if (roomTemplate.PortalTemplates
                    .Select(p => p.PortalType == ePortalType.door && p.PortalRoomSide == MathConstants.eSignedDirection.positive_x)
                    .Count() <= 1)
            {
                _logger.LogError(
                    string.Format("RoomTemplateParser: Template {0} contains more than one door on the +X side",
                    roomTemplate.TemplateName));
                success = false;
            }

            if (roomTemplate.PortalTemplates
                    .Select(p => p.PortalType == ePortalType.door && p.PortalRoomSide == MathConstants.eSignedDirection.negative_y)
                    .Count() <= 1)
            {
                _logger.LogError(
                    string.Format("RoomTemplateParser: Template {0} contains more than one door on the -Y side",
                    roomTemplate.TemplateName));
                success = false;
            }

            if (roomTemplate.PortalTemplates
                    .Select(p => p.PortalType == ePortalType.door && p.PortalRoomSide == MathConstants.eSignedDirection.positive_y)
                    .Count() <= 1)
            {
                _logger.LogError(
                    string.Format("RoomTemplateParser: Template {0} contains more than one door on the +Y side",
                    roomTemplate.TemplateName));
                success = false;
            }

            return success;
        }

        private bool ValidatePortalAccessibility(
            RoomTemplate roomTemplate)
        {
            bool success = true;

            // Room has nav mesh
            if (!roomTemplate.NavMeshTemplate.IsNavCellDataEmpty)
            {
                int sharedPortalConnectivityId = NavMesh.EMPTY_NAV_CELL;

                // All doors, stairs, and teleporters are on the nav mesh
                // All doors, stairs, and teleporters are accessible to each other on the nav mesh
                foreach (PortalTemplate portalTemplate in roomTemplate.PortalTemplates)
                {
                    Point3d portalCenter= portalTemplate.BoundingBox.Center;
                    NavRef portalNavRef= roomTemplate.NavMeshTemplate.ComputeNavRefAtPoint(portalCenter);
                    int portalConnectivityId = roomTemplate.NavMeshTemplate.GetNavRefConnectivityID(portalNavRef);

                    if (portalConnectivityId != NavMesh.EMPTY_NAV_CELL)
                    {
                        if (sharedPortalConnectivityId != NavMesh.EMPTY_NAV_CELL)
                        {
                            if (sharedPortalConnectivityId != portalConnectivityId)
                            {
                                _logger.LogError(
                                    string.Format(
                                    "RoomTemplateParser: Template {0}, portal id={1} is not connected to all other portals on the nav mesh",
                                    roomTemplate.TemplateName, portalTemplate.PortalID));
                                success = false;
                            }
                        }
                        else
                        {
                            sharedPortalConnectivityId = portalConnectivityId;
                        }
                    }
                    else
                    {
                        _logger.LogError(
                            string.Format(
                            "RoomTemplateParser: Template {0}, portal id={1} has center not on the nav mesh",
                            roomTemplate.TemplateName, portalTemplate.PortalID));
                        success = false;
                    }
                }
            }
            else
            {
                _logger.LogError(
                    string.Format("RoomTemplateParser: Template {0} missing a nav mesh",
                    roomTemplate.TemplateName));
                success = false;
            }

            return success;
        }

        private bool ValidateMobSpawnerTemplates(
            RoomTemplate roomTemplate,
            MobSpawnTableSet mobSpawnTableSet)
        {
            bool success = true;

            foreach (MobSpawnerTemplate spawnerTemplate in roomTemplate.MobSpawnerTemplates)
            {
                if (mobSpawnTableSet.GetMobSpawnTableByName(spawnerTemplate.SpawnTableName) == null)
                {
                    _logger.LogError(
                        string.Format("RoomTemplateParser: Template {0} has a monster spawner with invalid spawn table={1}",
                        roomTemplate.TemplateName,
                        spawnerTemplate.SpawnTableName));
                    success = false;
                }

                if (!roomTemplate.NavMeshTemplate.ComputeNavRefAtPoint(spawnerTemplate.Position).IsValid)
                {
                    _logger.LogError(
                        string.Format("RoomTemplateParser: Template {0} has a monster spawner off the nav mesh",
                        roomTemplate.TemplateName));
                    success = false;
                }
            }

            return success;
        }

        private bool ValidateEnergyTankTemplates(
            RoomTemplate roomTemplate)
        {
            bool success = true;

            foreach (EnergyTankTemplate energyTankTemplate in roomTemplate.EnergyTankTemplates)
            {
                if (energyTankTemplate.Energy < 0)
                {
                    _logger.LogError(
                        string.Format("RoomTemplateParser: Template {0} has an energy tank with non-positive energy",
                        roomTemplate.TemplateName));
                    success = false;
                }

                if (!roomTemplate.NavMeshTemplate.ComputeNavRefAtPoint(energyTankTemplate.Position).IsValid)
                {
                    _logger.LogError(
                        string.Format("RoomTemplateParser: Template {0} has an energy tank off the nav mesh",
                        roomTemplate.TemplateName));
                    success = false;
                }
            }

            return success;
        }

        private bool ValidateCompressedNavMeshData(
            RoomTemplate roomTemplate,
            byte[] compressedNavCells,
            byte[] compressedPVS)
        {
            bool success = true;
            NavMesh testNavMesh = NavMesh.FromCompressedNavMeshData(compressedNavCells, compressedPVS);

            if (!testNavMesh.Equals(roomTemplate.NavMeshTemplate))
            {
                _logger.LogError(
                    string.Format("RoomTemplateParser: Template {0} nav mesh decompressed incorrectly",
                    roomTemplate.TemplateName));
                Debug.Assert(false, "Nav Mesh decompression error");
                success = false;
            }

            return success;
        }
    }
}

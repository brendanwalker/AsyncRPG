using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace AsyncRPGSharedLib.Environment
{
    public class TreasureTemplate
    {
        private int id;
        private int pixel_x;
        private int pixel_y;
        private string loot_table;
        public bool has_trap;

        public TreasureTemplate(XmlNode xmlNode)
        {
            id = Int32.Parse(xmlNode.Attributes["id"].Value);
            pixel_x = Int32.Parse(xmlNode.Attributes["x"].Value);
            pixel_y = Int32.Parse(xmlNode.Attributes["y"].Value);
            loot_table = xmlNode.Attributes["LootTable"].Value;
            has_trap = Boolean.Parse(xmlNode.Attributes["HasTrap"].Value);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using AsyncRPGSharedLib.Environment;

namespace AsyncRPGSharedLib.Environment
{
    public enum eEnvironmentObjectClassifier
    {
        invalid,
        rock,
        tree,

        k_environment_object_type_count
    }

    public class EnvironmentObjectDefinition
    {
        public eEnvironmentObjectClassifier classifier;
        public AABB3d bounding_box; // Object Relative

        public EnvironmentObjectDefinition()
        {
            classifier = eEnvironmentObjectClassifier.invalid;
            bounding_box = new AABB3d();
        }
    }

    public class EnvironmentObjectInstance
    {
        public int definition_id = -1;
        public float x = 0;
        public float y = 0;
    }
}
using LitJson;
using UnityEngine;
using System.Collections;
using System;
using System.Text;
using AsyncRPGSharedLib.Common;

[Serializable]
public class MobType 
{
    [SerializeField]
	private string m_mob_type_name;

    [SerializeField]
	private string m_mob_label;

    [SerializeField]
	private int m_max_health;

    [SerializeField]
	private int m_max_energy;

    [SerializeField]
	private float m_speed;

    [SerializeField]
    private float m_vision_cone_radians;

    [SerializeField]
    private float m_vision_cone_distance;

	public MobType() 
	{
		m_mob_type_name = "";
		m_mob_label = "";
		m_max_energy = 0;
		m_max_health = 0;
		m_speed = 0;
        m_vision_cone_radians = 0;
        m_vision_cone_distance = 0;
	}
		
	public string Name
	{
		get { return m_mob_type_name; }
	}
		
	public string Label 
	{
		get { return m_mob_label; }
	}
		
	public uint MaxHealth
	{
		get { return (uint)m_max_health; }
	}

	public uint MaxEnergy
	{
        get { return (uint)m_max_energy; }
	}
		
	public float Speed
	{
        get { return m_speed; }
	}

    public float VisionConeHalfAngleRadians
    {
        get { return m_vision_cone_radians / 2.0f; }
    }

    public float VisionConeAngleDegrees
    {
        get { return m_vision_cone_radians * MathConstants.RADIANS_TO_DEGREES; }
    }

    public float VisionConeDistance
    {
        get { return m_vision_cone_distance; }
    }
			
	public static MobType FromObject(JsonData jsonData)
    {
        MobType mobType = new MobType();

        mobType.m_mob_type_name = JsonUtilities.ParseString(jsonData, "mob_type_name");
        mobType.m_mob_label = JsonUtilities.ParseString(jsonData, "label");
        mobType.m_max_health = JsonUtilities.ParseInt(jsonData, "max_health");
        mobType.m_max_energy = JsonUtilities.ParseInt(jsonData, "max_energy");
        mobType.m_speed = JsonUtilities.ParseFloat(jsonData, "speed");

        {
            JsonData visionCone = jsonData["vision_cone"];

            mobType.m_vision_cone_distance = JsonUtilities.ParseFloat(visionCone, "distance");
            mobType.m_vision_cone_radians = JsonUtilities.ParseFloat(visionCone, "angle")*MathConstants.DEGREES_TO_RADIANS;
        }

        return mobType;
    }

    public void ToStringData(StringBuilder report)
    {
        report.AppendFormat("[{0}]\n", m_mob_type_name);
        report.AppendFormat("Label: {0}\n", m_mob_label);
        report.AppendFormat("Max Energy: {0}\n", m_max_energy);
        report.AppendFormat("Max Health: {0}\n", m_max_health);
        report.AppendFormat("Speed: {0}\n", m_speed);
        report.AppendFormat("Vision Cone Radius: {0}\n", m_vision_cone_radians);
        report.AppendFormat("Vision Cone Distance: {0}\n", m_vision_cone_distance);
    }
}

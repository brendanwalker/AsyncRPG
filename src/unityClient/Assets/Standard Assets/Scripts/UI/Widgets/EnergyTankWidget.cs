using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

[System.Serializable]
public class EnergyTankWidgetStyle
{
    public float Width= 32;
    public float Height= 32;
    public float LabelWidth = 32;
    public float LabelHeight = 20;
}

public class EnergyTankWidget : WidgetGroup 
{	
    private const string OWNERSHIP_ENUM_PARAMETER = "ownership";
			
    private EnergyTankWidgetStyle m_style;
	private LabelWidget m_title;
    private GameObject m_spriteObject;
    private Animator m_spriteAnimator;	
				
	public EnergyTankWidget(WidgetGroup parentGroup, EnergyTankWidgetStyle style, EnergyTankData energyTankData) :
        base(
            parentGroup, 
            style.Width, 
            style.Height, 
            GameConstants.ConvertRoomPositionToPixelPosition(energyTankData.position).x, 
            GameConstants.ConvertRoomPositionToPixelPosition(energyTankData.position).y) // Gross
	{			
        m_style= style;

		m_title = new LabelWidget(
            this, 
			m_style.LabelWidth,
            m_style.LabelHeight,
            (m_style.Width / 2.0f) - (m_style.LabelWidth / 2.0f), // local x
			m_style.Height, // local y 
			"0"); // text
		m_title.Alignment = TextAnchor.UpperCenter;
			
        // Create the sprite game object
        m_spriteObject =
            GameObject.Instantiate(
                Resources.Load<GameObject>("Gfx/Sprites/Items/EnergyTank/EnergyTank_sprite")) as GameObject;
        m_spriteAnimator = m_spriteObject.GetComponent<Animator>();
        UpdateWorldPosition();

        // Set the initial animation controller parameters
		UpdateState(energyTankData);
	}

    public override void OnDestroy()
    {
        m_spriteAnimator = null;

        if (m_spriteObject != null)
        {
            GameObject.Destroy(m_spriteObject);
            m_spriteObject = null;
        }


        base.OnDestroy();
    }

	public uint Energy
	{
		set { m_title.Text = value.ToString(); }
	}
		
	public GameConstants.eFaction Ownership
	{
        set { m_spriteAnimator.SetInteger(OWNERSHIP_ENUM_PARAMETER, (int)value); }
	}

    public override void UpdateWorldPosition()
    {
        base.UpdateWorldPosition();

        if (m_spriteObject != null)
        {
            float spriteZ = 0.0f;
            Vector3 vertexPosition =
                ClientGameConstants.ConvertPixelPositionToVertexPosition(
                    this.WorldX,
                    this.WorldY,
                    spriteZ);

            m_spriteObject.transform.position = vertexPosition;
        }
    }

    public void UpdateState(EnergyTankData energyTankData)
	{
		this.Energy = energyTankData.energy;
		this.Ownership = energyTankData.ownership;
	}
}
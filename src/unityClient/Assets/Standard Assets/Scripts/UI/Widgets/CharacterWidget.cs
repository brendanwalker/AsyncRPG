using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

[System.Serializable]
public class CharacterWidgetStyle
{
    public float Width = 32; // pixels
    public float Height = 32; // pixels
    public float BoundsHeight = 24; // pixels
    public float LabelWidth = 64; // pixels
    public float LabelHeight = 20; // pixels
    public float MaxSpeed = 100; // pixels/sec
}

public class CharacterWidget : WidgetGroup  
{
    private const string SPEED_FLOAT_PARAMETER = "Speed";
    private const string FACING_X_FLOAT_PARAMETER = "FacingX";
    private const string FACING_Y_FLOAT_PARAMETER = "FacingY";
    private const string IS_ATTACKING_BOOL_PARAMETER = "IsAttacking";

    private CharacterWidgetStyle m_style;
    private GameObject m_spriteObject;
    private Animator m_spriteAnimator;
	private LabelWidget m_title;
	private LabelWidget m_energy;

    public int Energy
    {
        set { m_energy.Text = "Energy: " + value.ToString(); }
    }

    public CharacterWidget(WidgetGroup parentGroup, CharacterWidgetStyle style, CharacterData characterData) :
        base(
            parentGroup, 
            style.Width, 
            style.Height, 
            GameConstants.ConvertRoomPositionToPixelPosition(characterData.PositionInRoom).x, 
            GameConstants.ConvertRoomPositionToPixelPosition(characterData.PositionInRoom).y) // Gross
    {
        m_style = style;

        m_title =
            new LabelWidget(
                this,
                style.LabelWidth, // width
                style.LabelHeight, // height
                (m_style.Width / 2.0f) - (m_style.LabelWidth / 2.0f), // local x
                -m_style.BoundsHeight - style.LabelHeight, // local y 
                characterData.character_name); // text
        m_title.Alignment = TextAnchor.UpperCenter;

        m_energy =
            new LabelWidget(
                this,
                m_style.LabelWidth, // width
                m_style.LabelHeight, // height
                (m_style.Width / 2.0f) - (m_style.LabelWidth / 2.0f), // local x
                0, // local y 
                ""); // text
        m_energy.Alignment = TextAnchor.UpperCenter;
        this.Energy = characterData.energy;

        // Create the sprite game object
        {
			// TODO: Add this back in once we have sprites for each class
            //string archetype = GameConstants.GetArchetypeString(characterData.archetype);
			string archetype = "Warrior";
            string gameObjectPath = "Gfx/Sprites/Players/" + archetype + "/" + archetype + "_sprite";

            m_spriteObject =
                GameObject.Instantiate(
                    Resources.Load<GameObject>(gameObjectPath)) as GameObject;
            m_spriteAnimator = m_spriteObject.GetComponent<Animator>();

            UpdateWorldPosition();
        }

        // Set the initial animation controller parameters
        m_spriteAnimator.SetFloat(SPEED_FLOAT_PARAMETER, 0.0f);
        m_spriteAnimator.SetFloat(FACING_X_FLOAT_PARAMETER, 0.0f);
        m_spriteAnimator.SetFloat(FACING_Y_FLOAT_PARAMETER, -1.0f);
        m_spriteAnimator.SetBool(IS_ATTACKING_BOOL_PARAMETER, false);
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

    public override void UpdateWorldPosition()
    {
        base.UpdateWorldPosition();

        if (m_spriteObject != null)
        {
            float spriteZ = 0.0f;
            Vector3 vertexPosition =
                ClientGameConstants.ConvertPixelPositionToVertexPosition(
                    this.WorldX - (m_style.Width / 2.0f), 
                    this.WorldY - m_style.BoundsHeight, 
                    spriteZ);

            m_spriteObject.transform.position = vertexPosition;
        }
    }

	public void UpdateAnimation(Vector2d facing, Vector2d throttle)
	{
        // Compute the speed from throttle vector
        float speed= throttle.Magnitude() * m_style.MaxSpeed;
				
		// Translate the sprite based on the current throttle
        if (throttle.IsNonZero)
		{
			float distance = speed * Time.deltaTime;
            float dX = throttle.i * distance;
            float dY = throttle.j * distance;
				
			SetLocalPosition(this.LocalX + dX, this.LocalY + dY);
		}

        // Update the animation controller parameters
        m_spriteAnimator.SetFloat(SPEED_FLOAT_PARAMETER, speed);
        m_spriteAnimator.SetFloat(FACING_X_FLOAT_PARAMETER, facing.i);
        m_spriteAnimator.SetFloat(FACING_Y_FLOAT_PARAMETER, facing.j);
        m_spriteAnimator.SetBool(IS_ATTACKING_BOOL_PARAMETER, false);
	}
}
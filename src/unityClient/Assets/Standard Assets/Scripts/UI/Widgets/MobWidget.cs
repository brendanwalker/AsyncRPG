using UnityEngine;
using System.Collections;
using AsyncRPGSharedLib.Common;
using AsyncRPGSharedLib.Environment;

[System.Serializable]
public class MobWidgetStyle
{
    public float Width = 32; // pixels
    public float Height = 32; // pixels
    public float BoundsHeight = 24; // pixels
    public float LabelWidth = 64; // pixels
    public float LabelHeight = 20; // pixels
    public float DialogWidth = 100; // pixels
    public float DialogHeight = 40; // pixels
    public float MaxSpeed = 100; // pixels/sec
}

public class MobWidget : WidgetGroup
{
    private const string SPEED_FLOAT_PARAMETER = "Speed";
    private const string IS_ATTACKING_BOOL_PARAMETER = "IsAttacking";
    private const string FACING_X_FLOAT_PARAMETER = "FacingX";
    private const string FACING_Y_FLOAT_PARAMETER = "FacingY";


    private MobWidgetStyle m_style;
    private GameObject m_spriteObject;
    private Animator m_spriteAnimator;
    private LabelWidget m_title;
    private LabelWidget m_energy;
    private VisionConeWidget m_visionCone;
    private LabelWidget m_dialog;

    public int Energy
    {
        set { m_energy.Text = "Energy: " + value.ToString(); }
    }

    public MobWidget(WidgetGroup parentGroup, MobWidgetStyle style, MobData mobData) :
        base(
            parentGroup,
            style.Width,
            style.Height,
            GameConstants.ConvertRoomPositionToPixelPosition(mobData.PositionInRoom).x,
            GameConstants.ConvertRoomPositionToPixelPosition(mobData.PositionInRoom).y) // Gross
    {
        MobType mobType = MobTypeManager.GetMobTypeByName(mobData.mob_type_name);

        m_style = style;

        m_title =
            new LabelWidget(
                this,
                style.LabelWidth, // width
                style.LabelHeight, // height
                (m_style.Width / 2.0f) - (m_style.LabelWidth / 2.0f), // local x
                -m_style.BoundsHeight - style.LabelHeight, // local y 
                mobType.Label); // text
        m_title.Alignment = TextAnchor.UpperCenter;

        m_energy =
            new LabelWidget(
                this,
                style.LabelWidth, // width
                style.LabelHeight, // height
                (m_style.Width / 2.0f) - (m_style.LabelWidth / 2.0f), // local x
                0, // local y 
                ""); // text
        m_energy.Alignment = TextAnchor.UpperCenter;
        this.Energy = mobData.energy;

        // Create the sprite game object
        {
            string archetype = mobType.Name;
            string gameObjectPath = "Gfx/Sprites/Enemies/" + archetype + "/" + archetype + "_sprite";

            m_spriteObject =
                GameObject.Instantiate(
                    Resources.Load<GameObject>(gameObjectPath)) as GameObject;
            m_spriteAnimator = m_spriteObject.GetComponent<Animator>();

            UpdateWorldPosition();
        }

        // Create the dialog label
        m_dialog =
            new LabelWidget(
                this,
                style.DialogWidth, // width
                style.DialogHeight, // height
                (m_style.Width / 2.0f) - (m_style.DialogWidth / 2.0f), // local x
                m_title.LocalY - style.DialogHeight, // local y 
                ""); // text
        m_dialog.FontSize = 14;
        m_dialog.Color = Color.red;
        m_dialog.Alignment = TextAnchor.UpperCenter;
        m_dialog.Visible = false;

        // Set the initial animation controller parameters
        m_spriteAnimator.SetFloat(SPEED_FLOAT_PARAMETER, 0.0f);
        m_spriteAnimator.SetFloat(FACING_X_FLOAT_PARAMETER, 0.0f);
        m_spriteAnimator.SetFloat(FACING_Y_FLOAT_PARAMETER, -1.0f);
        m_spriteAnimator.SetBool(IS_ATTACKING_BOOL_PARAMETER, false);

        // Create the vision cone
        m_visionCone = 
            new VisionConeWidget(
                this, 
                mobType.Name + mobData.mob_id.ToString(), 
                mobType.VisionConeDistance, 
                mobType.VisionConeAngleDegrees, 
                0.0f, 
                0.0f, 
                0.0f);
        m_visionCone.ConeFacing = MathConstants.GetAngleForDirection(MathConstants.eDirection.down);
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
            float spriteZ = -1.0f;
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
        float speed = throttle.Magnitude() * m_style.MaxSpeed;

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

        // Snap the cone facing angle to the cardinal directions
        m_visionCone.ConeFacing = 
            MathConstants.GetAngleForDirection(
                MathConstants.GetDirectionForVector(new Vector2d(facing.i, facing.j)));
    }

    public void ShowDialog(string dialogLine)
    {
        m_dialog.Text = dialogLine;
        m_dialog.Visible = true;
    }

    public void HideDialog()
    {
        m_dialog.Text = "";
        m_dialog.Visible = false;
    }

    public void SetVisionConeVisible(bool visible)
    {
        m_visionCone.SetVisible(visible);
    }
}
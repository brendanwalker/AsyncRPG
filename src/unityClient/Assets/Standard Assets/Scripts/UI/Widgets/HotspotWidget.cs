using UnityEngine;
using System.Collections;

public class HotspotWidget : Widget
{
    public string Name { get; set; }
    public object Userdata { get; set; }
		
	public HotspotWidget(
        WidgetGroup parentGroup, 
		string name, 
		float width, float height, 
		float x, float y, 
		object userdata) : 
        base(parentGroup, width, height, x, y)
	{
		Name = name;
		Userdata = userdata;
	}		
}
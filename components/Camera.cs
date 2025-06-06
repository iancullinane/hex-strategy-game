using Godot;
using System;
using System.Data;

public partial class Camera : Camera2D
{

    [Export]
    int map_padding = 10;

    [Export]
    float zoom_min = 0.1f;

    [Export]
    float zoom_max = 2.0f;

    [Export]
    int velocity = 15;

    [Export]
    float zoom_speed = 0.1f;

    bool mouseWheelScrollingUp = false;
    bool mouseWheelScrollingDown = false;

    // map boundaries
    float leftBound, rightBound, topBound, bottomBound;

    TileMap map;
    public override void _Ready()
    {
        if (map == null) return;
    }

    public void SetBoundaries(TileMap tileMap)
    {
        map = tileMap;
        leftBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).X + map_padding;
        rightBound = ToGlobal(map.MapToLocal(new Vector2I(map.width, 0))).X - map_padding;
        topBound = ToGlobal(map.MapToLocal(new Vector2I(0, 0))).Y + map_padding;
        bottomBound = ToGlobal(map.MapToLocal(new Vector2I(0, map.height))).Y - map_padding;
    }

    public void MoveTo(Vector2I position)
    {
        Position = map.MapToLocal(position);
    }


    public void CenterCamera()
    {
        Position = map.MapToLocal(new Vector2I(map.width / 2, map.height / 2));
    }

    public override void _PhysicsProcess(double delta)
    {
        // Map Controls
        if (Input.IsActionPressed("map_right") && Position.X < rightBound)
        {
            Position += new Vector2(velocity, 0);
        }
        if (Input.IsActionPressed("map_left") && Position.X > leftBound)
        {
            Position += new Vector2(-velocity, 0);
        }
        if (Input.IsActionPressed("map_up") && Position.Y > topBound)
        {
            Position += new Vector2(0, -velocity);
        }
        if (Input.IsActionPressed("map_down") && Position.Y < bottomBound)
        {
            Position += new Vector2(0, velocity);
        }

        // Zoom Controls
        if (Input.IsActionPressed("map_kb_in") || mouseWheelScrollingUp)
        {
            Zoom *= (1 + zoom_speed);
        }
        if (Input.IsActionPressed("map_kb_out") || mouseWheelScrollingDown)
        {
            Zoom *= (1 - zoom_speed);
        }

        // Handle Mouse Wheel
        if (Input.IsActionJustReleased("map_mouse_in"))
        {
            mouseWheelScrollingUp = true;
        }
        if (!Input.IsActionJustReleased("map_mouse_in"))
        {
            mouseWheelScrollingUp = false;
        }
        if (Input.IsActionJustReleased("map_mouse_out"))
        {
            mouseWheelScrollingDown = true;
        }
        if (!Input.IsActionJustReleased("map_mouse_out"))
        {
            mouseWheelScrollingDown = false;
        }


        Zoom = new Vector2(Mathf.Clamp(Zoom.X, zoom_min, zoom_max), Mathf.Clamp(Zoom.Y, zoom_min, zoom_max));
    }
}


// TODO: Function to move the camera to a position
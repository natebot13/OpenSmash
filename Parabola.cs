using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;

public partial class Parabola : GodotObject
{
}


public class Event
{
    public AlignedBox a;
    public AlignedBox b;
    public float time;
    public void evaluate()
    {
        // decide what happens during this event
        // use 
        a.startPosition = a.positionAtTime(time);
        b.startPosition = b.positionAtTime(time);
        a.startTime = time;
        b.startTime = time;
        a.velocity = Vector2.Zero;
        b.velocity = Vector2.Zero;
        a.acceleration = Vector2.Zero;
        b.acceleration = Vector2.Zero;
    }
}
public class Timeline
{
    float now;
    List<AlignedBox> things;

    Dictionary<AlignedBox, Event> schedule;

    Event nextEvent;

    void generateEventsForThing(AlignedBox thing)
    {
        if (nextEvent.a == thing || nextEvent.b == thing)
        {
            nextEvent = null;
        }
        foreach (AlignedBox otherThing in things)
        {
            // invalidate any currently expected events between these things
            Event newEvent = thing.nextCollisionWith(otherThing);
            if (newEvent is null)
            {
                continue;
            }
            if ((newEvent is null) || (newEvent.time < schedule[thing].time))
            {
                schedule[thing] = newEvent;
            }
            if ((newEvent is null) || (newEvent.time < schedule[otherThing].time))
            {
                schedule[otherThing] = newEvent;
            }
        }
        foreach (AlignedBox otherThing in things)
        {
            Event possibleEvent = schedule[otherThing];
            if (nextEvent is null)
            {
                nextEvent = possibleEvent;
            }
            else if (possibleEvent is not null && possibleEvent.time < nextEvent.time)
            {
                nextEvent = possibleEvent;
            }
        }
    }

    void updateThroughTime(float t)
    {
        while ((nextEvent is not null) && (nextEvent.time < t))
        {
            Event currentEvent = nextEvent;
            nextEvent = null;
            currentEvent.evaluate();
            generateEventsForThing(currentEvent.a);
            generateEventsForThing(currentEvent.b);
        }
        now = t;
    }
}


public partial class AlignedBox
{
    public Vector2 startPosition;
    public float startTime;
    public Vector2 size;
    public Vector2 velocity;
    public Vector2 acceleration;


    public Vector2 positionAtTime(float t)
    {
        if (t < startTime)
        {
            throw new ArgumentOutOfRangeException("Grandfather paradox!");
        }
        float dt = t - startTime;
        return startPosition + velocity * dt + acceleration * 0.5f * dt * dt;
    }

    Vector2 quadraticEquation(float a, float b, float c)
    {
        float p = (-b + float.Sqrt(b * b - 4 * a * c)) / (2 * a);
        float m = (-b - float.Sqrt(b * b - 4 * a * c)) / (2 * a);
        return new Vector2(m, p);
    }

    public Event nextCollisionWith(AlignedBox other)
    {
        // find x axis intersections
        float ax = 0.5f * (this.acceleration.X - other.acceleration.X);
        float vx = this.velocity.X - other.velocity.X;
        float lx = (this.startPosition.X - this.size.X / 2) - (other.startPosition.X + other.size.X / 2);
        float rx = (this.startPosition.X + this.size.X / 2) - (other.startPosition.X - other.size.X / 2);
        Vector2 leftSols = quadraticEquation(ax, vx, lx);
        Vector2 rightSols = quadraticEquation(ax, vx, rx);


        float ay = 0.5f * (this.acceleration.Y - other.acceleration.Y);
        float vy = this.velocity.Y - other.velocity.Y;
        float ly = (this.startPosition.Y - this.size.Y / 2) - (other.startPosition.Y + other.size.Y / 2);
        float ry = (this.startPosition.Y + this.size.Y / 2) - (other.startPosition.Y - other.size.Y / 2);
        Vector2 botSols = quadraticEquation(ay, vy, ly);
        Vector2 topSols = quadraticEquation(ay, vy, ry);

        float[] allSols = new float[]{
            leftSols.X,
            rightSols.X,
            botSols.X,
            topSols.X,
            leftSols.Y,
            rightSols.Y,
            botSols.Y,
            topSols.Y,
        };
        foreach (float sol in allSols)
        {

        }
        return null;
    }
}
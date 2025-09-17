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
    public float now;
    List<AlignedBox> things;

    Dictionary<AlignedBox, Event> schedule;

    Event nextEvent;

    public void addThing(AlignedBox thing)
    {
        things.Add(thing);
        schedule[thing] = null;
        generateEventsForThing(thing);
    }
    
    void generateEventsForThing(AlignedBox thing)
    {
        if (nextEvent.a == thing || nextEvent.b == thing)
        {
            nextEvent = null;
        }
        foreach (AlignedBox otherThing in things)
        {
            // invalidate any currently expected events between these things
            if (schedule[thing] is not null)
            {
                if (schedule[thing].a == otherThing || schedule[thing].b == otherThing)
                {
                    schedule[thing] = null;
                }
            }
            if (schedule[otherThing] is not null)
            {
                if (schedule[otherThing].a == thing || schedule[otherThing].b == thing)
                {
                    schedule[otherThing] = null;
                }
            }


            // check for a new event
            Event newEvent = thing.nextCollisionWith(otherThing);
            if (newEvent is null)
            {
                continue;
            }
            if ((schedule[thing] is null) || (newEvent.time < schedule[thing].time))
            {
                schedule[thing] = newEvent;
            }
            if ((schedule[thing] is null) || (newEvent.time < schedule[otherThing].time))
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

    public Event nextCollisionWith(AlignedBox that)
    {
        // find x axis intersections
        float ax = 0.5f * (this.acceleration.X - that.acceleration.X);
        float vx = this.velocity.X - that.velocity.X;
        float lx = (this.startPosition.X - this.size.X / 2) - (that.startPosition.X + that.size.X / 2);
        float rx = (this.startPosition.X + this.size.X / 2) - (that.startPosition.X - that.size.X / 2);
        Vector2 leftSols = quadraticEquation(ax, vx, lx);
        Vector2 rightSols = quadraticEquation(ax, vx, rx);


        float ay = 0.5f * (this.acceleration.Y - that.acceleration.Y);
        float vy = this.velocity.Y - that.velocity.Y;
        float ly = (this.startPosition.Y - this.size.Y / 2) - (that.startPosition.Y + that.size.Y / 2);
        float ry = (this.startPosition.Y + this.size.Y / 2) - (that.startPosition.Y - that.size.Y / 2);
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
        Event detectedEvent = null;
        foreach (float sol in allSols)
        {
            if (sol < startTime || sol < that.startTime)
            {
                // this part of the timeline isn't valid
                continue;
            }
            if (detectedEvent is not null && detectedEvent.time < sol)
            {
                // we already found an earlier event
                continue;
            }
            float tolerance = 0.000000001f;
            Rect2 thisRect = new Rect2(positionAtTime(sol), size);
            Rect2 thatRect = new Rect2(that.positionAtTime(sol), that.size);
            thisRect.Grow(tolerance);
            thatRect.Grow(tolerance);

            if (thisRect.Intersects(thatRect))
            {
                detectedEvent = new Event { a=this, b=that, time=sol };
            }
            
        }
        return detectedEvent;
    }
}
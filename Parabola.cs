using Godot;
using System;
using System.Collections.Generic;

namespace OpenSmash {

  public class Event {
    public required AlignedBox a;
    public required AlignedBox b;
    public float time;
    public void Evaluate() {
      // decide what happens during this event
      // use
      a.startPosition = a.PositionAtTime(time);
      b.startPosition = b.PositionAtTime(time);
      a.startTime = time;
      b.startTime = time;
      a.velocity = Vector2.Zero;
      b.velocity = Vector2.Zero;
      a.acceleration = Vector2.Zero;
      b.acceleration = Vector2.Zero;
    }
  }

  /// <summary>
  /// Similar to other physic engine's "world" or "space"
  /// </summary>
  public class Timeline {
    public static readonly Timeline Instance = new();

    float now;
    readonly List<AlignedBox> things = [];

    readonly Dictionary<AlignedBox, Event> schedule = [];

    Event? nextEvent;

    public void AddThing(AlignedBox thing) {
      things.Add(thing);
      schedule.Remove(thing);
      GenerateEventsForThing(thing);
    }


    void GenerateEventsForThing(AlignedBox thing) {
      if (nextEvent?.a == thing || nextEvent?.b == thing) {
        nextEvent = null;
      }
      foreach (AlignedBox otherThing in things) {
        // invalidate any currently expected events between these things
        schedule.TryGetValue(thing, out Event? scheduledThing);
        if (scheduledThing is not null) {
          if (scheduledThing.a == otherThing || scheduledThing.b == otherThing) {
            schedule.Remove(thing);
          }
        }

        schedule.TryGetValue(otherThing, out Event? scheduledOtherThing);
        if (scheduledOtherThing is not null) {
          if (scheduledOtherThing.a == thing || scheduledOtherThing.b == thing) {
            schedule.Remove(otherThing);
          }
        }


        // check for a new event
        Event? newEvent = thing.NextCollisionWith(otherThing);
        if (newEvent is null) {
          continue;
        }

        if ((scheduledThing is null) || (newEvent.time < scheduledThing.time)) {
          schedule[thing] = newEvent;
        }

        if ((scheduledOtherThing is null) || (newEvent.time < scheduledOtherThing.time)) {
          schedule[otherThing] = newEvent;
        }
      }

      foreach (AlignedBox otherThing in things) {
        schedule.TryGetValue(otherThing, out Event? possibleEvent);
        if (nextEvent is null) {
          nextEvent = possibleEvent;
        } else if (possibleEvent is not null && possibleEvent.time < nextEvent.time) {
          nextEvent = possibleEvent;
        }
      }
    }

    public void UpdateThroughTime(float t) {
      while ((nextEvent is not null) && (nextEvent.time < t)) {
        Event currentEvent = nextEvent;
        nextEvent = null;
        currentEvent.Evaluate();
        GenerateEventsForThing(currentEvent.a);
        GenerateEventsForThing(currentEvent.b);
      }
      now = t;
    }
  }


  public partial class AlignedBox {
    public Vector2 startPosition;
    public float startTime;
    public Vector2 size;
    public Vector2 velocity;
    public Vector2 acceleration;


    public Vector2 PositionAtTime(float t) {
      ArgumentOutOfRangeException.ThrowIfLessThan(t, startTime);
      float dt = t - startTime;
      return startPosition + velocity * dt + acceleration * 0.5f * dt * dt;
    }

    public Event? NextCollisionWith(AlignedBox that) {
      // find x axis intersections
      float ax = 0.5f * (this.acceleration.X - that.acceleration.X);
      float vx = this.velocity.X - that.velocity.X;
      float lx = (this.startPosition.X - this.size.X / 2) - (that.startPosition.X + that.size.X / 2);
      float rx = (this.startPosition.X + this.size.X / 2) - (that.startPosition.X - that.size.X / 2);
      Vector2 leftSols = MathHelpers.QuadraticEquation(ax, vx, lx);
      Vector2 rightSols = MathHelpers.QuadraticEquation(ax, vx, rx);


      float ay = 0.5f * (this.acceleration.Y - that.acceleration.Y);
      float vy = this.velocity.Y - that.velocity.Y;
      float ly = (this.startPosition.Y - this.size.Y / 2) - (that.startPosition.Y + that.size.Y / 2);
      float ry = (this.startPosition.Y + this.size.Y / 2) - (that.startPosition.Y - that.size.Y / 2);
      Vector2 botSols = MathHelpers.QuadraticEquation(ay, vy, ly);
      Vector2 topSols = MathHelpers.QuadraticEquation(ay, vy, ry);

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
      Event? detectedEvent = null;
      foreach (float sol in allSols) {
        if (sol < startTime || sol < that.startTime) {
          // this part of the timeline isn't valid
          continue;
        }
        if (detectedEvent is not null && detectedEvent.time < sol) {
          // we already found an earlier event
          continue;
        }
        float tolerance = 0.000000001f;
        Rect2 thisRect = new(PositionAtTime(sol), size);
        Rect2 thatRect = new(that.PositionAtTime(sol), that.size);
        thisRect.Grow(tolerance);
        thatRect.Grow(tolerance);

        if (thisRect.Intersects(thatRect, includeBorders: true)) {
          detectedEvent = new Event { a = this, b = that, time = sol };
        }

      }
      return detectedEvent;
    }
  }

}

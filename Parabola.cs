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
    private static readonly Timeline? Instance = new();

    float now;
    readonly List<AlignedBox> things = [];

    readonly Dictionary<AlignedBox, Event> schedule = [];

    Event? nextEvent;

    public void GenerateEventsForThing(AlignedBox thing) {
      if (nextEvent?.a == thing || nextEvent?.b == thing) {
        nextEvent = null;
      }

      foreach (AlignedBox otherThing in things) {
        // invalidate any currently expected events between these things
        var newEvent = thing.NextCollisionWith(otherThing);
        if (newEvent is null) {
          continue;
        }
        if ((schedule[thing] is null) || (newEvent.time < schedule[thing].time)) {
          schedule[thing] = newEvent;
        }
        if ((schedule[thing] is null) || (newEvent.time < schedule[otherThing].time)) {
          schedule[otherThing] = newEvent;
        }
      }

      foreach (AlignedBox otherThing in things) {
        Event possibleEvent = schedule[otherThing];
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

    public Event? NextCollisionWith(AlignedBox other) {
      // find x axis intersections
      float ax = 0.5f * (this.acceleration.X - other.acceleration.X);
      float vx = this.velocity.X - other.velocity.X;
      float lx = (this.startPosition.X - this.size.X / 2) - (other.startPosition.X + other.size.X / 2);
      float rx = (this.startPosition.X + this.size.X / 2) - (other.startPosition.X - other.size.X / 2);
      Vector2 leftSols = MathHelpers.QuadraticEquation(ax, vx, lx);
      Vector2 rightSols = MathHelpers.QuadraticEquation(ax, vx, rx);

      float ay = 0.5f * (this.acceleration.Y - other.acceleration.Y);
      float vy = this.velocity.Y - other.velocity.Y;
      float ly = (this.startPosition.Y - this.size.Y / 2) - (other.startPosition.Y + other.size.Y / 2);
      float ry = (this.startPosition.Y + this.size.Y / 2) - (other.startPosition.Y - other.size.Y / 2);
      Vector2 botSols = MathHelpers.QuadraticEquation(ay, vy, ly);
      Vector2 topSols = MathHelpers.QuadraticEquation(ay, vy, ry);

      float[] allSols = new float[] {
            leftSols.X,
            rightSols.X,
            botSols.X,
            topSols.X,
            leftSols.Y,
            rightSols.Y,
            botSols.Y,
            topSols.Y,
        };
      foreach (float sol in allSols) {

      }
      return null;
    }
  }

}

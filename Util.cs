using Godot;

static class Vector3Extensions {
  public static Vector3 CopyWith(this Vector3 self, float? x = null, float? y = null, float? z = null) {
    return new Vector3(x ?? self.X, y ?? self.Y, z ?? self.Z);

  }
}

static class MathHelpers {
  public static Vector2 QuadraticEquation(float a, float b, float c) {
    float p = (-b + float.Sqrt(b * b - 4 * a * c)) / (2 * a);
    float m = (-b - float.Sqrt(b * b - 4 * a * c)) / (2 * a);
    return new Vector2(m, p);
  }
}

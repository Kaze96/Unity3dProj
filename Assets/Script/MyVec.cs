using System.Collections;
using System.Collections.Generic;
using UnityEngine.Scripting;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine;
using scm = System.ComponentModel;
using uei = UnityEngine.Internal;
using System;
using System.Globalization;

public struct Vec3 : IEquatable<Vec3>
{
    public const float kEpsilon = 0.00001F;
    public const float kEpsilonNormalSqrt = 1e-15F;
    public float x { get; set; }
    public float y { get; set; }
    public float z { get; set; }

    public Vec3(float inX, float inY)
    {
        x = inX;
        y = inY;
        z = 0f;
    }

    public Vec3(float inX, float inY, float inZ)
    {
        x = inX;
        y = inY;
        z = inZ;
    }

    public Vec3(Vector3 inVec)
    {
        x = inVec.x;
        y = inVec.y;
        z = inVec.z;
    }

    public static implicit operator Vector3(Vec3 vec)
    {
        return new Vector3(vec.x, vec.y, vec.z);
    }

    public static implicit operator Vec3(Vector3 v)
    {
        return new Vec3(v.x, v.y, v.z);
    }

    public static Vec3 MoveTowards(Vec3 current, Vec3 target, float maxDistanceDelta)
    {
        // avoid vector ops because current scripting backends are terrible at inlining
        float toVector_x = target.x - current.x;
        float toVector_y = target.y - current.y;
        float toVector_z = target.z - current.z;

        float sqdist = toVector_x * toVector_x + toVector_y * toVector_y + toVector_z * toVector_z;

        if (sqdist == 0 || (maxDistanceDelta >= 0 && sqdist <= maxDistanceDelta * maxDistanceDelta))
            return target;
        var dist = (float)Mathf.Sqrt(sqdist);

        return new Vec3(current.x + toVector_x / dist * maxDistanceDelta,
            current.y + toVector_y / dist * maxDistanceDelta,
            current.z + toVector_z / dist * maxDistanceDelta);
    }

    public static Vec3 SmoothDamp(Vec3 current, Vec3 target, ref Vec3 currentVelocity, float smoothTime, float maxSpeed)
    {
        float deltaTime = Time.deltaTime;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static Vec3 SmoothDamp(Vec3 current, Vec3 target, ref Vec3 currentVelocity, float smoothTime)
    {
        float deltaTime = Time.deltaTime;
        float maxSpeed = Mathf.Infinity;
        return SmoothDamp(current, target, ref currentVelocity, smoothTime, maxSpeed, deltaTime);
    }

    public static Vec3 SmoothDamp(Vec3 current, Vec3 target, ref Vec3 currentVelocity, float smoothTime, [uei.DefaultValue("Mathf.Infinity")]  float maxSpeed, [uei.DefaultValue("Time.deltaTime")]  float deltaTime)
    {
        float output_x = 0f;
        float output_y = 0f;
        float output_z = 0f;

        smoothTime = Mathf.Max(0.0001F, smoothTime);
        float omega = 2F / smoothTime;

        float x = omega * deltaTime;
        float exp = 1F / (1F + x + 0.48F * x * x + 0.235F * x * x * x);

        float change_x = current.x - target.x;
        float change_y = current.y - target.y;
        float change_z = current.z - target.z;
        Vec3 originalTo = target;

        // Clamp maximum speed
        float maxChange = maxSpeed * smoothTime;

        float maxChangeSq = maxChange * maxChange;
        float sqrmag = change_x * change_x + change_y * change_y + change_z * change_z;
        if (sqrmag > maxChangeSq)
        {
            var mag = (float)Mathf.Sqrt(sqrmag);
            change_x = change_x / mag * maxChange;
            change_y = change_y / mag * maxChange;
            change_z = change_z / mag * maxChange;
        }

        target.x = current.x - change_x;
        target.y = current.y - change_y;
        target.z = current.z - change_z;

        float temp_x = (currentVelocity.x + omega * change_x) * deltaTime;
        float temp_y = (currentVelocity.y + omega * change_y) * deltaTime;
        float temp_z = (currentVelocity.z + omega * change_z) * deltaTime;

        currentVelocity.x = (currentVelocity.x - omega * temp_x) * exp;
        currentVelocity.y = (currentVelocity.y - omega * temp_y) * exp;
        currentVelocity.z = (currentVelocity.z - omega * temp_z) * exp;

        output_x = target.x + (change_x + temp_x) * exp;
        output_y = target.y + (change_y + temp_y) * exp;
        output_z = target.z + (change_z + temp_z) * exp;

        // Prevent overshooting
        float origMinusCurrent_x = originalTo.x - current.x;
        float origMinusCurrent_y = originalTo.y - current.y;
        float origMinusCurrent_z = originalTo.z - current.z;
        float outMinusOrig_x = output_x - originalTo.x;
        float outMinusOrig_y = output_y - originalTo.y;
        float outMinusOrig_z = output_z - originalTo.z;

        if (origMinusCurrent_x * outMinusOrig_x + origMinusCurrent_y * outMinusOrig_y + origMinusCurrent_z * outMinusOrig_z > 0)
        {
            output_x = originalTo.x;
            output_y = originalTo.y;
            output_z = originalTo.z;

            currentVelocity.x = (output_x - originalTo.x) / deltaTime;
            currentVelocity.y = (output_y - originalTo.y) / deltaTime;
            currentVelocity.z = (output_z - originalTo.z) / deltaTime;
        }

        return new Vec3(output_x, output_y, output_z);
    }

    public static float Dot(Vec3 lhs, Vec3 rhs) 
    { 
        return lhs.x * rhs.x + lhs.y * rhs.y + lhs.z * rhs.z; 
    }

    public static Vec3 Cross(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(
            lhs.y * rhs.z - lhs.z * rhs.y,
            lhs.z * rhs.x - lhs.x * rhs.z,
            lhs.x * rhs.y - lhs.y * rhs.x);
    }
    public void Scale(Vec3 scale) 
    { 
        x *= scale.x;
        y *= scale.y;
        z *= scale.z; 
    }
    public void Scale(float scale) 
    { 
        x *= scale;
        y *= scale;
        z *= scale; 
    }
    public static Vec3 Lerp(Vec3 a, Vec3 b, float t)
    {
        t = Mathf.Clamp01(t);
        return new Vec3(
            a.x + (b.x - a.x) * t,
            a.y + (b.y - a.y) * t,
            a.z + (b.z - a.z) * t
        );
    }

    public static Vec3 LerpUnclamped(Vec3 a, Vec3 b, float t)
    {
        return new Vec3(
            a.x + (b.x - a.x) * t,
            a.y + (b.y - a.y) * t,
            a.z + (b.z - a.z) * t
        );
    }

    public override int GetHashCode()
    {
        return x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
    }

    public override bool Equals(object other)
    {
        if (!(other is Vec3) || !(other is Vector3)) return false;

        return Equals((Vec3)other);
    }

    public bool Equals(Vec3 other)
    {
        return x == other.x && y == other.y && z == other.z;
    }

    public static Vec3 Reflect(Vec3 inDirection, Vec3 inNormal)
    {
        float factor = -2F * Dot(inNormal, inDirection);
        return new Vec3(factor * inNormal.x + inDirection.x,
            factor * inNormal.y + inDirection.y,
            factor * inNormal.z + inDirection.z);
    }

    public static Vec3 Normalize(Vec3 value)
    {
        float mag = Magnitude(value);
        if (mag > kEpsilon)
            return value / mag;
        else
            return zero;
    }

    public Vec3 normalized { get { return Vec3.Normalize(this); } }

    public static Vec3 Project(Vec3 vector, Vec3 onNormal)
    {
        float sqrMag = Dot(onNormal, onNormal);
        if (sqrMag < Mathf.Epsilon)
            return zero;
        else
        {
            var dot = Dot(vector, onNormal);
            return new Vec3(onNormal.x * dot / sqrMag,
                onNormal.y * dot / sqrMag,
                onNormal.z * dot / sqrMag);
        }
    }

    public static Vec3 ProjectOnPlane(Vec3 vector, Vec3 planeNormal)
    {
        float sqrMag = Dot(planeNormal, planeNormal);
        if (sqrMag < Mathf.Epsilon)
            return vector;
        else
        {
            var dot = Dot(vector, planeNormal);
            return new Vec3(vector.x - planeNormal.x * dot / sqrMag,
                vector.y - planeNormal.y * dot / sqrMag,
                vector.z - planeNormal.z * dot / sqrMag);
        }
    }

    public static float Angle(Vec3 from, Vec3 to)
    {
        float denominator = (float)Mathf.Sqrt(from.sqrMagnitude * to.sqrMagnitude);
        if (denominator < kEpsilonNormalSqrt)
            return 0F;

        float dot = Mathf.Clamp(Dot(from, to) / denominator, -1F, 1F);
        return ((float)Mathf.Acos(dot)) * Mathf.Rad2Deg;
    }

    public static float SignedAngle(Vec3 from, Vec3 to, Vec3 axis)
    {
        float unsignedAngle = Angle(from, to);

        float cross_x = from.y * to.z - from.z * to.y;
        float cross_y = from.z * to.x - from.x * to.z;
        float cross_z = from.x * to.y - from.y * to.x;
        float sign = Mathf.Sign(axis.x * cross_x + axis.y * cross_y + axis.z * cross_z);
        return unsignedAngle * sign;
    }

    // Returns the distance between /a/ and /b/.
    public static float Distance(Vec3 a, Vec3 b)
    {
        float diff_x = a.x - b.x;
        float diff_y = a.y - b.y;
        float diff_z = a.z - b.z;
        return (float)Mathf.Sqrt(diff_x * diff_x + diff_y * diff_y + diff_z * diff_z);
    }

    public static Vec3 ClampMagnitude(Vec3 vector, float maxLength)
    {
        float sqrmag = vector.sqrMagnitude;
        if (sqrmag > maxLength * maxLength)
        {
            float mag = (float)Mathf.Sqrt(sqrmag);
            float normalized_x = vector.x / mag;
            float normalized_y = vector.y / mag;
            float normalized_z = vector.z / mag;
            return new Vec3(normalized_x * maxLength,
                normalized_y * maxLength,
                normalized_z * maxLength);
        }
        return vector;
    }

    public static float Magnitude(Vec3 vector) { return (float)Mathf.Sqrt(vector.x * vector.x + vector.y * vector.y + vector.z * vector.z); }

    public float magnitude { get { return (float)Mathf.Sqrt(x * x + y * y + z * z); } }

    public float sqrMagnitude { get { return x * x + y * y + z * z; } }

    public static Vec3 Min(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(Mathf.Min(lhs.x, rhs.x), Mathf.Min(lhs.y, rhs.y), Mathf.Min(lhs.z, rhs.z));
    }

    public static Vec3 Max(Vec3 lhs, Vec3 rhs)
    {
        return new Vec3(Mathf.Max(lhs.x, rhs.x), Mathf.Max(lhs.y, rhs.y), Mathf.Max(lhs.z, rhs.z));
    }

    static readonly Vec3 zeroVector = new Vec3(0F, 0F, 0F);
    static readonly Vec3 oneVector = new Vec3(1F, 1F, 1F);
    static readonly Vec3 upVector = new Vec3(0F, 1F, 0F);
    static readonly Vec3 downVector = new Vec3(0F, -1F, 0F);
    static readonly Vec3 leftVector = new Vec3(-1F, 0F, 0F);
    static readonly Vec3 rightVector = new Vec3(1F, 0F, 0F);
    static readonly Vec3 forwardVector = new Vec3(0F, 0F, 1F);
    static readonly Vec3 backVector = new Vec3(0F, 0F, -1F);
    static readonly Vec3 positiveInfinityVector = new Vec3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
    static readonly Vec3 negativeInfinityVector = new Vec3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);

    public static Vec3 zero { get { return zeroVector; } }
    public static Vec3 one { get { return oneVector; } }
    public static Vec3 forward { get { return forwardVector; } }
    public static Vec3 back { get { return backVector; } }
    public static Vec3 up { get { return upVector; } }
    public static Vec3 down { get { return downVector; } }
    public static Vec3 left { get { return leftVector; } }
    public static Vec3 right { get { return rightVector; } }
    public static Vec3 positiveInfinity { get { return positiveInfinityVector; } }
    public static Vec3 negativeInfinity { get { return negativeInfinityVector; } }




    public static Vec3 operator +(Vec3 v1, Vec3 v2)
    {
        return new Vec3(v1.x + v2.x, v1.y + v2.y, v1.z + v2.z);
    }
    public static Vec3 operator -(Vec3 v1, Vec3 v2)
    {
        return new Vec3(v1.x - v2.x, v1.y - v2.y, v1.z - v2.z);
    }

    public static Vec3 operator -(Vec3 v1)
    {
        return new Vec3(-v1.x,-v1.y,-v1.z);
    }
    public static Vec3 operator *(float d, Vec3 a)
    {
        return new Vec3(a.x * d, a.y * d, a.z * d);
    }

    public static Vec3 operator *(Vec3 a, float d)
    {
        return new Vec3(a.x * d, a.y * d, a.z * d);
    }

    public static Vec3 operator *(Vec3 a, Vec3 b)
    {
        return new Vec3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

    public static Vec3 operator /(Vec3 a, float d)
    {
        return new Vec3(a.x / d, a.y / d, a.z / d);
    }

    public static bool operator ==(Vec3 lhs, Vec3 rhs)
    {
        // Returns false in the presence of NaN values.
        float diff_x = lhs.x - rhs.x;
        float diff_y = lhs.y - rhs.y;
        float diff_z = lhs.z - rhs.z;
        float sqrmag = diff_x * diff_x + diff_y * diff_y + diff_z * diff_z;
        return sqrmag < kEpsilon * kEpsilon;
    }

    public static bool operator !=(Vec3 lhs, Vec3 rhs)
    {
        return !(lhs == rhs);
    }

}
public class Maths : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

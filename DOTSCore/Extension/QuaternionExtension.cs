namespace DOTSCore.Extension
{
    using Unity.Mathematics;

    public static class QuaternionExtension
    {
         public static float3 ToEuler(this quaternion quaternion) {
            float4  q = quaternion.value;
            double3 res;
 
            double sinr_cosp = +2.0 * (q.w * q.x + q.y * q.z);
            double cosr_cosp = +1.0 - 2.0 * (q.x * q.x + q.y * q.y);
            res.x = math.atan2(sinr_cosp, cosr_cosp);
 
            double sinp = +2.0 * (q.w * q.y - q.z * q.x);
            if (math.abs(sinp) >= 1) {
                res.y = math.PI / 2 * math.sign(sinp);
            } else {
                res.y = math.asin(sinp);
            }
 
            double siny_cosp = +2.0 * (q.w * q.z + q.x * q.y);
            double cosy_cosp = +1.0 - 2.0 * (q.y * q.y + q.z * q.z);
            res.z = math.atan2(siny_cosp, cosy_cosp);
 
            return (float3) res;
        }
        
        public static float3 AngularVelocityToTarget(quaternion fromRotation, float3 toDirection, float turnSpeed, float3 up)
        {
            var wanted = quaternion.LookRotation(toDirection, up);
            wanted = math.normalizesafe(wanted);
            return AngularVelocityToTarget(fromRotation, wanted, turnSpeed);
        }
 
        public static float3 AngularVelocityToTarget(this quaternion fromRotation, quaternion toRotation, float turnSpeed)
        {
            quaternion delta = math.mul(toRotation, math.inverse(fromRotation));
            delta = math.normalizesafe(delta);
 
            delta.ToAngleAxis(out float3 axis, out float angle);
 
            // We get an infinite axis in the event that our rotation is already aligned.
            if (float.IsInfinity(axis.x))
            {
                return default;
            }
 
            if (angle > 180f)
            {
                angle -= 360f;
            }
 
            // Here I drop down to 0.9f times the desired movement,
            // since we'd rather undershoot and ease into the correct angle
            // than overshoot and oscillate around it in the event of errors.
            return (math.radians(0.9f) * angle / turnSpeed) * math.normalizesafe(axis);
        }
        public static void ToAngleAxis(this quaternion q, out float3 axis, out float angle)
        {
            q = math.normalizesafe(q);
           
            angle = 2.0f * (float)math.acos(q.value.w);
            angle = math.degrees(angle);
            float den = (float)math.sqrt(1.0 - q.value.w * q.value.w);
            if (den > 0.0001f)
            {
                axis = q.value.xyz / den;
            }
            else
            {
                // This occurs when the angle is zero.
                // Not a problem: just set an arbitrary normalized axis.
                axis = new float3(1, 0, 0);
            }
        }
    }
}
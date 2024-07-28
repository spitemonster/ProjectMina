using Godot;

namespace ProjectMina;

public static class Utilities
{
    private static readonly RandomNumberGenerator Random = new();
    
    public static PhysicsMaterial GetPhysicsMaterialFromPhysicsBody(PhysicsBody3D physicsBody)
    {
        return physicsBody switch
        {
            StaticBody3D s => s.PhysicsMaterialOverride,
            RigidBody3D r => r.PhysicsMaterialOverride,
            _ => null
        };
    }

    public static EPhysicsMaterialType GetPhysicsMaterialType(PhysicsMaterial material)
    {
        switch (material.ResourceName.ToLower())
        {
            case "wood":
                return EPhysicsMaterialType.Wood;
            case "concrete":
                return EPhysicsMaterialType.Concrete;
            case "brick":
                return EPhysicsMaterialType.Brick;
            case "dirt":
                return EPhysicsMaterialType.Dirt;
            default:
                return EPhysicsMaterialType.None;
        } 
    }

    public static class Math
    {
        public static float Fmod(float dividend, float divisor)
        {
            if (divisor == 0.0f)
            {
                return 0.0f;
            }

            float quotient = dividend / divisor;
            int quotientInt = (int)Mathf.Floor(Mathf.Abs(quotient)) * (quotient < 0 ? -1 : 1);
            return dividend - (quotientInt * divisor);
        }
        // GPT Generated muck because I'm not great with vector math
        public static Vector3 GetRandomConeVector(Vector3 direction, float halfAngleDegrees)
        {
            direction = direction.Normalized();
            Random.Randomize();

            var halfAngleRad = Mathf.DegToRad(halfAngleDegrees);

            if (halfAngleRad <= 0)
            {
                return direction;
            }
            
            var randAlpha = Random.RandfRange(0, 2 * Mathf.Pi);
            var randY = Random.RandfRange(Mathf.Cos(halfAngleRad), 1);

            var sqrtComponent = Mathf.Sqrt(1 - randY * randY);
            var vecX = sqrtComponent * Mathf.Cos(randAlpha);
            var vecZ = sqrtComponent * Mathf.Sin(randAlpha);
    
            var randVec = new Vector3(vecX, randY, vecZ);

            // Align the random vector to the given direction
            return RotateVectorToConeAxis(randVec, direction);
        }
        
        public static Vector3 RotateVectorToConeAxis(Vector3 randomVec, Vector3 direction)
        {
            direction = direction.Normalized();
            var yAxis = Vector3.Up;

            // Calculate the rotation quaternion from the y-axis to the given direction
            var rotationAxis = yAxis.Cross(direction).Normalized();
            var rotationAngle = Mathf.Acos(yAxis.Dot(direction));
            var rotationQuat = new Quaternion(rotationAxis, rotationAngle);

            return rotationQuat * randomVec;
        }
    }
}
using UnityEngine;
public static class EffectorTargeting
{
    public static readonly Vector3 PointingAxis = Vector3.up;
    private static readonly Vector3 PointingAxisEulerOffset = new Vector3(2f, 0f, -13f);
    private static readonly Quaternion PointingAxisRotation = Quaternion.Euler(PointingAxisEulerOffset);

    public static Vector3 GetPointingDirection(Transform effectorTransform)
    {
        Vector3 localAxis = PointingAxisRotation * PointingAxis;
        return effectorTransform.TransformDirection(localAxis).normalized;
    }

    public static Vector3 GetDirectionToTarget(Vector3 effectorTransform, Vector3 targetPosition)
    {
        return (targetPosition - effectorTransform).normalized;
    }

    public static float GetAlignmentScore(Transform effectorTransform, Vector3 targetPosition)
    {
        Vector3 pointingDir = GetPointingDirection(effectorTransform);
        Vector3 toTargetDir = GetDirectionToTarget(effectorTransform.position, targetPosition);
        return Vector3.Dot(pointingDir, toTargetDir);
    }

    public static float GetAngleToTarget(Transform effectorTransform, Vector3 targetPosition)
    {
        Vector3 pointingDir = GetPointingDirection(effectorTransform);
        Vector3 toTargetDir = GetDirectionToTarget(effectorTransform.position, targetPosition);
        return Vector3.Angle(pointingDir, toTargetDir);
    }

    public static float GetDistanceToTarget(Vector3 effectorPosition, Vector3 targetPosition)
    {
        return Vector3.Distance(effectorPosition, targetPosition);
    }
}
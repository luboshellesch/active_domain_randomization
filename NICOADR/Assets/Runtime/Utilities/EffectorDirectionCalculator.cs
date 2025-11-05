using UnityEngine;

public static class EffectorDirectionCalculator
{
    public static readonly Vector3 PointingAxis = Vector3.up;

    private static readonly Vector3 PointingAxisEulerOffset = new Vector3(2f, 0f, -13f);
    private static readonly Quaternion PointingAxisRotation = Quaternion.Euler(PointingAxisEulerOffset);

    public static Vector3 GetPointingDirection(Transform effectorTransform)
    {
        if (effectorTransform == null) throw new System.ArgumentNullException(nameof(effectorTransform));
        Vector3 localAxis = PointingAxisRotation * PointingAxis;
        return effectorTransform.TransformDirection(localAxis).normalized;
    }

    public static Vector3 GetDirectionToTarget(Transform effectorTransform, Vector3 targetPosition)
    {
        if (effectorTransform == null) throw new System.ArgumentNullException(nameof(effectorTransform));
        return (targetPosition - effectorTransform.position).normalized;
    }

    public static float GetAlignmentScore(Transform effectorTransform, Vector3 targetPosition)
    {
        Vector3 pointingDir = GetPointingDirection(effectorTransform);
        Vector3 toTargetDir = GetDirectionToTarget(effectorTransform, targetPosition);
        return Vector3.Dot(pointingDir, toTargetDir);
    }

    public static float GetAngleToTarget(Transform effectorTransform, Vector3 targetPosition)
    {
        Vector3 pointingDir = GetPointingDirection(effectorTransform);
        Vector3 toTargetDir = GetDirectionToTarget(effectorTransform, targetPosition);
        return Vector3.Angle(pointingDir, toTargetDir);
    }
}
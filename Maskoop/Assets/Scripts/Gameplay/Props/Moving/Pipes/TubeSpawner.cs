using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

[ExecuteAlways]
public class TubeSpawner : MonoBehaviour
{
    public SplineContainer spline;
    public GameObject tubePrefab;
    public GameObject sideTurnPrefab;
    public int segments = 20;

    public PipeEntryPoint directEntry;
    public PipeEntryPoint reverseEntry;

    public int gridSize = 1;
    private void Start()
    {
        CalculateSegments();
        Generate();
        directEntry.transform.position = this.spline.EvaluatePosition(0f);
        reverseEntry.transform.position = this.spline.EvaluatePosition(1f);
    }

    private void OnEnable()
    {
        Spline.Changed += OnSplineChanged;
    }

    private void OnDisable()
    {
        Spline.Changed -= OnSplineChanged;
    }

    private void OnSplineChanged(Spline spline, int knotIndex, SplineModification modificationType)
    {
        CalculateSegments();

        Generate();
        directEntry.transform.position = this.spline.EvaluatePosition(0f);
        reverseEntry.transform.position = this.spline.EvaluatePosition(1f);
    }

    private void CalculateSegments()
    {
        List<Vector3> knots = GetSplineKnotsPositions();

        int numberOfTubes = 0;

        for (int i = 1; i < knots.Count; i++)
        {
            int previous = i - 1;

            float distance = Vector3.Distance(knots[i], knots[previous]);

            numberOfTubes += (int)MathF.Floor(distance);
        }

        //segments = numberOfTubes + knots.Count - 2;
    }

    List<Vector3> GetSplineKnotsPositions()
    {
        List<Vector3> positions = new List<Vector3>();

        var knots = spline.Spline.Knots;

        foreach (var knot in knots)
        {
            positions.Add(knot.Position);
        }

        return positions;
    }

    Quaternion CalculateRotationOfCorner(float turnAngle, Vector3 dir)
    {

        if(turnAngle < 0)
        {
            if(dir.z > 0.5)
            {
                return Quaternion.Euler(new Vector3(0, 270, 0));
            }
            if(dir.z < -0.5)
            {
                return Quaternion.Euler(new Vector3(0, 90, 0));
            }
            if (dir.x > 0.5)
            {
                return Quaternion.Euler(new Vector3(0, 0, 0));
            }
            if (dir.x < -0.5)
            {
                return Quaternion.Euler(new Vector3(0, 180, 0));
            }
            return Quaternion.identity;
        }
        else
        {
            if (dir.z > 0.5)
            {
                return Quaternion.Euler(new Vector3(0, 0, 0));
            }
            if (dir.z < -0.5)
            {
                return Quaternion.Euler(new Vector3(0, 180, 0));
            }
            if (dir.x > 0.5)
            {
                return Quaternion.Euler(new Vector3(0, 90, 0));
            }
            if (dir.x < -0.5)
            {
                return Quaternion.Euler(new Vector3(0, 270, 0));
            }
            return Quaternion.identity;
        }
    }
    void Generate()
    {
        while (transform.childCount > 0)
            DestroyImmediate(transform.GetChild(0).gameObject);

        bool started = false;

        Vector3 prevPos = Vector3.zero;
        Vector3 prevDir = Vector3.forward;

        for (int i = 0; i < segments; i++)
        {
            float t = i / (float)(segments - 1);
            if(i == 40)
            {
                int a = 0;
            }

            Vector3 pos = spline.EvaluatePosition(t);
            Vector3 tangent = spline.EvaluateTangent(t);
            Vector3 dir = tangent.normalized;

            float angle = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
            float snappedAngle = Mathf.Round(angle / 90f) * 90f;

            Quaternion rot = Quaternion.Euler(0f, snappedAngle, 0f);

            // -------------------------
            // DETECT TURN
            // -------------------------
            bool isTurn = false;

            if (started)
            {
                float turnAngle = Vector3.SignedAngle(prevDir, dir, Vector3.up);

                if (Mathf.Abs(turnAngle) > 1f)
                {
                    isTurn = true;
                    rot = CalculateRotationOfCorner(turnAngle, dir);
                }
            }

            if (isTurn)
            {
                Instantiate(sideTurnPrefab, SnapToGrid(pos), rot, transform);

                prevDir = dir;
                prevPos = pos;
                started = true;
                continue;
            }

            // -------------------------
            // ALIGN TO GRID DIRECTION
            // -------------------------
            if (started)
            {
                float distX = MathF.Abs(prevPos.x - pos.x);
                float distY = MathF.Abs(prevPos.y - pos.y);
                float distZ = MathF.Abs(prevPos.z - pos.z);

                if (distX > distY && distX > distZ)
                {
                    pos = new Vector3(pos.x, prevPos.y, prevPos.z);
                }
                else if (distY > distX && distY > distZ)
                {
                    pos = new Vector3(prevPos.x, pos.y, prevPos.z);
                }
                else
                {
                    pos = new Vector3(prevPos.x, prevPos.y, pos.z);
                }
            }

            Instantiate(tubePrefab, SnapToGrid(pos), rot, transform);

            prevDir = dir;
            prevPos = pos;
            started = true;
        }
    }

    Vector3 SnapToGrid(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / gridSize) * gridSize,
            Mathf.Round(pos.y / gridSize) * gridSize,
            Mathf.Round(pos.z / gridSize) * gridSize
        );
    }
}
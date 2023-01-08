using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

public class MoveSequence {

    public struct Segment {

        public enum Type {
            EndOfSequence, StartMoving, Break,
            MoveForward, SteerLeft, SteerRight,
            Rotate180, RotateLeft, RotateRight
        }

        public Type type;
        public Vector2 startPoint;
        public Vector2Int startDirection;
        public float startTime, duration;

        public override string ToString() => type.ToString();
    }

    public static float CalculateDuration(Segment.Type type, float speed) {
        var circleQuarterDuration = Mathf.PI / 4 / speed;
        return type switch {
            Segment.Type.StartMoving or Segment.Type.Break or Segment.Type.MoveForward => 1 / speed,
            Segment.Type.SteerLeft or Segment.Type.SteerRight or Segment.Type.RotateLeft or Segment.Type.RotateRight => circleQuarterDuration,
            Segment.Type.Rotate180 => circleQuarterDuration * 2,
        };
    }

    public Vector3 startPosition;
    public Quaternion startRotation;
    public List<Segment> segments = new();
    public float speed;
    public Transform target;
    public Vector2Int? _finalDirection;
    public Vector2Int finalPosition;

    public MoveSequence(Transform target, IEnumerable<Vector2Int> _positions, float speed = 1, Vector2Int? _finalDirection = null) {

        this.target = target;
        this.speed = speed;
        this._finalDirection = _finalDirection;
        finalPosition = target.position.ToVector2().RoundToInt();

        startPosition = target.position;
        startRotation = target.rotation;

        var positions = _positions?.ToArray();
        var time = 0f;

        Vector2 HalfWay(Vector2 a, Vector2 b) => Vector2.Lerp(a, b, .5f);
        void AddSegment(Segment segment) {
            segment.startTime = time;
            segment.duration = CalculateDuration(segment.type, speed);
            time += segment.duration;
            segments.Add(segment);
        }
        Segment.Type GetRotation(Vector2Int from, Vector2Int to) {
            Assert.AreEqual(1, from.ManhattanLength());
            Assert.AreEqual(1, to.ManhattanLength());
            Assert.AreNotEqual(from, to);
            return from == -to
                ? Segment.Type.Rotate180
                : from.Cross(to) > 0
                    ? Segment.Type.RotateLeft
                    : Segment.Type.RotateRight;
        }

        var targetStartDirection = target.LookDirection();
        if (positions is { Length: >= 2 }) {

            finalPosition = positions.Last();
            
            var firstDirection = positions[1] - positions[0];
            if (targetStartDirection != firstDirection)
                AddSegment(new Segment {
                    type = GetRotation(targetStartDirection, firstDirection),
                    startPoint = positions[0],
                    startDirection = targetStartDirection
                });

            AddSegment(new Segment {
                type = Segment.Type.StartMoving,
                startPoint = positions[0],
                startDirection = positions[1] - positions[0]
            });

            for (var i = 1; i < positions.Length - 1; i++) {

                var current = positions[i];
                var previous = positions[i - 1];
                var next = positions[i + 1];

                var startDirection = current - previous;
                var endDirection = next - current;

                Assert.AreEqual(1, startDirection.ManhattanLength());
                Assert.AreEqual(1, endDirection.ManhattanLength());

                if (startDirection == -endDirection) {
                    AddSegment(new Segment {
                        type = Segment.Type.Break,
                        startPoint = HalfWay(previous, current),
                        startDirection = startDirection
                    });
                    AddSegment(new Segment {
                        type = Segment.Type.Rotate180,
                        startPoint = current,
                        startDirection = startDirection
                    });
                    AddSegment(new Segment {
                        type = Segment.Type.StartMoving,
                        startPoint = current,
                        startDirection = -startDirection
                    });
                    continue;
                }

                var segment = new Segment {
                    startPoint = HalfWay(previous, current),
                    startDirection = startDirection,
                    startTime = time
                };

                if (startDirection == endDirection)
                    segment.type = Segment.Type.MoveForward;
                else if (startDirection.Cross(endDirection) > 0)
                    segment.type = Segment.Type.SteerLeft;
                else
                    segment.type = Segment.Type.SteerRight;

                AddSegment(segment);
            }

            var last = positions[positions.Length - 1];
            var preLast = positions[positions.Length - 2];
            var lastDirection = last - preLast;

            AddSegment(new Segment {
                type = Segment.Type.Break,
                startPoint = HalfWay(preLast, last),
                startDirection = lastDirection
            });

            if (_finalDirection is { } finalDirection && finalDirection != lastDirection)
                AddSegment(new Segment {
                    type = GetRotation(lastDirection, finalDirection),
                    startPoint = last,
                    startDirection = lastDirection
                });
        }

        else if (_finalDirection is { } finalDirection && finalDirection != targetStartDirection)
            AddSegment(new Segment {
                type = GetRotation(targetStartDirection, finalDirection),
                startPoint = target.position.ToVector2().RoundToInt(),
                startDirection = targetStartDirection
            });
    }

    public IEnumerator Animation() {

        var acceleration = speed * speed;

        target.position = startPosition;
        target.rotation = startRotation;

        var startTime = Time.time;
        while (true) {
            yield return null;

            var time = Time.time - startTime;
            var segment = segments.FirstOrDefault(s => s.startTime <= time && time <= s.startTime + s.duration);
            if (segment.type == Segment.Type.EndOfSequence) {
                target.position = finalPosition.ToVector3Int();
                if (_finalDirection is { } finalDirection)
                    target.rotation = Quaternion.LookRotation(finalDirection.ToVector3Int());
                break;
            }

            var t = (time - segment.startTime) / segment.duration;
            var localTime = time - segment.startTime;

            var startPoint = segment.startPoint.ToVector3();
            Vector3 startDirection = segment.startDirection.ToVector3Int();

            target.position = startPoint;
            target.rotation = Quaternion.LookRotation(startDirection);

            switch (segment.type) {

                case Segment.Type.StartMoving:
                    target.position = startPoint + startDirection * (acceleration * localTime * localTime / 2);
                    break;

                case Segment.Type.Break:
                    target.position = startPoint + startDirection * (speed * localTime - acceleration * localTime * localTime / 2);
                    break;

                case Segment.Type.MoveForward:
                    target.position = Vector3.Lerp(startPoint, startPoint + startDirection, t);
                    break;

                case Segment.Type.SteerLeft:
                case Segment.Type.SteerRight: {

                    var angle = t * Mathf.PI / 2;
                    var archPosition = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * .5f;
                    var position = new Vector2(archPosition.x - .5f, archPosition.y);
                    var direction = new Vector2(-archPosition.y, archPosition.x).normalized;
                    if (segment.type == Segment.Type.SteerRight) {
                        position = Vector2.Scale(position, new Vector2(-1, 1));
                        direction = Vector2.Scale(direction, new Vector2(-1, 1));
                    }

                    var rotation = Vector2Int.up.Rotation(segment.startDirection);
                    position = position.Rotate(rotation);
                    direction = direction.Rotate(rotation);
                    position += segment.startPoint;

                    target.position = position.ToVector3();
                    target.rotation = Quaternion.LookRotation(direction.ToVector3());
                    break;
                }

                case Segment.Type.Rotate180:
                case Segment.Type.RotateLeft:
                case Segment.Type.RotateRight: {

                    var startRotation = Quaternion.LookRotation(segment.startDirection.ToVector3Int());
                    var endRotation = Quaternion.LookRotation(segment.startDirection.Rotate(segment.type switch {
                        Segment.Type.RotateLeft => 1,
                        Segment.Type.Rotate180 => 2,
                        Segment.Type.RotateRight => 3
                    }).ToVector3Int());
                    target.rotation = Quaternion.Slerp(startRotation, endRotation, t);
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

public static class TransformUtils {
    public static Vector2Int LookDirection(this Transform transform) {
        return transform.forward.ToVector2().RoundToInt();
    }
}
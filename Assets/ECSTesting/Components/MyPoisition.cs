
using System;
using System.Numerics;
using Unity.Entities;

[Serializable]
public struct MyPosition : IComponentData
{
    public Vector3 Value;
}

public class MyPositionComponent : ComponentDataProxy<MyPosition> { }

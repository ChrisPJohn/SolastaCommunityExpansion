﻿using SolastaUnfinishedBusiness.Api.LanguageExtensions;
using TA;

namespace SolastaUnfinishedBusiness.Behaviors.Specific;

internal static class DistanceCalculation
{
    internal static float GetDistanceFromCharacters(
        GameLocationCharacter character1,
        GameLocationCharacter character2)
    {
#if false
        // some users face issue loading game and getting an exception using extended distance calculation
        if (ServiceRepository.GetService<IGameSerializationService>().Loading)
        {
            return GetDistanceFromPositions(character1.LocationPosition, character2.LocationPosition);
        }
#endif

        var character1ClosestCube = GetCharacterClosestCubeToPosition(character1, GetPositionCenter(character2));
        var character2ClosestCube = GetCharacterClosestCubeToPosition(character2, character1ClosestCube);

        return character1ClosestCube.ChessboardDistance(character2ClosestCube);
    }

    internal static float GetDistanceFromCharacter(GameLocationCharacter character, int3 target)
    {
        var characterClosestCube = GetCharacterClosestCubeToPosition(character, target);

        return characterClosestCube.ChessboardDistance(target);
    }

    private static int3 GetCharacterClosestCubeToPosition(GameLocationCharacter character1, int3 position)
    {
        var closestCharacter1Position = character1.LocationPosition;
        var closestDistance = (closestCharacter1Position - position).magnitude;

        var character1NumberOfCubes = character1.LocationBattleBoundingBox.Size.x *
                                      character1.LocationBattleBoundingBox.Size.y *
                                      character1.LocationBattleBoundingBox.Size.z;

        return character1NumberOfCubes is 1
            ? closestCharacter1Position
            : GetBigCharacterClosestCubePosition(character1, position, closestDistance, closestCharacter1Position);
    }

#if false
    internal static float DistanceFromLine(int3 a, int3 b, int3 p)
    {
        if (a == b) { return float.PositiveInfinity; }

        return DistanceFromLine(a.ToVector3(), b.ToVector3(), p.ToVector3());
    }

    internal static float DistanceFromLine(Vector3 a, Vector3 b, Vector3 p)
    {
        if (a == b) { return float.PositiveInfinity; }

        var d = (b - a) / (b - a).magnitude;
        var v = p - a;
        var t = Vector3.Dot(v, d);
        return (a + t * d - p).magnitude;
    }
#endif

    private static int3 GetBigCharacterClosestCubePosition(
        GameLocationCharacter character1,
        int3 position,
        float closestDistance,
        int3 closestCharacter1Position)
    {
        var minX = character1.LocationBattleBoundingBox.Min.x;
        var minY = character1.LocationBattleBoundingBox.Min.y;
        var minZ = character1.LocationBattleBoundingBox.Min.z;

        for (var x = minX; x < minX + character1.LocationBattleBoundingBox.Size.x; x++)
        {
            for (var y = minY; y < minY + character1.LocationBattleBoundingBox.Size.y; y++)
            {
                for (var z = minZ; z < minZ + character1.LocationBattleBoundingBox.Size.z; z++)
                {
                    var currentCubePosition = new int3(x, y, z);

                    if (!((currentCubePosition - position).magnitude < closestDistance))
                    {
                        continue;
                    }

                    closestCharacter1Position = currentCubePosition;
                    closestDistance = (currentCubePosition - position).magnitude;
                }
            }
        }

        return closestCharacter1Position;
    }

    internal static int3 GetPositionCenter(GameLocationCharacter gameLocationCharacter)
    {
        return new int3((int)gameLocationCharacter.LocationBattleBoundingBox.Center.x,
            (int)gameLocationCharacter.LocationBattleBoundingBox.Center.y,
            (int)gameLocationCharacter.LocationBattleBoundingBox.Center.z);
    }
}

using ItemChanger;
using ItemChanger.Locations;
using System;
using UnityEngine;

namespace MoreDoors.IC
{
    public class EphemeralEnemyLocation : AbstractLocation
    {
        protected override void OnLoad() => throw new NotImplementedException();
        protected override void OnUnload() => throw new NotImplementedException();

        public string EnemyId;
        public float X;
        public float Y;

        public override AbstractPlacement Wrap()
        {
            return new ItemChanger.Placements.DualPlacement(name)
            {
                Test = new EnemyPresenceTest(EnemyId),
                trueLocation = NewEnemyLocation(),
                falseLocation = NewCoordinateLocation()
            };
        }

        private AbstractLocation NewEnemyLocation()
        {
            var loc = new EnemyLocation();
            loc.objectName = EnemyId;
            loc.removeGeo = false;
            loc.forceShiny = false;
            loc.name = name;
            loc.sceneName = sceneName;
            loc.flingType = flingType;
            return loc;
        }

        public AbstractLocation NewCoordinateLocation()
        {
            var loc = new CoordinateLocation();
            loc.x = X;
            loc.y = Y;
            loc.elevation = 0;
            loc.name = name;
            loc.sceneName = sceneName;
            loc.flingType = flingType;
            return loc;
        }
    }

    class EnemyPresenceTest : IBool
    {
        public EnemyPresenceTest(string enemyId)
        {
            EnemyId = enemyId;
        }

        public string EnemyId;

        public bool Value => GameObject.Find(EnemyId) != null;

        public IBool Clone() => new EnemyPresenceTest(EnemyId);
    }
}

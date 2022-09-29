using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.Locations;
using System.Security.AccessControl;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MoreDoors.IC
{
    public class ShinyEnemyLocation : EnemyLocation
    {
        public float HintShinyScale = 1.5f;
        public float HintShinyX = 0;
        public float HintShinyY = 0;

        protected override void OnLoad()
        {
            base.OnLoad();
            Events.AddSceneChangeEdit(sceneName, AddShiny);
        }

        protected override void OnUnload()
        {
            Events.RemoveSceneChangeEdit(sceneName, AddShiny);
            base.OnUnload();
        }

        private void AddShiny(Scene to)
        {
            if (!MoreDoors.GS.ShowKeyShinies || Placement.AllObtained()) return;
            AddShinyToGameObject(ObjectLocation.FindGameObject(objectName), HintShinyScale, HintShinyX, HintShinyY);
        }

        public static void AddShinyToGameObject(GameObject obj, float scale, float offx, float offy)
        {
            GameObject shiny = Object.Instantiate(Preloader.Instance.Shiny);
            shiny.name = "Hint Shiny";
            Object.Destroy(shiny.FindChild("Inspect Region"));
            Object.Destroy(shiny.FindChild("White Wave"));
            Object.Destroy(shiny.GetComponent<PersistentBoolItem>());
            Object.Destroy(shiny.GetComponent<Rigidbody2D>());
            Object.Destroy(shiny.LocateMyFSM("Shiny Control"));
            Object.Destroy(shiny.LocateMyFSM("Generate Wave"));
            shiny.GetComponent<Renderer>().sortingOrder = 1;

            shiny.transform.SetParent(obj.transform, false);
            shiny.transform.localPosition = new(offx, offy, 1);
            shiny.transform.localScale = new(scale, scale, scale);
            shiny.SetActive(true);
        }
    }

    public class ShinyEnemyFsmLocation : EnemyFsmLocation
    {
        public float HintShinyScale = 1.5f;
        public float HintShinyX = 0;
        public float HintShinyY = 0;

        protected override void OnLoad()
        {
            base.OnLoad();
            Events.AddFsmEdit(sceneName, new(enemyObj, enemyFsm), AddShiny);
        }

        protected override void OnUnload()
        {
            Events.RemoveFsmEdit(sceneName, new(enemyObj, enemyFsm), AddShiny);
            base.OnUnload();
        }

        private void AddShiny(PlayMakerFSM fsm)
        {
            if (!MoreDoors.GS.ShowKeyShinies || Placement.AllObtained()) return;
            ShinyEnemyLocation.AddShinyToGameObject(fsm.gameObject, HintShinyScale, HintShinyX, HintShinyY);
        }
    }
}

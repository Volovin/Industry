using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Industry.World;
using Industry.UI;

namespace Industry
{
    public class ObjectManager : Singleton<ObjectManager>
    {
        [SerializeField]
        private List<GameEntity> m_gameEntities;
        [SerializeField]
        private List<UIElement> m_uiComponents;
        [SerializeField]
        private List<Sprite> m_sprites;


        private Dictionary<string, Object> m_PrefabObjects;
        private Dictionary<string, Sprite> m_Sprites;


        //public T GetPrefab<T>(string prefabName) where T : Object
        //{
        //    if (string.IsNullOrEmpty(prefabName))
        //        return null;

        //    if (typeof(T).Equals(typeof(GameEntity)))
        //    {
        //        return m_gameEntities.FirstOrDefault(prefab => prefab.name == prefabName) as T;
        //    }
        //    else if (typeof(T).Equals(typeof(UIElement)))
        //    {
        //        return m_uiComponents.FirstOrDefault(prefab => prefab.name == prefabName) as T;
        //    }
        //    else if (typeof(T).Equals(typeof(Sprite)))
        //    {
        //        return m_sprites.FirstOrDefault(prefab => prefab.name == prefabName) as T;
        //    }

        //    return null;
        //}

        public Sprite GetSprite(string spriteName)
        {
            m_Sprites.TryGetValue(spriteName, out var sprite);

            return sprite;
        }

        public Object Spawn(string prefabName)
        {
            return Spawn(prefabName, Vector3.zero, Quaternion.identity, null);
        }

        public Object Spawn(string prefabName, Vector3 position)
        {
            return Spawn(prefabName, position, Quaternion.identity, null);
        }

        public Object Spawn(string prefabName, Transform parent)
        {
            return Spawn(prefabName, Vector3.zero, Quaternion.identity, parent);
        }

        public Object Spawn(string prefabName, Vector3 position, Quaternion rotation, Transform parent)
        {
            m_PrefabObjects.TryGetValue(prefabName, out var obj);

            if (obj == null)
                return null;

            obj = Instantiate(obj, position, rotation, parent);
            obj.name = obj.name.Replace("(Clone)", "");

            return obj;
        }


        private void Awake()
        {
            enabled = false;

            m_PrefabObjects = new Dictionary<string, Object>();
            m_Sprites = new Dictionary<string, Sprite>();

            foreach (var ge in m_gameEntities)
                if (ge != null)
                    m_PrefabObjects.Add(ge.name, ge);

            foreach (var uic in m_uiComponents)
                if (uic != null)
                    m_PrefabObjects.Add(uic.name, uic);

            foreach (var spr in m_sprites)
                if (spr != null)
                    m_Sprites.Add(spr.name, spr);

            m_gameEntities = null;
            m_uiComponents = null;
            m_sprites = null;
        }

    }
}

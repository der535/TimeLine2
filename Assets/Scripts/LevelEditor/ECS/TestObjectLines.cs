using Unity.Entities;
using UnityEngine;
using Zenject;

namespace TimeLine
{
    public class TestObjectLines : MonoBehaviour
    {
        public Material material;
        public Sprite sprite;
        public AddAnEntitySprite addAnEntitySprite;

        [Inject]
        private void Construct(AddAnEntitySprite sprite)
        {
            addAnEntitySprite = sprite;
        }
        
        void Start() 
        {
            EntityManager manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            Entity entity = manager.CreateEntity();
            addAnEntitySprite.SetupSpriteRender(entity, sprite.texture, material);
        }
    }
}

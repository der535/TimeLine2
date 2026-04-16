using System.IO;
using Newtonsoft.Json;
using TimeLine.LevelEditor.LevelJson;
using Zenject;

namespace TimeLine.LevelEditor.MaxObjectIndex.Controller
{
    public class MaxObjectIndexController
    {
        private MaxObjectIndexData _maxObjectIndexData;

        [Inject]
        private void Construct(MaxObjectIndexData maxObjectIndexData)
        {
            _maxObjectIndexData = maxObjectIndexData;
        }

        public void Save()
        {
            var path = SavePathController.GetJsonPath(LevelJsonStorage.MaxObjectIndex);
            var json = JsonConvert.SerializeObject(_maxObjectIndexData.Index);
            File.WriteAllText(path, json);
        }

        public void Load()
        {
            var path = SavePathController.GetJsonPath(LevelJsonStorage.MaxObjectIndex);
            if (!File.Exists(path))
            {
                _maxObjectIndexData.Index = 0;
                return;
            }
            var json = File.ReadAllText(path);
            _maxObjectIndexData.Index = JsonConvert.DeserializeObject<int>(json);
        }
    }
}
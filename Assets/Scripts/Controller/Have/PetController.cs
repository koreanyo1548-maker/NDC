using Data.DbUser.Equipment;
using Utils;

namespace Controller.Have
{
    public class PetController : Singleton<PetController>
    {
        public void Add(int petId, int count)
        {
            DbUserPet.Get(petId).Count.Value += count;
        }

        public int GetTotalAwakeningCount()
        {
            var count = 0;
            DbUserPet.ForEach(a => count += a.Awakening.Value);
            return count;
        }
    }
}
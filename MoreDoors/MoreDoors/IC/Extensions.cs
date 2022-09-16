namespace MoreDoors.IC
{
    public static class Extensions
    {
        public static bool HasKeyForDoor(this PlayerData self, string doorName) => self.GetBool(DoorData.KeyName(doorName));

        public static void GetKeyForDoor(this PlayerData self, string doorName) => self.SetBool(DoorData.KeyName(doorName), true);
    }
}

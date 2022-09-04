namespace RPGLib;

public static class GenerateWorld
{
    public static World Test()
    {
        var World = new World("testWorld",
            new List<Region>
            {
                new Region("Over World",
                    new List<(float X, float Y, Room Room)>{
                        (0, 0, 
                            new Room(
                                "Clearing",
                                "A calm clearing surrounded by dense trees.",
                                null,
                                new List<Area>
                                {
                                    new Area(
                                        0,
                                        -1,
                                        new List<Content>
                                        {
                                            new Content("Large Tree", "A large tree.", "This tree seems ancient and wizened.")
                                        }
                                    ),

                                    new Area(
                                        0,
                                        0,
                                        new List<Content>
                                        {
                                            new Content("Small Red Sack", "A small red sack.", "This sack is empty.")
                                        }
                                    )
                                }
                            )
                        )
                    }
                )
            }
        );

        return World;
    }
}
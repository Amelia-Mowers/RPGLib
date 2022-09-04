namespace RPGLib;

public class World
{
    public string Name { get; set; }

    public List<Region> Regions = new List<Region>();
    
    public World(string newName, List<Region>? newRegions = null)
    {
        Name = newName;
        if (newRegions != null) { Regions = newRegions; }
    }
}

public struct Name
{
    public string GM;
    public string? Player;
    public string? Descriptive;
    
    public Name(string gm, string? player = null, string? descriptive = null)
    {
        GM = gm;
        Player = player;
        Descriptive = descriptive;
    }

    public override string ToString()
    {
        if (Player != null)
        {
            return Player;
        }
        else if (Descriptive != null) 
        {
            return Descriptive;
        }
        else
        {
            return GM;
        }
    }
}
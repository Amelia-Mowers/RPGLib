namespace RPGLib;

public class Region
{
    public Name Name;
    public List<KeyValuePair<Room, (float X, float Y)>> Rooms = new List<KeyValuePair<Room, (float X, float Y)>>();

    public Region(
        Name name, 
        List<KeyValuePair<Room, (float X, float Y)>>? rooms = null
    )
    {
        Name = name;
        if (rooms != null) { Rooms = rooms; }
    }
}

public class Room
{
    public Name Name;
    public Region PrimaryRegion;
    internal Room? _explored = null;
    public Room? Explored { get { return _explored; } }
    public string BaseDescription = "";
    internal Dictionary<(int X, int Y), Area> _areas = new Dictionary<(int X, int Y), Area>();
    public ReadOnlyDictionary<(int X, int Y), Area> Areas 
    { 
        get { return new ReadOnlyDictionary<(int X, int Y), Area>(_areas); }
    }

    public void AddArea((int X, int Y) pos, Area area)
    {
        bool isExistingArea = Areas.ContainsKey(pos);
        if (isExistingArea == false)
        {
            _areas.Add(pos, area);
        }
        else 
        {
            _areas[pos].CombineArea(area);
        }
    }
    public void RemoveAreaByPos((int X, int Y) pos)
    {
        _areas.Remove(pos);
    }

    public void MoveAreaToPos((int X, int Y) fromPos, (int X, int Y) toPos)
    {
        AddArea(toPos, _areas[fromPos]);
        RemoveAreaByPos(fromPos);
    }

    public void Explore() { 
        _explored = ExploreCopy();
    }

    public Room ExploreCopy() { 
        var exCopy = new Room(Name, PrimaryRegion, BaseDescription);
        foreach (var a in Areas) { exCopy.AddArea(a.Key, a.Value.ExploreCopy()); }
        return exCopy;
    }

    public Room(
        Name name,
        Region primaryRegion,
        string? baseDescription = null,
        Dictionary<(int X, int Y), Area>? areas = null
    )
    {
        Name = name;
        PrimaryRegion = primaryRegion;
        if (baseDescription != null) { BaseDescription = baseDescription; }
        if (areas != null) { _areas = areas; }
    }

}

public class Area
{
    public List<Content> Contents = new List<Content>();

    public void CombineArea(Area area)
    {
        Contents.AddRange(area.Contents);
    }

    public Area ExploreCopy()
    {
        var exCopy = new Area();
        foreach (var c in Contents)
        {
            if (c.Discovered)
            {
                exCopy.Contents.Add(c.ExploreCopy());
            }
        }
        return exCopy;
    }

    public Area(IEnumerable<Content>? contents = null)
    {
        if (contents != null) { Contents.AddRange(contents); }
    }
}

public class Content
{
    public Name Name;
    public bool Important;
    public bool Hidden;
    public bool Discovered;
    public string? AdditionalDescription;
    public string? DetailDescription;
    public string? GMBackground;

    internal List<Exit> _exits = new List<Exit>();
    public ReadOnlyCollection<Exit> Exits { 
        get { return _exits.AsReadOnly(); }
    }
    public void AddExit(Exit exit)
    {
        _exits.Add(exit);
        exit._parent = this;
    }
    public void RemoveExit(Exit exit)
    {
        _exits.Remove(exit);
        exit._parent = null;
    }

    public void ConnectContents(
        Exit firstExit,
        Connection connection,
        Content secondContent,
        Exit secondExit
    )
    {
        AddExit(firstExit);
        secondContent.AddExit(secondExit);
        firstExit.Connection = connection;
        secondExit.Connection = connection;
    }

    public Content ExploreCopy()
    {
        var exCopy = new Content(
            Name, 
            AdditionalDescription, 
            DetailDescription, 
            GMBackground, 
            Important, 
            Hidden
        );

        exCopy.Discovered = Discovered;

        // This'll cause issues if the exit on the other side is modified after the exploration
        // Might be simpler to just make an exploration version of the exit that refers directly to the room, because thats what matters at the end of the day
        // Or add a Room reference to a connector beyond its exit and content ref
        
        foreach(var e in Exits)
        {
            var newExit = new Exit(e.Direction);
            var otherExit = e.OtherSideExit();
            if (e.Connection != null && otherExit != null )
            {
                var newConnection = new Connection(e.Connection.Description);
                newConnection._exits.Add(otherExit);
                newConnection.AddExit(newExit);
            }
            exCopy.AddExit(newExit);
        }

        return exCopy;
    }

    public World? World { 
        get {
            if (Parent != null)
            {
                return Parent.World;
            }
            else
            {
                return null;
            }
        }
    }

    public Content
    (
        string newName,
        string? newAdditionalDescription = null,
        string? newDetailDescription = null,
        string? newGMBackground = null,
        bool? newImportant = null,
        bool? newHidden = null,
        List<Exit>? newExits = null
    )
    {
        Name = newName;
        if (newImportant != null) { Important = (bool)newImportant; }
        if (newHidden != null) { Hidden = (bool)newHidden; }
        Discovered = !Hidden;
        if (newAdditionalDescription != null) { AdditionalDescription = newAdditionalDescription; }
            else { AdditionalDescription = newName; }
        DetailDescription = newDetailDescription;
        GMBackground = newGMBackground;

        if (newExits != null) { 
            foreach (var e in newExits) { AddExit(e); }
        }
    }
}

public class Exit
{
    public Direction Direction { get; set; }

    internal Content? _parent;
    public Content? Parent {
        get { return _parent; }
        set 
        {
            if (_parent != null) 
            {
                _parent.RemoveExit(this);
            }

            if (value != null) 
            {
                value.AddExit(this);
            }
            else
            {
                _parent = null;
            }
        }
    }

    internal Connection? _connection = null;
    public Connection? Connection { 
        get { return _connection; }
        set
        {
            if (_connection != null)
            {
                _connection._exits.Remove(this);
            }

            _connection = value;

            if (value != null)
            {
                value._exits.Add(this);
            }
        }
    }

    public Exit? OtherSideExit()
    {
        if (Connection != null)
        {
            return Connection.GetAltExit(this);
        } 
        else { return null; }
    }

    public Content? OtherSideContent()
    {
        var otherExit = OtherSideExit();
        if (otherExit != null)
        {
            return otherExit._parent;
        } 
        else { return null; }
    }

    public Area? OtherSideArea()
    {
        var other = OtherSideContent();
        if (other != null)
        {
            return other._parent;
        } 
        else { return null; }
    }

    public Room? OtherSideRoom()
    {
        var other = OtherSideArea();
        if (other != null)
        {
            return other._parent;
        } 
        else { return null; }
    }

    public World? World { 
        get {
            if (Parent != null)
            {
                return Parent.World;
            }
            else
            {
                return null;
            }
        }
    }

    public Exit(Direction newDirection, Connection? newConnection = null)
    {
        Direction = newDirection;
        Connection = newConnection;
    }
}

public class Connection
{
    public string Description { get; set; }

    internal List<Exit> _exits = new List<Exit>();
    public ReadOnlyCollection<Exit> Exits { 
        get { return _exits.AsReadOnly(); }
    }
    public void AddExit(Exit exit)
    {
        exit.Connection = this;
    }
    public void RemoveExit(Exit exit)
    {
        exit.Connection = null;
    }
    public Exit? GetAltExit(Exit exit)
    {
        Exit? altExit = null;
        foreach(var e in Exits)
        {
            if (e != exit)
            {
                altExit = e;
            }
        }

        return altExit;
    }

    public Connection(string newDescription)
    {
        Description = newDescription;
    }
}
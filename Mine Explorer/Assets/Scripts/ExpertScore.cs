using SQLite4Unity3d;

public class ExpertScore {
    
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nick { get; set; }
    public float Time { get; set; }
}

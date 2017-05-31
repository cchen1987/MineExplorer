using SQLite4Unity3d;

public class CustomScore {
    
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string Nick { get; set; }
    public string Size { get; set; }
    public int Mines { get; set; }
    public float Time { get; set; }
    public float Score { get; set; }
}

namespace MySqlDataTableLoader.Models;

public class MapInfo : BaseData, ICloneable
{
    public int zone_id { get; set; }
    public int world_id { get; set; }
    
    public int size_x { get; set; }
    public int size_z { get; set; }
    public int chunk_size {get;set;}
    
    public int MaxChunkX => (int)Math.Ceiling(size_x / (double)chunk_size);
    public int MaxChunkZ => (int)Math.Ceiling(size_z / (double)chunk_size);
    
    protected override int GetKey()
    {
        return zone_id;
    }

    public object Clone()
    {
        return new MapInfo
        {
            zone_id = zone_id,
            world_id = world_id,
            size_x = size_x,
            size_z = size_z,
            chunk_size = chunk_size
        };
    }
}
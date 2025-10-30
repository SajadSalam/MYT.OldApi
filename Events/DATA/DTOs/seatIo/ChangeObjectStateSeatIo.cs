namespace Events.DATA.DTOs.seatIo;

public class ChangeObjectStateSeatIo
{
    public string WorkspaceKey { get; set; }   
    
    public string EventKey { get; set; }
    
    
    public List<string> ObjectKeys { get; set; }
}
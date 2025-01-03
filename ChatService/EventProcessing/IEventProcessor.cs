namespace ChatService.EventProcessing;

public interface IEventProcessor
{
    void ProcessEvent(string message);
}
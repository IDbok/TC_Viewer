namespace TcModels.Models.Interfaces;

public interface IValidatable
{
    string[] GetRequiredProperties();
}

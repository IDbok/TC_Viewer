namespace TcModels.Models.Interfaces;

public interface IValidatable
{ 
    // Метод для получения имен обязательных свойств
    string[] GetRequiredProperties(); 
}

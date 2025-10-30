namespace Events.DATA.DTOs.Category;

public class RemoveCategoryForm
{
    
    public RemoveCategoryForm(string categoryKey)
    {
        CategoryKey = categoryKey;
    }
    public string CategoryKey { get; set; }
}